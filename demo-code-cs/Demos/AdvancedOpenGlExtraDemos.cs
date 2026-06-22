using LearnOpenGL.OpenTK.Core;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace LearnOpenGL.OpenTK.Demos;

public enum CubemapVariant
{
    Skybox,
    EnvironmentMapping
}

public sealed class CubemapDemo : DemoWindow
{
    private readonly CubemapVariant _variant;
    private readonly Camera _camera = new(new Vector3(0.0f, 0.0f, 3.0f));
    private Shader? _shader;
    private Shader? _skyboxShader;
    private GlVertexArray? _cube;
    private GlVertexArray? _skybox;
    private int _cubeTexture;
    private int _cubemapTexture;

    public CubemapDemo(RunOptions options, CubemapVariant variant) : base(options)
    {
        _variant = variant;
    }

    protected override void LoadDemo()
    {
        GL.Enable(EnableCap.DepthTest);

        var folder = _variant == CubemapVariant.Skybox ? "6.1.cubemaps_skybox" : "6.2.cubemaps_environment_mapping";
        var prefix = _variant == CubemapVariant.Skybox ? "6.1" : "6.2";

        _shader = new Shader(
            Paths.Shader("4.advanced_opengl", folder, $"{prefix}.cubemaps.vs"),
            Paths.Shader("4.advanced_opengl", folder, $"{prefix}.cubemaps.fs"));
        _skyboxShader = new Shader(
            Paths.Shader("4.advanced_opengl", folder, $"{prefix}.skybox.vs"),
            Paths.Shader("4.advanced_opengl", folder, $"{prefix}.skybox.fs"));

        _cube = _variant == CubemapVariant.Skybox
            ? new GlVertexArray(TexturedCubeVertices, 5, (0, 3, 0), (1, 2, 3))
            : new GlVertexArray(NormalCubeVertices, 6, (0, 3, 0), (1, 3, 3));
        _skybox = new GlVertexArray(SkyboxVertices, 3, (0, 3, 0));

        _cubeTexture = _variant == CubemapVariant.Skybox
            ? TextureLoader.LoadTexture(Paths.Resource("resources/textures/container.jpg"))
            : 0;
        _cubemapTexture = TextureLoader.LoadCubemap(SkyboxFaces());

        _shader.Use();
        _shader.SetInt(_variant == CubemapVariant.Skybox ? "texture1" : "skybox", 0);
        _skyboxShader.Use();
        _skyboxShader.SetInt("skybox", 0);
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

        var view = _camera.GetViewMatrix();
        var projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(_camera.Zoom), AspectRatioPx, 0.1f, 100.0f);

        _shader!.Use();
        _shader.SetMatrix4("model", Matrix4.Identity);
        _shader.SetMatrix4("view", view);
        _shader.SetMatrix4("projection", projection);

        GL.ActiveTexture(TextureUnit.Texture0);
        if (_variant == CubemapVariant.Skybox)
        {
            GL.BindTexture(TextureTarget.Texture2D, _cubeTexture);
        }
        else
        {
            _shader.SetVector3("cameraPos", _camera.Position);
            GL.BindTexture(TextureTarget.TextureCubeMap, _cubemapTexture);
        }
        _cube!.Draw();

