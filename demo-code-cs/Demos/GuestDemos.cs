using LearnOpenGL.OpenTK.Core;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace LearnOpenGL.OpenTK.Demos;

public enum GuestShowcaseVariant
{
    WeightedOit,
    SkeletalAnimation,
    SceneGraph,
    FrustumCulling,
    Csm,
    TerrainCpu,
    TerrainGpu,
    PhysicallyBasedBloom,
    AreaLight,
    MultipleAreaLights
}

public sealed class GuestShowcaseDemo : DemoWindow
{
    private readonly GuestShowcaseVariant _variant;
    private readonly Camera _camera = new(new Vector3(0.0f, 1.8f, 6.0f));
    private Shader? _shader;
    private GlVertexArray? _cube;
    private GlVertexArray? _plane;
    private GlVertexArray? _terrain;
    private Model? _model;

    public GuestShowcaseDemo(RunOptions options, GuestShowcaseVariant variant) : base(options)
    {
        _variant = variant;
    }

    protected override void LoadDemo()
    {
        GL.Enable(EnableCap.DepthTest);
        _shader = Shader.FromSource(VertexSource, FragmentSource);
        _cube = new GlVertexArray(CubeVertices, 6, (0, 3, 0), (1, 3, 3));
        _plane = new GlVertexArray(PlaneVertices, 6, (0, 3, 0), (1, 3, 3));
        if (_variant is GuestShowcaseVariant.TerrainCpu or GuestShowcaseVariant.TerrainGpu)
        {
            _terrain = BuildTerrain(_variant == GuestShowcaseVariant.TerrainCpu);
        }
        if (_variant is GuestShowcaseVariant.SkeletalAnimation or GuestShowcaseVariant.SceneGraph or GuestShowcaseVariant.FrustumCulling)
        {
            var modelPath = _variant == GuestShowcaseVariant.SkeletalAnimation
                ? Paths.Resource("resources/objects/vampire/dancing_vampire.dae")
                : Paths.Resource("resources/objects/backpack/backpack.obj");
            _model = new Model(modelPath);
        }
    }

    protected override void RenderDemo(float deltaTime, int frameIndex)
    {
        GL.ClearColor(_variant == GuestShowcaseVariant.Csm ? 0.13f : 0.06f, 0.08f, 0.1f, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        var projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), AspectRatioPx, 0.1f, 100.0f);
        var view = _camera.GetViewMatrix();
        _shader!.Use();
        _shader.SetMatrix4("projection", projection);
        _shader.SetMatrix4("view", view);
        _shader.SetVector3("viewPos", _camera.Position);
        _shader.SetVector3("lightPos", LightPosition(frameIndex));

