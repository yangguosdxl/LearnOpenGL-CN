using OpenTK.Mathematics;

namespace LearnOpenGL.OpenTK.Core;

public enum CameraMovement
{
    Forward,
    Backward,
    Left,
    Right
}

public sealed class Camera
{
    private const float DefaultYaw = -90.0f;
    private const float DefaultPitch = 0.0f;
    private const float DefaultSpeed = 2.5f;
    private const float DefaultSensitivity = 0.1f;
    private const float DefaultZoom = 45.0f;

    public Vector3 Position { get; private set; }
    public Vector3 Front { get; private set; } = -Vector3.UnitZ;
    public Vector3 Up { get; private set; }
    public Vector3 Right { get; private set; }
    public Vector3 WorldUp { get; }
    public float Yaw { get; private set; }
    public float Pitch { get; private set; }
    public float MovementSpeed { get; set; } = DefaultSpeed;
    public float MouseSensitivity { get; set; } = DefaultSensitivity;
    public float Zoom { get; private set; } = DefaultZoom;

    public Camera(Vector3 position, Vector3? up = null, float yaw = DefaultYaw, float pitch = DefaultPitch)
    {
        Position = position;
        WorldUp = up ?? Vector3.UnitY;
        Yaw = yaw;
        Pitch = pitch;
        UpdateVectors();
    }

    public Matrix4 GetViewMatrix()
    {
        return Matrix4.LookAt(Position, Position + Front, Up);
    }

    public void SetPosition(Vector3 position)
    {
        Position = position;
    }

    public void ProcessKeyboard(CameraMovement direction, float deltaTime)
    {
        var velocity = MovementSpeed * deltaTime;
        Position += direction switch
        {
            CameraMovement.Forward => Front * velocity,
            CameraMovement.Backward => -Front * velocity,
            CameraMovement.Left => -Right * velocity,
            CameraMovement.Right => Right * velocity,
            _ => Vector3.Zero
        };
    }

    public void ProcessMouseMovement(float xOffset, float yOffset, bool constrainPitch = true)
    {
        Yaw += xOffset * MouseSensitivity;
        Pitch += yOffset * MouseSensitivity;

        if (constrainPitch)
        {
            Pitch = Math.Clamp(Pitch, -89.0f, 89.0f);
        }

        UpdateVectors();
    }

    public void ProcessMouseScroll(float yOffset)
    {
        Zoom = Math.Clamp(Zoom - yOffset, 1.0f, 45.0f);
    }

    private void UpdateVectors()
    {
        var yaw = MathHelper.DegreesToRadians(Yaw);
        var pitch = MathHelper.DegreesToRadians(Pitch);
        var front = new Vector3(
            MathF.Cos(yaw) * MathF.Cos(pitch),
            MathF.Sin(pitch),
            MathF.Sin(yaw) * MathF.Cos(pitch));

        Front = Vector3.Normalize(front);
        Right = Vector3.Normalize(Vector3.Cross(Front, WorldUp));
        Up = Vector3.Normalize(Vector3.Cross(Right, Front));
    }
}
