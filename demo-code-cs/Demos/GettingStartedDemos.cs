using LearnOpenGL.OpenTK.Core;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace LearnOpenGL.OpenTK.Demos;

public sealed class ClearColorDemo : DemoWindow
{
    private readonly bool _clear;

    public ClearColorDemo(RunOptions options, bool clear) : base(options)
    {
        _clear = clear;
    }

    protected override void RenderDemo(float deltaTime, int frameIndex)
    {
        if (_clear)
        {
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);
        }
    }
}

public enum TriangleVariant
{
    Single,
    IndexedRectangle,
    TwoTriangles,
    TwoSeparateTriangles,
    TwoColoredTriangles
}

public sealed class TriangleDemo : DemoWindow
{
    private const string VertexShader = """
        #version 330 core
        layout (location = 0) in vec3 aPos;
        void main()
        {
            gl_Position = vec4(aPos, 1.0);
        }
        """;

    private const string OrangeFragmentShader = """
        #version 330 core
        out vec4 FragColor;
        void main()
        {
            FragColor = vec4(1.0, 0.5, 0.2, 1.0);
        }
        """;

    private const string YellowFragmentShader = """
        #version 330 core
        out vec4 FragColor;
        void main()
        {
            FragColor = vec4(1.0, 1.0, 0.0, 1.0);
        }
        """;

    private readonly TriangleVariant _variant;
    private Shader? _orange;
    private Shader? _yellow;
    private GlVertexArray? _main;
    private GlVertexArray? _secondary;

    public TriangleDemo(RunOptions options, TriangleVariant variant) : base(options)
    {
        _variant = variant;
    }

    protected override void LoadDemo()
    {
        _orange = Shader.FromSource(VertexShader, OrangeFragmentShader);
        _yellow = Shader.FromSource(VertexShader, YellowFragmentShader);

        switch (_variant)
        {
            case TriangleVariant.Single:
                _main = new GlVertexArray(new[]
                {
                    -0.5f, -0.5f, 0.0f,
                     0.5f, -0.5f, 0.0f,
                     0.0f,  0.5f, 0.0f
                }, 3, (0, 3, 0));
                break;
            case TriangleVariant.IndexedRectangle:
                _main = new GlVertexArray(new[]
                {
                     0.5f,  0.5f, 0.0f,
                     0.5f, -0.5f, 0.0f,
                    -0.5f, -0.5f, 0.0f,
                    -0.5f,  0.5f, 0.0f
                }, 3, new uint[] { 0, 1, 3, 1, 2, 3 }, (0, 3, 0));
                break;
            case TriangleVariant.TwoTriangles:
                _main = new GlVertexArray(new[]
                {
                    -0.9f, -0.5f, 0.0f,
                    -0.0f, -0.5f, 0.0f,
                    -0.45f, 0.5f, 0.0f,
                     0.0f, -0.5f, 0.0f,
                     0.9f, -0.5f, 0.0f,
                     0.45f, 0.5f, 0.0f
                }, 3, (0, 3, 0));
                break;
            case TriangleVariant.TwoSeparateTriangles:
            case TriangleVariant.TwoColoredTriangles:
                _main = new GlVertexArray(new[]
                {
                    -0.9f, -0.5f, 0.0f,
                    -0.0f, -0.5f, 0.0f,
                    -0.45f, 0.5f, 0.0f
                }, 3, (0, 3, 0));
                _secondary = new GlVertexArray(new[]
                {
                    0.0f, -0.5f, 0.0f,
                    0.9f, -0.5f, 0.0f,
                    0.45f, 0.5f, 0.0f
                }, 3, (0, 3, 0));
                break;
        }
    }

    protected override void RenderDemo(float deltaTime, int frameIndex)
    {
        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        _orange!.Use();
        _main!.Draw();

        if (_secondary is not null)
        {
            (_variant == TriangleVariant.TwoColoredTriangles ? _yellow : _orange)!.Use();
            _secondary.Draw();
        }
    }
}

