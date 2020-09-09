using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace MultimediaRetrieval
{
    public class Shader: IDisposable
    {
        private int _handle;
        private readonly Dictionary<string, int> _uniformLocations;

        public Shader(string vertexPath, string fragmentPath)
        {
            var vertexShader = Compile(vertexPath, ShaderType.VertexShader);
            var fragmentShader = Compile(fragmentPath, ShaderType.FragmentShader);

            _handle = GL.CreateProgram();
            GL.AttachShader(_handle, vertexShader);
            GL.AttachShader(_handle, fragmentShader);
            GL.LinkProgram(_handle);

            GL.DetachShader(_handle, vertexShader);
            GL.DetachShader(_handle, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);


            // Querying this from the shader is very slow, hence we cache it
            GL.GetProgram(_handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);
            _uniformLocations = new Dictionary<string, int>();
            for (var i = 0; i < numberOfUniforms; i++)
            {
                var key = GL.GetActiveUniform(_handle, i, out _, out _);
                var location = GL.GetUniformLocation(_handle, key);
                _uniformLocations.Add(key, location);
            }
        }

        private int Compile(string path, ShaderType type)
        {
            string source;

            using (StreamReader reader = new StreamReader(path, Encoding.UTF8))
            {
                source = reader.ReadToEnd();
            }

            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);

            GL.CompileShader(shader);

            string infoLogVert = GL.GetShaderInfoLog(shader);
            if (infoLogVert != System.String.Empty)
                System.Console.WriteLine(infoLogVert);

            return(shader);
        }

        public void Use()
        {
            GL.UseProgram(_handle);
        }

        public int GetAttribLocation(string attribName)
        {
            return GL.GetAttribLocation(_handle, attribName);
        }

        public void SetMatrix4(string name, Matrix4 data)
        {
            GL.UseProgram(_handle);
            GL.UniformMatrix4(_uniformLocations[name], true, ref data);
        }

        public void SetVector3(string name, Vector3 data)
        {
            GL.UseProgram(_handle);
            GL.Uniform3(_uniformLocations[name], data);
        }

        private bool disposedValue = false;

        ~Shader()
        {
            GL.DeleteProgram(_handle);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                GL.DeleteProgram(_handle);

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
