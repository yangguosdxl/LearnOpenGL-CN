using LearnOpenGL.OpenTK.Core;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace LearnOpenGL.OpenTK.Demos;

public enum LightingVariant
{
    Colors,
    Diffuse,
    Specular,
    MovingLight,
    ViewSpaceLighting,
    Gouraud,
    Materials,
    MaterialsExercise1,
    DiffuseMap,
    SpecularMap,
    InvertedSpecularMap,
    EmissionMap,
    DirectionalLight,
    PointLight,
    SpotLight,
    SoftSpotLight,
    MultipleLights,
    MultipleLightsDesert
}

public sealed class LightingDemo : DemoWindow
{
    private static readonly Vector3 LightPos = new(1.2f, 1.0f, 2.0f);
    private static readonly Vector3[] CubePositions =
    {
        new(0.0f, 0.0f, 0.0f), new(2.0f, 5.0f, -15.0f), new(-1.5f, -2.2f, -2.5f),
        new(-3.8f, -2.0f, -12.3f), new(2.4f, -0.4f, -3.5f), new(-1.7f, 3.0f, -7.5f),
        new(1.3f, -2.0f, -2.5f), new(1.5f, 2.0f, -2.5f), new(1.5f, 0.2f, -1.5f),
        new(-1.3f, 1.0f, -1.5f)
    };
    private static readonly Vector3[] PointLightPositions =
    {
        new(0.7f, 0.2f, 2.0f), new(2.3f, -3.3f, -4.0f), new(-4.0f, 2.0f, -12.0f), new(0.0f, 0.0f, -3.0f)
    };

    private readonly LightingVariant _variant;
    private readonly Camera _camera = new(new Vector3(0.0f, 0.0f, 3.0f));
    private Shader? _lightingShader;
    private Shader? _lightCubeShader;
    private GlVertexArray? _cube;
    private GlVertexArray? _lamp;
    private int _diffuseMap;
    private int _specularMap;
    private int _emissionMap;

    public LightingDemo(RunOptions options, LightingVariant variant) : base(options)
    {
        _variant = variant;
    }

