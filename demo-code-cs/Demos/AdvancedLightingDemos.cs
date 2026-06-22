using LearnOpenGL.OpenTK.Core;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace LearnOpenGL.OpenTK.Demos;

public enum AdvancedLightingVariant
{
    AdvancedLighting,
    GammaCorrection,
    NormalMapping,
    ParallaxMapping,
    SteepParallaxMapping,
    ParallaxOcclusionMapping
}

public sealed class AdvancedLightingDemo : DemoWindow
{
    private readonly AdvancedLightingVariant _variant;
    private readonly Camera _camera = new(new Vector3(0.0f, 0.0f, 3.0f));
    private Shader? _shader;
    private GlVertexArray? _plane;
    private GlVertexArray? _quad;
    private int _texture0;
    private int _texture1;
    private int _texture2;
    private readonly Vector3 _lightPos = new(0.5f, 1.0f, 0.3f);

    public AdvancedLightingDemo(RunOptions options, AdvancedLightingVariant variant) : base(options)
    {
        _variant = variant;
    }

    protected override void LoadDemo()
    {
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        switch (_variant)
        {
            case AdvancedLightingVariant.AdvancedLighting:
                _shader = new Shader(
                    Paths.Shader("5.advanced_lighting", "1.advanced_lighting", "1.advanced_lighting.vs"),
                    Paths.Shader("5.advanced_lighting", "1.advanced_lighting", "1.advanced_lighting.fs"));
                _plane = new GlVertexArray(FloorPlaneVertices, 8, (0, 3, 0), (1, 3, 3), (2, 2, 6));
                _texture0 = TextureLoader.LoadTexture(Paths.Resource("resources/textures/wood.png"));
                _shader.Use();
                _shader.SetInt("floorTexture", 0);
                break;
            case AdvancedLightingVariant.GammaCorrection:
                _shader = new Shader(
                    Paths.Shader("5.advanced_lighting", "2.gamma_correction", "2.gamma_correction.vs"),
                    Paths.Shader("5.advanced_lighting", "2.gamma_correction", "2.gamma_correction.fs"));
                _plane = new GlVertexArray(FloorPlaneVertices, 8, (0, 3, 0), (1, 3, 3), (2, 2, 6));
                _texture0 = TextureLoader.LoadTexture(Paths.Resource("resources/textures/wood.png"));
                _shader.Use();
                _shader.SetInt("floorTexture", 0);
                break;
            default:
                var (folder, prefix) = _variant switch
                {
                    AdvancedLightingVariant.NormalMapping => ("4.normal_mapping", "4.normal_mapping"),
                    AdvancedLightingVariant.ParallaxMapping => ("5.1.parallax_mapping", "5.1.parallax_mapping"),
                    AdvancedLightingVariant.SteepParallaxMapping => ("5.2.steep_parallax_mapping", "5.2.parallax_mapping"),
                    AdvancedLightingVariant.ParallaxOcclusionMapping => ("5.3.parallax_occlusion_mapping", "5.3.parallax_mapping"),
                    _ => throw new ArgumentOutOfRangeException()
                };
                _shader = new Shader(
                    Paths.Shader("5.advanced_lighting", folder, $"{prefix}.vs"),
                    Paths.Shader("5.advanced_lighting", folder, $"{prefix}.fs"));
                _quad = new GlVertexArray(TangentQuadVertices(), 14, (0, 3, 0), (1, 3, 3), (2, 2, 6), (3, 3, 8), (4, 3, 11));
                _texture0 = TextureLoader.LoadTexture(Paths.Resource(_variant == AdvancedLightingVariant.NormalMapping ? "resources/textures/brickwall.jpg" : "resources/textures/bricks2.jpg"));
                _texture1 = TextureLoader.LoadTexture(Paths.Resource(_variant == AdvancedLightingVariant.NormalMapping ? "resources/textures/brickwall_normal.jpg" : "resources/textures/bricks2_normal.jpg"));
                if (_variant != AdvancedLightingVariant.NormalMapping)
                {
                    _texture2 = TextureLoader.LoadTexture(Paths.Resource("resources/textures/bricks2_disp.jpg"));
                }
                _shader.Use();
                _shader.SetInt("diffuseMap", 0);
                _shader.SetInt("normalMap", 1);
                if (_variant != AdvancedLightingVariant.NormalMapping)
                {
                    _shader.SetInt("depthMap", 2);
                }
                break;
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
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        _shader!.Use();
        _shader.SetMatrix4("projection", Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(_camera.Zoom), AspectRatioPx, 0.1f, 100.0f));
        _shader.SetMatrix4("view", _camera.GetViewMatrix());
        _shader.SetVector3("viewPos", _camera.Position);

        if (_variant is AdvancedLightingVariant.AdvancedLighting or AdvancedLightingVariant.GammaCorrection)
        {
            RenderFloor();
        }
        else
        {
            RenderTangentSpaceQuad(frameIndex);
        }
    }