public enum ShaderTriangleVariant
{
    Uniform,
    Interpolation,
    ClassShader,
    UpsideDown,
    OffsetRight,
    PositionColor
}

public sealed class ShaderTriangleDemo : DemoWindow
{
    private readonly ShaderTriangleVariant _variant;
    private Shader? _shader;
    private GlVertexArray? _vao;

    public ShaderTriangleDemo(RunOptions options, ShaderTriangleVariant variant) : base(options)
    {
        _variant = variant;
    }

    protected override void LoadDemo()
    {
        if (_variant == ShaderTriangleVariant.Uniform)
        {
            _shader = Shader.FromSource("""
                #version 330 core
                layout (location = 0) in vec3 aPos;
                void main()
                {
                    gl_Position = vec4(aPos, 1.0);
                }
                """, """
                #version 330 core
                out vec4 FragColor;
                uniform vec4 ourColor;
                void main()
                {
                    FragColor = ourColor;
                }
                """);
            _vao = new GlVertexArray(new[]
            {
                -0.5f, -0.5f, 0.0f,
                 0.5f, -0.5f, 0.0f,
                 0.0f,  0.5f, 0.0f
            }, 3, (0, 3, 0));
        }
        else
        {
            var chapter = "1.getting_started";
            var demo = _variant == ShaderTriangleVariant.ClassShader ? "3.3.shaders_class" : "3.2.shaders_interpolation";
            var vertex = _variant switch
            {
                ShaderTriangleVariant.ClassShader => "3.3.shader.vs",
                ShaderTriangleVariant.UpsideDown => InlineUpsideDownVertex,
                ShaderTriangleVariant.OffsetRight => InlineOffsetVertex,
                ShaderTriangleVariant.PositionColor => InlinePositionColorVertex,
                _ => InlineInterpolationVertex
            };
            var fragment = _variant switch
            {
                ShaderTriangleVariant.ClassShader => "3.3.shader.fs",
                ShaderTriangleVariant.PositionColor => InlinePositionColorFragment,
                _ => InlineInterpolationFragment
            };
            _shader = _variant == ShaderTriangleVariant.ClassShader
                ? new Shader(Paths.Shader(chapter, demo, vertex), Paths.Shader(chapter, demo, fragment))
                : Shader.FromSource(vertex, fragment);
            _vao = new GlVertexArray(new[]
            {
                -0.5f, -0.5f, 0.0f, 1.0f, 0.0f, 0.0f,
                 0.5f, -0.5f, 0.0f, 0.0f, 1.0f, 0.0f,
                 0.0f,  0.5f, 0.0f, 0.0f, 0.0f, 1.0f
            }, 6, (0, 3, 0), (1, 3, 3));
        }
    }

    protected override void RenderDemo(float deltaTime, int frameIndex)
    {
        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        _shader!.Use();

        if (_variant == ShaderTriangleVariant.Uniform)
        {
            var green = MathF.Sin(frameIndex / 15.0f) / 2.0f + 0.5f;
            GL.Uniform4(GL.GetUniformLocation(_shader.Handle, "ourColor"), 0.0f, green, 0.0f, 1.0f);
        }
        else if (_variant == ShaderTriangleVariant.OffsetRight)
        {
            _shader.SetFloat("xOffset", 0.5f);
        }

        _vao!.Draw();
    }

    private const string InlineInterpolationVertex = """
        #version 330 core
        layout (location = 0) in vec3 aPos;
        layout (location = 1) in vec3 aColor;
        out vec3 ourColor;
        void main()
        {
            gl_Position = vec4(aPos, 1.0);
            ourColor = aColor;
        }
        """;

    private const string InlineInterpolationFragment = """
        #version 330 core
        out vec4 FragColor;
        in vec3 ourColor;
        void main()
        {
            FragColor = vec4(ourColor, 1.0);
        }
        """;

    private const string InlineUpsideDownVertex = """
        #version 330 core
        layout (location = 0) in vec3 aPos;
        layout (location = 1) in vec3 aColor;
        out vec3 ourColor;
        void main()
        {
            gl_Position = vec4(aPos.x, -aPos.y, aPos.z, 1.0);
            ourColor = aColor;
        }
        """;

