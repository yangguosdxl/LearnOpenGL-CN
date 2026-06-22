using OpenTK.Graphics.OpenGL4;

namespace LearnOpenGL.OpenTK.Core;

public sealed class GlVertexArray : IDisposable
{
    private readonly int _vbo;
    private readonly int _ebo;

    public int Handle { get; }
    public int ElementCount { get; }
    public int VertexCount { get; }
    public bool HasElements => _ebo != 0;

    public GlVertexArray(float[] vertices, int strideFloats, params (int Index, int Size, int OffsetFloats)[] attributes)
        : this(vertices, strideFloats, null, attributes)
    {
    }

    public GlVertexArray(float[] vertices, int strideFloats, uint[]? indices, params (int Index, int Size, int OffsetFloats)[] attributes)
    {
        VertexCount = vertices.Length / strideFloats;
        ElementCount = indices?.Length ?? 0;

        Handle = GL.GenVertexArray();
        _vbo = GL.GenBuffer();
        _ebo = indices is null ? 0 : GL.GenBuffer();

        GL.BindVertexArray(Handle);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        if (indices is not null)
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);
        }

        foreach (var attribute in attributes)
        {
            GL.VertexAttribPointer(attribute.Index, attribute.Size, VertexAttribPointerType.Float, false, strideFloats * sizeof(float), attribute.OffsetFloats * sizeof(float));
            GL.EnableVertexAttribArray(attribute.Index);
        }

        GL.BindVertexArray(0);
    }

    public void Bind()
    {
        GL.BindVertexArray(Handle);
    }

    public void Draw(PrimitiveType primitiveType = PrimitiveType.Triangles)
    {
        Bind();
        if (HasElements)
        {
            GL.DrawElements(primitiveType, ElementCount, DrawElementsType.UnsignedInt, 0);
        }
        else
        {
            GL.DrawArrays(primitiveType, 0, VertexCount);
        }
    }

    public void Dispose()
    {
        GL.DeleteVertexArray(Handle);
        GL.DeleteBuffer(_vbo);
        if (_ebo != 0)
        {
            GL.DeleteBuffer(_ebo);
        }
    }
}
