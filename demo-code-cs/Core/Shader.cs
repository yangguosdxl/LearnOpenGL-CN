using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace LearnOpenGL.OpenTK.Core;

public sealed class Shader : IDisposable
{
    public int Handle { get; }

    public Shader(string vertexPath, string fragmentPath, string? geometryPath = null)
        : this(File.ReadAllText(vertexPath), File.ReadAllText(fragmentPath), geometryPath is null ? null : File.ReadAllText(geometryPath), true)
    {
    }

    public static Shader FromSource(string vertexSource, string fragmentSource, string? geometrySource = null)
    {
        return new Shader(vertexSource, fragmentSource, geometrySource, sourceProvided: true);
    }

    private Shader(string vertexSource, string fragmentSource, string? geometrySource, bool sourceProvided)
    {
        var vertex = Compile(ShaderType.VertexShader, vertexSource);
        var fragment = Compile(ShaderType.FragmentShader, fragmentSource);
        var geometry = geometrySource is null ? 0 : Compile(ShaderType.GeometryShader, geometrySource);

        Handle = GL.CreateProgram();
        GL.AttachShader(Handle, vertex);
        GL.AttachShader(Handle, fragment);
        if (geometry != 0)
        {
            GL.AttachShader(Handle, geometry);
        }

        GL.LinkProgram(Handle);
        GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out var success);
        if (success == 0)
        {
            throw new InvalidOperationException(GL.GetProgramInfoLog(Handle));
        }

        GL.DeleteShader(vertex);
        GL.DeleteShader(fragment);
        if (geometry != 0)
        {
            GL.DeleteShader(geometry);
        }
    }

    public void Use()
    {
        GL.UseProgram(Handle);
    }

    public void SetInt(string name, int value)
    {
        GL.Uniform1(GL.GetUniformLocation(Handle, name), value);
    }

    public void SetFloat(string name, float value)
    {
        GL.Uniform1(GL.GetUniformLocation(Handle, name), value);
    }

    public void SetVector3(string name, Vector3 value)
    {
        GL.Uniform3(GL.GetUniformLocation(Handle, name), value);
    }

    public void SetVector3(string name, float x, float y, float z)
    {
        GL.Uniform3(GL.GetUniformLocation(Handle, name), x, y, z);
    }

    public void SetMatrix4(string name, Matrix4 value)
    {
        GL.UniformMatrix4(GL.GetUniformLocation(Handle, name), false, ref value);
    }

    public void SetMatrix3(string name, Matrix3 value)
    {
        GL.UniformMatrix3(GL.GetUniformLocation(Handle, name), false, ref value);
    }

    public void Dispose()
    {
        GL.DeleteProgram(Handle);
    }

    private static int Compile(ShaderType type, string source)
    {
        var shader = GL.CreateShader(type);
        GL.ShaderSource(shader, source);
        GL.CompileShader(shader);
        GL.GetShader(shader, ShaderParameter.CompileStatus, out var success);
        if (success == 0)
        {
            throw new InvalidOperationException($"{type}: {GL.GetShaderInfoLog(shader)}");
        }

        return shader;
    }
}
