param(
    [string]$Configuration = "Debug",
    [int]$Frames = 3,
    [int]$Width = 800,
    [int]$Height = 600,
    [string]$OutputDir = "verification/screenshots"
)

$ErrorActionPreference = "Stop"
$projectDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$outputPath = Join-Path $projectDir $OutputDir
New-Item -ItemType Directory -Force -Path $outputPath | Out-Null
try {
    Add-Type -AssemblyName System.Drawing
    $canInspectImages = $true
}
catch {
    $canInspectImages = $false
}

function Get-ColorRange([string]$Path) {
    if (-not $canInspectImages) {
        return -1
    }

    $image = [System.Drawing.Bitmap]::new($Path)
    try {
        $min = 255
        $max = 0
        for ($yi = 0; $yi -le 40; $yi++) {
            $y = [Math]::Min($image.Height - 1, [int](($image.Height - 1) * $yi / 40.0))
            for ($xi = 0; $xi -le 40; $xi++) {
                $x = [Math]::Min($image.Width - 1, [int](($image.Width - 1) * $xi / 40.0))
                $pixel = $image.GetPixel($x, $y)
                foreach ($component in @($pixel.R, $pixel.G, $pixel.B)) {
                    if ($component -lt $min) { $min = $component }
                    if ($component -gt $max) { $max = $component }
                }
            }
        }
        return $max - $min
    }
    finally {
        $image.Dispose()
    }
}

dotnet build "$projectDir/LearnOpenGL.OpenTK.csproj" -c $Configuration | Out-Host
$demos = dotnet run --project "$projectDir/LearnOpenGL.OpenTK.csproj" -c $Configuration -- --list

$results = @()
foreach ($demo in $demos) {
    if ([string]::IsNullOrWhiteSpace($demo)) {
        continue
    }

    $safeName = $demo.Replace("/", "__").Replace("\", "__")
    $capture = Join-Path $outputPath "$safeName.png"
    $args = @(
        "run", "--project", "$projectDir/LearnOpenGL.OpenTK.csproj", "-c", $Configuration, "--no-build", "--",
        $demo, "--frames", "$Frames", "--width", "$Width", "--height", "$Height", "--capture", $capture
    )

    $started = Get-Date
    & dotnet @args
    $exit = $LASTEXITCODE
    $exists = Test-Path $capture
    $bytes = if ($exists) { (Get-Item $capture).Length } else { 0 }
    $colorRange = if ($exists) { Get-ColorRange $capture } else { -1 }
    $elapsed = [Math]::Round(((Get-Date) - $started).TotalSeconds, 2)

    $results += [pscustomobject]@{
        Demo = $demo
        ExitCode = $exit
        Screenshot = $capture
        Bytes = $bytes
        ColorRange = $colorRange
        Seconds = $elapsed
    }

    $allowsFlatImage = $demo -eq "1.getting_started/1.1.hello_window"
    if ($exit -ne 0 -or -not $exists -or $bytes -lt 1024 -or ((-not $allowsFlatImage) -and $colorRange -ge 0 -and $colorRange -lt 5)) {
        throw "Verification failed for $demo. ExitCode=$exit ScreenshotExists=$exists Bytes=$bytes ColorRange=$colorRange"
    }
}

$manifest = Join-Path $outputPath "verification-results.csv"
$results | Export-Csv -NoTypeInformation -Encoding UTF8 -Path $manifest
$results | Format-Table -AutoSize
Write-Host "Verification manifest: $manifest"
