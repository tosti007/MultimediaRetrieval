using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;

namespace MultimediaRetrieval
{
    public class Camera
    {
        public float Speed;
        public float Sensitivity;
        public float FOV;

        public Vector3 Position = new Vector3(0.0f, 0.0f, 3.0f);
        public Vector3 Direction = -Vector3.UnitZ;

        private float _pitch;
        private float _yaw = -90f;
        private Vector3 _right = Vector3.UnitX;
        private Vector3 _up = Vector3.UnitY;

        public Camera(float speed, float sensitivity, float fov)
        {
            Speed = speed;
            Sensitivity = sensitivity;
            FOV = 45f;
        }

        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Position + Direction, _up);
        }

        public void HandleInput(FrameEventArgs e, KeyboardState input)
        {
            if (input.IsKeyDown(Key.W))
                Position += Direction * Speed * (float)e.Time; //Forward 

            if (input.IsKeyDown(Key.S))
                Position -= Direction * Speed * (float)e.Time; //Backwards

            if (input.IsKeyDown(Key.A))
                Position -= Vector3.Normalize(Vector3.Cross(Direction, _up)) * Speed * (float)e.Time; //Left

            if (input.IsKeyDown(Key.D))
                Position += Vector3.Normalize(Vector3.Cross(Direction, _up)) * Speed * (float)e.Time; //Right

            if (input.IsKeyDown(Key.Space))
                Position += _up * Speed * (float)e.Time; //Up 

            if (input.IsKeyDown(Key.LShift))
                Position -= _up * Speed * (float)e.Time; //Down

            if (input.IsKeyDown(Key.E))
                FOV += Sensitivity * (float)e.Time;

            if (input.IsKeyDown(Key.Q))
                FOV -= Sensitivity * (float)e.Time;

            if (FOV > 45.0f)
            {
                FOV = 45.0f;
            }
            if (FOV < 1.0f)
            {
                FOV = 1.0f;
            }

            if (input.IsKeyDown(Key.Left))
                _yaw -= Sensitivity * (float)e.Time;

            if (input.IsKeyDown(Key.Right))
                _yaw += Sensitivity * (float)e.Time;

            if (_yaw > 360.0f)
            {
                _yaw -= 360.0f;
            }
            if (_yaw < 0.0f)
            {
                _yaw += 360.0f;
            }

            if (input.IsKeyDown(Key.Up))
                _pitch += Sensitivity * (float)e.Time;

            if (input.IsKeyDown(Key.Down))
                _pitch -= Sensitivity * (float)e.Time;

            if (_pitch > 89.0f)
            {
                _pitch = 89.0f;
            }
            if (_pitch < -89.0f)
            {
                _pitch = -89.0f;
            }

            Direction.X = (float)Math.Cos(MathHelper.DegreesToRadians(_pitch)) * (float)Math.Cos(MathHelper.DegreesToRadians(_yaw));
            Direction.Y = (float)Math.Sin(MathHelper.DegreesToRadians(_pitch));
            Direction.Z = (float)Math.Cos(MathHelper.DegreesToRadians(_pitch)) * (float)Math.Sin(MathHelper.DegreesToRadians(_yaw));
            Direction = Vector3.Normalize(Direction);
            _right = Vector3.Normalize(Vector3.Cross(Direction, Vector3.UnitY));
            _up = Vector3.Normalize(Vector3.Cross(_right, Direction));
        }
    }
}