    private void RenderFloor()
    {
        if (_variant == AdvancedLightingVariant.AdvancedLighting)
        {
            _shader!.SetVector3("lightPos", Vector3.Zero);
            _shader.SetInt("blinn", 1);
        }
        else
        {
            var positions = new[] { new Vector3(-3.0f, 0.0f, 0.0f), new Vector3(-1.0f, 0.0f, 0.0f), new Vector3(1.0f, 0.0f, 0.0f), new Vector3(3.0f, 0.0f, 0.0f) };
            var colors = new[] { new Vector3(0.25f), new Vector3(0.50f), new Vector3(0.75f), new Vector3(1.00f) };
            for (var i = 0; i < 4; i++)
            {
                _shader!.SetVector3($"lightPositions[{i}]", positions[i]);
                _shader.SetVector3($"lightColors[{i}]", colors[i]);
            }
            _shader!.SetInt("gamma", 1);
        }

        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, _texture0);
        _plane!.Draw();
    }

    private void RenderTangentSpaceQuad(int frameIndex)
    {
        var axis = new Vector3(1.0f, 0.0f, 1.0f).Normalized();
        var model = Matrix4.CreateFromAxisAngle(axis, MathHelper.DegreesToRadians(-10.0f - frameIndex * 2.0f));
        _shader!.SetMatrix4("model", model);
        _shader.SetVector3("lightPos", _lightPos);
        if (_variant != AdvancedLightingVariant.NormalMapping)
        {
            _shader.SetFloat("heightScale", 0.1f);
        }

        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, _texture0);
        GL.ActiveTexture(TextureUnit.Texture1);
        GL.BindTexture(TextureTarget.Texture2D, _texture1);
        if (_texture2 != 0)
        {
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, _texture2);
        }
        _quad!.Draw();

        _shader.SetMatrix4("model", Matrix4.CreateScale(0.1f) * Matrix4.CreateTranslation(_lightPos));
        _quad.Draw();
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        _shader?.Dispose();
        _plane?.Dispose();
        _quad?.Dispose();
        if (_texture0 != 0) GL.DeleteTexture(_texture0);
        if (_texture1 != 0) GL.DeleteTexture(_texture1);
        if (_texture2 != 0) GL.DeleteTexture(_texture2);
    }

    private static float[] TangentQuadVertices()
    {
        var pos1 = new Vector3(-1.0f, 1.0f, 0.0f);
        var pos2 = new Vector3(-1.0f, -1.0f, 0.0f);
        var pos3 = new Vector3(1.0f, -1.0f, 0.0f);
        var pos4 = new Vector3(1.0f, 1.0f, 0.0f);
        var uv1 = new Vector2(0.0f, 1.0f);
        var uv2 = new Vector2(0.0f, 0.0f);
        var uv3 = new Vector2(1.0f, 0.0f);
        var uv4 = new Vector2(1.0f, 1.0f);
        var normal = new Vector3(0.0f, 0.0f, 1.0f);

        var (tangent1, bitangent1) = TangentFrame(pos1, pos2, pos3, uv1, uv2, uv3);
        var (tangent2, bitangent2) = TangentFrame(pos1, pos3, pos4, uv1, uv3, uv4);

        return
        [
            ..Vertex(pos1, normal, uv1, tangent1, bitangent1), ..Vertex(pos2, normal, uv2, tangent1, bitangent1), ..Vertex(pos3, normal, uv3, tangent1, bitangent1),
            ..Vertex(pos1, normal, uv1, tangent2, bitangent2), ..Vertex(pos3, normal, uv3, tangent2, bitangent2), ..Vertex(pos4, normal, uv4, tangent2, bitangent2)
        ];
    }

    private static (Vector3 Tangent, Vector3 Bitangent) TangentFrame(Vector3 p1, Vector3 p2, Vector3 p3, Vector2 uv1, Vector2 uv2, Vector2 uv3)
    {
        var edge1 = p2 - p1;
        var edge2 = p3 - p1;
        var deltaUv1 = uv2 - uv1;
        var deltaUv2 = uv3 - uv1;
        var f = 1.0f / (deltaUv1.X * deltaUv2.Y - deltaUv2.X * deltaUv1.Y);
        var tangent = new Vector3(
            f * (deltaUv2.Y * edge1.X - deltaUv1.Y * edge2.X),
            f * (deltaUv2.Y * edge1.Y - deltaUv1.Y * edge2.Y),
            f * (deltaUv2.Y * edge1.Z - deltaUv1.Y * edge2.Z)).Normalized();
        var bitangent = new Vector3(
            f * (-deltaUv2.X * edge1.X + deltaUv1.X * edge2.X),
            f * (-deltaUv2.X * edge1.Y + deltaUv1.X * edge2.Y),
            f * (-deltaUv2.X * edge1.Z + deltaUv1.X * edge2.Z)).Normalized();
        return (tangent, bitangent);
    }

    private static float[] Vertex(Vector3 position, Vector3 normal, Vector2 texCoords, Vector3 tangent, Vector3 bitangent) =>
    [
        position.X, position.Y, position.Z,
        normal.X, normal.Y, normal.Z,
        texCoords.X, texCoords.Y,
        tangent.X, tangent.Y, tangent.Z,
        bitangent.X, bitangent.Y, bitangent.Z
    ];

    private static readonly float[] FloorPlaneVertices =
    {
        10.0f,-0.5f,10.0f,0,1,0,10,0, -10.0f,-0.5f,10.0f,0,1,0,0,0, -10.0f,-0.5f,-10.0f,0,1,0,0,10,
        10.0f,-0.5f,10.0f,0,1,0,10,0, -10.0f,-0.5f,-10.0f,0,1,0,0,10, 10.0f,-0.5f,-10.0f,0,1,0,10,10
    };
}

public enum ShadowMappingVariant
{
    DepthOnly,
    Base,
    Complete
}

public sealed class ShadowMappingDemo : DemoWindow
{
    private const int ShadowSize = 1024;
    private readonly ShadowMappingVariant _variant;
    private readonly Camera _camera = new(new Vector3(0.0f, 0.0f, 3.0f));
    private readonly Vector3 _lightPos = new(-2.0f, 4.0f, -1.0f);
    private Shader? _shader;
    private Shader? _depthShader;
    private Shader? _debugShader;
    private GlVertexArray? _plane;
    private GlVertexArray? _cube;
    private GlVertexArray? _quad;
    private int _woodTexture;
    private int _depthMapFbo;
    private int _depthMap;

    public ShadowMappingDemo(RunOptions options, ShadowMappingVariant variant) : base(options)
    {
        _variant = variant;
    }

