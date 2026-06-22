using LearnOpenGL.OpenTK.Core;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace LearnOpenGL.OpenTK.Demos;

public enum PbrLightingVariant
{
    ScalarMaterial,
    TexturedMaterial
}

public sealed class PbrLightingDemo : DemoWindow
{
    private readonly PbrLightingVariant _variant;
    private readonly Camera _camera = new(new Vector3(0.0f, 0.0f, 3.0f));
    private Shader? _shader;
    private GlVertexArray? _sphere;
    private int _albedo;
    private int _normal;
    private int _metallic;
    private int _roughness;
    private int _ao;
    private readonly Vector3[] _lightPositions = [
        new(-10.0f, 10.0f, 10.0f),
        new(10.0f, 10.0f, 10.0f),
        new(-10.0f, -10.0f, 10.0f),
        new(10.0f, -10.0f, 10.0f)
    ];
    private readonly Vector3[] _lightColors = [
        new(300.0f), new(300.0f), new(300.0f), new(300.0f)
    ];

    public PbrLightingDemo(RunOptions options, PbrLightingVariant variant) : base(options)
    {
        _variant = variant;
    }

    protected override void LoadDemo()
    {
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Multisample);
        var folder = _variant == PbrLightingVariant.ScalarMaterial ? "1.1.lighting" : "1.2.lighting_textured";
        var prefix = _variant == PbrLightingVariant.ScalarMaterial ? "1.1" : "1.2";
        _shader = new Shader(
            Paths.Shader("6.pbr", folder, $"{prefix}.pbr.vs"),
            Paths.Shader("6.pbr", folder, $"{prefix}.pbr.fs"));
        _sphere = CreateSphere();
        _shader.Use();
        if (_variant == PbrLightingVariant.ScalarMaterial)
        {
            _shader.SetVector3("albedo", 0.5f, 0.0f, 0.0f);
            _shader.SetFloat("ao", 1.0f);
        }
        else
        {
            _shader.SetInt("albedoMap", 0);
            _shader.SetInt("normalMap", 1);
            _shader.SetInt("metallicMap", 2);
            _shader.SetInt("roughnessMap", 3);
            _shader.SetInt("aoMap", 4);
            const string material = "resources/textures/pbr/rusted_iron";
            _albedo = TextureLoader.LoadTexture(Paths.Resource($"{material}/albedo.png"));
            _normal = TextureLoader.LoadTexture(Paths.Resource($"{material}/normal.png"));
            _metallic = TextureLoader.LoadTexture(Paths.Resource($"{material}/metallic.png"));
            _roughness = TextureLoader.LoadTexture(Paths.Resource($"{material}/roughness.png"));
            _ao = TextureLoader.LoadTexture(Paths.Resource($"{material}/ao.png"));
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
        _shader.SetVector3("camPos", _camera.Position);

        for (var i = 0; i < _lightPositions.Length; i++)
        {
            var color = _variant == PbrLightingVariant.TexturedMaterial && i > 0 ? Vector3.Zero : _lightColors[i];
            _shader.SetVector3($"lightPositions[{i}]", _lightPositions[i]);
            _shader.SetVector3($"lightColors[{i}]", color);
        }

        if (_variant == PbrLightingVariant.TexturedMaterial)
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _albedo);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, _normal);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, _metallic);
            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, _roughness);
            GL.ActiveTexture(TextureUnit.Texture4);
            GL.BindTexture(TextureTarget.Texture2D, _ao);
        }

        const int rows = 7;
        const int columns = 7;
        const float spacing = 2.5f;
        for (var row = 0; row < rows; row++)
        {
            if (_variant == PbrLightingVariant.ScalarMaterial)
            {
                _shader.SetFloat("metallic", (float)row / rows);
            }
            for (var column = 0; column < columns; column++)
            {
                if (_variant == PbrLightingVariant.ScalarMaterial)
                {
                    _shader.SetFloat("roughness", MathHelper.Clamp((float)column / columns, 0.05f, 1.0f));
                }
                DrawSphere(Matrix4.CreateTranslation((column - columns / 2) * spacing, (row - rows / 2) * spacing, 0.0f));
            }
        }

        for (var i = 0; i < _lightPositions.Length; i++)
        {
            if (_variant == PbrLightingVariant.TexturedMaterial && i > 0)
            {
                break;
            }
            DrawSphere(Matrix4.CreateScale(0.5f) * Matrix4.CreateTranslation(_lightPositions[i]));
        }
    }

    private void DrawSphere(Matrix4 model)
    {
        _shader!.SetMatrix4("model", model);
        var normalMatrix = new Matrix3(model);
        normalMatrix.Invert();
        normalMatrix.Transpose();
        _shader.SetMatrix3("normalMatrix", normalMatrix);
        _sphere!.Draw(PrimitiveType.TriangleStrip);
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        _shader?.Dispose();
        _sphere?.Dispose();
        foreach (var texture in new[] { _albedo, _normal, _metallic, _roughness, _ao })
        {
            if (texture != 0) GL.DeleteTexture(texture);
        }
    }

    internal static GlVertexArray CreateSphere()
    {
        const int xSegments = 64;
        const int ySegments = 64;
        var vertices = new List<float>();
        var indices = new List<uint>();

        for (var y = 0; y <= ySegments; y++)
        {
            for (var x = 0; x <= xSegments; x++)
            {
                var xSegment = (float)x / xSegments;
                var ySegment = (float)y / ySegments;
                var xPos = MathF.Cos(xSegment * MathHelper.TwoPi) * MathF.Sin(ySegment * MathF.PI);
                var yPos = MathF.Cos(ySegment * MathF.PI);
                var zPos = MathF.Sin(xSegment * MathHelper.TwoPi) * MathF.Sin(ySegment * MathF.PI);

                vertices.AddRange([xPos, yPos, zPos, xPos, yPos, zPos, xSegment, ySegment]);
            }
        }

        var oddRow = false;
        for (var y = 0; y < ySegments; y++)
        {
            if (!oddRow)
            {
                for (var x = 0; x <= xSegments; x++)
                {
                    indices.Add((uint)(y * (xSegments + 1) + x));
                    indices.Add((uint)((y + 1) * (xSegments + 1) + x));
                }
            }
            else
            {
                for (var x = xSegments; x >= 0; x--)
                {
                    indices.Add((uint)((y + 1) * (xSegments + 1) + x));
                    indices.Add((uint)(y * (xSegments + 1) + x));
                }
            }
            oddRow = !oddRow;
        }

        return new GlVertexArray(vertices.ToArray(), 8, indices.ToArray(), (0, 3, 0), (1, 3, 3), (2, 2, 6));
    }
}

