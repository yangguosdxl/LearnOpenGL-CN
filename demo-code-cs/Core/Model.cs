using Assimp;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace LearnOpenGL.OpenTK.Core;

public sealed class Model : IDisposable
{
    private readonly List<Mesh> _meshes = [];
    private readonly Dictionary<string, ModelTexture> _loadedTextures = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _directory;

    public Model(string path)
    {
        _directory = Path.GetDirectoryName(path) ?? AppContext.BaseDirectory;
        using var importer = new AssimpContext();
        var scene = importer.ImportFile(path,
            PostProcessSteps.Triangulate |
            PostProcessSteps.GenerateSmoothNormals |
            PostProcessSteps.FlipUVs |
            PostProcessSteps.CalculateTangentSpace);

        if (scene.RootNode is null)
        {
            throw new InvalidOperationException($"Assimp failed to load model: {path}");
        }

        ProcessNode(scene.RootNode, scene);
    }

    public ModelStats Stats => new(
        _meshes.Count,
        _meshes.Sum(mesh => mesh.VertexCount),
        _meshes.Sum(mesh => mesh.IndexCount),
        _loadedTextures.Count);

    public void Draw(Shader shader)
    {
        foreach (var mesh in _meshes)
        {
            mesh.Draw(shader);
        }
    }

    public void SetInstanceMatrices(IReadOnlyList<Matrix4> modelMatrices)
    {
        foreach (var mesh in _meshes)
        {
            mesh.SetInstanceMatrices(modelMatrices);
        }
    }

    public void DrawInstanced(Shader shader, int amount)
    {
        foreach (var mesh in _meshes)
        {
            mesh.DrawInstanced(shader, amount);
        }
    }

    public void Dispose()
    {
        foreach (var mesh in _meshes)
        {
            mesh.Dispose();
        }

        foreach (var texture in _loadedTextures.Values)
        {
            GL.DeleteTexture(texture.Id);
        }
    }

    private void ProcessNode(Node node, Scene scene)
    {
        foreach (var meshIndex in node.MeshIndices)
        {
            _meshes.Add(ProcessMesh(scene.Meshes[meshIndex], scene));
        }

        foreach (var child in node.Children)
        {
            ProcessNode(child, scene);
        }
    }

    private Mesh ProcessMesh(Assimp.Mesh mesh, Scene scene)
    {
        var vertices = new List<ModelVertex>(mesh.VertexCount);
        for (var i = 0; i < mesh.VertexCount; i++)
        {
            var position = mesh.Vertices[i];
            var normal = mesh.HasNormals ? mesh.Normals[i] : new Vector3D();
            var texCoord = mesh.TextureCoordinateChannelCount > 0 && mesh.TextureCoordinateChannels[0].Count > i
                ? mesh.TextureCoordinateChannels[0][i]
                : new Vector3D();

            vertices.Add(new ModelVertex(
                new Vector3(position.X, position.Y, position.Z),
                new Vector3(normal.X, normal.Y, normal.Z),
                new Vector2(texCoord.X, texCoord.Y)));
        }

        var indices = new List<uint>();
        foreach (var face in mesh.Faces)
        {
            foreach (var index in face.Indices)
            {
                indices.Add((uint)index);
            }
        }

        var textures = new List<ModelTexture>();
        if (mesh.MaterialIndex >= 0 && mesh.MaterialIndex < scene.Materials.Count)
        {
            var material = scene.Materials[mesh.MaterialIndex];
            textures.AddRange(LoadMaterialTextures(material, TextureType.Diffuse, "texture_diffuse"));
            textures.AddRange(LoadMaterialTextures(material, TextureType.Specular, "texture_specular"));
            textures.AddRange(LoadMaterialTextures(material, TextureType.Height, "texture_normal"));
            textures.AddRange(LoadMaterialTextures(material, TextureType.Ambient, "texture_height"));
        }

        return new Mesh(vertices, indices, textures);
    }

    private IEnumerable<ModelTexture> LoadMaterialTextures(Material material, TextureType type, string typeName)
    {
        var textures = new List<ModelTexture>();
        foreach (var slot in material.GetMaterialTextures(type))
        {
            var relativePath = slot.FilePath.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
            if (_loadedTextures.TryGetValue(relativePath, out var existing))
            {
                textures.Add(existing);
                continue;
            }

            var fullPath = Path.Combine(_directory, relativePath);
            var id = TextureLoader.LoadTexture(fullPath);
            var texture = new ModelTexture(id, typeName, relativePath);
            _loadedTextures[relativePath] = texture;
            textures.Add(texture);
        }

        return textures;
    }
}