        GL.DepthFunc(DepthFunction.Lequal);
        _skyboxShader!.Use();
        var skyboxView = view;
        skyboxView.M41 = 0.0f;
        skyboxView.M42 = 0.0f;
        skyboxView.M43 = 0.0f;
        _skyboxShader.SetMatrix4("view", skyboxView);
        _skyboxShader.SetMatrix4("projection", projection);
        GL.BindTexture(TextureTarget.TextureCubeMap, _cubemapTexture);
        _skybox!.Draw();
        GL.DepthFunc(DepthFunction.Less);
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        if (_cubeTexture != 0) GL.DeleteTexture(_cubeTexture);
        if (_cubemapTexture != 0) GL.DeleteTexture(_cubemapTexture);
        _shader?.Dispose();
        _skyboxShader?.Dispose();
        _cube?.Dispose();
        _skybox?.Dispose();
    }

    private static string[] SkyboxFaces() =>
    [
        Paths.Resource("resources/textures/skybox/right.jpg"),
        Paths.Resource("resources/textures/skybox/left.jpg"),
        Paths.Resource("resources/textures/skybox/top.jpg"),
        Paths.Resource("resources/textures/skybox/bottom.jpg"),
        Paths.Resource("resources/textures/skybox/front.jpg"),
        Paths.Resource("resources/textures/skybox/back.jpg")
    ];

    private static readonly float[] TexturedCubeVertices =
    {
        -0.5f,-0.5f,-0.5f,0,0, 0.5f,-0.5f,-0.5f,1,0, 0.5f,0.5f,-0.5f,1,1, 0.5f,0.5f,-0.5f,1,1, -0.5f,0.5f,-0.5f,0,1, -0.5f,-0.5f,-0.5f,0,0,
        -0.5f,-0.5f,0.5f,0,0, 0.5f,-0.5f,0.5f,1,0, 0.5f,0.5f,0.5f,1,1, 0.5f,0.5f,0.5f,1,1, -0.5f,0.5f,0.5f,0,1, -0.5f,-0.5f,0.5f,0,0,
        -0.5f,0.5f,0.5f,1,0, -0.5f,0.5f,-0.5f,1,1, -0.5f,-0.5f,-0.5f,0,1, -0.5f,-0.5f,-0.5f,0,1, -0.5f,-0.5f,0.5f,0,0, -0.5f,0.5f,0.5f,1,0,
        0.5f,0.5f,0.5f,1,0, 0.5f,0.5f,-0.5f,1,1, 0.5f,-0.5f,-0.5f,0,1, 0.5f,-0.5f,-0.5f,0,1, 0.5f,-0.5f,0.5f,0,0, 0.5f,0.5f,0.5f,1,0,
        -0.5f,-0.5f,-0.5f,0,1, 0.5f,-0.5f,-0.5f,1,1, 0.5f,-0.5f,0.5f,1,0, 0.5f,-0.5f,0.5f,1,0, -0.5f,-0.5f,0.5f,0,0, -0.5f,-0.5f,-0.5f,0,1,
        -0.5f,0.5f,-0.5f,0,1, 0.5f,0.5f,-0.5f,1,1, 0.5f,0.5f,0.5f,1,0, 0.5f,0.5f,0.5f,1,0, -0.5f,0.5f,0.5f,0,0, -0.5f,0.5f,-0.5f,0,1
    };

    private static readonly float[] NormalCubeVertices =
    {
        -0.5f,-0.5f,-0.5f,0,0,-1, 0.5f,-0.5f,-0.5f,0,0,-1, 0.5f,0.5f,-0.5f,0,0,-1, 0.5f,0.5f,-0.5f,0,0,-1, -0.5f,0.5f,-0.5f,0,0,-1, -0.5f,-0.5f,-0.5f,0,0,-1,
        -0.5f,-0.5f,0.5f,0,0,1, 0.5f,-0.5f,0.5f,0,0,1, 0.5f,0.5f,0.5f,0,0,1, 0.5f,0.5f,0.5f,0,0,1, -0.5f,0.5f,0.5f,0,0,1, -0.5f,-0.5f,0.5f,0,0,1,
        -0.5f,0.5f,0.5f,-1,0,0, -0.5f,0.5f,-0.5f,-1,0,0, -0.5f,-0.5f,-0.5f,-1,0,0, -0.5f,-0.5f,-0.5f,-1,0,0, -0.5f,-0.5f,0.5f,-1,0,0, -0.5f,0.5f,0.5f,-1,0,0,
        0.5f,0.5f,0.5f,1,0,0, 0.5f,0.5f,-0.5f,1,0,0, 0.5f,-0.5f,-0.5f,1,0,0, 0.5f,-0.5f,-0.5f,1,0,0, 0.5f,-0.5f,0.5f,1,0,0, 0.5f,0.5f,0.5f,1,0,0,
        -0.5f,-0.5f,-0.5f,0,-1,0, 0.5f,-0.5f,-0.5f,0,-1,0, 0.5f,-0.5f,0.5f,0,-1,0, 0.5f,-0.5f,0.5f,0,-1,0, -0.5f,-0.5f,0.5f,0,-1,0, -0.5f,-0.5f,-0.5f,0,-1,0,
        -0.5f,0.5f,-0.5f,0,1,0, 0.5f,0.5f,-0.5f,0,1,0, 0.5f,0.5f,0.5f,0,1,0, 0.5f,0.5f,0.5f,0,1,0, -0.5f,0.5f,0.5f,0,1,0, -0.5f,0.5f,-0.5f,0,1,0
    };

    private static readonly float[] SkyboxVertices =
    {
        -1,1,-1, -1,-1,-1, 1,-1,-1, 1,-1,-1, 1,1,-1, -1,1,-1,
        -1,-1,1, -1,-1,-1, -1,1,-1, -1,1,-1, -1,1,1, -1,-1,1,
        1,-1,-1, 1,-1,1, 1,1,1, 1,1,1, 1,1,-1, 1,-1,-1,
        -1,-1,1, -1,1,1, 1,1,1, 1,1,1, 1,-1,1, -1,-1,1,
        -1,1,-1, 1,1,-1, 1,1,1, 1,1,1, -1,1,1, -1,1,-1,
        -1,-1,-1, -1,-1,1, 1,-1,-1, 1,-1,-1, -1,-1,1, 1,-1,1
    };
}