    protected override void LoadDemo()
    {
        GL.Enable(EnableCap.DepthTest);
        var folder = _variant switch
        {
            ShadowMappingVariant.DepthOnly => "3.1.1.shadow_mapping_depth",
            ShadowMappingVariant.Base => "3.1.2.shadow_mapping_base",
            ShadowMappingVariant.Complete => "3.1.3.shadow_mapping",
            _ => throw new ArgumentOutOfRangeException()
        };
        var prefix = _variant switch
        {
            ShadowMappingVariant.DepthOnly => "3.1.1",
            ShadowMappingVariant.Base => "3.1.2",
            ShadowMappingVariant.Complete => "3.1.3",
            _ => throw new ArgumentOutOfRangeException()
        };

        _depthShader = new Shader(
            Paths.Shader("5.advanced_lighting", folder, $"{prefix}.shadow_mapping_depth.vs"),
            Paths.Shader("5.advanced_lighting", folder, $"{prefix}.shadow_mapping_depth.fs"));
        _debugShader = new Shader(
            Paths.Shader("5.advanced_lighting", folder, $"{prefix}.debug_quad.vs"),
            Paths.Shader("5.advanced_lighting", folder, $"{prefix}.debug_quad_depth.fs"));
        if (_variant != ShadowMappingVariant.DepthOnly)
        {
            _shader = new Shader(
                Paths.Shader("5.advanced_lighting", folder, $"{prefix}.shadow_mapping.vs"),
                Paths.Shader("5.advanced_lighting", folder, $"{prefix}.shadow_mapping.fs"));
            _shader.Use();
            _shader.SetInt("diffuseTexture", 0);
            _shader.SetInt("shadowMap", 1);
        }
        _debugShader.Use();
        _debugShader.SetInt("depthMap", 0);

        _plane = new GlVertexArray(ShadowPlaneVertices, 8, (0, 3, 0), (1, 3, 3), (2, 2, 6));
        _cube = new GlVertexArray(ShadowCubeVertices, 8, (0, 3, 0), (1, 3, 3), (2, 2, 6));
        _quad = new GlVertexArray(DebugQuadVertices, 5, (0, 3, 0), (1, 2, 3));
        _woodTexture = TextureLoader.LoadTexture(Paths.Resource("resources/textures/wood.png"));
        SetupDepthFramebuffer();
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
        const float nearPlane = 1.0f;
        const float farPlane = 7.5f;
        var lightProjection = Matrix4.CreateOrthographicOffCenter(-10.0f, 10.0f, -10.0f, 10.0f, nearPlane, farPlane);
        var lightView = Matrix4.LookAt(_lightPos, Vector3.Zero, Vector3.UnitY);
        var lightSpaceMatrix = lightView * lightProjection;

        GL.Viewport(0, 0, ShadowSize, ShadowSize);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _depthMapFbo);
        GL.Clear(ClearBufferMask.DepthBufferBit);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, _woodTexture);
        _depthShader!.Use();
        _depthShader.SetMatrix4("lightSpaceMatrix", lightSpaceMatrix);
        RenderScene(_depthShader);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        GL.Viewport(0, 0, WidthPx, HeightPx);
        GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        if (_variant == ShadowMappingVariant.DepthOnly)
        {
            _debugShader!.Use();
            _debugShader.SetFloat("near_plane", nearPlane);
            _debugShader.SetFloat("far_plane", farPlane);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _depthMap);
            _quad!.Draw(PrimitiveType.TriangleStrip);
            return;
        }

        _shader!.Use();
        _shader.SetMatrix4("projection", Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(_camera.Zoom), AspectRatioPx, 0.1f, 100.0f));
        _shader.SetMatrix4("view", _camera.GetViewMatrix());
        _shader.SetVector3("viewPos", _camera.Position);
        _shader.SetVector3("lightPos", _lightPos);
        _shader.SetMatrix4("lightSpaceMatrix", lightSpaceMatrix);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, _woodTexture);
        GL.ActiveTexture(TextureUnit.Texture1);
        GL.BindTexture(TextureTarget.Texture2D, _depthMap);
        RenderScene(_shader);
    }

    private void SetupDepthFramebuffer()
    {
        _depthMapFbo = GL.GenFramebuffer();
        _depthMap = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _depthMap);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, ShadowSize, ShadowSize, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)(_variant == ShadowMappingVariant.Complete ? TextureWrapMode.ClampToBorder : TextureWrapMode.Repeat));
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)(_variant == ShadowMappingVariant.Complete ? TextureWrapMode.ClampToBorder : TextureWrapMode.Repeat));

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _depthMapFbo);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, _depthMap, 0);
        GL.DrawBuffer(DrawBufferMode.None);
        GL.ReadBuffer(ReadBufferMode.None);
        var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (status != FramebufferErrorCode.FramebufferComplete)
        {
            throw new InvalidOperationException($"Shadow framebuffer is not complete: {status}");
        }
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    private void RenderScene(Shader shader)
    {
        shader.SetMatrix4("model", Matrix4.Identity);
        _plane!.Draw();

        shader.SetMatrix4("model", Matrix4.CreateScale(0.5f) * Matrix4.CreateTranslation(0.0f, 1.5f, 0.0f));
        _cube!.Draw();
        shader.SetMatrix4("model", Matrix4.CreateScale(0.5f) * Matrix4.CreateTranslation(2.0f, 0.0f, 1.0f));
        _cube.Draw();
        shader.SetMatrix4("model", Matrix4.CreateScale(0.25f) * Matrix4.CreateFromAxisAngle(new Vector3(1.0f, 0.0f, 1.0f).Normalized(), MathHelper.DegreesToRadians(60.0f)) * Matrix4.CreateTranslation(-1.0f, 0.0f, 2.0f));
        _cube.Draw();
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        _shader?.Dispose();
        _depthShader?.Dispose();
        _debugShader?.Dispose();
        _plane?.Dispose();
        _cube?.Dispose();
        _quad?.Dispose();
        if (_woodTexture != 0) GL.DeleteTexture(_woodTexture);
        if (_depthMapFbo != 0) GL.DeleteFramebuffer(_depthMapFbo);
        if (_depthMap != 0) GL.DeleteTexture(_depthMap);
    }

    private static readonly float[] ShadowPlaneVertices =
    {
        25.0f,-0.5f,25.0f,0,1,0,25,0, -25.0f,-0.5f,25.0f,0,1,0,0,0, -25.0f,-0.5f,-25.0f,0,1,0,0,25,
        25.0f,-0.5f,25.0f,0,1,0,25,0, -25.0f,-0.5f,-25.0f,0,1,0,0,25, 25.0f,-0.5f,-25.0f,0,1,0,25,25
    };

    internal static readonly float[] ShadowCubeVertices =
    {
        -1,-1,-1,0,0,-1,0,0, 1,1,-1,0,0,-1,1,1, 1,-1,-1,0,0,-1,1,0, 1,1,-1,0,0,-1,1,1, -1,-1,-1,0,0,-1,0,0, -1,1,-1,0,0,-1,0,1,
        -1,-1,1,0,0,1,0,0, 1,-1,1,0,0,1,1,0, 1,1,1,0,0,1,1,1, 1,1,1,0,0,1,1,1, -1,1,1,0,0,1,0,1, -1,-1,1,0,0,1,0,0,
        -1,1,1,-1,0,0,1,0, -1,1,-1,-1,0,0,1,1, -1,-1,-1,-1,0,0,0,1, -1,-1,-1,-1,0,0,0,1, -1,-1,1,-1,0,0,0,0, -1,1,1,-1,0,0,1,0,
        1,1,1,1,0,0,1,0, 1,-1,-1,1,0,0,0,1, 1,1,-1,1,0,0,1,1, 1,-1,-1,1,0,0,0,1, 1,1,1,1,0,0,1,0, 1,-1,1,1,0,0,0,0,
        -1,-1,-1,0,-1,0,0,1, 1,-1,-1,0,-1,0,1,1, 1,-1,1,0,-1,0,1,0, 1,-1,1,0,-1,0,1,0, -1,-1,1,0,-1,0,0,0, -1,-1,-1,0,-1,0,0,1,
        -1,1,-1,0,1,0,0,1, 1,1,1,0,1,0,1,0, 1,1,-1,0,1,0,1,1, 1,1,1,0,1,0,1,0, -1,1,-1,0,1,0,0,1, -1,1,1,0,1,0,0,0
    };

    private static readonly float[] DebugQuadVertices =
    {
        -1,1,0,0,1, -1,-1,0,0,0, 1,1,0,1,1, 1,-1,0,1,0
    };
}

public enum PointShadowsVariant
{
    Hard,
    Soft
}

public sealed class PointShadowsDemo : DemoWindow
{
    private const int ShadowSize = 1024;
    private readonly PointShadowsVariant _variant;
    private readonly Camera _camera = new(new Vector3(0.0f, 0.0f, 3.0f));
    private readonly Vector3 _lightPos = Vector3.Zero;
    private Shader? _shader;
    private Shader? _depthShader;
    private GlVertexArray? _cube;
    private int _woodTexture;
    private int _depthMapFbo;
    private int _depthCubemap;

    public PointShadowsDemo(RunOptions options, PointShadowsVariant variant) : base(options)
    {
        _variant = variant;
    }