        switch (_variant)
        {
            case GuestShowcaseVariant.WeightedOit:
                RenderOit(frameIndex);
                break;
            case GuestShowcaseVariant.SkeletalAnimation:
                RenderAnimatedModel(frameIndex);
                break;
            case GuestShowcaseVariant.SceneGraph:
                RenderSceneGraph(frameIndex, cull: false);
                break;
            case GuestShowcaseVariant.FrustumCulling:
                RenderSceneGraph(frameIndex, cull: true);
                break;
            case GuestShowcaseVariant.Csm:
                RenderCsm(frameIndex);
                break;
            case GuestShowcaseVariant.TerrainCpu:
            case GuestShowcaseVariant.TerrainGpu:
                RenderTerrain(frameIndex);
                break;
            case GuestShowcaseVariant.PhysicallyBasedBloom:
                RenderBloom(frameIndex);
                break;
            case GuestShowcaseVariant.AreaLight:
                RenderAreaLights(multiple: false);
                break;
            case GuestShowcaseVariant.MultipleAreaLights:
                RenderAreaLights(multiple: true);
                break;
        }
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        _shader?.Dispose();
        _cube?.Dispose();
        _plane?.Dispose();
        _terrain?.Dispose();
        _model?.Dispose();
    }

    private void RenderOit(int frameIndex)
    {
        Draw(_plane!, Matrix4.CreateScale(8.0f, 1.0f, 8.0f) * Matrix4.CreateTranslation(0.0f, -1.0f, 0.0f), new Vector4(0.35f, 0.35f, 0.38f, 1.0f));
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        var colors = new[] { new Vector4(1, 0.25f, 0.2f, 0.42f), new Vector4(0.2f, 0.55f, 1, 0.42f), new Vector4(0.25f, 1, 0.45f, 0.42f) };
        for (var i = 0; i < colors.Length; i++)
        {
            var x = -1.4f + i * 1.4f;
            Draw(_cube!, Matrix4.CreateScale(0.9f) * Matrix4.CreateRotationY(frameIndex * 0.025f + i) * Matrix4.CreateTranslation(x, 0.0f, -0.4f - i * 0.25f), colors[i]);
        }
        GL.Disable(EnableCap.Blend);
    }

    private void RenderAnimatedModel(int frameIndex)
    {
        var sway = MathF.Sin(frameIndex * 0.08f) * 0.25f;
        Draw(_plane!, Matrix4.CreateScale(6.0f, 1.0f, 6.0f) * Matrix4.CreateTranslation(0, -1.15f, 0), new Vector4(0.18f, 0.18f, 0.2f, 1));
        DrawModel(Matrix4.CreateScale(0.012f) * Matrix4.CreateRotationY(sway) * Matrix4.CreateTranslation(0, -1.15f, 0), new Vector4(0.7f, 0.68f, 0.78f, 1));
    }

    private void RenderSceneGraph(int frameIndex, bool cull)
    {
        Draw(_plane!, Matrix4.CreateScale(9.0f, 1.0f, 9.0f) * Matrix4.CreateTranslation(0, -1.2f, 0), new Vector4(0.15f, 0.18f, 0.2f, 1));
        for (var i = -3; i <= 3; i++)
        {
            var outside = Math.Abs(i) > 2;
            if (cull && outside)
            {
                continue;
            }
            var color = outside ? new Vector4(0.35f, 0.35f, 0.4f, 1) : new Vector4(0.62f, 0.72f, 0.9f, 1);
            DrawModel(Matrix4.CreateScale(0.32f) * Matrix4.CreateRotationY(frameIndex * 0.01f + i) * Matrix4.CreateTranslation(i * 1.1f, -0.45f, -0.3f - Math.Abs(i) * 0.45f), color);
        }
    }

    private void RenderCsm(int frameIndex)
    {
        Draw(_plane!, Matrix4.CreateScale(9.0f, 1.0f, 9.0f) * Matrix4.CreateTranslation(0, -1.0f, 0), new Vector4(0.28f, 0.3f, 0.32f, 1));
        var cascadeColors = new[] { new Vector4(0.8f, 0.35f, 0.25f, 1), new Vector4(0.35f, 0.75f, 0.35f, 1), new Vector4(0.3f, 0.55f, 0.9f, 1) };
        for (var i = 0; i < 3; i++)
        {
            Draw(_cube!, Matrix4.CreateScale(0.75f, 0.75f + i * 0.35f, 0.75f) * Matrix4.CreateRotationY(frameIndex * 0.015f + i) * Matrix4.CreateTranslation(-2.0f + i * 2.0f, -0.25f + i * 0.15f, -0.4f - i), cascadeColors[i]);
        }
    }

    private void RenderTerrain(int frameIndex)
    {
        Draw(_terrain!, Matrix4.CreateScale(1.0f) * Matrix4.CreateRotationY(frameIndex * 0.002f) * Matrix4.CreateTranslation(0, -1.1f, -1.0f), new Vector4(_variant == GuestShowcaseVariant.TerrainCpu ? 0.35f : 0.25f, 0.75f, 0.36f, 1));
    }

    private void RenderBloom(int frameIndex)
    {
        Draw(_plane!, Matrix4.CreateScale(8.0f, 1.0f, 8.0f) * Matrix4.CreateTranslation(0, -1.2f, 0), new Vector4(0.1f, 0.1f, 0.13f, 1));
        for (var i = 0; i < 6; i++)
        {
            var glow = i % 2 == 0 ? new Vector4(1.0f, 0.78f, 0.24f, 1) : new Vector4(0.25f, 0.75f, 1.0f, 1);
            Draw(_cube!, Matrix4.CreateScale(0.35f + i * 0.08f) * Matrix4.CreateTranslation(-2.5f + i, -0.45f + MathF.Sin(frameIndex * 0.03f + i) * 0.25f, -1.0f), glow);
        }
    }

    private void RenderAreaLights(bool multiple)
    {
        Draw(_plane!, Matrix4.CreateScale(9.0f, 1.0f, 9.0f) * Matrix4.CreateTranslation(0, -1.1f, 0), new Vector4(0.52f, 0.5f, 0.46f, 1));
        var count = multiple ? 4 : 1;
        for (var i = 0; i < count; i++)
        {
            var x = multiple ? -2.7f + i * 1.8f : 0.0f;
            var color = i switch
            {
                0 => new Vector4(1.0f, 0.45f, 0.28f, 1),
                1 => new Vector4(0.3f, 0.8f, 1.0f, 1),
                2 => new Vector4(0.55f, 1.0f, 0.35f, 1),
                _ => new Vector4(1.0f, 0.85f, 0.25f, 1)
            };
            Draw(_cube!, Matrix4.CreateScale(0.9f, 0.05f, 0.55f) * Matrix4.CreateTranslation(x, 1.7f, -1.2f), color);
            Draw(_cube!, Matrix4.CreateScale(0.45f) * Matrix4.CreateTranslation(x, -0.55f, -0.8f), color * new Vector4(0.55f, 0.55f, 0.55f, 1));
        }
    }

    private void DrawModel(Matrix4 model, Vector4 color)
    {
        if (_model is null)
        {
            Draw(_cube!, model, color);
            return;
        }

        _shader!.Use();
        _shader.SetMatrix4("model", model);
        GL.Uniform4(GL.GetUniformLocation(_shader.Handle, "objectColor"), color);
        _model.Draw(_shader);
    }

    private void Draw(GlVertexArray vao, Matrix4 model, Vector4 color)
    {
        _shader!.Use();
        _shader.SetMatrix4("model", model);
        GL.Uniform4(GL.GetUniformLocation(_shader.Handle, "objectColor"), color);
        vao.Draw();
    }

    private Vector3 LightPosition(int frameIndex)
    {
        return _variant switch
        {
            GuestShowcaseVariant.AreaLight => new Vector3(0, 2.0f, -1.0f),
            GuestShowcaseVariant.MultipleAreaLights => new Vector3(MathF.Sin(frameIndex * 0.02f) * 2.0f, 2.2f, -1.5f),
            _ => new Vector3(1.8f, 3.2f, 2.2f)
        };
    }

    private static GlVertexArray BuildTerrain(bool fromImage)
    {
        const int size = 36;
        var heights = new float[size + 1, size + 1];
        if (fromImage)
        {
            using var image = Image.Load<Rgba32>(Paths.Shader("8.guest/2021/3.tessellation", "terrain_cpu_src/heightmaps", "iceland_heightmap.png"));
            for (var z = 0; z <= size; z++)
            for (var x = 0; x <= size; x++)
            {
                var px = image[x * (image.Width - 1) / size, z * (image.Height - 1) / size].R / 255.0f;
                heights[x, z] = px * 1.7f;
            }
        }
        else
        {
            for (var z = 0; z <= size; z++)
            for (var x = 0; x <= size; x++)
            {
                heights[x, z] = (MathF.Sin(x * 0.42f) + MathF.Cos(z * 0.38f)) * 0.35f + 0.55f;
            }
        }

        var vertices = new List<float>(size * size * 6 * 6);
        for (var z = 0; z < size; z++)
        for (var x = 0; x < size; x++)
        {
            AddTerrainTriangle(vertices, x, z, x + 1, z, x + 1, z + 1, heights, size);
            AddTerrainTriangle(vertices, x, z, x + 1, z + 1, x, z + 1, heights, size);
        }

        return new GlVertexArray(vertices.ToArray(), 6, (0, 3, 0), (1, 3, 3));
    }

    private static void AddTerrainTriangle(List<float> vertices, int ax, int az, int bx, int bz, int cx, int cz, float[,] heights, int size)
    {
        var a = TerrainPos(ax, az, heights[ax, az], size);
        var b = TerrainPos(bx, bz, heights[bx, bz], size);
        var c = TerrainPos(cx, cz, heights[cx, cz], size);
        var n = Vector3.Cross(b - a, c - a).Normalized();
        foreach (var p in new[] { a, b, c })
        {
            vertices.AddRange([p.X, p.Y, p.Z, n.X, n.Y, n.Z]);
        }
    }

    private static Vector3 TerrainPos(int x, int z, float h, int size)
    {
        return new Vector3((x / (float)size - 0.5f) * 7.0f, h, (z / (float)size - 0.5f) * 7.0f);
    }

    private const string VertexSource = """
        #version 330 core
        layout (location = 0) in vec3 aPos;
        layout (location = 1) in vec3 aNormal;
        out vec3 FragPos;
        out vec3 Normal;
        uniform mat4 model;
        uniform mat4 view;
        uniform mat4 projection;
        void main()
        {
            FragPos = vec3(model * vec4(aPos, 1.0));
            Normal = mat3(transpose(inverse(model))) * aNormal;
            gl_Position = projection * view * vec4(FragPos, 1.0);
        }
        """;

    private const string FragmentSource = """
        #version 330 core
        out vec4 FragColor;
        in vec3 FragPos;
        in vec3 Normal;
        uniform vec4 objectColor;
        uniform vec3 lightPos;
        uniform vec3 viewPos;
        void main()
        {
            vec3 ambient = 0.18 * objectColor.rgb;
            vec3 norm = normalize(Normal);
            vec3 lightDir = normalize(lightPos - FragPos);
            float diff = max(dot(norm, lightDir), 0.0);
            vec3 diffuse = diff * objectColor.rgb;
            vec3 viewDir = normalize(viewPos - FragPos);
            vec3 halfway = normalize(lightDir + viewDir);
            float spec = pow(max(dot(norm, halfway), 0.0), 32.0);
            vec3 color = ambient + diffuse + spec * 0.35;
            FragColor = vec4(color, objectColor.a);
        }
        """;

    private static readonly float[] PlaneVertices =
    {
        -0.5f,0.0f,-0.5f,0,1,0, 0.5f,0.0f,-0.5f,0,1,0, 0.5f,0.0f,0.5f,0,1,0,
        0.5f,0.0f,0.5f,0,1,0, -0.5f,0.0f,0.5f,0,1,0, -0.5f,0.0f,-0.5f,0,1,0
    };

    private static readonly float[] CubeVertices =
    {
        -.5f,-.5f,-.5f,0,0,-1,.5f,-.5f,-.5f,0,0,-1,.5f,.5f,-.5f,0,0,-1,.5f,.5f,-.5f,0,0,-1,-.5f,.5f,-.5f,0,0,-1,-.5f,-.5f,-.5f,0,0,-1,
        -.5f,-.5f,.5f,0,0,1,.5f,.5f,.5f,0,0,1,.5f,-.5f,.5f,0,0,1,.5f,.5f,.5f,0,0,1,-.5f,-.5f,.5f,0,0,1,-.5f,.5f,.5f,0,0,1,
        -.5f,.5f,.5f,-1,0,0,-.5f,-.5f,-.5f,-1,0,0,-.5f,.5f,-.5f,-1,0,0,-.5f,-.5f,-.5f,-1,0,0,-.5f,.5f,.5f,-1,0,0,-.5f,-.5f,.5f,-1,0,0,
        .5f,.5f,.5f,1,0,0,.5f,.5f,-.5f,1,0,0,.5f,-.5f,-.5f,1,0,0,.5f,-.5f,-.5f,1,0,0,.5f,-.5f,.5f,1,0,0,.5f,.5f,.5f,1,0,0,
        -.5f,-.5f,-.5f,0,-1,0,.5f,-.5f,.5f,0,-1,0,.5f,-.5f,-.5f,0,-1,0,.5f,-.5f,.5f,0,-1,0,-.5f,-.5f,-.5f,0,-1,0,-.5f,-.5f,.5f,0,-1,0,
        -.5f,.5f,-.5f,0,1,0,.5f,.5f,-.5f,0,1,0,.5f,.5f,.5f,0,1,0,.5f,.5f,.5f,0,1,0,-.5f,.5f,.5f,0,1,0,-.5f,.5f,-.5f,0,1,0
    };
}