    private const string InlineOffsetVertex = """
        #version 330 core
        layout (location = 0) in vec3 aPos;
        layout (location = 1) in vec3 aColor;
        out vec3 ourColor;
        uniform float xOffset;
        void main()
        {
            gl_Position = vec4(aPos.x + xOffset, aPos.y, aPos.z, 1.0);
            ourColor = aColor;
        }
        """;

    private const string InlinePositionColorVertex = """
        #version 330 core
        layout (location = 0) in vec3 aPos;
        layout (location = 1) in vec3 aColor;
        out vec3 ourPosition;
        void main()
        {
            gl_Position = vec4(aPos, 1.0);
            ourPosition = aPos;
        }
        """;

    private const string InlinePositionColorFragment = """
        #version 330 core
        out vec4 FragColor;
        in vec3 ourPosition;
        void main()
        {
            FragColor = vec4(ourPosition, 1.0);
        }
        """;
}

public enum TextureRectangleVariant
{
    SingleTexture,
    CombinedTextures,
    FaceMirrorInShader,
    FaceFlipped,
    OffsetCoordinates,
    MixControlled
}

public class TextureRectangleDemo : DemoWindow
{
    private readonly TextureRectangleVariant _variant;
    private Shader? _shader;
    private GlVertexArray? _vao;
    private int _texture1;
    private int _texture2;

    public TextureRectangleDemo(RunOptions options, TextureRectangleVariant variant) : base(options)
    {
        _variant = variant;
    }

    protected override void LoadDemo()
    {
        var demo = _variant switch
        {
            TextureRectangleVariant.SingleTexture => "4.1.textures",
            TextureRectangleVariant.CombinedTextures => "4.2.textures_combined",
            TextureRectangleVariant.FaceMirrorInShader => "4.2.textures_combined",
            TextureRectangleVariant.FaceFlipped => "4.4.textures_exercise2",
            TextureRectangleVariant.OffsetCoordinates => "4.5.textures_exercise3",
            TextureRectangleVariant.MixControlled => "4.6.textures_exercise4",
            _ => throw new ArgumentOutOfRangeException()
        };
        var vertex = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "src", "1.getting_started", demo), "*.vs").Single();
        var fragment = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "src", "1.getting_started", demo), "*.fs").Single();

        _shader = _variant == TextureRectangleVariant.FaceMirrorInShader
            ? Shader.FromSource(File.ReadAllText(vertex), InlineTextureMirrorFragment)
            : new Shader(vertex, fragment);
        _vao = new GlVertexArray(RectangleVertices(_variant), 8, new uint[] { 0, 1, 3, 1, 2, 3 }, (0, 3, 0), (1, 3, 3), (2, 2, 6));

        _texture1 = TextureLoader.LoadTexture(Paths.Resource("resources/textures/container.jpg"));
        if (_variant != TextureRectangleVariant.SingleTexture)
        {
            _texture2 = TextureLoader.LoadTexture(Paths.Resource("resources/textures/awesomeface.png"));
            _shader.Use();
            _shader.SetInt(_variant == TextureRectangleVariant.FaceMirrorInShader ? "ourTexture1" : "texture1", 0);
            _shader.SetInt(_variant == TextureRectangleVariant.FaceMirrorInShader ? "ourTexture2" : "texture2", 1);
            if (_variant == TextureRectangleVariant.MixControlled)
            {
                _shader.SetFloat("mixValue", 0.2f);
            }
        }
    }

    protected override void RenderDemo(float deltaTime, int frameIndex)
    {
        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, _texture1);
        if (_texture2 != 0)
        {
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, _texture2);
        }

        _shader!.Use();
        _vao!.Draw();
    }

    private static float[] RectangleVertices(TextureRectangleVariant variant)
    {
        var topRight = variant == TextureRectangleVariant.OffsetCoordinates ? (2.0f, 2.0f) : (1.0f, 1.0f);
        var bottomRight = variant == TextureRectangleVariant.OffsetCoordinates ? (2.0f, 0.0f) : (1.0f, 0.0f);
        var bottomLeft = (0.0f, 0.0f);
        var topLeft = variant == TextureRectangleVariant.OffsetCoordinates ? (0.0f, 2.0f) : (0.0f, 1.0f);

        return new[]
        {
             0.5f,  0.5f, 0.0f, 1.0f, 0.0f, 0.0f, topRight.Item1, topRight.Item2,
             0.5f, -0.5f, 0.0f, 0.0f, 1.0f, 0.0f, bottomRight.Item1, bottomRight.Item2,
            -0.5f, -0.5f, 0.0f, 0.0f, 0.0f, 1.0f, bottomLeft.Item1, bottomLeft.Item2,
            -0.5f,  0.5f, 0.0f, 1.0f, 1.0f, 0.0f, topLeft.Item1, topLeft.Item2
        };
    }

    private const string InlineTextureMirrorFragment = """
        #version 330 core
        out vec4 FragColor;
        in vec3 ourColor;
        in vec2 TexCoord;
        uniform sampler2D ourTexture1;
        uniform sampler2D ourTexture2;
        void main()
        {
            FragColor = mix(texture(ourTexture1, TexCoord), texture(ourTexture2, vec2(1.0 - TexCoord.x, TexCoord.y)), 0.2);
        }
        """;
}