    protected override void LoadDemo()
    {
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        var folder = _variant == PointShadowsVariant.Hard ? "3.2.1.point_shadows" : "3.2.2.point_shadows_soft";
        var prefix = _variant == PointShadowsVariant.Hard ? "3.2.1" : "3.2.2";
        _shader = new Shader(
            Paths.Shader("5.advanced_lighting", folder, $"{prefix}.point_shadows.vs"),
            Paths.Shader("5.advanced_lighting", folder, $"{prefix}.point_shadows.fs"));
        _depthShader = new Shader(
            Paths.Shader("5.advanced_lighting", folder, $"{prefix}.point_shadows_depth.vs"),
            Paths.Shader("5.advanced_lighting", folder, $"{prefix}.point_shadows_depth.fs"),
            Paths.Shader("5.advanced_lighting", folder, $"{prefix}.point_shadows_depth.gs"));
        _cube = new GlVertexArray(ShadowMappingDemo.ShadowCubeVertices, 8, (0, 3, 0), (1, 3, 3), (2, 2, 6));
        _woodTexture = TextureLoader.LoadTexture(Paths.Resource("resources/textures/wood.png"));
        _shader.Use();
        _shader.SetInt("diffuseTexture", 0);
        _shader.SetInt("depthMap", 1);
        SetupDepthCubemap();
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
        const float nearPlane = 1.0f;
        const float farPlane = 25.0f;
        var shadowProj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90.0f), 1.0f, nearPlane, farPlane);
        var transforms = new[]
        {
            Matrix4.LookAt(_lightPos, _lightPos + Vector3.UnitX, -Vector3.UnitY) * shadowProj,
            Matrix4.LookAt(_lightPos, _lightPos - Vector3.UnitX, -Vector3.UnitY) * shadowProj,
            Matrix4.LookAt(_lightPos, _lightPos + Vector3.UnitY, Vector3.UnitZ) * shadowProj,
            Matrix4.LookAt(_lightPos, _lightPos - Vector3.UnitY, -Vector3.UnitZ) * shadowProj,
            Matrix4.LookAt(_lightPos, _lightPos + Vector3.UnitZ, -Vector3.UnitY) * shadowProj,
            Matrix4.LookAt(_lightPos, _lightPos - Vector3.UnitZ, -Vector3.UnitY) * shadowProj
        };

        GL.Viewport(0, 0, ShadowSize, ShadowSize);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _depthMapFbo);
        GL.Clear(ClearBufferMask.DepthBufferBit);
        _depthShader!.Use();
        for (var i = 0; i < transforms.Length; i++)
        {
            _depthShader.SetMatrix4($"shadowMatrices[{i}]", transforms[i]);
        }
        _depthShader.SetFloat("far_plane", farPlane);
        _depthShader.SetVector3("lightPos", _lightPos);
        RenderRoomScene(_depthShader);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        GL.Viewport(0, 0, WidthPx, HeightPx);
        GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        _shader!.Use();
        _shader.SetMatrix4("projection", Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(_camera.Zoom), AspectRatioPx, 0.1f, 100.0f));
        _shader.SetMatrix4("view", _camera.GetViewMatrix());
        _shader.SetVector3("lightPos", _lightPos);
        _shader.SetVector3("viewPos", _camera.Position);
        _shader.SetInt("shadows", 1);
        _shader.SetFloat("far_plane", farPlane);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, _woodTexture);
        GL.ActiveTexture(TextureUnit.Texture1);
        GL.BindTexture(TextureTarget.TextureCubeMap, _depthCubemap);
        RenderRoomScene(_shader);
    }

    private void SetupDepthCubemap()
    {
        _depthMapFbo = GL.GenFramebuffer();
        _depthCubemap = GL.GenTexture();
        GL.BindTexture(TextureTarget.TextureCubeMap, _depthCubemap);
        for (var i = 0; i < 6; i++)
        {
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.DepthComponent, ShadowSize, ShadowSize, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
        }
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _depthMapFbo);
        GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, _depthCubemap, 0);
        GL.DrawBuffer(DrawBufferMode.None);
        GL.ReadBuffer(ReadBufferMode.None);
        var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (status != FramebufferErrorCode.FramebufferComplete)
        {
            throw new InvalidOperationException($"Point shadow framebuffer is not complete: {status}");
        }
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    private void RenderRoomScene(Shader shader)
    {
        shader.SetMatrix4("model", Matrix4.CreateScale(5.0f));
        GL.Disable(EnableCap.CullFace);
        shader.SetInt("reverse_normals", 1);
        _cube!.Draw();
        shader.SetInt("reverse_normals", 0);
        GL.Enable(EnableCap.CullFace);

        DrawCube(shader, Matrix4.CreateScale(0.5f) * Matrix4.CreateTranslation(4.0f, -3.5f, 0.0f));
        DrawCube(shader, Matrix4.CreateScale(0.75f) * Matrix4.CreateTranslation(2.0f, 3.0f, 1.0f));
        DrawCube(shader, Matrix4.CreateScale(0.5f) * Matrix4.CreateTranslation(-3.0f, -1.0f, 0.0f));
        DrawCube(shader, Matrix4.CreateScale(0.5f) * Matrix4.CreateTranslation(-1.5f, 1.0f, 1.5f));
        DrawCube(shader, Matrix4.CreateScale(0.75f) * Matrix4.CreateFromAxisAngle(new Vector3(1.0f, 0.0f, 1.0f).Normalized(), MathHelper.DegreesToRadians(60.0f)) * Matrix4.CreateTranslation(-1.5f, 2.0f, -3.0f));
    }

    private void DrawCube(Shader shader, Matrix4 model)
    {
        shader.SetMatrix4("model", model);
        _cube!.Draw();
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        _shader?.Dispose();
        _depthShader?.Dispose();
        _cube?.Dispose();
        if (_woodTexture != 0) GL.DeleteTexture(_woodTexture);
        if (_depthMapFbo != 0) GL.DeleteFramebuffer(_depthMapFbo);
        if (_depthCubemap != 0) GL.DeleteTexture(_depthCubemap);
    }
}

public enum HdrBloomVariant
{
    Hdr,
    Bloom
}

public sealed class HdrBloomDemo : DemoWindow
{
    private readonly HdrBloomVariant _variant;
    private readonly Camera _camera = new(new Vector3(0.0f, 0.0f, 5.0f));
    private Shader? _sceneShader;
    private Shader? _lightShader;
    private Shader? _blurShader;
    private Shader? _finalShader;
    private GlVertexArray? _cube;
    private GlVertexArray? _quad;
    private int _woodTexture;
    private int _containerTexture;
    private int _hdrFbo;
    private int _depthRbo;
    private readonly int[] _colorBuffers = new int[2];
    private readonly int[] _pingpongFbos = new int[2];
    private readonly int[] _pingpongColorBuffers = new int[2];

    public HdrBloomDemo(RunOptions options, HdrBloomVariant variant) : base(options)
    {
        _variant = variant;
    }