public sealed class AdvancedGlslUboDemo : DemoWindow
{
    private readonly Camera _camera = new(new Vector3(0.0f, 0.0f, 3.0f));
    private readonly List<Shader> _shaders = [];
    private GlVertexArray? _cube;
    private int _uboMatrices;

    public AdvancedGlslUboDemo(RunOptions options) : base(options)
    {
    }

    protected override void LoadDemo()
    {
        GL.Enable(EnableCap.DepthTest);
        var folder = "8.advanced_glsl_ubo";
        foreach (var color in new[] { "red", "green", "blue", "yellow" })
        {
            _shaders.Add(new Shader(
                Paths.Shader("4.advanced_opengl", folder, "8.advanced_glsl.vs"),
                Paths.Shader("4.advanced_opengl", folder, $"8.{color}.fs")));
        }

        _cube = new GlVertexArray(CubeVertices, 3, (0, 3, 0));

        foreach (var shader in _shaders)
        {
            var blockIndex = GL.GetUniformBlockIndex(shader.Handle, "Matrices");
            GL.UniformBlockBinding(shader.Handle, blockIndex, 0);
        }

        _uboMatrices = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.UniformBuffer, _uboMatrices);
        GL.BufferData(BufferTarget.UniformBuffer, 2 * 64, IntPtr.Zero, BufferUsageHint.StaticDraw);
        GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        GL.BindBufferRange(BufferRangeTarget.UniformBuffer, 0, _uboMatrices, IntPtr.Zero, 2 * 64);

