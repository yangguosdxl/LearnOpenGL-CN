using LearnOpenGL.OpenTK.Core;
using LearnOpenGL.OpenTK.Demos;

var options = RunOptions.Parse(args);

if (options.ModelStatsPath is not null)
{
    using var statsWindow = new ModelStatsWindow(options);
    statsWindow.Run();
    return 0;
}

if (options.List)
{
    foreach (var id in DemoRegistry.Ids)
    {
        Console.WriteLine(id);
    }

    return 0;
}

if (string.IsNullOrWhiteSpace(options.DemoId))
{
    Console.Error.WriteLine("Usage: dotnet run -- <demo-id> [--frames N] [--capture path]");
    Console.Error.WriteLine("Use --list to show available demos.");
    return 2;
}

if (!DemoRegistry.TryCreate(options.DemoId, options, out var window))
{
    Console.Error.WriteLine($"Unknown demo id: {options.DemoId}");
    Console.Error.WriteLine("Use --list to show available demos.");
    return 2;
}

using (window)
{
    window.Run();
}

return 0;
