using LearnOpenGL.OpenTK.Core;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace LearnOpenGL.OpenTK.Demos;

public sealed class DebuggingDemo : DemoWindow
{
    private Shader? _shader;
    private GlVertexArray? _cube;
    private int _texture;

    public DebuggingDemo(RunOptions options) : base(options)
    {
    }

    protected override void LoadDemo()
    {
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        _shader = new Shader(
            Paths.Shader("7.in_practice", "1.debugging", "debugging.vs"),
            Paths.Shader("7.in_practice", "1.debugging", "debugging.fs"));
        _cube = new GlVertexArray(CubeVertices, 5, (0, 3, 0), (1, 2, 3));
        _texture = TextureLoader.LoadTexture(Paths.Resource("resources/textures/wood.png"));
        _shader.Use();
        _shader.SetInt("tex", 0);
    }

    protected override void RenderDemo(float deltaTime, int frameIndex)
    {
        GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _shader!.Use();
        _shader.SetMatrix4("projection", Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), AspectRatioPx, 0.1f, 10.0f));
        var angle = MathHelper.DegreesToRadians(frameIndex * 8.0f + 20.0f);
        _shader.SetMatrix4("model", Matrix4.CreateFromAxisAngle(Vector3.One.Normalized(), angle) * Matrix4.CreateTranslation(0.0f, 0.0f, -2.5f));
        GL.BindTexture(TextureTarget.Texture2D, _texture);
        _cube!.Draw();
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        _shader?.Dispose();
        _cube?.Dispose();
        if (_texture != 0) GL.DeleteTexture(_texture);
    }

    private static readonly float[] CubeVertices =
    {
        -0.5f,-0.5f,-0.5f,0,0, 0.5f,0.5f,-0.5f,1,1, 0.5f,-0.5f,-0.5f,1,0, 0.5f,0.5f,-0.5f,1,1, -0.5f,-0.5f,-0.5f,0,0, -0.5f,0.5f,-0.5f,0,1,
        -0.5f,-0.5f,0.5f,0,0, 0.5f,-0.5f,0.5f,1,0, 0.5f,0.5f,0.5f,1,1, 0.5f,0.5f,0.5f,1,1, -0.5f,0.5f,0.5f,0,1, -0.5f,-0.5f,0.5f,0,0,
        -0.5f,0.5f,0.5f,1,0, -0.5f,0.5f,-0.5f,1,1, -0.5f,-0.5f,-0.5f,0,1, -0.5f,-0.5f,-0.5f,0,1, -0.5f,-0.5f,0.5f,0,0, -0.5f,0.5f,0.5f,1,0,
        0.5f,0.5f,0.5f,1,0, 0.5f,-0.5f,-0.5f,0,1, 0.5f,0.5f,-0.5f,1,1, 0.5f,-0.5f,-0.5f,0,1, 0.5f,0.5f,0.5f,1,0, 0.5f,-0.5f,0.5f,0,0,
        -0.5f,-0.5f,-0.5f,0,1, 0.5f,-0.5f,-0.5f,1,1, 0.5f,-0.5f,0.5f,1,0, 0.5f,-0.5f,0.5f,1,0, -0.5f,-0.5f,0.5f,0,0, -0.5f,-0.5f,-0.5f,0,1,
        -0.5f,0.5f,-0.5f,0,1, 0.5f,0.5f,0.5f,1,0, 0.5f,0.5f,-0.5f,1,1, 0.5f,0.5f,0.5f,1,0, -0.5f,0.5f,-0.5f,0,1, -0.5f,0.5f,0.5f,0,0
    };
}

public sealed class TextRenderingDemo : DemoWindow
{
    private Shader? _shader;
    private GlVertexArray? _quad;
    private int _texture;

    public TextRenderingDemo(RunOptions options) : base(options)
    {
    }

    protected override void LoadDemo()
    {
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        _shader = new Shader(
            Paths.Shader("7.in_practice", "2.text_rendering", "text.vs"),
            Paths.Shader("7.in_practice", "2.text_rendering", "text.fs"));
        var text = "LEARNOPENGL OPENTK";
        var (pixels, width, height) = Rasterize(text);
        _texture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _texture);
        GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8, width, height, 0, PixelFormat.Red, PixelType.UnsignedByte, pixels);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        var scale = 5.0f;
        var x = 25.0f;
        var y = HeightPx * 0.55f;
        var w = width * scale;
        var h = height * scale;
        _quad = new GlVertexArray([
            x, y + h, 0, 1,
            x, y, 0, 0,
            x + w, y + h, 1, 1,
            x + w, y, 1, 0
        ], 4, (0, 4, 0));

        _shader.Use();
        _shader.SetInt("text", 0);
        _shader.SetVector3("textColor", 0.5f, 0.8f, 0.2f);
        _shader.SetMatrix4("projection", Matrix4.CreateOrthographicOffCenter(0.0f, WidthPx, 0.0f, HeightPx, -1.0f, 1.0f));
    }

    protected override void RenderDemo(float deltaTime, int frameIndex)
    {
        GL.ClearColor(0.05f, 0.05f, 0.08f, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        _shader!.Use();
        GL.BindTexture(TextureTarget.Texture2D, _texture);
        _quad!.Draw(PrimitiveType.TriangleStrip);
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        _shader?.Dispose();
        _quad?.Dispose();
        if (_texture != 0) GL.DeleteTexture(_texture);
    }

    private static (byte[] Pixels, int Width, int Height) Rasterize(string text)
    {
        const int glyphW = 5;
        const int glyphH = 7;
        const int spacing = 1;
        var width = text.Length * (glyphW + spacing);
        var height = glyphH;
        var pixels = new byte[width * height];
        for (var i = 0; i < text.Length; i++)
        {
            if (!Font.TryGetValue(text[i], out var rows))
            {
                continue;
            }
            for (var y = 0; y < glyphH; y++)
            {
                var bits = rows[y];
                for (var x = 0; x < glyphW; x++)
                {
                    if ((bits & (1 << (glyphW - 1 - x))) != 0)
                    {
                        pixels[(glyphH - 1 - y) * width + i * (glyphW + spacing) + x] = 255;
                    }
                }
            }
        }
        return (pixels, width, height);
    }

    private static readonly Dictionary<char, byte[]> Font = new()
    {
        [' '] = [0,0,0,0,0,0,0],
        ['A'] = [0b01110,0b10001,0b10001,0b11111,0b10001,0b10001,0b10001],
        ['E'] = [0b11111,0b10000,0b10000,0b11110,0b10000,0b10000,0b11111],
        ['G'] = [0b01110,0b10001,0b10000,0b10111,0b10001,0b10001,0b01110],
        ['K'] = [0b10001,0b10010,0b10100,0b11000,0b10100,0b10010,0b10001],
        ['L'] = [0b10000,0b10000,0b10000,0b10000,0b10000,0b10000,0b11111],
        ['N'] = [0b10001,0b11001,0b10101,0b10011,0b10001,0b10001,0b10001],
        ['O'] = [0b01110,0b10001,0b10001,0b10001,0b10001,0b10001,0b01110],
        ['P'] = [0b11110,0b10001,0b10001,0b11110,0b10000,0b10000,0b10000],
        ['R'] = [0b11110,0b10001,0b10001,0b11110,0b10100,0b10010,0b10001],
        ['T'] = [0b11111,0b00100,0b00100,0b00100,0b00100,0b00100,0b00100]
    };
}
