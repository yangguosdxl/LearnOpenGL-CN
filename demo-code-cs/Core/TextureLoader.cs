using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace LearnOpenGL.OpenTK.Core;

public static class TextureLoader
{
    public static int LoadTexture(string path, bool flipVertically = true, bool generateMipmap = true)
    {
        using var image = Image.Load<Rgba32>(path);
        if (flipVertically)
        {
            image.MutateFlipY();
        }

        var pixels = new byte[image.Width * image.Height * 4];
        image.CopyPixelDataTo(pixels);

        var texture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, texture);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, generateMipmap ? (int)TextureMinFilter.LinearMipmapLinear : (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
        if (generateMipmap)
        {
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        return texture;
    }

    public static int LoadCubemap(IReadOnlyList<string> faces)
    {
        if (faces.Count != 6)
        {
            throw new ArgumentException("A cubemap requires exactly 6 faces.", nameof(faces));
        }

        var texture = GL.GenTexture();
        GL.BindTexture(TextureTarget.TextureCubeMap, texture);

        for (var i = 0; i < faces.Count; i++)
        {
            using var image = Image.Load<Rgb24>(faces[i]);
            var pixels = new byte[image.Width * image.Height * 3];
            image.CopyPixelDataTo(pixels);

            GL.TexImage2D(
                TextureTarget.TextureCubeMapPositiveX + i,
                0,
                PixelInternalFormat.Rgb,
                image.Width,
                image.Height,
                0,
                PixelFormat.Rgb,
                PixelType.UnsignedByte,
                pixels);
        }

        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);

        return texture;
    }

    private static void MutateFlipY(this Image<Rgba32> image)
    {
        image.ProcessPixelRows(accessor =>
        {
            var scratch = new Rgba32[accessor.Width];
            for (var y = 0; y < accessor.Height / 2; y++)
            {
                var top = accessor.GetRowSpan(y);
                var bottom = accessor.GetRowSpan(accessor.Height - y - 1);
                top.CopyTo(scratch);
                bottom.CopyTo(top);
                scratch.AsSpan(0, accessor.Width).CopyTo(bottom);
            }
        });
    }
}