public enum PbrIblVariant
{
    IrradianceConversion,
    Irradiance,
    Specular,
    SpecularTextured
}

public sealed class PbrIblDemo : DemoWindow
{
    private readonly PbrIblVariant _variant;
    private readonly Camera _camera = new(new Vector3(0.0f, 0.0f, 3.0f));
    private Shader? _pbrShader;
    private Shader? _backgroundShader;
    private GlVertexArray? _sphere;
    private GlVertexArray? _cube;
    private int _environmentMap;
    private int _brdfLut;
    private readonly int[] _materialTextures = new int[5];

    public PbrIblDemo(RunOptions options, PbrIblVariant variant) : base(options)
    {
        _variant = variant;
    }

    protected override void LoadDemo()
    {
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Lequal);
        var (folder, prefix) = _variant switch
        {
            PbrIblVariant.IrradianceConversion => ("2.1.1.ibl_irradiance_conversion", "2.1.1"),
            PbrIblVariant.Irradiance => ("2.1.2.ibl_irradiance", "2.1.2"),
            PbrIblVariant.Specular => ("2.2.1.ibl_specular", "2.2.1"),
            PbrIblVariant.SpecularTextured => ("2.2.2.ibl_specular_textured", "2.2.2"),
            _ => throw new ArgumentOutOfRangeException()
        };
        _pbrShader = new Shader(
            Paths.Shader("6.pbr", folder, $"{prefix}.pbr.vs"),
            Paths.Shader("6.pbr", folder, $"{prefix}.pbr.fs"));
        _backgroundShader = new Shader(
            Paths.Shader("6.pbr", folder, $"{prefix}.background.vs"),
            Paths.Shader("6.pbr", folder, $"{prefix}.background.fs"));
        _sphere = PbrLightingDemo.CreateSphere();
        _cube = new GlVertexArray(SkyboxCubeVertices, 3, (0, 3, 0));
        _environmentMap = TextureLoader.LoadCubemap([
            Paths.Resource("resources/textures/skybox/right.jpg"),
            Paths.Resource("resources/textures/skybox/left.jpg"),
            Paths.Resource("resources/textures/skybox/top.jpg"),
            Paths.Resource("resources/textures/skybox/bottom.jpg"),
            Paths.Resource("resources/textures/skybox/front.jpg"),
            Paths.Resource("resources/textures/skybox/back.jpg")
        ]);
        _brdfLut = CreateBrdfLut();