        var projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), AspectRatioPx, 0.1f, 100.0f);
        GL.BindBuffer(BufferTarget.UniformBuffer, _uboMatrices);
        GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, 64, ref projection);
        GL.BindBuffer(BufferTarget.UniformBuffer, 0);
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

        var view = _camera.GetViewMatrix();
        GL.BindBuffer(BufferTarget.UniformBuffer, _uboMatrices);
        GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)64, 64, ref view);
        GL.BindBuffer(BufferTarget.UniformBuffer, 0);

        var positions = new[]
        {
            new Vector3(-0.75f, 0.75f, 0.0f),
            new Vector3(0.75f, 0.75f, 0.0f),
            new Vector3(0.75f, -0.75f, 0.0f),
            new Vector3(-0.75f, -0.75f, 0.0f)
        };

        _cube!.Bind();
        for (var i = 0; i < _shaders.Count; i++)
        {
            _shaders[i].Use();
            _shaders[i].SetMatrix4("model", Matrix4.CreateTranslation(positions[i]));
            _cube.Draw();
        }
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        if (_uboMatrices != 0) GL.DeleteBuffer(_uboMatrices);
        foreach (var shader in _shaders) shader.Dispose();
        _cube?.Dispose();
    }

    private static readonly float[] CubeVertices =
    {
        -0.5f,-0.5f,-0.5f, 0.5f,-0.5f,-0.5f, 0.5f,0.5f,-0.5f, 0.5f,0.5f,-0.5f, -0.5f,0.5f,-0.5f, -0.5f,-0.5f,-0.5f,
        -0.5f,-0.5f,0.5f, 0.5f,-0.5f,0.5f, 0.5f,0.5f,0.5f, 0.5f,0.5f,0.5f, -0.5f,0.5f,0.5f, -0.5f,-0.5f,0.5f,
        -0.5f,0.5f,0.5f, -0.5f,0.5f,-0.5f, -0.5f,-0.5f,-0.5f, -0.5f,-0.5f,-0.5f, -0.5f,-0.5f,0.5f, -0.5f,0.5f,0.5f,
        0.5f,0.5f,0.5f, 0.5f,0.5f,-0.5f, 0.5f,-0.5f,-0.5f, 0.5f,-0.5f,-0.5f, 0.5f,-0.5f,0.5f, 0.5f,0.5f,0.5f,
        -0.5f,-0.5f,-0.5f, 0.5f,-0.5f,-0.5f, 0.5f,-0.5f,0.5f, 0.5f,-0.5f,0.5f, -0.5f,-0.5f,0.5f, -0.5f,-0.5f,-0.5f,
        -0.5f,0.5f,-0.5f, 0.5f,0.5f,-0.5f, 0.5f,0.5f,0.5f, 0.5f,0.5f,0.5f, -0.5f,0.5f,0.5f, -0.5f,0.5f,-0.5f
    };
}

public sealed class GeometryShaderHousesDemo : DemoWindow
{
    private Shader? _shader;
    private GlVertexArray? _points;

    public GeometryShaderHousesDemo(RunOptions options) : base(options)
    {
    }

    protected override void LoadDemo()
    {
        GL.Enable(EnableCap.DepthTest);
        var folder = "9.1.geometry_shader_houses";
        _shader = new Shader(
            Paths.Shader("4.advanced_opengl", folder, "9.1.geometry_shader.vs"),
            Paths.Shader("4.advanced_opengl", folder, "9.1.geometry_shader.fs"),
            Paths.Shader("4.advanced_opengl", folder, "9.1.geometry_shader.gs"));
        _points = new GlVertexArray(Points, 5, (0, 2, 0), (1, 3, 2));
    }

    protected override void RenderDemo(float deltaTime, int frameIndex)
    {
        GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _shader!.Use();
        _points!.Draw(PrimitiveType.Points);
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        _shader?.Dispose();
        _points?.Dispose();
    }

    private static readonly float[] Points =
    {
        -0.5f, 0.5f, 1.0f, 0.0f, 0.0f,
        0.5f, 0.5f, 0.0f, 1.0f, 0.0f,
        0.5f, -0.5f, 0.0f, 0.0f, 1.0f,
        -0.5f, -0.5f, 1.0f, 1.0f, 0.0f
    };
}

public enum ModelGeometryVariant
{
    Exploding,
    Normals
}

public sealed class ModelGeometryDemo : DemoWindow
{
    private readonly ModelGeometryVariant _variant;
    private readonly Camera _camera = new(new Vector3(0.0f, 0.0f, 3.0f));
    private Shader? _shader;
    private Shader? _normalShader;
    private Model? _model;

    public ModelGeometryDemo(RunOptions options, ModelGeometryVariant variant) : base(options)
    {
        _variant = variant;
    }

