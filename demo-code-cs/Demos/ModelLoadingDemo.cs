using LearnOpenGL.OpenTK.Core;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace LearnOpenGL.OpenTK.Demos;

public sealed class ModelLoadingDemo : DemoWindow
{
    private readonly Camera _camera = new(new Vector3(0.0f, 0.0f, 3.0f));
    private Shader? _shader;
    private Model? _model;

    public ModelLoadingDemo(RunOptions options) : base(options)
    {
    }

    protected override void LoadDemo()
    {
        GL.Enable(EnableCap.DepthTest);
        _shader = new Shader(
            Paths.Shader("3.model_loading", "1.model_loading", "1.model_loading.vs"),
            Paths.Shader("3.model_loading", "1.model_loading", "1.model_loading.fs"));
        _model = new Model(Paths.Resource("resources/objects/backpack/backpack.obj"));
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
        GL.ClearColor(0.05f, 0.05f, 0.05f, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        _shader!.Use();
        var projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(_camera.Zoom), AspectRatioPx, 0.1f, 100.0f);
        var view = _camera.GetViewMatrix();
        var model = Matrix4.Identity;

        _shader.SetMatrix4("projection", projection);
        _shader.SetMatrix4("view", view);
        _shader.SetMatrix4("model", model);
        _model!.Draw(_shader);
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        _model?.Dispose();
        _shader?.Dispose();
    }
}
