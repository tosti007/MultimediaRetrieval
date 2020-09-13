using OpenTK.Graphics.ES11;
using OpenTK.Graphics.OpenGL;
using System;
using System.IO;
using OpenTK;
using System.Globalization;

namespace MultimediaRetrieval
{
    public class Mesh
    {
        public float[,] vertexPos;
        public float[,] faceNorm;
        public uint[,] faces;

        public Matrix4 Model = Matrix4.Identity;

        public Mesh(float[,] vertexPos, uint[,] faces, float[,] faceNorm)
        {
            this.vertexPos = vertexPos;
            this.faces = faces;
            this.faceNorm = faceNorm;
        }

        public static Mesh ReadMesh(string filepath)
        {
            //Check if the file is an .off file:
            string[] getext = filepath.Split('.');
            string ext = getext[getext.Length - 1];
            if (ext == "off")
            {
                return ReadOffMesh(filepath);
            }
            else
            {
                throw new Exception("Unknown filetype: " + ext);
            }
        }

        //OFF mesh reader, based on work by Philip Shilane, Patrick Min, Michael Kazhdan, and Thomas Funkhouser (Princeton Shape Benchmark)
        private static Mesh ReadOffMesh(string filepath)
        {
            // Open file
            string[] lines = File.ReadAllLines(filepath);

            // Read file
            float[,] vertices = new float[0,0];
            uint[,] faces = new uint[0,0];
            float[,] faceNorm = new float[0,0];
            int readverts = 0;
            int readfaces = 0;
            int line_count = -1;
            while (line_count < lines.Length - 1)
            {
                // Increment line counter
                line_count++;

                // Get the current line:
                string line = lines[line_count];

                // Skip blank lines and comments
                if (line[0] == '#') 
                    continue;
                if (String.IsNullOrEmpty(line)) 
                    continue;

                // Read the header first:
                if (vertices.Length == 0)
                {
                    // Read header 
                    if (!(line == "OFF"))
                    {
                        // Read mesh counts
                        string[] counts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (counts.Length != 3)
                        {
                            throw new Exception($"Syntax error reading header on line {line_count} in file {filepath}");
                        }

                        // Allocate memory for mesh
                        vertices = new float[int.Parse(counts[0]),3];
                        faces = new uint[int.Parse(counts[1]),3];
                        faceNorm = new float[int.Parse(counts[1]), 3];
                    }
                }
                else if (readverts < vertices.GetLength(0))
                {
                    // Read vertex coordinates
                    float[] vertex = Array.ConvertAll(line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries), (s) => float.Parse(s, CultureInfo.InvariantCulture));
                    if (vertex.Length != 3)
                    {
                        throw new Exception($"Syntax error reading vertex on line {line_count} in file {filepath}");
                    }
                    vertices[readverts, 0] = vertex[0];
                    vertices[readverts, 1] = vertex[1];
                    vertices[readverts, 2] = vertex[2];
                    readverts++;
                }
                else if (readfaces < faces.GetLength(0))
                {
                    // Read face:
                    uint[] face = Array.ConvertAll(line.Split(new char[]{ ' ' }, StringSplitOptions.RemoveEmptyEntries), uint.Parse);
                    if (face.Length < face[0] + 1)
                        throw new Exception($"Syntax error reading face on line {line_count} in file {filepath}");

                    if (face[0] < 3)
                        throw new Exception($"Face found containing only 2 vertices on line {line_count} in file {filepath}");

                    if (face[0] > 3)
                        throw new NotImplementedException($"Only triangles currently supported");

                    faces[readfaces, 0] = face[1];
                    faces[readfaces, 1] = face[2];
                    faces[readfaces, 2] = face[3];
                    
                    // Compute normal for face
                    faceNorm[readfaces,0] = faceNorm[readfaces,1] = faceNorm[readfaces, 2] = 0;
                    uint v1_index = faces[readfaces, face[0] - 1]; //Remember, face[0] is the amount of indices per face.
                    for (int i = 0; i < face[0]; i++)
                    {
                        uint v2_index = faces[readfaces, i];
                        faceNorm[readfaces, 0] += (vertices[v1_index, 1] - vertices[v2_index, 1]) * (vertices[v1_index, 2] + vertices[v2_index, 2]);
                        faceNorm[readfaces, 1] += (vertices[v1_index, 2] - vertices[v2_index, 2]) * (vertices[v1_index, 0] + vertices[v2_index, 0]);
                        faceNorm[readfaces, 2] += (vertices[v1_index, 0] - vertices[v2_index, 0]) * (vertices[v1_index, 1] + vertices[v2_index, 1]);
                        v1_index = v2_index;
                    }

                    // Normalize normal for face
                    double squared_normal_length = 0.0f;
                    squared_normal_length += faceNorm[readfaces, 0] * faceNorm[readfaces, 0];
                    squared_normal_length += faceNorm[readfaces, 1] * faceNorm[readfaces, 1];
                    squared_normal_length += faceNorm[readfaces, 2] * faceNorm[readfaces, 2];
                    float normal_length = (float)Math.Sqrt(squared_normal_length);
                    if (normal_length > 1.0E-6)
                    {
                        faceNorm[readfaces, 0] /= normal_length;
                        faceNorm[readfaces, 1] /= normal_length;
                        faceNorm[readfaces, 2] /= normal_length;
                    }
                    

				    readfaces++;
                }
                else
                {
                    // Silently discard unused data.
                    break;
                }
            }

            // Check whether read all faces
            if (faces.GetLength(0) != readfaces)
            {
                throw new Exception($"Expected {faces.GetLength(0)} faces, but read only {readfaces} faces in file {filepath}");
            }

            // Return mesh 
            return new Mesh(vertices, faces, faceNorm);
        }
    }

}