    protected override void LoadDemo()
    {
        GL.Enable(EnableCap.DepthTest);
        if (_variant == ModelGeometryVariant.Exploding)
        {
            var folder = "9.2.geometry_shader_exploding";
            _shader = new Shader(
                Paths.Shader("4.advanced_opengl", folder, "9.2.geometry_shader.vs"),
                Paths.Shader("4.advanced_opengl", folder, "9.2.geometry_shader.fs"),
                Paths.Shader("4.advanced_opengl", folder, "9.2.geometry_shader.gs"));
            _model = new Model(Paths.Resource("resources/objects/nanosuit/nanosuit.obj"));
        }
        else
        {
            var folder = "9.3.geometry_shader_normals";
            _shader = new Shader(
                Paths.Shader("4.advanced_opengl", folder, "9.3.default.vs"),
                Paths.Shader("4.advanced_opengl", folder, "9.3.default.fs"));
            _normalShader = new Shader(
                Paths.Shader("4.advanced_opengl", folder, "9.3.normal_visualization.vs"),
                Paths.Shader("4.advanced_opengl", folder, "9.3.normal_visualization.fs"),
                Paths.Shader("4.advanced_opengl", folder, "9.3.normal_visualization.gs"));
            _model = new Model(Paths.Resource("resources/objects/backpack/backpack.obj"));
        }
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

        var projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), AspectRatioPx, 1.0f, 100.0f);
        var view = _camera.GetViewMatrix();
        var modelMatrix = Matrix4.Identity;

        _shader!.Use();
        _shader.SetMatrix4("projection", projection);
        _shader.SetMatrix4("view", view);
        _shader.SetMatrix4("model", modelMatrix);
        if (_variant == ModelGeometryVariant.Exploding)
        {
            _shader.SetFloat("time", frameIndex * 0.2f);
            _model!.Draw(_shader);
        }
        else
        {
            _model!.Draw(_shader);
            _normalShader!.Use();
            _normalShader.SetMatrix4("projection", projection);
            _normalShader.SetMatrix4("view", view);
            _normalShader.SetMatrix4("model", modelMatrix);
            _model.Draw(_normalShader);
        }
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        _shader?.Dispose();
        _normalShader?.Dispose();
        _model?.Dispose();
    }
}

public sealed class InstancingQuadsDemo : DemoWindow
{
    private Shader? _shader;
    private int _vao;
    private int _vbo;
    private int _instanceVbo;

    public InstancingQuadsDemo(RunOptions options) : base(options)
    {
    }

    protected override void LoadDemo()
    {
        GL.Enable(EnableCap.DepthTest);
        _shader = new Shader(
            Paths.Shader("4.advanced_opengl", "10.1.instancing_quads", "10.1.instancing.vs"),
            Paths.Shader("4.advanced_opengl", "10.1.instancing_quads", "10.1.instancing.fs"));

        var translations = new Vector2[100];
        var index = 0;
        const float offset = 0.1f;
        for (var y = -10; y < 10; y += 2)
        {
            for (var x = -10; x < 10; x += 2)
            {
                translations[index++] = new Vector2(x / 10.0f + offset, y / 10.0f + offset);
            }
        }

        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();
        _instanceVbo = GL.GenBuffer();

        GL.BindBuffer(BufferTarget.ArrayBuffer, _instanceVbo);
        GL.BufferData(BufferTarget.ArrayBuffer, translations.Length * 2 * sizeof(float), translations, BufferUsageHint.StaticDraw);

        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, QuadVertices.Length * sizeof(float), QuadVertices, BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 2 * sizeof(float));
        GL.EnableVertexAttribArray(1);

