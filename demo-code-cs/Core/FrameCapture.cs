using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace LearnOpenGL.OpenTK.Core;

public static class FrameCapture
{
    public static void SavePng(string path, int width, int height)
    {
        var fullPath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        var pixels = new byte[width * height * 4];
        GL.ReadBuffer(ReadBufferMode.Back);
        GL.ReadPixels(0, 0, width, height, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

        using var image = Image.LoadPixelData<Rgba32>(pixels, width, height);
        image.MutateFlipY();
        image.SaveAsPng(fullPath);
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
