using OpenTK.Graphics.OpenGL4;

namespace LearnOpenGL.OpenTK.Core;

public sealed class ModelStatsWindow : DemoWindow
{
    private readonly RunOptions _options;

    public ModelStatsWindow(RunOptions options) : base(options with { DemoId = "model-stats", Frames = 1 })
    {
        _options = options;
    }

    protected override void LoadDemo()
    {
        var path = _options.ModelStatsPath ?? throw new InvalidOperationException("--model-stats path is required.");
        using var model = new Model(Path.GetFullPath(path));
        var stats = model.Stats;
        Console.WriteLine($"Meshes={stats.MeshCount}");
        Console.WriteLine($"Vertices={stats.VertexCount}");
        Console.WriteLine($"Indices={stats.IndexCount}");
        Console.WriteLine($"Textures={stats.TextureCount}");
    }

    protected override void RenderDemo(float deltaTime, int frameIndex)
    {
        GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit);
    }
}
