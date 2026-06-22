using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace LearnOpenGL.OpenTK.Core;

public abstract class DemoWindow : GameWindow
{
    private readonly RunOptions _options;
    private int _frameIndex;

    protected DemoWindow(RunOptions options)
        : base(
            GameWindowSettings.Default,
            new NativeWindowSettings
            {
                Size = new Vector2i(options.Width, options.Height),
                Title = $"LearnOpenGL OpenTK - {options.DemoId}",
                API = ContextAPI.OpenGL,
                APIVersion = new Version(4, 3),
                Profile = ContextProfile.Core,
                Flags = ContextFlags.ForwardCompatible
            })
    {
        _options = options;
    }

    protected int WidthPx => ClientSize.X;

    protected int HeightPx => ClientSize.Y;

    protected float AspectRatioPx => WidthPx / (float)Math.Max(1, HeightPx);

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.Viewport(0, 0, WidthPx, HeightPx);
        LoadDemo();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        GL.Viewport(0, 0, e.Width, e.Height);
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        if (KeyboardState.IsKeyDown(Keys.Escape))
        {
            Close();
        }

        UpdateDemo((float)args.Time);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        RenderDemo((float)args.Time, _frameIndex);

        if (_options.CapturePath is not null && _frameIndex + 1 == Math.Max(1, _options.Frames))
        {
            FrameCapture.SavePng(_options.CapturePath, WidthPx, HeightPx);
        }

        SwapBuffers();
        _frameIndex++;

        if (_options.Frames > 0 && _frameIndex >= _options.Frames)
        {
            Close();
        }
    }

    protected virtual void LoadDemo()
    {
    }

    protected virtual void UpdateDemo(float deltaTime)
    {
    }

    protected abstract void RenderDemo(float deltaTime, int frameIndex);
}
