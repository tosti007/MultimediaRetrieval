﻿using System;
using System.IO;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using GLOLD = OpenTK.Graphics.OpenGL.GL;
using GL = OpenTK.Graphics.OpenGL4.GL;
using PolygonMode = OpenTK.Graphics.OpenGL4.PolygonMode;
using MaterialFace = OpenTK.Graphics.OpenGL4.MaterialFace;
using DrawElementsType = OpenTK.Graphics.OpenGL4.DrawElementsType;
using PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType;
using BufferUsageHint = OpenTK.Graphics.OpenGL4.BufferUsageHint;
using BufferTarget = OpenTK.Graphics.OpenGL4.BufferTarget;
using VertexAttribPointerType = OpenTK.Graphics.OpenGL4.VertexAttribPointerType;
using ClearBufferMask = OpenTK.Graphics.OpenGL4.ClearBufferMask;
using EnableCap = OpenTK.Graphics.OpenGL4.EnableCap;

namespace MultimediaRetrieval
{
    public class MeshViewer : GameWindow
    {
        private static Vector3 CLEAR_COLOR = new Vector3(1f);
        private static Vector3 OBJECT_COLOR = new Vector3(0.5f);
        private static Vector3 AMBIENT_COLOR = new Vector3(0.1f);
        private static Vector3 LIGHT_COLOR = new Vector3(1f);
        private const float LINE_WIDTH = 3f;

        private Mesh _mesh;
        private Camera _camera;
        private Shader _shader;

        private int _vertexArrayObject; // VAO = Attribute properties
        private int _vertexBufferObject; // VBO = Data
        private int _elementBufferObject; // EBO = Indices

        private bool _stepTime = false;
        private bool _stepTimeDown = false;

        private int _drawMode = 3;
        private bool _drawModeDown = false;

        private bool _drawAxis = true;
        private bool _drawAxisDown = false;

        private float[,] vertices;
        private uint[,] faces;


        public MeshViewer(int width, int height, string title, string meshfile)
            : this(width, height, title, Mesh.ReadMesh(meshfile), new Camera(1.5f, 30f, 45f))
        {
        }

        public MeshViewer(int width, int height, string title, Mesh mesh, Camera camera)
            : base(width, height, GraphicsMode.Default, title)
        {
            _mesh = mesh;
            _camera = camera;
        }

        protected override void OnLoad(EventArgs e)
        {
            // Setup global openGL settings
            CursorVisible = false;
            GL.ClearColor(CLEAR_COLOR.X, CLEAR_COLOR.Y, CLEAR_COLOR.Z, 1.0f);
            GL.LineWidth(LINE_WIDTH);
            GL.Enable(EnableCap.DepthTest); // Enable Z-Buffer testing

            // Setup the shader
            string basepath = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location)));
            _shader = new Shader(basepath + "/Viewer/shader.vert", basepath + "/Viewer/shader.frag");
            _shader.Use();
            _shader.SetMatrix4("model", _mesh.Model);
            RefreshCameraMatrix();
            _shader.SetVector3("ambientColor", AMBIENT_COLOR);
            _shader.SetVector3("lightColor", LIGHT_COLOR);

            // Setup vertices
            vertices = _mesh.BufferVertices();
            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            // Setup faces
            faces = _mesh.BufferFaces();
            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, faces.Length * sizeof(uint), faces, BufferUsageHint.StaticDraw);

            // Setup VAO
            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            var vertexPosition = _shader.GetAttribLocation("inPosition");
            GL.VertexAttribPointer(vertexPosition, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(vertexPosition);

            var vertexNormal = _shader.GetAttribLocation("inNormal");
            GL.VertexAttribPointer(vertexNormal, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(vertexNormal);

            base.OnLoad(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            _shader.Use();
            if (_drawAxis)
                DrawAxis();

            // Bind the mesh to the current draw
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);

            // Bind the settings
            GL.BindVertexArray(_vertexArrayObject);

            if ((_drawMode & 1) > 0)
            {
                // Draw the triangles
                _shader.SetVector3("objectColor", OBJECT_COLOR);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                GL.DrawElements(PrimitiveType.Triangles, faces.Length, DrawElementsType.UnsignedInt, 0);
            }
            if ((_drawMode & 2) > 0)
            {
                // Draw the lines           
                _shader.SetVector3("objectColor", Vector3.Zero);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                GL.DrawElements(PrimitiveType.Triangles, faces.Length, DrawElementsType.UnsignedInt, 0);
            }

            Context.SwapBuffers();
            base.OnRenderFrame(e);
        }

        protected void DrawAxis()
        {
            _shader.SetVector3("objectColor", Vector3.Zero);
            GLOLD.Begin(OpenTK.Graphics.OpenGL.PrimitiveType.Lines);
            // x aix
            GLOLD.Vertex3(-4.0, 0.0f, 0.0f);
            GLOLD.Vertex3(4.0, 0.0f, 0.0f);

            GLOLD.Vertex3(4.0, 0.0f, 0.0f);
            GLOLD.Vertex3(3.0, 1.0f, 0.0f);

            GLOLD.Vertex3(4.0, 0.0f, 0.0f);
            GLOLD.Vertex3(3.0, -1.0f, 0.0f);

            // y 
            GLOLD.Vertex3(0.0, -4.0f, 0.0f);
            GLOLD.Vertex3(0.0, 4.0f, 0.0f);

            GLOLD.Vertex3(0.0, 4.0f, 0.0f);
            GLOLD.Vertex3(1.0, 3.0f, 0.0f);

            GLOLD.Vertex3(0.0, 4.0f, 0.0f);
            GLOLD.Vertex3(-1.0, 3.0f, 0.0f);

            // z 
            GLOLD.Vertex3(0.0, 0.0f, -4.0f);
            GLOLD.Vertex3(0.0, 0.0f, 4.0f);

            GLOLD.Vertex3(0.0, 0.0f, 4.0f);
            GLOLD.Vertex3(0.0, 1.0f, 3.0f);

            GLOLD.Vertex3(0.0, 0.0f, 4.0f);
            GLOLD.Vertex3(0.0, -1.0f, 3.0f);
            GLOLD.End();
        }

        protected void RefreshCameraMatrix()
        {
            _shader.SetMatrix4("camera", _camera.GetViewMatrix(Width, Height));
            _shader.SetVector3("lightPosition", _camera.Position);
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            RefreshCameraMatrix();
            base.OnResize(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (_stepTime)
            {
                _mesh.Model = _mesh.Model * Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(10.0 * e.Time));
                _shader.SetMatrix4("model", _mesh.Model);
            }

            if (!Focused)
                return;

            KeyboardState input = Keyboard.GetState();

            if (input.IsKeyDown(Key.Escape))
                Exit();

            if (input.IsKeyUp(Key.T) && _stepTimeDown)
                _stepTime = !_stepTime;
            _stepTimeDown = input.IsKeyDown(Key.T);

            if (input.IsKeyUp(Key.Tab) && _drawModeDown)
                _drawMode = Math.Max((_drawMode + 1) & 3, 1);
            _drawModeDown = input.IsKeyDown(Key.Tab);

            if (input.IsKeyUp(Key.Z) && _drawAxisDown)
                _drawAxis = !_drawAxis;
            _drawAxisDown = input.IsKeyDown(Key.Z);

            if (_camera.HandleInput(e, input))
                RefreshCameraMatrix();

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