public readonly record struct ModelStats(int MeshCount, int VertexCount, int IndexCount, int TextureCount);

internal sealed class Mesh : IDisposable
{
    private readonly int _vao;
    private readonly int _vbo;
    private readonly int _ebo;
    private readonly List<ModelTexture> _textures;
    private int _instanceVbo;

    public Mesh(IReadOnlyList<ModelVertex> vertices, IReadOnlyList<uint> indices, List<ModelTexture> textures)
    {
        VertexCount = vertices.Count;
        IndexCount = indices.Count;
        _textures = textures;

        var vertexData = new float[vertices.Count * 8];
        for (var i = 0; i < vertices.Count; i++)
        {
            var offset = i * 8;
            vertexData[offset + 0] = vertices[i].Position.X;
            vertexData[offset + 1] = vertices[i].Position.Y;
            vertexData[offset + 2] = vertices[i].Position.Z;
            vertexData[offset + 3] = vertices[i].Normal.X;
            vertexData[offset + 4] = vertices[i].Normal.Y;
            vertexData[offset + 5] = vertices[i].Normal.Z;
            vertexData[offset + 6] = vertices[i].TexCoords.X;
            vertexData[offset + 7] = vertices[i].TexCoords.Y;
        }

        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();
        _ebo = GL.GenBuffer();

        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertexData.Length * sizeof(float), vertexData, BufferUsageHint.StaticDraw);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Count * sizeof(uint), indices.ToArray(), BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
        GL.EnableVertexAttribArray(2);
        GL.BindVertexArray(0);
    }

    public int VertexCount { get; }

    public int IndexCount { get; }

    public void Draw(Shader shader)
    {
        BindTextures(shader);

        GL.BindVertexArray(_vao);
        GL.DrawElements(global::OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, IndexCount, DrawElementsType.UnsignedInt, 0);
        GL.BindVertexArray(0);
        GL.ActiveTexture(TextureUnit.Texture0);
    }

    public void SetInstanceMatrices(IReadOnlyList<Matrix4> modelMatrices)
    {
        _instanceVbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _instanceVbo);
        GL.BufferData(BufferTarget.ArrayBuffer, modelMatrices.Count * 64, modelMatrices.ToArray(), BufferUsageHint.StaticDraw);

        GL.BindVertexArray(_vao);
        for (var i = 0; i < 4; i++)
        {
            var index = 3 + i;
            GL.EnableVertexAttribArray(index);
            GL.VertexAttribPointer(index, 4, VertexAttribPointerType.Float, false, 64, i * 16);
            GL.VertexAttribDivisor(index, 1);
        }
        GL.BindVertexArray(0);
    }

    public void DrawInstanced(Shader shader, int amount)
    {
        BindTextures(shader);

        GL.BindVertexArray(_vao);
        GL.DrawElementsInstanced(global::OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, IndexCount, DrawElementsType.UnsignedInt, IntPtr.Zero, amount);
        GL.BindVertexArray(0);
        GL.ActiveTexture(TextureUnit.Texture0);
    }

    private void BindTextures(Shader shader)
    {
        var diffuseNr = 1;
        var specularNr = 1;
        var normalNr = 1;
        var heightNr = 1;

        for (var i = 0; i < _textures.Count; i++)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + i);
            var texture = _textures[i];
            var number = texture.Type switch
            {
                "texture_diffuse" => diffuseNr++,
                "texture_specular" => specularNr++,
                "texture_normal" => normalNr++,
                "texture_height" => heightNr++,
                _ => 1
            };
            shader.SetInt(texture.Type + number, i);
            GL.BindTexture(TextureTarget.Texture2D, texture.Id);
        }
    }

    public void Dispose()
    {
        GL.DeleteVertexArray(_vao);
        GL.DeleteBuffer(_vbo);
        GL.DeleteBuffer(_ebo);
        if (_instanceVbo != 0)
        {
            GL.DeleteBuffer(_instanceVbo);
        }
    }
}

internal readonly record struct ModelVertex(Vector3 Position, Vector3 Normal, Vector2 TexCoords);

internal readonly record struct ModelTexture(int Id, string Type, string Path);