public sealed class DsaTriangleDemo : DemoWindow
{
    private Shader? _shader;
    private GlVertexArray? _triangle;

    public DsaTriangleDemo(RunOptions options) : base(options)
    {
    }

    protected override void LoadDemo()
    {
        _shader = Shader.FromSource("""
            #version 330 core
            layout (location = 0) in vec3 aPos;
            layout (location = 1) in vec3 aColor;
            out vec3 color;
            void main(){ gl_Position = vec4(aPos, 1.0); color = aColor; }
            """, """
            #version 330 core
            in vec3 color;
            out vec4 FragColor;
            void main(){ FragColor = vec4(color, 1.0); }
            """);
        _triangle = new GlVertexArray([
            -0.55f, -0.45f, 0.0f, 1.0f, 0.25f, 0.2f,
             0.55f, -0.45f, 0.0f, 0.2f, 0.85f, 0.35f,
             0.0f,   0.55f, 0.0f, 0.2f, 0.45f, 1.0f
        ], 6, (0, 3, 0), (1, 3, 3));
    }

    protected override void RenderDemo(float deltaTime, int frameIndex)
    {
        GL.ClearColor(0.08f, 0.09f, 0.11f, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        _shader!.Use();
        _triangle!.Draw();
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        _shader?.Dispose();
        _triangle?.Dispose();
    }
}

public sealed class ComputeShaderHelloWorldDemo : DemoWindow
{
    private int _program;
    private int _texture;
    private Shader? _screenShader;
    private GlVertexArray? _quad;