        GL.BindBuffer(BufferTarget.ArrayBuffer, _instanceVbo);
        GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
        GL.EnableVertexAttribArray(2);
        GL.VertexAttribDivisor(2, 1);
        GL.BindVertexArray(0);
    }

    protected override void RenderDemo(float deltaTime, int frameIndex)
    {
        GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _shader!.Use();
        GL.BindVertexArray(_vao);
        GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 6, 100);
        GL.BindVertexArray(0);
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        _shader?.Dispose();
        if (_vao != 0) GL.DeleteVertexArray(_vao);
        if (_vbo != 0) GL.DeleteBuffer(_vbo);
        if (_instanceVbo != 0) GL.DeleteBuffer(_instanceVbo);
    }

    private static readonly float[] QuadVertices =
    {
        -0.05f, 0.05f, 1.0f, 0.0f, 0.0f,
        0.05f, -0.05f, 0.0f, 1.0f, 0.0f,
        -0.05f, -0.05f, 0.0f, 0.0f, 1.0f,
        -0.05f, 0.05f, 1.0f, 0.0f, 0.0f,
        0.05f, -0.05f, 0.0f, 1.0f, 0.0f,
        0.05f, 0.05f, 0.0f, 1.0f, 1.0f
    };
}

public enum AsteroidsVariant
{
    ManyDrawCalls,
    Instanced
}

public sealed class AsteroidsDemo : DemoWindow
{
    private readonly AsteroidsVariant _variant;
    private readonly Camera _camera;
    private Shader? _shader;
    private Shader? _planetShader;
    private Model? _rock;
    private Model? _planet;
    private Matrix4[] _modelMatrices = [];

    public AsteroidsDemo(RunOptions options, AsteroidsVariant variant) : base(options)
    {
        _variant = variant;
        _camera = new Camera(new Vector3(0.0f, 0.0f, variant == AsteroidsVariant.Instanced ? 155.0f : 55.0f));
    }

    protected override void LoadDemo()
    {
        GL.Enable(EnableCap.DepthTest);
        if (_variant == AsteroidsVariant.Instanced)
        {
            _shader = new Shader(
                Paths.Shader("4.advanced_opengl", "10.3.asteroids_instanced", "10.3.asteroids.vs"),
                Paths.Shader("4.advanced_opengl", "10.3.asteroids_instanced", "10.3.asteroids.fs"));
            _planetShader = new Shader(
                Paths.Shader("4.advanced_opengl", "10.3.asteroids_instanced", "10.3.planet.vs"),
                Paths.Shader("4.advanced_opengl", "10.3.asteroids_instanced", "10.3.planet.fs"));
        }
        else
        {
            _shader = new Shader(
                Paths.Shader("4.advanced_opengl", "10.2.asteroids", "10.2.instancing.vs"),
                Paths.Shader("4.advanced_opengl", "10.2.asteroids", "10.2.instancing.fs"));
        }

        _rock = new Model(Paths.Resource("resources/objects/rock/rock.obj"));
        _planet = new Model(Paths.Resource("resources/objects/planet/planet.obj"));
        _modelMatrices = GenerateAsteroidMatrices(
            _variant == AsteroidsVariant.Instanced ? 100000 : 1000,
            _variant == AsteroidsVariant.Instanced ? 150.0f : 50.0f,
            _variant == AsteroidsVariant.Instanced ? 25.0f : 2.5f);
        if (_variant == AsteroidsVariant.Instanced)
        {
            _rock.SetInstanceMatrices(_modelMatrices);
        }
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

        var projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), AspectRatioPx, 0.1f, 1000.0f);
        var view = _camera.GetViewMatrix();
        var planetShader = _planetShader ?? _shader!;

        planetShader.Use();
        planetShader.SetMatrix4("projection", projection);
        planetShader.SetMatrix4("view", view);
        planetShader.SetMatrix4("model", Matrix4.CreateScale(4.0f) * Matrix4.CreateTranslation(0.0f, -3.0f, 0.0f));
        _planet!.Draw(planetShader);

        _shader!.Use();
        _shader.SetMatrix4("projection", projection);
        _shader.SetMatrix4("view", view);
        if (_variant == AsteroidsVariant.Instanced)
        {
            _rock!.DrawInstanced(_shader, _modelMatrices.Length);
        }
        else
        {
            foreach (var matrix in _modelMatrices)
            {
                _shader.SetMatrix4("model", matrix);
                _rock!.Draw(_shader);
            }
        }
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        _shader?.Dispose();
        _planetShader?.Dispose();
        _rock?.Dispose();
        _planet?.Dispose();
    }

    private static Matrix4[] GenerateAsteroidMatrices(int amount, float radius, float offset)
    {
        var random = new Random(0);
        var matrices = new Matrix4[amount];
        for (var i = 0; i < amount; i++)
        {
            var angle = (float)i / amount * MathHelper.TwoPi;
            var x = MathF.Sin(angle) * radius + Displacement(random, offset);
            var y = Displacement(random, offset) * 0.4f;
            var z = MathF.Cos(angle) * radius + Displacement(random, offset);
            var scale = random.Next(0, 20) / 100.0f + 0.05f;
            var rotation = MathHelper.DegreesToRadians(random.Next(0, 360));

            matrices[i] =
                Matrix4.CreateScale(scale) *
                Matrix4.CreateFromAxisAngle(new Vector3(0.4f, 0.6f, 0.8f).Normalized(), rotation) *
                Matrix4.CreateTranslation(x, y, z);
        }

        return matrices;
    }

    private static float Displacement(Random random, float offset)
    {
        return random.Next((int)(2 * offset * 100)) / 100.0f - offset;
    }
}

