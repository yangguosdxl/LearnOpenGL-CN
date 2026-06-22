using LearnOpenGL.OpenTK.Core;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace LearnOpenGL.OpenTK.Demos;

public enum AdvancedOpenGlVariant
{
    DepthTesting,
    DepthTestingView,
    StencilTesting,
    BlendingDiscard,
    BlendingSort,
    FaceCullingExercise,
    Framebuffers,
    FramebuffersExercise1
}

public sealed class AdvancedOpenGlDemo : DemoWindow
{
    private readonly AdvancedOpenGlVariant _variant;
    private readonly Camera _camera = new(new Vector3(0.0f, 0.0f, 3.0f));
    private Shader? _shader;
    private Shader? _singleColorShader;
    private Shader? _screenShader;
    private GlVertexArray? _cube;
    private GlVertexArray? _plane;
    private GlVertexArray? _transparent;
    private GlVertexArray? _screenQuad;
    private int _cubeTexture;
    private int _floorTexture;
    private int _transparentTexture;
    private int _framebuffer;
    private int _framebufferColorTexture;
    private int _framebufferRenderbuffer;

    public AdvancedOpenGlDemo(RunOptions options, AdvancedOpenGlVariant variant) : base(options)
    {
        _variant = variant;
    }

    protected override void LoadDemo()
    {
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(_variant == AdvancedOpenGlVariant.DepthTesting ? DepthFunction.Always : DepthFunction.Less);
        if (_variant == AdvancedOpenGlVariant.StencilTesting)
        {
            GL.Enable(EnableCap.StencilTest);
            GL.StencilFunc(StencilFunction.Notequal, 1, 0xFF);
            GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);
        }
        if (_variant == AdvancedOpenGlVariant.BlendingSort)
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }
        if (_variant == AdvancedOpenGlVariant.FaceCullingExercise)
        {
            GL.Enable(EnableCap.CullFace);
            GL.FrontFace(FrontFaceDirection.Cw);
        }

        var (folder, prefix) = _variant switch
        {
            AdvancedOpenGlVariant.DepthTesting => ("1.1.depth_testing", "1.1.depth_testing"),
            AdvancedOpenGlVariant.DepthTestingView => ("1.2.depth_testing_view", "1.2.depth_testing"),
            AdvancedOpenGlVariant.StencilTesting => ("2.stencil_testing", "2.stencil_testing"),
            AdvancedOpenGlVariant.BlendingDiscard => ("3.1.blending_discard", "3.1.blending"),
            AdvancedOpenGlVariant.BlendingSort => ("3.2.blending_sort", "3.2.blending"),
            AdvancedOpenGlVariant.FaceCullingExercise => ("1.1.depth_testing", "1.1.depth_testing"),
            AdvancedOpenGlVariant.Framebuffers => ("5.1.framebuffers", "5.1.framebuffers"),
            AdvancedOpenGlVariant.FramebuffersExercise1 => ("5.2.framebuffers_exercise1", "5.2.framebuffers"),
            _ => throw new ArgumentOutOfRangeException()
        };

        _shader = new Shader(Paths.Shader("4.advanced_opengl", folder, $"{prefix}.vs"), Paths.Shader("4.advanced_opengl", folder, $"{prefix}.fs"));
        if (_variant == AdvancedOpenGlVariant.StencilTesting)
        {
            _singleColorShader = new Shader(
                Paths.Shader("4.advanced_opengl", folder, "2.stencil_testing.vs"),
                Paths.Shader("4.advanced_opengl", folder, "2.stencil_single_color.fs"));
        }
        if (_variant is AdvancedOpenGlVariant.Framebuffers or AdvancedOpenGlVariant.FramebuffersExercise1)
        {
            var screenPrefix = _variant == AdvancedOpenGlVariant.Framebuffers ? "5.1.framebuffers_screen" : "5.2.framebuffers_screen";
            _screenShader = new Shader(
                Paths.Shader("4.advanced_opengl", folder, $"{screenPrefix}.vs"),
                Paths.Shader("4.advanced_opengl", folder, $"{screenPrefix}.fs"));
        }

        _cube = new GlVertexArray(CubeVertices, 5, (0, 3, 0), (1, 2, 3));
        _plane = new GlVertexArray(PlaneVertices, 5, (0, 3, 0), (1, 2, 3));
        if (_variant is AdvancedOpenGlVariant.BlendingDiscard or AdvancedOpenGlVariant.BlendingSort)
        {
            _transparent = new GlVertexArray(TransparentVertices, 5, (0, 3, 0), (1, 2, 3));
        }
        if (_variant == AdvancedOpenGlVariant.Framebuffers)
        {
            _screenQuad = new GlVertexArray(ScreenQuadVertices, 4, (0, 2, 0), (1, 2, 2));
        }
        else if (_variant == AdvancedOpenGlVariant.FramebuffersExercise1)
        {
            _screenQuad = new GlVertexArray(MirrorQuadVertices, 4, (0, 2, 0), (1, 2, 2));
        }
        _cubeTexture = TextureLoader.LoadTexture(Paths.Resource("resources/textures/marble.jpg"));
        _floorTexture = TextureLoader.LoadTexture(Paths.Resource("resources/textures/metal.png"));
        if (_variant == AdvancedOpenGlVariant.BlendingDiscard)
        {
            _transparentTexture = TextureLoader.LoadTexture(Paths.Resource("resources/textures/grass.png"));
        }
        else if (_variant == AdvancedOpenGlVariant.BlendingSort)
        {
            _transparentTexture = TextureLoader.LoadTexture(Paths.Resource("resources/textures/window.png"));
        }
        _shader.Use();
        _shader.SetInt("texture1", 0);
        if (_screenShader is not null)
        {
            _screenShader.Use();
            _screenShader.SetInt("screenTexture", 0);
            SetupFramebuffer();
        }
    }

    protected override void UpdateDemo(float deltaTime)
    {
        if (KeyboardState.IsKeyDown(Keys.W)) _camera.ProcessKeyboard(CameraMovement.Forward, deltaTime);
        if (KeyboardState.IsKeyDown(Keys.S)) _camera.ProcessKeyboard(CameraMovement.Backward, deltaTime);
        if (KeyboardState.IsKeyDown(Keys.A)) _camera.ProcessKeyboard(CameraMovement.Left, deltaTime);
        if (KeyboardState.IsKeyDown(Keys.D)) _camera.ProcessKeyboard(CameraMovement.Right, deltaTime);
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);
        _camera.ProcessMouseScroll(e.OffsetY);
    }

    protected override void RenderDemo(float deltaTime, int frameIndex)
    {
        GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        var clearMask = ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit;
        if (_variant == AdvancedOpenGlVariant.StencilTesting)
        {
            clearMask |= ClearBufferMask.StencilBufferBit;
        }
        GL.Clear(clearMask);

        var view = _camera.GetViewMatrix();
        var projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(_camera.Zoom), AspectRatioPx, 0.1f, 100.0f);

        if (_variant == AdvancedOpenGlVariant.StencilTesting)
        {
            RenderStencilScene(view, projection);
        }
        else if (_variant is AdvancedOpenGlVariant.BlendingDiscard or AdvancedOpenGlVariant.BlendingSort)
        {
            _shader!.Use();
            _shader.SetMatrix4("view", view);
            _shader.SetMatrix4("projection", projection);
            DrawTexturedScene(_shader);
            DrawTransparentQuads(_shader);
        }
        else if (_variant is AdvancedOpenGlVariant.Framebuffers or AdvancedOpenGlVariant.FramebuffersExercise1)
        {
            RenderFramebufferScene(view, projection);
        }
        else
        {
            _shader!.Use();
            _shader.SetMatrix4("view", view);
            _shader.SetMatrix4("projection", projection);
            DrawTexturedScene(_shader);
        }
    }

    private void SetupFramebuffer()
    {
        _framebuffer = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _framebuffer);

        _framebufferColorTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _framebufferColorTexture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, WidthPx, HeightPx, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, _framebufferColorTexture, 0);

        _framebufferRenderbuffer = GL.GenRenderbuffer();
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _framebufferRenderbuffer);
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, WidthPx, HeightPx);
        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, _framebufferRenderbuffer);

        var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (status != FramebufferErrorCode.FramebufferComplete)
        {
            throw new InvalidOperationException($"Framebuffer is not complete: {status}");
        }

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    private void RenderFramebufferScene(Matrix4 view, Matrix4 projection)
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _framebuffer);
        GL.Enable(EnableCap.DepthTest);
        GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        _shader!.Use();
        var framebufferView = _variant == AdvancedOpenGlVariant.FramebuffersExercise1
            ? Matrix4.LookAt(_camera.Position, _camera.Position - _camera.Front, _camera.Up)
            : view;
        _shader.SetMatrix4("view", framebufferView);
        _shader.SetMatrix4("projection", projection);
        DrawTexturedScene(_shader);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.Disable(EnableCap.DepthTest);

        if (_variant == AdvancedOpenGlVariant.Framebuffers)
        {
            GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);
        }
        else
        {
            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            _shader.SetMatrix4("view", view);
            DrawTexturedScene(_shader);
            GL.Disable(EnableCap.DepthTest);
        }

        _screenShader!.Use();
        GL.BindTexture(TextureTarget.Texture2D, _framebufferColorTexture);
        _screenQuad!.Draw();
        GL.Enable(EnableCap.DepthTest);
    }

    private void RenderStencilScene(Matrix4 view, Matrix4 projection)
    {
        _singleColorShader!.Use();
        _singleColorShader.SetMatrix4("view", view);
        _singleColorShader.SetMatrix4("projection", projection);

        _shader!.Use();
        _shader.SetMatrix4("view", view);
        _shader.SetMatrix4("projection", projection);

        GL.StencilMask(0x00);
        GL.BindTexture(TextureTarget.Texture2D, _floorTexture);
        _shader.SetMatrix4("model", Matrix4.Identity);
        _plane!.Draw();

        GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
        GL.StencilMask(0xFF);
        GL.BindTexture(TextureTarget.Texture2D, _cubeTexture);
        DrawCubes(_shader, 1.0f);

        GL.StencilFunc(StencilFunction.Notequal, 1, 0xFF);
        GL.StencilMask(0x00);
        GL.Disable(EnableCap.DepthTest);
        _singleColorShader.Use();
        DrawCubes(_singleColorShader, 1.1f);

        GL.StencilMask(0xFF);
        GL.StencilFunc(StencilFunction.Always, 0, 0xFF);
        GL.Enable(EnableCap.DepthTest);
    }

    private void DrawTexturedScene(Shader shader)
    {
        GL.BindTexture(TextureTarget.Texture2D, _cubeTexture);
        DrawCubes(shader, 1.0f);
        GL.BindTexture(TextureTarget.Texture2D, _floorTexture);
        shader.SetMatrix4("model", Matrix4.Identity);
        _plane!.Draw();
    }

    private void DrawTransparentQuads(Shader shader)
    {
        var locations = new[]
        {
            new Vector3(-1.5f, 0.0f, -0.48f),
            new Vector3(1.5f, 0.0f, 0.51f),
            new Vector3(0.0f, 0.0f, 0.7f),
            new Vector3(-0.3f, 0.0f, -2.3f),
            new Vector3(0.5f, 0.0f, -0.6f)
        };

        GL.BindTexture(TextureTarget.Texture2D, _transparentTexture);
        IEnumerable<Vector3> ordered = _variant == AdvancedOpenGlVariant.BlendingSort
            ? locations.OrderByDescending(location => (_camera.Position - location).Length)
            : locations;

        foreach (var location in ordered)
        {
            shader.SetMatrix4("model", Matrix4.CreateTranslation(location));
            _transparent!.Draw();
        }
    }

    private void DrawCubes(Shader shader, float scale)
    {
        var first = Matrix4.CreateScale(scale) * Matrix4.CreateTranslation(-1.0f, 0.0f, -1.0f);
        shader.SetMatrix4("model", first);
        _cube!.Draw();

        var second = Matrix4.CreateScale(scale) * Matrix4.CreateTranslation(2.0f, 0.0f, 0.0f);
        shader.SetMatrix4("model", second);
        _cube.Draw();
    }

    private static readonly float[] CubeVertices =
    {
        -0.5f,-0.5f,-0.5f,0,0, 0.5f,-0.5f,-0.5f,1,0, 0.5f,0.5f,-0.5f,1,1, 0.5f,0.5f,-0.5f,1,1, -0.5f,0.5f,-0.5f,0,1, -0.5f,-0.5f,-0.5f,0,0,
        -0.5f,-0.5f,0.5f,0,0, 0.5f,-0.5f,0.5f,1,0, 0.5f,0.5f,0.5f,1,1, 0.5f,0.5f,0.5f,1,1, -0.5f,0.5f,0.5f,0,1, -0.5f,-0.5f,0.5f,0,0,
        -0.5f,0.5f,0.5f,1,0, -0.5f,0.5f,-0.5f,1,1, -0.5f,-0.5f,-0.5f,0,1, -0.5f,-0.5f,-0.5f,0,1, -0.5f,-0.5f,0.5f,0,0, -0.5f,0.5f,0.5f,1,0,
        0.5f,0.5f,0.5f,1,0, 0.5f,0.5f,-0.5f,1,1, 0.5f,-0.5f,-0.5f,0,1, 0.5f,-0.5f,-0.5f,0,1, 0.5f,-0.5f,0.5f,0,0, 0.5f,0.5f,0.5f,1,0,
        -0.5f,-0.5f,-0.5f,0,1, 0.5f,-0.5f,-0.5f,1,1, 0.5f,-0.5f,0.5f,1,0, 0.5f,-0.5f,0.5f,1,0, -0.5f,-0.5f,0.5f,0,0, -0.5f,-0.5f,-0.5f,0,1,
        -0.5f,0.5f,-0.5f,0,1, 0.5f,0.5f,-0.5f,1,1, 0.5f,0.5f,0.5f,1,0, 0.5f,0.5f,0.5f,1,0, -0.5f,0.5f,0.5f,0,0, -0.5f,0.5f,-0.5f,0,1
    };

    private static readonly float[] PlaneVertices =
    {
        5.0f,-0.5f,5.0f,2.0f,0.0f, -5.0f,-0.5f,5.0f,0.0f,0.0f, -5.0f,-0.5f,-5.0f,0.0f,2.0f,
        5.0f,-0.5f,5.0f,2.0f,0.0f, -5.0f,-0.5f,-5.0f,0.0f,2.0f, 5.0f,-0.5f,-5.0f,2.0f,2.0f
    };

    private static readonly float[] TransparentVertices =
    {
        0.0f,0.5f,0.0f,0.0f,0.0f, 0.0f,-0.5f,0.0f,0.0f,1.0f, 1.0f,-0.5f,0.0f,1.0f,1.0f,
        0.0f,0.5f,0.0f,0.0f,0.0f, 1.0f,-0.5f,0.0f,1.0f,1.0f, 1.0f,0.5f,0.0f,1.0f,0.0f
    };

    private static readonly float[] ScreenQuadVertices =
    {
        -1.0f,1.0f,0.0f,1.0f, -1.0f,-1.0f,0.0f,0.0f, 1.0f,-1.0f,1.0f,0.0f,
        -1.0f,1.0f,0.0f,1.0f, 1.0f,-1.0f,1.0f,0.0f, 1.0f,1.0f,1.0f,1.0f
    };

    private static readonly float[] MirrorQuadVertices =
    {
        -0.3f,1.0f,0.0f,1.0f, -0.3f,0.7f,0.0f,0.0f, 0.3f,0.7f,1.0f,0.0f,
        -0.3f,1.0f,0.0f,1.0f, 0.3f,0.7f,1.0f,0.0f, 0.3f,1.0f,1.0f,1.0f
    };

    protected override void OnUnload()
    {
        base.OnUnload();
        if (_framebuffer != 0) GL.DeleteFramebuffer(_framebuffer);
        if (_framebufferColorTexture != 0) GL.DeleteTexture(_framebufferColorTexture);
        if (_framebufferRenderbuffer != 0) GL.DeleteRenderbuffer(_framebufferRenderbuffer);
    }
}