    protected override void LoadDemo()
    {
        GL.Enable(EnableCap.DepthTest);
        var (folder, lightingVertex, lightingFragment, lightVertex, lightFragment) = ShaderFiles();
        var lightingVertexPath = Paths.Shader("2.lighting", folder, lightingVertex);
        var lightingFragmentPath = Paths.Shader("2.lighting", folder, lightingFragment);
        _lightingShader = _variant switch
        {
            LightingVariant.ViewSpaceLighting => Shader.FromSource(InlineViewSpaceVertex, InlineViewSpaceFragment),
            LightingVariant.Gouraud => Shader.FromSource(InlineGouraudVertex, InlineGouraudFragment),
            LightingVariant.InvertedSpecularMap => Shader.FromSource(File.ReadAllText(lightingVertexPath), InlineInvertedSpecularFragment),
            _ => new Shader(lightingVertexPath, lightingFragmentPath)
        };
        _lightCubeShader = new Shader(Paths.Shader("2.lighting", folder, lightVertex), Paths.Shader("2.lighting", folder, lightFragment));

        var stride = _variant == LightingVariant.Colors ? 3 : UsesTexture ? 8 : 6;
        _cube = new GlVertexArray(VertexData(), stride, Attributes().ToArray());
        _lamp = new GlVertexArray(VertexData(), stride, (0, 3, 0));

        if (UsesTexture)
        {
            _diffuseMap = TextureLoader.LoadTexture(Paths.Resource("resources/textures/container2.png"));
            _specularMap = TextureLoader.LoadTexture(Paths.Resource("resources/textures/container2_specular.png"));
            _emissionMap = _variant == LightingVariant.EmissionMap
                ? TextureLoader.LoadTexture(Paths.Resource("resources/textures/matrix.jpg"))
                : 0;
            _lightingShader.Use();
            _lightingShader.SetInt("material.diffuse", 0);
            if (_variant != LightingVariant.DiffuseMap)
            {
                _lightingShader.SetInt("material.specular", 1);
            }
            if (_variant == LightingVariant.EmissionMap)
            {
                _lightingShader.SetInt("material.emission", 2);
            }
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
        if (_variant == LightingVariant.MultipleLightsDesert)
        {
            GL.ClearColor(0.75f, 0.52f, 0.3f, 1.0f);
        }
        else
        {
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        }
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        var projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(_camera.Zoom), AspectRatioPx, 0.1f, 100.0f);
        var view = _camera.GetViewMatrix();

        _lightingShader!.Use();
        SetLightingUniforms(frameIndex);
        _lightingShader.SetMatrix4("projection", projection);
        _lightingShader.SetMatrix4("view", view);

        if (UsesTexture)
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _diffuseMap);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, _specularMap);
            if (_emissionMap != 0)
            {
                GL.ActiveTexture(TextureUnit.Texture2);
                GL.BindTexture(TextureTarget.Texture2D, _emissionMap);
            }
        }

        var cubes = RendersManyCubes ? CubePositions.Length : 1;
        for (var i = 0; i < cubes; i++)
        {
            var model = Matrix4.CreateFromAxisAngle(new Vector3(1.0f, 0.3f, 0.5f).Normalized(), MathHelper.DegreesToRadians(20.0f * i)) *
                        Matrix4.CreateTranslation(CubePositions[i]);
            _lightingShader.SetMatrix4("model", model);
            _cube!.Draw();
        }

        if (_variant == LightingVariant.DirectionalLight)
        {
            return;
        }

        _lightCubeShader!.Use();
        _lightCubeShader.SetMatrix4("projection", projection);
        _lightCubeShader.SetMatrix4("view", view);

        var lights = _variant is LightingVariant.MultipleLights or LightingVariant.MultipleLightsDesert
            ? PointLightPositions
            : new[] { CurrentLightPosition(frameIndex) };
        foreach (var light in lights)
        {
            var model = Matrix4.CreateScale(0.2f) * Matrix4.CreateTranslation(light);
            _lightCubeShader.SetMatrix4("model", model);
            _lamp!.Draw();
        }
    }

    private void SetLightingUniforms(int frameIndex)
    {
        var shader = _lightingShader!;
        shader.SetVector3("objectColor", 1.0f, 0.5f, 0.31f);
        shader.SetVector3("lightColor", 1.0f, 1.0f, 1.0f);
        shader.SetVector3("lightPos", CurrentLightPosition(frameIndex));
        shader.SetVector3("viewPos", _camera.Position);

        switch (_variant)
        {
            case LightingVariant.Materials:
                var t = frameIndex / 60.0f;
                var lightColor = new Vector3(MathF.Sin(t * 2.0f), MathF.Sin(t * 0.7f), MathF.Sin(t * 1.3f));
                shader.SetVector3("light.position", LightPos);
                shader.SetVector3("light.ambient", lightColor * 0.2f);
                shader.SetVector3("light.diffuse", lightColor * 0.5f);
                shader.SetVector3("light.specular", 1.0f, 1.0f, 1.0f);
                shader.SetVector3("material.ambient", 1.0f, 0.5f, 0.31f);
                shader.SetVector3("material.diffuse", 1.0f, 0.5f, 0.31f);
                shader.SetVector3("material.specular", 0.5f, 0.5f, 0.5f);
                shader.SetFloat("material.shininess", 32.0f);
                break;
            case LightingVariant.MaterialsExercise1:
                shader.SetVector3("light.position", LightPos);
                shader.SetVector3("light.ambient", 1.0f, 1.0f, 1.0f);
                shader.SetVector3("light.diffuse", 1.0f, 1.0f, 1.0f);
                shader.SetVector3("light.specular", 1.0f, 1.0f, 1.0f);
                shader.SetVector3("material.ambient", 0.0f, 0.1f, 0.06f);
                shader.SetVector3("material.diffuse", 0.0f, 0.50980392f, 0.50980392f);
                shader.SetVector3("material.specular", 0.50196078f, 0.50196078f, 0.50196078f);
                shader.SetFloat("material.shininess", 32.0f);
                break;
            case LightingVariant.DiffuseMap:
            case LightingVariant.SpecularMap:
            case LightingVariant.EmissionMap:
                shader.SetVector3("light.position", LightPos);
                shader.SetVector3("light.ambient", 0.2f, 0.2f, 0.2f);
                shader.SetVector3("light.diffuse", 0.5f, 0.5f, 0.5f);
                shader.SetVector3("light.specular", 1.0f, 1.0f, 1.0f);
                shader.SetVector3("material.specular", 0.5f, 0.5f, 0.5f);
                shader.SetFloat("material.shininess", 64.0f);
                break;
            case LightingVariant.DirectionalLight:
                shader.SetVector3("light.direction", -0.2f, -1.0f, -0.3f);
                shader.SetVector3("light.ambient", 0.2f, 0.2f, 0.2f);
                shader.SetVector3("light.diffuse", 0.5f, 0.5f, 0.5f);
                shader.SetVector3("light.specular", 1.0f, 1.0f, 1.0f);
                shader.SetFloat("material.shininess", 32.0f);
                break;
            case LightingVariant.PointLight:
            case LightingVariant.SpotLight:
            case LightingVariant.SoftSpotLight:
                shader.SetVector3("light.position", _variant == LightingVariant.PointLight ? LightPos : _camera.Position);
                shader.SetVector3("light.direction", _camera.Front);
                shader.SetVector3("light.ambient", 0.1f, 0.1f, 0.1f);
                shader.SetVector3("light.diffuse", 0.8f, 0.8f, 0.8f);
                shader.SetVector3("light.specular", 1.0f, 1.0f, 1.0f);
                shader.SetFloat("light.constant", 1.0f);
                shader.SetFloat("light.linear", 0.09f);
                shader.SetFloat("light.quadratic", 0.032f);
                shader.SetFloat("light.cutOff", MathF.Cos(MathHelper.DegreesToRadians(12.5f)));
                shader.SetFloat("light.outerCutOff", MathF.Cos(MathHelper.DegreesToRadians(15.0f)));
                shader.SetFloat("material.shininess", 32.0f);
                break;
            case LightingVariant.MultipleLights:
            case LightingVariant.MultipleLightsDesert:
                SetMultipleLights(shader, desert: _variant == LightingVariant.MultipleLightsDesert);
                break;
        }
    }

    private Vector3 CurrentLightPosition(int frameIndex)
    {
        if (_variant != LightingVariant.MovingLight)
        {
            return LightPos;
        }

        var time = frameIndex / 60.0f;
        return new Vector3(1.0f + MathF.Sin(time) * 2.0f, MathF.Sin(time / 2.0f), LightPos.Z);
    }

    private void SetMultipleLights(Shader shader, bool desert)
    {
        shader.SetVector3("dirLight.direction", -0.2f, -1.0f, -0.3f);
        shader.SetVector3("dirLight.ambient", desert ? new Vector3(0.3f, 0.24f, 0.14f) : new Vector3(0.05f, 0.05f, 0.05f));
        shader.SetVector3("dirLight.diffuse", desert ? new Vector3(0.7f, 0.42f, 0.26f) : new Vector3(0.4f, 0.4f, 0.4f));
        shader.SetVector3("dirLight.specular", 0.5f, 0.5f, 0.5f);
        var pointLightColors = desert
            ? new[] { new Vector3(1.0f, 0.6f, 0.0f), new Vector3(1.0f, 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 0.0f), new Vector3(0.2f, 0.2f, 1.0f) }
            : Enumerable.Repeat(new Vector3(0.8f), PointLightPositions.Length).ToArray();
        for (var i = 0; i < PointLightPositions.Length; i++)
        {
            shader.SetVector3($"pointLights[{i}].position", PointLightPositions[i]);
            shader.SetVector3($"pointLights[{i}].ambient", desert ? pointLightColors[i] * 0.1f : new Vector3(0.05f));
            shader.SetVector3($"pointLights[{i}].diffuse", desert ? pointLightColors[i] : new Vector3(0.8f));
            shader.SetVector3($"pointLights[{i}].specular", desert ? pointLightColors[i] : Vector3.One);
            shader.SetFloat($"pointLights[{i}].constant", 1.0f);
            shader.SetFloat($"pointLights[{i}].linear", 0.09f);
            shader.SetFloat($"pointLights[{i}].quadratic", 0.032f);
        }
        shader.SetVector3("spotLight.position", _camera.Position);
        shader.SetVector3("spotLight.direction", _camera.Front);
        shader.SetVector3("spotLight.ambient", 0.0f, 0.0f, 0.0f);
        shader.SetVector3("spotLight.diffuse", desert ? new Vector3(0.8f, 0.8f, 0.0f) : Vector3.One);
        shader.SetVector3("spotLight.specular", desert ? new Vector3(0.8f, 0.8f, 0.0f) : Vector3.One);
        shader.SetFloat("spotLight.constant", 1.0f);
        shader.SetFloat("spotLight.linear", 0.09f);
        shader.SetFloat("spotLight.quadratic", 0.032f);
        shader.SetFloat("spotLight.cutOff", MathF.Cos(MathHelper.DegreesToRadians(12.5f)));
        shader.SetFloat("spotLight.outerCutOff", MathF.Cos(MathHelper.DegreesToRadians(15.0f)));
        shader.SetFloat("material.shininess", 32.0f);
    }

    private bool UsesTexture => _variant is LightingVariant.DiffuseMap or LightingVariant.SpecularMap or LightingVariant.InvertedSpecularMap or LightingVariant.EmissionMap
        or LightingVariant.DirectionalLight or LightingVariant.PointLight or LightingVariant.SpotLight or LightingVariant.SoftSpotLight
        or LightingVariant.MultipleLights or LightingVariant.MultipleLightsDesert;

    private bool RendersManyCubes => _variant is LightingVariant.DirectionalLight or LightingVariant.PointLight
        or LightingVariant.SpotLight or LightingVariant.SoftSpotLight or LightingVariant.MultipleLights or LightingVariant.MultipleLightsDesert;

    private (string Folder, string LightingVs, string LightingFs, string LightVs, string LightFs) ShaderFiles()
    {
        return _variant switch
        {
            LightingVariant.Colors => ("1.colors", "1.colors.vs", "1.colors.fs", "1.light_cube.vs", "1.light_cube.fs"),
            LightingVariant.Diffuse => ("2.1.basic_lighting_diffuse", "2.1.basic_lighting.vs", "2.1.basic_lighting.fs", "2.1.light_cube.vs", "2.1.light_cube.fs"),
            LightingVariant.Specular or LightingVariant.MovingLight => ("2.2.basic_lighting_specular", "2.2.basic_lighting.vs", "2.2.basic_lighting.fs", "2.2.light_cube.vs", "2.2.light_cube.fs"),
            LightingVariant.ViewSpaceLighting or LightingVariant.Gouraud => ("2.2.basic_lighting_specular", "2.2.basic_lighting.vs", "2.2.basic_lighting.fs", "2.2.light_cube.vs", "2.2.light_cube.fs"),
            LightingVariant.Materials => ("3.1.materials", "3.1.materials.vs", "3.1.materials.fs", "3.1.light_cube.vs", "3.1.light_cube.fs"),
            LightingVariant.MaterialsExercise1 => ("3.2.materials_exercise1", "3.2.materials.vs", "3.2.materials.fs", "3.2.light_cube.vs", "3.2.light_cube.fs"),
            LightingVariant.DiffuseMap => ("4.1.lighting_maps_diffuse_map", "4.1.lighting_maps.vs", "4.1.lighting_maps.fs", "4.1.light_cube.vs", "4.1.light_cube.fs"),
            LightingVariant.SpecularMap or LightingVariant.InvertedSpecularMap => ("4.2.lighting_maps_specular_map", "4.2.lighting_maps.vs", "4.2.lighting_maps.fs", "4.2.light_cube.vs", "4.2.light_cube.fs"),
            LightingVariant.EmissionMap => ("4.4.lighting_maps_exercise4", "4.4.lighting_maps.vs", "4.4.lighting_maps.fs", "4.4.light_cube.vs", "4.4.light_cube.fs"),
            LightingVariant.DirectionalLight => ("5.1.light_casters_directional", "5.1.light_casters.vs", "5.1.light_casters.fs", "5.1.light_cube.vs", "5.1.light_cube.fs"),
            LightingVariant.PointLight => ("5.2.light_casters_point", "5.2.light_casters.vs", "5.2.light_casters.fs", "5.2.light_cube.vs", "5.2.light_cube.fs"),
            LightingVariant.SpotLight => ("5.3.light_casters_spot", "5.3.light_casters.vs", "5.3.light_casters.fs", "5.3.light_cube.vs", "5.3.light_cube.fs"),
            LightingVariant.SoftSpotLight => ("5.4.light_casters_spot_soft", "5.4.light_casters.vs", "5.4.light_casters.fs", "5.4.light_cube.vs", "5.4.light_cube.fs"),
            LightingVariant.MultipleLights or LightingVariant.MultipleLightsDesert => ("6.multiple_lights", "6.multiple_lights.vs", "6.multiple_lights.fs", "6.light_cube.vs", "6.light_cube.fs"),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private IEnumerable<(int Index, int Size, int OffsetFloats)> Attributes()
    {
        yield return (0, 3, 0);
        if (_variant != LightingVariant.Colors)
        {
            yield return (1, 3, 3);
        }
        if (UsesTexture)
        {
            yield return (2, 2, 6);
        }
    }

    private const string InlineViewSpaceVertex = """
        #version 330 core
        layout (location = 0) in vec3 aPos;
        layout (location = 1) in vec3 aNormal;
        out vec3 FragPos;
        out vec3 Normal;
        out vec3 LightPos;
        uniform vec3 lightPos;
        uniform mat4 model;
        uniform mat4 view;
        uniform mat4 projection;
        void main()
        {
            gl_Position = projection * view * model * vec4(aPos, 1.0);
            FragPos = vec3(view * model * vec4(aPos, 1.0));
            Normal = mat3(transpose(inverse(view * model))) * aNormal;
            LightPos = vec3(view * vec4(lightPos, 1.0));
        }
        """;

    private const string InlineViewSpaceFragment = """
        #version 330 core
        out vec4 FragColor;
        in vec3 FragPos;
        in vec3 Normal;
        in vec3 LightPos;
        uniform vec3 lightColor;
        uniform vec3 objectColor;
        void main()
        {
            vec3 ambient = 0.1 * lightColor;
            vec3 norm = normalize(Normal);
            vec3 lightDir = normalize(LightPos - FragPos);
            float diff = max(dot(norm, lightDir), 0.0);
            vec3 diffuse = diff * lightColor;
            vec3 viewDir = normalize(-FragPos);
            vec3 reflectDir = reflect(-lightDir, norm);
            float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32.0);
            vec3 specular = 0.5 * spec * lightColor;
            FragColor = vec4((ambient + diffuse + specular) * objectColor, 1.0);
        }
        """;

    private const string InlineGouraudVertex = """
        #version 330 core
        layout (location = 0) in vec3 aPos;
        layout (location = 1) in vec3 aNormal;
        out vec3 LightingColor;
        uniform vec3 lightPos;
        uniform vec3 viewPos;
        uniform vec3 lightColor;
        uniform mat4 model;
        uniform mat4 view;
        uniform mat4 projection;
        void main()
        {
            gl_Position = projection * view * model * vec4(aPos, 1.0);
            vec3 Position = vec3(model * vec4(aPos, 1.0));
            vec3 Normal = mat3(transpose(inverse(model))) * aNormal;
            vec3 ambient = 0.1 * lightColor;
            vec3 norm = normalize(Normal);
            vec3 lightDir = normalize(lightPos - Position);
            float diff = max(dot(norm, lightDir), 0.0);
            vec3 diffuse = diff * lightColor;
            vec3 viewDir = normalize(viewPos - Position);
            vec3 reflectDir = reflect(-lightDir, norm);
            float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32.0);
            vec3 specular = spec * lightColor;
            LightingColor = ambient + diffuse + specular;
        }
        """;

    private const string InlineGouraudFragment = """
        #version 330 core
        out vec4 FragColor;
        in vec3 LightingColor;
        uniform vec3 objectColor;
        void main()
        {
            FragColor = vec4(LightingColor * objectColor, 1.0);
        }
        """;

    private const string InlineInvertedSpecularFragment = """
        #version 330 core
        out vec4 FragColor;
        struct Material {
            sampler2D diffuse;
            sampler2D specular;
            float shininess;
        };
        struct Light {
            vec3 position;
            vec3 ambient;
            vec3 diffuse;
            vec3 specular;
        };
        in vec3 FragPos;
        in vec3 Normal;
        in vec2 TexCoords;
        uniform vec3 viewPos;
        uniform Material material;
        uniform Light light;
        void main()
        {
            vec3 ambient = light.ambient * vec3(texture(material.diffuse, TexCoords));
            vec3 norm = normalize(Normal);
            vec3 lightDir = normalize(light.position - FragPos);
            float diff = max(dot(norm, lightDir), 0.0);
            vec3 diffuse = light.diffuse * diff * vec3(texture(material.diffuse, TexCoords));
            vec3 viewDir = normalize(viewPos - FragPos);
            vec3 reflectDir = reflect(-lightDir, norm);
            float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
            vec3 specular = light.specular * spec * (vec3(1.0) - vec3(texture(material.specular, TexCoords)));
            FragColor = vec4(ambient + diffuse + specular, 1.0);
        }
        """;

    private float[] VertexData()
    {
        if (_variant == LightingVariant.Colors)
        {
            return CubePositionsOnly;
        }
        return UsesTexture ? CubeWithNormalsAndTexCoords : CubeWithNormals;
    }

    private static readonly float[] CubePositionsOnly =
    {
        -0.5f,-0.5f,-0.5f, 0.5f,-0.5f,-0.5f, 0.5f,0.5f,-0.5f, 0.5f,0.5f,-0.5f, -0.5f,0.5f,-0.5f, -0.5f,-0.5f,-0.5f,
        -0.5f,-0.5f,0.5f, 0.5f,-0.5f,0.5f, 0.5f,0.5f,0.5f, 0.5f,0.5f,0.5f, -0.5f,0.5f,0.5f, -0.5f,-0.5f,0.5f,
        -0.5f,0.5f,0.5f, -0.5f,0.5f,-0.5f, -0.5f,-0.5f,-0.5f, -0.5f,-0.5f,-0.5f, -0.5f,-0.5f,0.5f, -0.5f,0.5f,0.5f,
        0.5f,0.5f,0.5f, 0.5f,0.5f,-0.5f, 0.5f,-0.5f,-0.5f, 0.5f,-0.5f,-0.5f, 0.5f,-0.5f,0.5f, 0.5f,0.5f,0.5f,
        -0.5f,-0.5f,-0.5f, 0.5f,-0.5f,-0.5f, 0.5f,-0.5f,0.5f, 0.5f,-0.5f,0.5f, -0.5f,-0.5f,0.5f, -0.5f,-0.5f,-0.5f,
        -0.5f,0.5f,-0.5f, 0.5f,0.5f,-0.5f, 0.5f,0.5f,0.5f, 0.5f,0.5f,0.5f, -0.5f,0.5f,0.5f, -0.5f,0.5f,-0.5f
    };

    private static readonly float[] CubeWithNormals = ExpandNormals(includeTexCoords: false);
    private static readonly float[] CubeWithNormalsAndTexCoords = ExpandNormals(includeTexCoords: true);

    private static float[] ExpandNormals(bool includeTexCoords)
    {
        var rows = new (float X, float Y, float Z, float Nx, float Ny, float Nz, float U, float V)[]
        {
            (-.5f,-.5f,-.5f,0,0,-1,0,0),(.5f,-.5f,-.5f,0,0,-1,1,0),(.5f,.5f,-.5f,0,0,-1,1,1),(.5f,.5f,-.5f,0,0,-1,1,1),(-.5f,.5f,-.5f,0,0,-1,0,1),(-.5f,-.5f,-.5f,0,0,-1,0,0),
            (-.5f,-.5f,.5f,0,0,1,0,0),(.5f,-.5f,.5f,0,0,1,1,0),(.5f,.5f,.5f,0,0,1,1,1),(.5f,.5f,.5f,0,0,1,1,1),(-.5f,.5f,.5f,0,0,1,0,1),(-.5f,-.5f,.5f,0,0,1,0,0),
            (-.5f,.5f,.5f,-1,0,0,1,0),(-.5f,.5f,-.5f,-1,0,0,1,1),(-.5f,-.5f,-.5f,-1,0,0,0,1),(-.5f,-.5f,-.5f,-1,0,0,0,1),(-.5f,-.5f,.5f,-1,0,0,0,0),(-.5f,.5f,.5f,-1,0,0,1,0),
            (.5f,.5f,.5f,1,0,0,1,0),(.5f,.5f,-.5f,1,0,0,1,1),(.5f,-.5f,-.5f,1,0,0,0,1),(.5f,-.5f,-.5f,1,0,0,0,1),(.5f,-.5f,.5f,1,0,0,0,0),(.5f,.5f,.5f,1,0,0,1,0),
            (-.5f,-.5f,-.5f,0,-1,0,0,1),(.5f,-.5f,-.5f,0,-1,0,1,1),(.5f,-.5f,.5f,0,-1,0,1,0),(.5f,-.5f,.5f,0,-1,0,1,0),(-.5f,-.5f,.5f,0,-1,0,0,0),(-.5f,-.5f,-.5f,0,-1,0,0,1),
            (-.5f,.5f,-.5f,0,1,0,0,1),(.5f,.5f,-.5f,0,1,0,1,1),(.5f,.5f,.5f,0,1,0,1,0),(.5f,.5f,.5f,0,1,0,1,0),(-.5f,.5f,.5f,0,1,0,0,0),(-.5f,.5f,-.5f,0,1,0,0,1)
        };
        var result = new List<float>(rows.Length * (includeTexCoords ? 8 : 6));
        foreach (var r in rows)
        {
            result.AddRange(new[] { r.X, r.Y, r.Z, r.Nx, r.Ny, r.Nz });
            if (includeTexCoords) result.AddRange(new[] { r.U, r.V });
        }
        return result.ToArray();
    }
}
