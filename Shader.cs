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
