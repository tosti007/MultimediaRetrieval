using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

namespace MultimediaRetrieval
{
    public class Game : GameWindow
    {
        private Shader _shader;

        private readonly float[,] _vertices =
        {
            { -0.5f, -0.5f, -0.5f },//LOA 0
            { -0.5f, -0.5f,  0.5f },//LOV 1
            { -0.5f,  0.5f, -0.5f },//LBA 2
            { -0.5f,  0.5f,  0.5f },//LBV 3
            {  0.5f, -0.5f, -0.5f },//ROA 4
            {  0.5f, -0.5f,  0.5f },//ROV 5
            {  0.5f,  0.5f, -0.5f },//RBA 6
            {  0.5f,  0.5f,  0.5f },//RBV 7
        };

        private readonly uint[,] _faces =
        {
            { 3, 2, 0 },
            { 0, 1, 3 },

            { 1, 5, 7 },
            { 7, 3, 1 },

            { 0, 4, 6 }, // Plane vooraan
            { 6, 2, 0 },

            { 7, 6, 4 },
            { 4, 5, 7 },

            { 0, 4, 5 },
            { 5, 1, 0 },

            { 2, 6, 7 },
            { 7, 3, 2 },
        };

        private int _vertexArrayObject; // VAO = Attribute properties
        private int _vertexBufferObject; // VBO = Data
        private int _elementBufferObject; // EBO = Indices

        private Mesh _mesh;

        private Camera _camera;

        private bool _timeDown = false;
        private bool _stepTime = true;

        public Game(int width, int height, string title, Mesh mesh, Camera camera) : base(width, height, GraphicsMode.Default, title)
        {
            _mesh = mesh;
            _camera = camera;
        }

        protected override void OnLoad(EventArgs e)
        {
            CursorVisible = false;
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
            GL.LineWidth(3f);
            GL.Enable(EnableCap.DepthTest);

            _shader = new Shader("../../shader.vert", "../../shader.frag");
            _shader.Use();
            _shader.SetMatrix4("model", _mesh.Model);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _faces.Length * sizeof(uint), _faces, BufferUsageHint.StaticDraw);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            var vertexLocation = _shader.GetAttribLocation("aPosition");
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(vertexLocation);

            base.OnLoad(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            _shader.Use();

            if (_stepTime)
            {
                _mesh.Model = _mesh.Model * Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(10.0 * e.Time));
                _shader.SetMatrix4("model", _mesh.Model);
            }

            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(_camera.FOV), Width / (float)Height, 0.1f, 100.0f);
            _shader.SetMatrix4("view", _camera.GetViewMatrix());
            _shader.SetMatrix4("projection", projection);
            
            // Bind the EBO & VBO to the VAO, so when we use the VAO it uses the same EBO & VBO
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);

            GL.BindVertexArray(_vertexArrayObject);

            _shader.SetVector3("drawColor", new Vector3(0.7f));
            GL.DrawElements(PrimitiveType.Triangles, _faces.Length, DrawElementsType.UnsignedInt, 0);

            _shader.SetVector3("drawColor", new Vector3(0f));
            GL.DrawElements(PrimitiveType.LineStrip, _faces.Length, DrawElementsType.UnsignedInt, 0);

            Context.SwapBuffers();
            base.OnRenderFrame(e);
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            base.OnResize(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (!Focused)
                return;

            KeyboardState input = Keyboard.GetState();

            if (input.IsKeyDown(Key.Escape))
                Exit();

            if (input.IsKeyUp(Key.T) && _timeDown)
                _stepTime = !_stepTime;
            _timeDown = input.IsKeyDown(Key.T);

            _camera.HandleInput(e, input);

            base.OnUpdateFrame(e);
        }

        protected override void OnUnload(EventArgs e)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            GL.DeleteBuffer(_vertexBufferObject);
            GL.DeleteBuffer(_elementBufferObject);
            GL.DeleteVertexArray(_vertexArrayObject);

            _shader.Dispose();
            base.OnUnload(e);
        }
    }
}