public enum TransformVariant
{
    Primary,
    ReversedOrder,
    TwoContainers
}

public sealed class TransformationsDemo : TextureRectangleDemo
{
    private readonly TransformVariant _transformVariant;
    private Shader? _shader;
    private GlVertexArray? _vao;
    private int _texture1;
    private int _texture2;

    public TransformationsDemo(RunOptions options, TransformVariant variant) : base(options, TextureRectangleVariant.CombinedTextures)
    {
        _transformVariant = variant;
    }

    protected override void LoadDemo()
    {
        var demo = _transformVariant == TransformVariant.Primary || _transformVariant == TransformVariant.ReversedOrder ? "5.1.transformations" : "5.2.transformations_exercise2";
        _shader = new Shader(
            Paths.Shader("1.getting_started", demo, _transformVariant == TransformVariant.Primary || _transformVariant == TransformVariant.ReversedOrder ? "5.1.transform.vs" : "5.2.transform.vs"),
            Paths.Shader("1.getting_started", demo, _transformVariant == TransformVariant.Primary || _transformVariant == TransformVariant.ReversedOrder ? "5.1.transform.fs" : "5.2.transform.fs"));
        _vao = new GlVertexArray(new[]
        {
             0.5f,  0.5f, 0.0f, 1.0f, 1.0f,
             0.5f, -0.5f, 0.0f, 1.0f, 0.0f,
            -0.5f, -0.5f, 0.0f, 0.0f, 0.0f,
            -0.5f,  0.5f, 0.0f, 0.0f, 1.0f
        }, 5, new uint[] { 0, 1, 3, 1, 2, 3 }, (0, 3, 0), (1, 2, 3));
        _texture1 = TextureLoader.LoadTexture(Paths.Resource("resources/textures/container.jpg"));
        _texture2 = TextureLoader.LoadTexture(Paths.Resource("resources/textures/awesomeface.png"));
        _shader.Use();
        _shader.SetInt("texture1", 0);
        _shader.SetInt("texture2", 1);
    }

    protected override void RenderDemo(float deltaTime, int frameIndex)
    {
        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, _texture1);
        GL.ActiveTexture(TextureUnit.Texture1);
        GL.BindTexture(TextureTarget.Texture2D, _texture2);