public enum AntiAliasingVariant
{
    Msaa,
    Offscreen
}

public sealed class AntiAliasingDemo : DemoWindow
{
    private readonly AntiAliasingVariant _variant;
    private readonly Camera _camera = new(new Vector3(0.0f, 0.0f, 3.0f));
    private Shader? _shader;
    private Shader? _screenShader;
    private GlVertexArray? _cube;
    private GlVertexArray? _quad;
    private int _msaaFramebuffer;
    private int _msaaColorTexture;
    private int _msaaRenderbuffer;
    private int _intermediateFramebuffer;
    private int _screenTexture;

    public AntiAliasingDemo(RunOptions options, AntiAliasingVariant variant) : base(options)
    {
        _variant = variant;
    }

    protected override void LoadDemo()
    {
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Multisample);
        var folder = _variant == AntiAliasingVariant.Msaa ? "11.1.anti_aliasing_msaa" : "11.2.anti_aliasing_offscreen";
        var prefix = _variant == AntiAliasingVariant.Msaa ? "11.1.anti_aliasing" : "11.2.anti_aliasing";
        _shader = new Shader(
            Paths.Shader("4.advanced_opengl", folder, $"{prefix}.vs"),
            Paths.Shader("4.advanced_opengl", folder, $"{prefix}.fs"));
        _cube = new GlVertexArray(AdvancedGlslUboDemoCubeVertices, 3, (0, 3, 0));

