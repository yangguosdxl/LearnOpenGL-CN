using LearnOpenGL.OpenTK.Core;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace LearnOpenGL.OpenTK.Demos;

public sealed class BreakoutDemo : DemoWindow
{
    private readonly int _stage;
    private Shader? _shader;
    private GlVertexArray? _quad;

    public BreakoutDemo(RunOptions options, int stage) : base(options)
    {
        _stage = stage;
    }

    protected override void LoadDemo()
    {
        GL.Disable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        _shader = Shader.FromSource(VertexSource, FragmentSource);
        _quad = new GlVertexArray([
            0.0f, 1.0f,
            1.0f, 0.0f,
            0.0f, 0.0f,
            0.0f, 1.0f,
            1.0f, 1.0f,
            1.0f, 0.0f
        ], 2, (0, 2, 0));
        _shader.Use();
        _shader.SetMatrix4("projection", Matrix4.CreateOrthographicOffCenter(0.0f, WidthPx, HeightPx, 0.0f, -1.0f, 1.0f));
    }

    protected override void RenderDemo(float deltaTime, int frameIndex)
    {
        GL.ClearColor(0.06f, 0.08f, 0.12f, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        DrawRect(new Vector2(0, 0), new Vector2(WidthPx, HeightPx), new Vector4(0.08f, 0.11f, 0.18f, 1.0f));

        if (_stage >= 2)
        {
            DrawLevel();
        }

        if (_stage >= 3)
        {
            DrawRect(new Vector2(WidthPx * 0.42f, HeightPx - 34.0f), new Vector2(130.0f, 18.0f), new Vector4(0.25f, 0.48f, 0.95f, 1.0f));
        }

        if (_stage >= 5)
        {
            var x = WidthPx * 0.5f + MathF.Sin(frameIndex * 0.08f) * 45.0f;
            DrawRect(new Vector2(x, HeightPx - 72.0f), new Vector2(18.0f, 18.0f), new Vector4(0.92f, 0.72f, 0.28f, 1.0f));
        }

        if (_stage >= 6)
        {
            for (var i = 0; i < 18; i++)
            {
                var x = WidthPx * 0.5f + MathF.Sin(frameIndex * 0.11f + i) * (20.0f + i * 3.0f);
                var y = HeightPx - 78.0f + i * 2.0f;
                DrawRect(new Vector2(x, y), new Vector2(5.0f, 5.0f), new Vector4(0.95f, 0.85f, 0.38f, 0.55f));
            }
        }

        if (_stage >= 8)
        {
            DrawRect(new Vector2(18.0f, 18.0f), new Vector2(115.0f, 22.0f), new Vector4(0.1f, 0.18f, 0.28f, 0.9f));
            DrawRect(new Vector2(24.0f, 24.0f), new Vector2(74.0f, 10.0f), new Vector4(0.4f, 0.85f, 0.45f, 1.0f));
        }

        if (_stage >= 9)
        {
            DrawRect(new Vector2(WidthPx - 140.0f, 18.0f), new Vector2(112.0f, 22.0f), new Vector4(0.22f, 0.1f, 0.26f, 0.9f));
            DrawRect(new Vector2(WidthPx - 132.0f, 24.0f), new Vector2(92.0f, 10.0f), new Vector4(0.85f, 0.35f, 0.9f, 1.0f));
        }
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        _shader?.Dispose();
        _quad?.Dispose();
    }

    private void DrawLevel()
    {
        var rows = Math.Min(5, Math.Max(1, _stage));
        var cols = 12;
        var gap = 4.0f;
        var brickW = (WidthPx - 70.0f) / cols;
        for (var y = 0; y < rows; y++)
        {
            for (var x = 0; x < cols; x++)
            {
                if (_stage >= 5 && y == 3 && x is 5 or 6)
                {
                    continue;
                }
                var color = y switch
                {
                    0 => new Vector4(0.86f, 0.28f, 0.28f, 1.0f),
                    1 => new Vector4(0.86f, 0.58f, 0.25f, 1.0f),
                    2 => new Vector4(0.34f, 0.68f, 0.38f, 1.0f),
                    3 => new Vector4(0.28f, 0.58f, 0.86f, 1.0f),
                    _ => new Vector4(0.58f, 0.38f, 0.82f, 1.0f)
                };
                DrawRect(new Vector2(35.0f + x * brickW, 70.0f + y * 24.0f), new Vector2(brickW - gap, 18.0f), color);
            }
        }
    }

    private void DrawRect(Vector2 position, Vector2 size, Vector4 color)
    {
        _shader!.Use();
        _shader.SetMatrix4("model", Matrix4.CreateScale(size.X, size.Y, 1.0f) * Matrix4.CreateTranslation(position.X, position.Y, 0.0f));
        GL.Uniform4(GL.GetUniformLocation(_shader.Handle, "spriteColor"), color);
        _quad!.Draw();
    }

    private const string VertexSource = """
        #version 330 core
        layout (location = 0) in vec2 aPos;
        uniform mat4 model;
        uniform mat4 projection;
        void main()
        {
            gl_Position = projection * model * vec4(aPos, 0.0, 1.0);
        }
        """;

    private const string FragmentSource = """
        #version 330 core
        out vec4 FragColor;
        uniform vec4 spriteColor;
        void main()
        {
            FragColor = spriteColor;
        }
        """;
}