    protected override void LoadDemo()
    {
        GL.Enable(EnableCap.DepthTest);
        var folder = _variant == HdrBloomVariant.Hdr ? "6.hdr" : "7.bloom";
        if (_variant == HdrBloomVariant.Hdr)
        {
            _sceneShader = new Shader(
                Paths.Shader("5.advanced_lighting", folder, "6.lighting.vs"),
                Paths.Shader("5.advanced_lighting", folder, "6.lighting.fs"));
            _finalShader = new Shader(
                Paths.Shader("5.advanced_lighting", folder, "6.hdr.vs"),
                Paths.Shader("5.advanced_lighting", folder, "6.hdr.fs"));
            _finalShader.Use();
            _finalShader.SetInt("hdrBuffer", 0);
        }
        else
        {
            _sceneShader = new Shader(
                Paths.Shader("5.advanced_lighting", folder, "7.bloom.vs"),
                Paths.Shader("5.advanced_lighting", folder, "7.bloom.fs"));
            _lightShader = new Shader(
                Paths.Shader("5.advanced_lighting", folder, "7.bloom.vs"),
                Paths.Shader("5.advanced_lighting", folder, "7.light_box.fs"));
            _blurShader = new Shader(
                Paths.Shader("5.advanced_lighting", folder, "7.blur.vs"),
                Paths.Shader("5.advanced_lighting", folder, "7.blur.fs"));
            _finalShader = new Shader(
                Paths.Shader("5.advanced_lighting", folder, "7.bloom_final.vs"),
                Paths.Shader("5.advanced_lighting", folder, "7.bloom_final.fs"));
            _blurShader.Use();
            _blurShader.SetInt("image", 0);
            _finalShader.Use();
            _finalShader.SetInt("scene", 0);
            _finalShader.SetInt("bloomBlur", 1);
        }

        _sceneShader.Use();
        _sceneShader.SetInt("diffuseTexture", 0);
        _cube = new GlVertexArray(ShadowMappingDemo.ShadowCubeVertices, 8, (0, 3, 0), (1, 3, 3), (2, 2, 6));
        _quad = new GlVertexArray(ScreenQuadVertices, 5, (0, 3, 0), (1, 2, 3));
        _woodTexture = TextureLoader.LoadTexture(Paths.Resource("resources/textures/wood.png"));
        _containerTexture = TextureLoader.LoadTexture(Paths.Resource("resources/textures/container2.png"));
        SetupFramebuffers();
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
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _hdrFbo);
        GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        RenderSceneToHdrBuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        var bloomTexture = _colorBuffers[1];
        if (_variant == HdrBloomVariant.Bloom)
        {
            bloomTexture = BlurBrightTexture();
        }

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _finalShader!.Use();
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, _colorBuffers[0]);
        if (_variant == HdrBloomVariant.Hdr)
        {
            _finalShader.SetInt("hdr", 1);
        }
        else
        {
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, bloomTexture);
            _finalShader.SetInt("bloom", 1);
        }
        _finalShader.SetFloat("exposure", 1.0f);
        _quad!.Draw(PrimitiveType.TriangleStrip);
    }

    private void RenderSceneToHdrBuffer()
    {
        var projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(_camera.Zoom), AspectRatioPx, 0.1f, 100.0f);
        var view = _camera.GetViewMatrix();
        _sceneShader!.Use();
        _sceneShader.SetMatrix4("projection", projection);
        _sceneShader.SetMatrix4("view", view);
        _sceneShader.SetVector3("viewPos", _camera.Position);

        var (positions, colors) = _variant == HdrBloomVariant.Hdr ? HdrLights() : BloomLights();
        for (var i = 0; i < positions.Length; i++)
        {
            _sceneShader.SetVector3($"lights[{i}].Position", positions[i]);
            _sceneShader.SetVector3($"lights[{i}].Color", colors[i]);
        }

        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, _woodTexture);
        if (_variant == HdrBloomVariant.Hdr)
        {
            _sceneShader.SetInt("inverse_normals", 1);
            DrawCube(Matrix4.CreateScale(2.5f, 2.5f, 27.5f) * Matrix4.CreateTranslation(0.0f, 0.0f, 25.0f), _sceneShader);
            _sceneShader.SetInt("inverse_normals", 0);
            return;
        }

        DrawCube(Matrix4.CreateScale(12.5f, 0.5f, 12.5f) * Matrix4.CreateTranslation(0.0f, -1.0f, 0.0f), _sceneShader);
        GL.BindTexture(TextureTarget.Texture2D, _containerTexture);
        DrawCube(Matrix4.CreateScale(0.5f) * Matrix4.CreateTranslation(0.0f, 1.5f, 0.0f), _sceneShader);
        DrawCube(Matrix4.CreateScale(0.5f) * Matrix4.CreateTranslation(2.0f, 0.0f, 1.0f), _sceneShader);
        DrawCube(Matrix4.CreateFromAxisAngle(new Vector3(1.0f, 0.0f, 1.0f).Normalized(), MathHelper.DegreesToRadians(60.0f)) * Matrix4.CreateTranslation(-1.0f, -1.0f, 2.0f), _sceneShader);
        DrawCube(Matrix4.CreateScale(1.25f) * Matrix4.CreateFromAxisAngle(new Vector3(1.0f, 0.0f, 1.0f).Normalized(), MathHelper.DegreesToRadians(23.0f)) * Matrix4.CreateTranslation(0.0f, 2.7f, 4.0f), _sceneShader);
        DrawCube(Matrix4.CreateFromAxisAngle(new Vector3(1.0f, 0.0f, 1.0f).Normalized(), MathHelper.DegreesToRadians(124.0f)) * Matrix4.CreateTranslation(-2.0f, 1.0f, -3.0f), _sceneShader);
        DrawCube(Matrix4.CreateScale(0.5f) * Matrix4.CreateTranslation(-3.0f, 0.0f, 0.0f), _sceneShader);

        _lightShader!.Use();
        _lightShader.SetMatrix4("projection", projection);
        _lightShader.SetMatrix4("view", view);
        for (var i = 0; i < positions.Length; i++)
        {
            _lightShader.SetVector3("lightColor", colors[i]);
            DrawCube(Matrix4.CreateScale(0.25f) * Matrix4.CreateTranslation(positions[i]), _lightShader);
        }
    }

    private int BlurBrightTexture()
    {
        var horizontal = true;
        var first = true;
        _blurShader!.Use();
        for (var i = 0; i < 10; i++)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _pingpongFbos[horizontal ? 1 : 0]);
            _blurShader.SetInt("horizontal", horizontal ? 1 : 0);
            GL.BindTexture(TextureTarget.Texture2D, first ? _colorBuffers[1] : _pingpongColorBuffers[horizontal ? 0 : 1]);
            _quad!.Draw(PrimitiveType.TriangleStrip);
            horizontal = !horizontal;
            first = false;
        }
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        return _pingpongColorBuffers[horizontal ? 1 : 0];
    }

    private void DrawCube(Matrix4 model, Shader shader)
    {
        shader.SetMatrix4("model", model);
        _cube!.Draw();
    }

    private void SetupFramebuffers()
    {
        _hdrFbo = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _hdrFbo);
        var attachments = _variant == HdrBloomVariant.Hdr ? 1 : 2;
        for (var i = 0; i < attachments; i++)
        {
            _colorBuffers[i] = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _colorBuffers[i]);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, WidthPx, HeightPx, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0 + i, TextureTarget.Texture2D, _colorBuffers[i], 0);
        }

        _depthRbo = GL.GenRenderbuffer();
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _depthRbo);
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, WidthPx, HeightPx);
        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, _depthRbo);
        if (_variant == HdrBloomVariant.Bloom)
        {
            GL.DrawBuffers(2, new[] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1 });
        }
        CheckFramebuffer("HDR framebuffer");
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        if (_variant != HdrBloomVariant.Bloom)
        {
            return;
        }

        for (var i = 0; i < 2; i++)
        {
            _pingpongFbos[i] = GL.GenFramebuffer();
            _pingpongColorBuffers[i] = GL.GenTexture();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _pingpongFbos[i]);
            GL.BindTexture(TextureTarget.Texture2D, _pingpongColorBuffers[i]);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, WidthPx, HeightPx, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, _pingpongColorBuffers[i], 0);
            CheckFramebuffer("Bloom ping-pong framebuffer");
        }
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    private static void CheckFramebuffer(string name)
    {
        var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (status != FramebufferErrorCode.FramebufferComplete)
        {
            throw new InvalidOperationException($"{name} is not complete: {status}");
        }
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        _sceneShader?.Dispose();
        _lightShader?.Dispose();
        _blurShader?.Dispose();
        _finalShader?.Dispose();
        _cube?.Dispose();
        _quad?.Dispose();
        if (_woodTexture != 0) GL.DeleteTexture(_woodTexture);
        if (_containerTexture != 0) GL.DeleteTexture(_containerTexture);
        if (_hdrFbo != 0) GL.DeleteFramebuffer(_hdrFbo);
        if (_depthRbo != 0) GL.DeleteRenderbuffer(_depthRbo);
        foreach (var texture in _colorBuffers) if (texture != 0) GL.DeleteTexture(texture);
        foreach (var fbo in _pingpongFbos) if (fbo != 0) GL.DeleteFramebuffer(fbo);
        foreach (var texture in _pingpongColorBuffers) if (texture != 0) GL.DeleteTexture(texture);
    }

    private static (Vector3[] Positions, Vector3[] Colors) HdrLights() => (
        [new Vector3(0.0f, 0.0f, 49.5f), new Vector3(-1.4f, -1.9f, 9.0f), new Vector3(0.0f, -1.8f, 4.0f), new Vector3(0.8f, -1.7f, 6.0f)],
        [new Vector3(200.0f), new Vector3(0.1f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.2f), new Vector3(0.0f, 0.1f, 0.0f)]);

    private static (Vector3[] Positions, Vector3[] Colors) BloomLights() => (
        [new Vector3(0.0f, 0.5f, 1.5f), new Vector3(-4.0f, 0.5f, -3.0f), new Vector3(3.0f, 0.5f, 1.0f), new Vector3(-0.8f, 2.4f, -1.0f)],
        [new Vector3(5.0f), new Vector3(10.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 15.0f), new Vector3(0.0f, 5.0f, 0.0f)]);

    private static readonly float[] ScreenQuadVertices =
    {
        -1,1,0,0,1, -1,-1,0,0,0, 1,1,0,1,1, 1,-1,0,1,0
    };
}