        if (_variant == AntiAliasingVariant.Offscreen)
        {
            _screenShader = new Shader(
                Paths.Shader("4.advanced_opengl", folder, "11.2.aa_post.vs"),
                Paths.Shader("4.advanced_opengl", folder, "11.2.aa_post.fs"));
            _screenShader.Use();
            _screenShader.SetInt("screenTexture", 0);
            _quad = new GlVertexArray(ScreenQuadVertices, 4, (0, 2, 0), (1, 2, 2));
            SetupOffscreenBuffers();
        }
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
        if (_variant == AntiAliasingVariant.Offscreen)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _msaaFramebuffer);
        }

        GL.Enable(EnableCap.DepthTest);
        GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        DrawCube();

        if (_variant == AntiAliasingVariant.Offscreen)
        {
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, _msaaFramebuffer);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, _intermediateFramebuffer);
            GL.BlitFramebuffer(0, 0, WidthPx, HeightPx, 0, 0, WidthPx, HeightPx, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Disable(EnableCap.DepthTest);
            GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            _screenShader!.Use();
            GL.BindTexture(TextureTarget.Texture2D, _screenTexture);
            _quad!.Draw();
        }
    }

    private void DrawCube()
    {
        _shader!.Use();
        _shader.SetMatrix4("projection", Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(_camera.Zoom), AspectRatioPx, 0.1f, 1000.0f));
        _shader.SetMatrix4("view", _camera.GetViewMatrix());
        _shader.SetMatrix4("model", Matrix4.Identity);
        _cube!.Draw();
    }

    private void SetupOffscreenBuffers()
    {
        _msaaFramebuffer = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _msaaFramebuffer);

        _msaaColorTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2DMultisample, _msaaColorTexture);
        GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, 4, PixelInternalFormat.Rgb, WidthPx, HeightPx, true);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2DMultisample, _msaaColorTexture, 0);

        _msaaRenderbuffer = GL.GenRenderbuffer();
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _msaaRenderbuffer);
        GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, 4, RenderbufferStorage.Depth24Stencil8, WidthPx, HeightPx);
        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, _msaaRenderbuffer);
        CheckFramebuffer("MSAA framebuffer");

        _intermediateFramebuffer = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _intermediateFramebuffer);
        _screenTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _screenTexture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, WidthPx, HeightPx, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, _screenTexture, 0);
        CheckFramebuffer("intermediate framebuffer");
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
        _shader?.Dispose();
        _screenShader?.Dispose();
        _cube?.Dispose();
        _quad?.Dispose();
        if (_msaaFramebuffer != 0) GL.DeleteFramebuffer(_msaaFramebuffer);
        if (_msaaColorTexture != 0) GL.DeleteTexture(_msaaColorTexture);
        if (_msaaRenderbuffer != 0) GL.DeleteRenderbuffer(_msaaRenderbuffer);
        if (_intermediateFramebuffer != 0) GL.DeleteFramebuffer(_intermediateFramebuffer);
        if (_screenTexture != 0) GL.DeleteTexture(_screenTexture);
    }

    private static readonly float[] AdvancedGlslUboDemoCubeVertices =
    {
        -0.5f,-0.5f,-0.5f, 0.5f,-0.5f,-0.5f, 0.5f,0.5f,-0.5f, 0.5f,0.5f,-0.5f, -0.5f,0.5f,-0.5f, -0.5f,-0.5f,-0.5f,
        -0.5f,-0.5f,0.5f, 0.5f,-0.5f,0.5f, 0.5f,0.5f,0.5f, 0.5f,0.5f,0.5f, -0.5f,0.5f,0.5f, -0.5f,-0.5f,0.5f,
        -0.5f,0.5f,0.5f, -0.5f,0.5f,-0.5f, -0.5f,-0.5f,-0.5f, -0.5f,-0.5f,-0.5f, -0.5f,-0.5f,0.5f, -0.5f,0.5f,0.5f,
        0.5f,0.5f,0.5f, 0.5f,0.5f,-0.5f, 0.5f,-0.5f,-0.5f, 0.5f,-0.5f,-0.5f, 0.5f,-0.5f,0.5f, 0.5f,0.5f,0.5f,
        -0.5f,-0.5f,-0.5f, 0.5f,-0.5f,-0.5f, 0.5f,-0.5f,0.5f, 0.5f,-0.5f,0.5f, -0.5f,-0.5f,0.5f, -0.5f,-0.5f,-0.5f,
        -0.5f,0.5f,-0.5f, 0.5f,0.5f,-0.5f, 0.5f,0.5f,0.5f, 0.5f,0.5f,0.5f, -0.5f,0.5f,0.5f, -0.5f,0.5f,-0.5f
    };

    private static readonly float[] ScreenQuadVertices =
    {
        -1.0f, 1.0f, 0.0f, 1.0f, -1.0f, -1.0f, 0.0f, 0.0f, 1.0f, -1.0f, 1.0f, 0.0f,
        -1.0f, 1.0f, 0.0f, 1.0f, 1.0f, -1.0f, 1.0f, 0.0f, 1.0f, 1.0f, 1.0f, 1.0f
    };
}