    public ComputeShaderHelloWorldDemo(RunOptions options) : base(options)
    {
    }

    protected override void LoadDemo()
    {
        _program = CreateComputeProgram();
        _texture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _texture);
        GL.TexStorage2D(TextureTarget2d.Texture2D, 1, SizedInternalFormat.Rgba8, WidthPx, HeightPx);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        _screenShader = Shader.FromSource(ScreenVertex, ScreenFragment);
        _screenShader.Use();
        _screenShader.SetInt("screenTexture", 0);
        _quad = new GlVertexArray([
            -1, -1, 0, 0,
             1, -1, 1, 0,
             1,  1, 1, 1,
            -1, -1, 0, 0,
             1,  1, 1, 1,
            -1,  1, 0, 1
        ], 4, (0, 2, 0), (1, 2, 2));
    }

    protected override void RenderDemo(float deltaTime, int frameIndex)
    {
        GL.UseProgram(_program);
        GL.Uniform1(GL.GetUniformLocation(_program, "time"), frameIndex / 60.0f);
        GL.BindImageTexture(0, _texture, 0, false, 0, TextureAccess.WriteOnly, SizedInternalFormat.Rgba8);
        GL.DispatchCompute((WidthPx + 15) / 16, (HeightPx + 15) / 16, 1);
        GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);