public enum DeferredVariant
{
    Shading,
    Volumes
}

public sealed class DeferredShadingDemo : DemoWindow
{
    private readonly DeferredVariant _variant;
    private readonly Camera _camera = new(new Vector3(0.0f, 0.0f, 5.0f));
    private Shader? _geometryShader;
    private Shader? _lightingShader;
    private Shader? _lightBoxShader;
    private Model? _backpack;
    private GlVertexArray? _cube;
    private GlVertexArray? _quad;
    private int _gBuffer;
    private int _gPosition;
    private int _gNormal;
    private int _gAlbedoSpec;
    private int _depthRbo;
    private Vector3[] _lightPositions = [];
    private Vector3[] _lightColors = [];

    public DeferredShadingDemo(RunOptions options, DeferredVariant variant) : base(options)
    {
        _variant = variant;
    }

    protected override void LoadDemo()
    {
        GL.Enable(EnableCap.DepthTest);
        var folder = _variant == DeferredVariant.Shading ? "8.1.deferred_shading" : "8.2.deferred_shading_volumes";
        var prefix = _variant == DeferredVariant.Shading ? "8.1" : "8.2";
        _geometryShader = new Shader(
            Paths.Shader("5.advanced_lighting", folder, $"{prefix}.g_buffer.vs"),
            Paths.Shader("5.advanced_lighting", folder, $"{prefix}.g_buffer.fs"));
        _lightingShader = new Shader(
            Paths.Shader("5.advanced_lighting", folder, $"{prefix}.deferred_shading.vs"),
            Paths.Shader("5.advanced_lighting", folder, $"{prefix}.deferred_shading.fs"));
        _lightBoxShader = new Shader(
            Paths.Shader("5.advanced_lighting", folder, $"{prefix}.deferred_light_box.vs"),
            Paths.Shader("5.advanced_lighting", folder, $"{prefix}.deferred_light_box.fs"));
        _backpack = new Model(Paths.Resource("resources/objects/backpack/backpack.obj"));
        _cube = new GlVertexArray(ShadowMappingDemo.ShadowCubeVertices, 8, (0, 3, 0), (1, 3, 3), (2, 2, 6));
        _quad = new GlVertexArray(HdrBloomDemoQuad, 5, (0, 3, 0), (1, 2, 3));
        SetupGBuffer();
        SetupLights();

        _lightingShader.Use();
        _lightingShader.SetInt("gPosition", 0);
        _lightingShader.SetInt("gNormal", 1);
        _lightingShader.SetInt("gAlbedoSpec", 2);
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
        var projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(_camera.Zoom), AspectRatioPx, 0.1f, 100.0f);
        var view = _camera.GetViewMatrix();

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _gBuffer);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _geometryShader!.Use();
        _geometryShader.SetMatrix4("projection", projection);
        _geometryShader.SetMatrix4("view", view);
        foreach (var position in ObjectPositions)
        {
            _geometryShader.SetMatrix4("model", Matrix4.CreateScale(0.5f) * Matrix4.CreateTranslation(position));
            _backpack!.Draw(_geometryShader);
        }
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _lightingShader!.Use();
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, _gPosition);
        GL.ActiveTexture(TextureUnit.Texture1);
        GL.BindTexture(TextureTarget.Texture2D, _gNormal);
        GL.ActiveTexture(TextureUnit.Texture2);
        GL.BindTexture(TextureTarget.Texture2D, _gAlbedoSpec);
        for (var i = 0; i < _lightPositions.Length; i++)
        {
            _lightingShader.SetVector3($"lights[{i}].Position", _lightPositions[i]);
            _lightingShader.SetVector3($"lights[{i}].Color", _lightColors[i]);
            _lightingShader.SetFloat($"lights[{i}].Linear", 0.7f);
            _lightingShader.SetFloat($"lights[{i}].Quadratic", 1.8f);
            if (_variant == DeferredVariant.Volumes)
            {
                var maxBrightness = MathF.Max(MathF.Max(_lightColors[i].X, _lightColors[i].Y), _lightColors[i].Z);
                var radius = (-0.7f + MathF.Sqrt(0.7f * 0.7f - 4.0f * 1.8f * (1.0f - 256.0f / 5.0f * maxBrightness))) / (2.0f * 1.8f);
                _lightingShader.SetFloat($"lights[{i}].Radius", radius);
            }
        }
        _lightingShader.SetVector3("viewPos", _camera.Position);
        _quad!.Draw(PrimitiveType.TriangleStrip);

        _lightBoxShader!.Use();
        _lightBoxShader.SetMatrix4("projection", projection);
        _lightBoxShader.SetMatrix4("view", view);
        for (var i = 0; i < _lightPositions.Length; i++)
        {
            _lightBoxShader.SetVector3("lightColor", _lightColors[i]);
            _lightBoxShader.SetMatrix4("model", Matrix4.CreateScale(0.125f) * Matrix4.CreateTranslation(_lightPositions[i]));
            _cube!.Draw();
        }
    }

    private void SetupGBuffer()
    {
        _gBuffer = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _gBuffer);
        _gPosition = CreateGBufferTexture(PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float, FramebufferAttachment.ColorAttachment0);
        _gNormal = CreateGBufferTexture(PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float, FramebufferAttachment.ColorAttachment1);
        _gAlbedoSpec = CreateGBufferTexture(PixelInternalFormat.Rgba, PixelFormat.Rgba, PixelType.UnsignedByte, FramebufferAttachment.ColorAttachment2);
        GL.DrawBuffers(3, new[] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment2 });
        _depthRbo = GL.GenRenderbuffer();
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _depthRbo);
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, WidthPx, HeightPx);
        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, _depthRbo);
        var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (status != FramebufferErrorCode.FramebufferComplete)
        {
            throw new InvalidOperationException($"G-buffer is not complete: {status}");
        }
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    private int CreateGBufferTexture(PixelInternalFormat internalFormat, PixelFormat format, PixelType type, FramebufferAttachment attachment)
    {
        var texture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, texture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, internalFormat, WidthPx, HeightPx, 0, format, type, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, attachment, TextureTarget.Texture2D, texture, 0);
        return texture;
    }

    private void SetupLights()
    {
        var random = new Random(13);
        _lightPositions = new Vector3[32];
        _lightColors = new Vector3[32];
        for (var i = 0; i < 32; i++)
        {
            _lightPositions[i] = new Vector3(
                (float)(random.NextDouble() * 6.0 - 3.0),
                (float)(random.NextDouble() * 6.0 - 4.0),
                (float)(random.NextDouble() * 6.0 - 3.0));
            _lightColors[i] = new Vector3(
                (float)(random.NextDouble() * 0.5 + 0.5),
                (float)(random.NextDouble() * 0.5 + 0.5),
                (float)(random.NextDouble() * 0.5 + 0.5));
        }
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        _geometryShader?.Dispose();
        _lightingShader?.Dispose();
        _lightBoxShader?.Dispose();
        _backpack?.Dispose();
        _cube?.Dispose();
        _quad?.Dispose();
        if (_gBuffer != 0) GL.DeleteFramebuffer(_gBuffer);
        if (_gPosition != 0) GL.DeleteTexture(_gPosition);
        if (_gNormal != 0) GL.DeleteTexture(_gNormal);
        if (_gAlbedoSpec != 0) GL.DeleteTexture(_gAlbedoSpec);
        if (_depthRbo != 0) GL.DeleteRenderbuffer(_depthRbo);
    }

    private static readonly Vector3[] ObjectPositions =
    [
        new(-3.0f, -0.5f, -3.0f), new(0.0f, -0.5f, -3.0f), new(3.0f, -0.5f, -3.0f),
        new(-3.0f, -0.5f, 0.0f), new(0.0f, -0.5f, 0.0f), new(3.0f, -0.5f, 0.0f),
        new(-3.0f, -0.5f, 3.0f), new(0.0f, -0.5f, 3.0f), new(3.0f, -0.5f, 3.0f)
    ];

    private static readonly float[] HdrBloomDemoQuad =
    {
        -1,1,0,0,1, -1,-1,0,0,0, 1,1,0,1,1, 1,-1,0,1,0
    };
}