        _shader!.Use();
        if (_transformVariant == TransformVariant.Primary || _transformVariant == TransformVariant.ReversedOrder)
        {
            var transform = _transformVariant == TransformVariant.ReversedOrder
                ? Matrix4.CreateTranslation(0.5f, -0.5f, 0.0f) * Matrix4.CreateRotationZ(frameIndex / 60.0f)
                : Matrix4.CreateRotationZ(frameIndex / 60.0f) * Matrix4.CreateTranslation(0.5f, -0.5f, 0.0f);
            _shader.SetMatrix4("transform", transform);
            _vao!.Draw();
        }
        else
        {
            var first = Matrix4.CreateRotationZ(frameIndex / 60.0f) * Matrix4.CreateTranslation(0.5f, -0.5f, 0.0f);
            _shader.SetMatrix4("transform", first);
            _vao!.Draw();

            var scale = MathF.Sin(frameIndex / 15.0f);
            var second = Matrix4.CreateScale(scale) * Matrix4.CreateTranslation(-0.5f, 0.5f, 0.0f);
            _shader.SetMatrix4("transform", second);
            _vao.Draw();
        }
    }
}

public enum CoordinateVariant
{
    SingleCube,
    DepthCube,
    MultipleCubes,
    AnimatedEveryThirdCube
}

public class CoordinateSystemsDemo : DemoWindow
{
    private static readonly Vector3[] CubePositions =
    {
        new(0.0f, 0.0f, 0.0f),
        new(2.0f, 5.0f, -15.0f),
        new(-1.5f, -2.2f, -2.5f),
        new(-3.8f, -2.0f, -12.3f),
        new(2.4f, -0.4f, -3.5f),
        new(-1.7f, 3.0f, -7.5f),
        new(1.3f, -2.0f, -2.5f),
        new(1.5f, 2.0f, -2.5f),
        new(1.5f, 0.2f, -1.5f),
        new(-1.3f, 1.0f, -1.5f)
    };

    private readonly CoordinateVariant _variant;
    private Shader? _shader;
    private GlVertexArray? _vao;
    private int _texture1;
    private int _texture2;

    public CoordinateSystemsDemo(RunOptions options, CoordinateVariant variant) : base(options)
    {
        _variant = variant;
    }

    protected override void LoadDemo()
    {
        if (_variant != CoordinateVariant.SingleCube)
        {
            GL.Enable(EnableCap.DepthTest);
        }

        var demo = _variant switch
        {
            CoordinateVariant.SingleCube => "6.1.coordinate_systems",
            CoordinateVariant.DepthCube => "6.2.coordinate_systems_depth",
            CoordinateVariant.MultipleCubes or CoordinateVariant.AnimatedEveryThirdCube => "6.3.coordinate_systems_multiple",
            _ => throw new ArgumentOutOfRangeException()
        };
        var prefix = _variant switch
        {
            CoordinateVariant.SingleCube => "6.1.coordinate_systems",
            CoordinateVariant.DepthCube => "6.2.coordinate_systems",
            CoordinateVariant.MultipleCubes or CoordinateVariant.AnimatedEveryThirdCube => "6.3.coordinate_systems",
            _ => throw new ArgumentOutOfRangeException()
        };

        _shader = new Shader(Paths.Shader("1.getting_started", demo, $"{prefix}.vs"), Paths.Shader("1.getting_started", demo, $"{prefix}.fs"));
        _vao = new GlVertexArray(CubeVertices, 5, (0, 3, 0), (1, 2, 3));
        _texture1 = TextureLoader.LoadTexture(Paths.Resource("resources/textures/container.jpg"));
        _texture2 = TextureLoader.LoadTexture(Paths.Resource("resources/textures/awesomeface.png"));
        _shader.Use();
        _shader.SetInt("texture1", 0);
        _shader.SetInt("texture2", 1);
    }

    protected override void RenderDemo(float deltaTime, int frameIndex)
    {
        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, _texture1);
        GL.ActiveTexture(TextureUnit.Texture1);
        GL.BindTexture(TextureTarget.Texture2D, _texture2);

        _shader!.Use();
        var view = CreateViewMatrix(frameIndex);
        var projection = CreateProjectionMatrix();
        _shader.SetMatrix4("view", view);
        _shader.SetMatrix4("projection", projection);