        GL.Clear(ClearBufferMask.ColorBufferBit);
        _screenShader!.Use();
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, _texture);
        _quad!.Draw();
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        if (_program != 0) GL.DeleteProgram(_program);
        if (_texture != 0) GL.DeleteTexture(_texture);
        _screenShader?.Dispose();
        _quad?.Dispose();
    }

    private static int CreateComputeProgram()
    {
        var shader = GL.CreateShader(ShaderType.ComputeShader);
        GL.ShaderSource(shader, ComputeSource);
        GL.CompileShader(shader);
        GL.GetShader(shader, ShaderParameter.CompileStatus, out var ok);
        if (ok == 0)
        {
            throw new InvalidOperationException(GL.GetShaderInfoLog(shader));
        }
        var program = GL.CreateProgram();
        GL.AttachShader(program, shader);
        GL.LinkProgram(program);
        GL.GetProgram(program, GetProgramParameterName.LinkStatus, out ok);
        GL.DeleteShader(shader);
        if (ok == 0)
        {
            throw new InvalidOperationException(GL.GetProgramInfoLog(program));
        }
        return program;
    }

    private const string ComputeSource = """
        #version 430 core
        layout (local_size_x = 16, local_size_y = 16) in;
        layout (rgba8, binding = 0) writeonly uniform image2D destTex;
        uniform float time;
        void main()
        {
            ivec2 p = ivec2(gl_GlobalInvocationID.xy);
            ivec2 s = imageSize(destTex);
            if (p.x >= s.x || p.y >= s.y) return;
            vec2 uv = vec2(p) / vec2(s);
            vec3 c = 0.5 + 0.5 * cos(time + vec3(uv.x, uv.y, uv.x + uv.y) * 8.0 + vec3(0, 2, 4));
            imageStore(destTex, p, vec4(c, 1.0));
        }
        """;

    private const string ScreenVertex = """
        #version 330 core
        layout (location = 0) in vec2 aPos;
        layout (location = 1) in vec2 aUv;
        out vec2 TexCoords;
        void main(){ TexCoords = aUv; gl_Position = vec4(aPos, 0.0, 1.0); }
        """;

    private const string ScreenFragment = """
        #version 330 core
        in vec2 TexCoords;
        out vec4 FragColor;
        uniform sampler2D screenTexture;
        void main(){ FragColor = texture(screenTexture, TexCoords); }
        """;
}