public sealed class SsaoDemo : DemoWindow
{
    private readonly Camera _camera = new(new Vector3(0.0f, 0.0f, 5.0f));
    private Shader? _geometryShader;
    private Shader? _ssaoShader;
    private Shader? _blurShader;
    private Shader? _lightingShader;
    private Model? _backpack;
    private GlVertexArray? _cube;
    private GlVertexArray? _quad;
    private int _gBuffer;
    private int _gPosition;
    private int _gNormal;
    private int _gAlbedo;
    private int _depthRbo;
    private int _ssaoFbo;
    private int _ssaoBlurFbo;
    private int _ssaoColorBuffer;
    private int _ssaoColorBufferBlur;
    private int _noiseTexture;
    private Vector3[] _kernel = [];

    public SsaoDemo(RunOptions options) : base(options)
    {
    }

    protected override void LoadDemo()
    {
        GL.Enable(EnableCap.DepthTest);
        const string folder = "9.ssao";
        _geometryShader = new Shader(
            Paths.Shader("5.advanced_lighting", folder, "9.ssao_geometry.vs"),
            Paths.Shader("5.advanced_lighting", folder, "9.ssao_geometry.fs"));
        _ssaoShader = new Shader(
            Paths.Shader("5.advanced_lighting", folder, "9.ssao.vs"),
            Paths.Shader("5.advanced_lighting", folder, "9.ssao.fs"));
        _blurShader = new Shader(
            Paths.Shader("5.advanced_lighting", folder, "9.ssao.vs"),
            Paths.Shader("5.advanced_lighting", folder, "9.ssao_blur.fs"));
        _lightingShader = new Shader(
            Paths.Shader("5.advanced_lighting", folder, "9.ssao.vs"),
            Paths.Shader("5.advanced_lighting", folder, "9.ssao_lighting.fs"));

        _backpack = new Model(Paths.Resource("resources/objects/backpack/backpack.obj"));
        _cube = new GlVertexArray(ShadowMappingDemo.ShadowCubeVertices, 8, (0, 3, 0), (1, 3, 3), (2, 2, 6));
        _quad = new GlVertexArray(SsaoQuadVertices, 5, (0, 3, 0), (1, 2, 3));
        SetupGBuffer();
        SetupSsaoBuffers();
        SetupKernelAndNoise();

        _lightingShader.Use();
        _lightingShader.SetInt("gPosition", 0);
        _lightingShader.SetInt("gNormal", 1);
        _lightingShader.SetInt("gAlbedo", 2);
        _lightingShader.SetInt("ssao", 3);
        _ssaoShader.Use();
        _ssaoShader.SetInt("gPosition", 0);
        _ssaoShader.SetInt("gNormal", 1);
        _ssaoShader.SetInt("texNoise", 2);
        _blurShader.Use();
        _blurShader.SetInt("ssaoInput", 0);
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
        var projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(_camera.Zoom), AspectRatioPx, 0.1f, 50.0f);
        var view = _camera.GetViewMatrix();

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _gBuffer);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _geometryShader!.Use();
        _geometryShader.SetMatrix4("projection", projection);
        _geometryShader.SetMatrix4("view", view);
        _geometryShader.SetMatrix4("model", Matrix4.CreateScale(7.5f) * Matrix4.CreateTranslation(0.0f, 7.0f, 0.0f));
        _geometryShader.SetInt("invertedNormals", 1);
        _cube!.Draw();
        _geometryShader.SetInt("invertedNormals", 0);
        _geometryShader.SetMatrix4("model", Matrix4.CreateScale(1.0f) * Matrix4.CreateRotationX(MathHelper.DegreesToRadians(-90.0f)) * Matrix4.CreateTranslation(0.0f, 0.5f, 0.0f));
        _backpack!.Draw(_geometryShader);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _ssaoFbo);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        _ssaoShader!.Use();
        for (var i = 0; i < _kernel.Length; i++)
        {
            _ssaoShader.SetVector3($"samples[{i}]", _kernel[i]);
        }
        _ssaoShader.SetMatrix4("projection", projection);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, _gPosition);
        GL.ActiveTexture(TextureUnit.Texture1);
        GL.BindTexture(TextureTarget.Texture2D, _gNormal);
        GL.ActiveTexture(TextureUnit.Texture2);
        GL.BindTexture(TextureTarget.Texture2D, _noiseTexture);
        _quad!.Draw(PrimitiveType.TriangleStrip);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _ssaoBlurFbo);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        _blurShader!.Use();
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, _ssaoColorBuffer);
        _quad.Draw(PrimitiveType.TriangleStrip);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _lightingShader!.Use();
        var lightPosView = Vector3.TransformPosition(new Vector3(2.0f, 4.0f, -2.0f), view);
        _lightingShader.SetVector3("light.Position", lightPosView);
        _lightingShader.SetVector3("light.Color", new Vector3(0.2f, 0.2f, 0.7f));
        _lightingShader.SetFloat("light.Linear", 0.09f);
        _lightingShader.SetFloat("light.Quadratic", 0.032f);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, _gPosition);
        GL.ActiveTexture(TextureUnit.Texture1);
        GL.BindTexture(TextureTarget.Texture2D, _gNormal);
        GL.ActiveTexture(TextureUnit.Texture2);
        GL.BindTexture(TextureTarget.Texture2D, _gAlbedo);
        GL.ActiveTexture(TextureUnit.Texture3);
        GL.BindTexture(TextureTarget.Texture2D, _ssaoColorBufferBlur);
        _quad.Draw(PrimitiveType.TriangleStrip);
    }

    private void SetupGBuffer()
    {
        _gBuffer = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _gBuffer);
        _gPosition = CreateTexture(PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float, FramebufferAttachment.ColorAttachment0);
        _gNormal = CreateTexture(PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float, FramebufferAttachment.ColorAttachment1);
        _gAlbedo = CreateTexture(PixelInternalFormat.Rgba, PixelFormat.Rgba, PixelType.UnsignedByte, FramebufferAttachment.ColorAttachment2);
        GL.DrawBuffers(3, new[] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment2 });
        _depthRbo = GL.GenRenderbuffer();
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _depthRbo);
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, WidthPx, HeightPx);
        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, _depthRbo);
        CheckFramebuffer("SSAO G-buffer");
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    private int CreateTexture(PixelInternalFormat internalFormat, PixelFormat format, PixelType type, FramebufferAttachment attachment)
    {
        var texture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, texture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, internalFormat, WidthPx, HeightPx, 0, format, type, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, attachment, TextureTarget.Texture2D, texture, 0);
        return texture;
    }

    private void SetupSsaoBuffers()
    {
        _ssaoFbo = GL.GenFramebuffer();
        _ssaoBlurFbo = GL.GenFramebuffer();
        _ssaoColorBuffer = CreateRedFramebufferTexture(_ssaoFbo, "SSAO framebuffer");
        _ssaoColorBufferBlur = CreateRedFramebufferTexture(_ssaoBlurFbo, "SSAO blur framebuffer");
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    private int CreateRedFramebufferTexture(int framebuffer, string name)
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
        var texture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, texture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R32f, WidthPx, HeightPx, 0, PixelFormat.Red, PixelType.Float, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, texture, 0);
        CheckFramebuffer(name);
        return texture;
    }

    private void SetupKernelAndNoise()
    {
        static float Lerp(float a, float b, float f) => a + f * (b - a);

        var random = new Random(0);
        _kernel = new Vector3[64];
        for (var i = 0; i < _kernel.Length; i++)
        {
            var sample = new Vector3((float)random.NextDouble() * 2.0f - 1.0f, (float)random.NextDouble() * 2.0f - 1.0f, (float)random.NextDouble()).Normalized();
            sample *= (float)random.NextDouble();
            var scale = (float)i / _kernel.Length;
            sample *= Lerp(0.1f, 1.0f, scale * scale);
            _kernel[i] = sample;
        }

        var noise = new float[16 * 3];
        for (var i = 0; i < 16; i++)
        {
            noise[i * 3 + 0] = (float)random.NextDouble() * 2.0f - 1.0f;
            noise[i * 3 + 1] = (float)random.NextDouble() * 2.0f - 1.0f;
            noise[i * 3 + 2] = 0.0f;
        }

        _noiseTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _noiseTexture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb32f, 4, 4, 0, PixelFormat.Rgb, PixelType.Float, noise);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
    }

    private static void CheckFramebuffer(string name)
    {
        var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (status != FramebufferErrorCode.FramebufferComplete)
        {
            throw new InvalidOperationException($"{name} is not complete: {status}");
        }
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        _geometryShader?.Dispose();
        _ssaoShader?.Dispose();
        _blurShader?.Dispose();
        _lightingShader?.Dispose();
        _backpack?.Dispose();
        _cube?.Dispose();
        _quad?.Dispose();
        foreach (var framebuffer in new[] { _gBuffer, _ssaoFbo, _ssaoBlurFbo }) if (framebuffer != 0) GL.DeleteFramebuffer(framebuffer);
        foreach (var texture in new[] { _gPosition, _gNormal, _gAlbedo, _ssaoColorBuffer, _ssaoColorBufferBlur, _noiseTexture }) if (texture != 0) GL.DeleteTexture(texture);
        if (_depthRbo != 0) GL.DeleteRenderbuffer(_depthRbo);
    }

    private static readonly float[] SsaoQuadVertices =
    {
        -1,1,0,0,1, -1,-1,0,0,0, 1,1,0,1,1, 1,-1,0,1,0
    };
}