        var count = _variant is CoordinateVariant.MultipleCubes or CoordinateVariant.AnimatedEveryThirdCube ? CubePositions.Length : 1;
        for (var i = 0; i < count; i++)
        {
            var angle = _variant == CoordinateVariant.AnimatedEveryThirdCube && i % 3 == 0
                ? frameIndex * 25.0f / 60.0f
                : 20.0f * i;
            var model = Matrix4.CreateFromAxisAngle(new Vector3(1.0f, 0.3f, 0.5f).Normalized(), MathHelper.DegreesToRadians(angle)) *
                        Matrix4.CreateTranslation(CubePositions[i]);
            _shader.SetMatrix4("model", model);
            _vao!.Draw();
        }
    }

    protected virtual Matrix4 CreateViewMatrix(int frameIndex)
    {
        return Matrix4.CreateTranslation(0.0f, 0.0f, -3.0f);
    }

    protected virtual Matrix4 CreateProjectionMatrix()
    {
        return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), AspectRatioPx, 0.1f, 100.0f);
    }

    protected static readonly float[] CubeVertices =
    {
        -0.5f, -0.5f, -0.5f, 0.0f, 0.0f, 0.5f, -0.5f, -0.5f, 1.0f, 0.0f, 0.5f, 0.5f, -0.5f, 1.0f, 1.0f,
        0.5f, 0.5f, -0.5f, 1.0f, 1.0f, -0.5f, 0.5f, -0.5f, 0.0f, 1.0f, -0.5f, -0.5f, -0.5f, 0.0f, 0.0f,
        -0.5f, -0.5f, 0.5f, 0.0f, 0.0f, 0.5f, -0.5f, 0.5f, 1.0f, 0.0f, 0.5f, 0.5f, 0.5f, 1.0f, 1.0f,
        0.5f, 0.5f, 0.5f, 1.0f, 1.0f, -0.5f, 0.5f, 0.5f, 0.0f, 1.0f, -0.5f, -0.5f, 0.5f, 0.0f, 0.0f,
        -0.5f, 0.5f, 0.5f, 1.0f, 0.0f, -0.5f, 0.5f, -0.5f, 1.0f, 1.0f, -0.5f, -0.5f, -0.5f, 0.0f, 1.0f,
        -0.5f, -0.5f, -0.5f, 0.0f, 1.0f, -0.5f, -0.5f, 0.5f, 0.0f, 0.0f, -0.5f, 0.5f, 0.5f, 1.0f, 0.0f,
        0.5f, 0.5f, 0.5f, 1.0f, 0.0f, 0.5f, 0.5f, -0.5f, 1.0f, 1.0f, 0.5f, -0.5f, -0.5f, 0.0f, 1.0f,
        0.5f, -0.5f, -0.5f, 0.0f, 1.0f, 0.5f, -0.5f, 0.5f, 0.0f, 0.0f, 0.5f, 0.5f, 0.5f, 1.0f, 0.0f,
        -0.5f, -0.5f, -0.5f, 0.0f, 1.0f, 0.5f, -0.5f, -0.5f, 1.0f, 1.0f, 0.5f, -0.5f, 0.5f, 1.0f, 0.0f,
        0.5f, -0.5f, 0.5f, 1.0f, 0.0f, -0.5f, -0.5f, 0.5f, 0.0f, 0.0f, -0.5f, -0.5f, -0.5f, 0.0f, 1.0f,
        -0.5f, 0.5f, -0.5f, 0.0f, 1.0f, 0.5f, 0.5f, -0.5f, 1.0f, 1.0f, 0.5f, 0.5f, 0.5f, 1.0f, 0.0f,
        0.5f, 0.5f, 0.5f, 1.0f, 0.0f, -0.5f, 0.5f, 0.5f, 0.0f, 0.0f, -0.5f, 0.5f, -0.5f, 0.0f, 1.0f
    };
}

public enum CameraVariant
{
    Circle,
    Keyboard,
    MouseZoom,
    ClassCamera,
    GroundLocked,
    CustomLookAt
}