        _pbrShader.Use();
        if (_variant == PbrIblVariant.SpecularTextured)
        {
            _pbrShader.SetInt("albedoMap", 0);
            _pbrShader.SetInt("normalMap", 1);
            _pbrShader.SetInt("metallicMap", 2);
            _pbrShader.SetInt("roughnessMap", 3);
            _pbrShader.SetInt("aoMap", 4);
            _pbrShader.SetInt("irradianceMap", 5);
            _pbrShader.SetInt("prefilterMap", 6);
            _pbrShader.SetInt("brdfLUT", 7);
            const string material = "resources/textures/pbr/rusted_iron";
            _materialTextures[0] = TextureLoader.LoadTexture(Paths.Resource($"{material}/albedo.png"));
            _materialTextures[1] = TextureLoader.LoadTexture(Paths.Resource($"{material}/normal.png"));
            _materialTextures[2] = TextureLoader.LoadTexture(Paths.Resource($"{material}/metallic.png"));
            _materialTextures[3] = TextureLoader.LoadTexture(Paths.Resource($"{material}/roughness.png"));
            _materialTextures[4] = TextureLoader.LoadTexture(Paths.Resource($"{material}/ao.png"));
        }
        else
        {
            _pbrShader.SetVector3("albedo", 0.5f, 0.0f, 0.0f);
            _pbrShader.SetFloat("ao", 1.0f);
            if (_variant != PbrIblVariant.IrradianceConversion)
            {
                _pbrShader.SetInt("irradianceMap", 0);
            }
            if (_variant == PbrIblVariant.Specular)
            {
                _pbrShader.SetInt("prefilterMap", 1);
                _pbrShader.SetInt("brdfLUT", 2);
            }
        }
        _backgroundShader.Use();
        _backgroundShader.SetInt("environmentMap", 0);
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
        var projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(_camera.Zoom), AspectRatioPx, 0.1f, 100.0f);
        var view = _camera.GetViewMatrix();

        _pbrShader!.Use();
        _pbrShader.SetMatrix4("projection", projection);
        _pbrShader.SetMatrix4("view", view);
        _pbrShader.SetVector3("camPos", _camera.Position);
        SetLights(_pbrShader);
        BindIblTextures();

        var textured = _variant == PbrIblVariant.SpecularTextured;
        const int rows = 7;
        const int columns = 7;
        const float spacing = 2.5f;
        for (var row = 0; row < rows; row++)
        {
            if (!textured)
            {
                _pbrShader.SetFloat("metallic", (float)row / rows);
            }
            for (var column = 0; column < columns; column++)
            {
                if (!textured)
                {
                    _pbrShader.SetFloat("roughness", MathHelper.Clamp((float)column / columns, 0.05f, 1.0f));
                }
                DrawSphere(Matrix4.CreateTranslation((column - columns / 2) * spacing, (row - rows / 2) * spacing, 0.0f));
            }
        }

        _backgroundShader!.Use();
        _backgroundShader.SetMatrix4("projection", projection);
        _backgroundShader.SetMatrix4("view", view);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.TextureCubeMap, _environmentMap);
        _cube!.Draw();
    }

    private void BindIblTextures()
    {
        if (_variant == PbrIblVariant.SpecularTextured)
        {
            for (var i = 0; i < _materialTextures.Length; i++)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + i);
                GL.BindTexture(TextureTarget.Texture2D, _materialTextures[i]);
            }
            GL.ActiveTexture(TextureUnit.Texture5);
            GL.BindTexture(TextureTarget.TextureCubeMap, _environmentMap);
            GL.ActiveTexture(TextureUnit.Texture6);
            GL.BindTexture(TextureTarget.TextureCubeMap, _environmentMap);
            GL.ActiveTexture(TextureUnit.Texture7);
            GL.BindTexture(TextureTarget.Texture2D, _brdfLut);
            return;
        }

        if (_variant != PbrIblVariant.IrradianceConversion)
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, _environmentMap);
        }
        if (_variant == PbrIblVariant.Specular)
        {
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.TextureCubeMap, _environmentMap);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, _brdfLut);
        }
    }

    private void SetLights(Shader shader)
    {
        var positions = new[] { new Vector3(-10.0f, 10.0f, 10.0f), new Vector3(10.0f, 10.0f, 10.0f), new Vector3(-10.0f, -10.0f, 10.0f), new Vector3(10.0f, -10.0f, 10.0f) };
        var colors = new[] { new Vector3(300.0f), new Vector3(300.0f), new Vector3(300.0f), new Vector3(300.0f) };
        for (var i = 0; i < positions.Length; i++)
        {
            shader.SetVector3($"lightPositions[{i}]", positions[i]);
            shader.SetVector3($"lightColors[{i}]", colors[i]);
        }
    }

    private void DrawSphere(Matrix4 model)
    {
        _pbrShader!.SetMatrix4("model", model);
        var normalMatrix = new Matrix3(model);
        normalMatrix.Invert();
        normalMatrix.Transpose();
        _pbrShader.SetMatrix3("normalMatrix", normalMatrix);
        _sphere!.Draw(PrimitiveType.TriangleStrip);
    }

    private static int CreateBrdfLut()
    {
        var texture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, texture);
        var data = new float[] { 1.0f, 0.0f };
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rg16f, 1, 1, 0, PixelFormat.Rg, PixelType.Float, data);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        return texture;
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        _pbrShader?.Dispose();
        _backgroundShader?.Dispose();
        _sphere?.Dispose();
        _cube?.Dispose();
        if (_environmentMap != 0) GL.DeleteTexture(_environmentMap);
        if (_brdfLut != 0) GL.DeleteTexture(_brdfLut);
        foreach (var texture in _materialTextures)
        {
            if (texture != 0) GL.DeleteTexture(texture);
        }
    }

    private static readonly float[] SkyboxCubeVertices =
    {
        -1,1,-1, -1,-1,-1, 1,-1,-1, 1,-1,-1, 1,1,-1, -1,1,-1,
        -1,-1,1, -1,-1,-1, -1,1,-1, -1,1,-1, -1,1,1, -1,-1,1,
        1,-1,-1, 1,-1,1, 1,1,1, 1,1,1, 1,1,-1, 1,-1,-1,
        -1,-1,1, -1,1,1, 1,1,1, 1,1,1, 1,-1,1, -1,-1,1,
        -1,1,-1, 1,1,-1, 1,1,1, 1,1,1, -1,1,1, -1,1,-1,
        -1,-1,-1, -1,-1,1, 1,-1,-1, 1,-1,-1, -1,-1,1, 1,-1,1
    };
}