public sealed class CsmLegacyDepthDemo : DemoWindow
{
    private readonly Camera _camera = new(new Vector3(0.0f, 0.0f, 3.0f));
    private Shader? _shader;
    private GlVertexArray? _cube;
    private GlVertexArray? _plane;

    public CsmLegacyDepthDemo(RunOptions options) : base(options)
    {
    }

    protected override void LoadDemo()
    {
        GL.Enable(EnableCap.DepthTest);
        _shader = new Shader(
            Paths.Shader("5.advanced_lighting", "3.3.csm", "csm.vs"),
            Paths.Shader("5.advanced_lighting", "3.3.csm", "csm.fs"));
        _cube = new GlVertexArray(CubeVertices, 5, (0, 3, 0), (1, 2, 3));
        _plane = new GlVertexArray(PlaneVertices, 5, (0, 3, 0), (1, 2, 3));
    }

    protected override void UpdateDemo(float deltaTime)
    {
        if (KeyboardState.IsKeyDown(Keys.W)) _camera.ProcessKeyboard(CameraMovement.Forward, deltaTime);
        if (KeyboardState.IsKeyDown(Keys.S)) _camera.ProcessKeyboard(CameraMovement.Backward, deltaTime);
        if (KeyboardState.IsKeyDown(Keys.A)) _camera.ProcessKeyboard(CameraMovement.Left, deltaTime);
        if (KeyboardState.IsKeyDown(Keys.D)) _camera.ProcessKeyboard(CameraMovement.Right, deltaTime);
    }

    protected override void RenderDemo(float deltaTime, int frameIndex)
    {
        GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _shader!.Use();
        _shader.SetMatrix4("view", _camera.GetViewMatrix());
        _shader.SetMatrix4("projection", Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(_camera.Zoom), AspectRatioPx, 0.1f, 100.0f));
        _shader.SetMatrix4("model", Matrix4.CreateTranslation(-1.0f, 0.0f, -1.0f));
        _cube!.Draw();
        _shader.SetMatrix4("model", Matrix4.CreateTranslation(2.0f, 0.0f, 0.0f));
        _cube.Draw();
        _shader.SetMatrix4("model", Matrix4.Identity);
        _plane!.Draw();
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        _shader?.Dispose();
        _cube?.Dispose();
        _plane?.Dispose();
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
}