public sealed class CameraDemo : CoordinateSystemsDemo
{
    private readonly CameraVariant _cameraVariant;
    private readonly Camera _camera = new(new Vector3(0.0f, 0.0f, 3.0f));
    private bool _firstMouse = true;
    private Vector2 _lastMouse = new(400.0f, 300.0f);

    public CameraDemo(RunOptions options, CameraVariant variant) : base(options, CoordinateVariant.MultipleCubes)
    {
        _cameraVariant = variant;
    }

    protected override void OnMouseMove(MouseMoveEventArgs e)
    {
        base.OnMouseMove(e);
        if (_cameraVariant is CameraVariant.MouseZoom or CameraVariant.ClassCamera or CameraVariant.GroundLocked)
        {
            if (_firstMouse)
            {
                _lastMouse = e.Position;
                _firstMouse = false;
            }

            var xOffset = e.X - _lastMouse.X;
            var yOffset = _lastMouse.Y - e.Y;
            _lastMouse = e.Position;
            _camera.ProcessMouseMovement(xOffset, yOffset);
        }
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);
        if (_cameraVariant is CameraVariant.MouseZoom or CameraVariant.ClassCamera or CameraVariant.GroundLocked)
        {
            _camera.ProcessMouseScroll(e.OffsetY);
        }
    }

    protected override void UpdateDemo(float deltaTime)
    {
        if (_cameraVariant is CameraVariant.Keyboard or CameraVariant.MouseZoom or CameraVariant.ClassCamera or CameraVariant.GroundLocked)
        {
            if (KeyboardState.IsKeyDown(Keys.W))
            {
                _camera.ProcessKeyboard(CameraMovement.Forward, deltaTime);
            }
            if (KeyboardState.IsKeyDown(Keys.S))
            {
                _camera.ProcessKeyboard(CameraMovement.Backward, deltaTime);
            }
            if (KeyboardState.IsKeyDown(Keys.A))
            {
                _camera.ProcessKeyboard(CameraMovement.Left, deltaTime);
            }
            if (KeyboardState.IsKeyDown(Keys.D))
            {
                _camera.ProcessKeyboard(CameraMovement.Right, deltaTime);
            }
            if (_cameraVariant == CameraVariant.GroundLocked)
            {
                _camera.SetPosition(new Vector3(_camera.Position.X, 0.0f, _camera.Position.Z));
            }
        }
    }

    protected override Matrix4 CreateViewMatrix(int frameIndex)
    {
        if (_cameraVariant == CameraVariant.Circle || _cameraVariant == CameraVariant.CustomLookAt)
        {
            const float radius = 10.0f;
            var camX = MathF.Sin(frameIndex / 60.0f) * radius;
            var camZ = MathF.Cos(frameIndex / 60.0f) * radius;
            var position = new Vector3(camX, 0.0f, camZ);
            return _cameraVariant == CameraVariant.CustomLookAt
                ? CalculateLookAt(position, Vector3.Zero, Vector3.UnitY)
                : Matrix4.LookAt(position, Vector3.Zero, Vector3.UnitY);
        }

        return _camera.GetViewMatrix();
    }

    protected override Matrix4 CreateProjectionMatrix()
    {
        return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(_camera.Zoom), AspectRatioPx, 0.1f, 100.0f);
    }

    private static Matrix4 CalculateLookAt(Vector3 position, Vector3 target, Vector3 worldUp)
    {
        var zAxis = (position - target).Normalized();
        var xAxis = Vector3.Cross(worldUp.Normalized(), zAxis).Normalized();
        var yAxis = Vector3.Cross(zAxis, xAxis);
        return new Matrix4(
            xAxis.X, yAxis.X, zAxis.X, 0.0f,
            xAxis.Y, yAxis.Y, zAxis.Y, 0.0f,
            xAxis.Z, yAxis.Z, zAxis.Z, 0.0f,
            -Vector3.Dot(xAxis, position), -Vector3.Dot(yAxis, position), -Vector3.Dot(zAxis, position), 1.0f);
    }
}
