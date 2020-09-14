using OpenTK.Graphics.ES11;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using System.Globalization;

namespace MultimediaRetrieval
{
    public class Vertex
    {
        public Vector3 position;
        public Vector3 normal;
        public Vertex(Vector3 position, Vector3 normal)
        {
            this.position = position;
            this.normal = normal;
        }
    }

    public struct Face
    {
        public List<uint> indices;
        public Vector3 normal;
        public Face(List<uint> indices, Vector3 normal)
        {
            this.indices = indices;
            this.normal = normal;
        }
    }

    public class Mesh
    {
        public List<Vertex> vertices;
        public List<Face> faces;

        public Matrix4 Model = Matrix4.Identity;

        public Mesh(List<Vertex> vertices, List<Face> faces)
        {
            this.vertices = vertices;
            this.faces = faces;
        }

        //Gives all vertex information ready to be put into a buffer
        public float[,] BufferVertices()
        {
            float[,] result = new float[vertices.Count, 3 + 3];
            for(int i = 0; i < vertices.Count; i++)
            {
                result[i, 0] = vertices[i].position.X;
                result[i, 1] = vertices[i].position.Y;
                result[i, 2] = vertices[i].position.Z;

                result[i, 3] = vertices[i].normal.X;
                result[i, 4] = vertices[i].normal.Y;
                result[i, 5] = vertices[i].normal.Z;  
            }

            return result;
        }

        //Gives all face information ready to be put into a buffer
        public uint[,] BufferFaces()
        {
            uint[,] result = new uint[faces.Count, faces[0].indices.Count];
            for(int i = 0; i < faces.Count; i++)
            {
                for(int j = 0; j < faces[i].indices.Count; j++)
                {
                    result[i, j] = faces[i].indices[j];
                }
            }
            return result;
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

            throw new Exception("Unknown filetype: " + ext);
        }

        //OFF mesh reader, based on work by Philip Shilane, Patrick Min, Michael Kazhdan, and Thomas Funkhouser (Princeton Shape Benchmark)
        private static Mesh ReadOffMesh(string filepath)
        {
            // Open file
            string[] lines = File.ReadAllLines(filepath);

            // Read file
            List<Vertex> vertices = new List<Vertex>();
            List<Face> faces = new List<Face>();

            uint[] vertexAdjFaces = new uint[0];

            int readverts = 0;
            int readfaces = 0;
            int totalverts = 0;
            int totalfaces = 0;
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
                if (totalverts == 0)
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

                        // Set count of vertices/faces.
                        totalverts = int.Parse(counts[0]);
                        totalfaces = int.Parse(counts[1]);

                        vertexAdjFaces = new uint[int.Parse(counts[0])];
                    }
                }
                else if (readverts < totalverts)
                {
                    // Read vertex coordinates
                    float[] vertex = Array.ConvertAll(line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries), (s) => float.Parse(s, CultureInfo.InvariantCulture));
                    if (vertex.Length != 3)
                    {
                        throw new Exception($"Syntax error reading vertex on line {line_count} in file {filepath}");
                    }

                    vertices.Add(new Vertex(new Vector3(vertex[0], vertex[1], vertex[2]), new Vector3(0)));

                    readverts++;
                }
                else if (readfaces < totalfaces)
                {
                    // Read face:
                    uint[] face = Array.ConvertAll(line.Split(new char[]{ ' ' }, StringSplitOptions.RemoveEmptyEntries), uint.Parse);
                    if (face.Length < face[0] + 1)
                        throw new Exception($"Syntax error reading face on line {line_count} in file {filepath}");

                    if (face[0] < 3)
                        throw new Exception($"Face found containing only 2 vertices on line {line_count} in file {filepath}");

                    if (face[0] > 3)
                        throw new NotImplementedException($"Only triangles currently supported");

                    List<uint> indices = new List<uint>();
                    for (int i = 0; i < face[0]; i++)
                        indices.Add(face[i + 1]);
                    

                    // Compute normal for face
                    Vector3 facenorm = new Vector3(0);
                    int v1_index = (int)indices[indices.Count - 1];
                    for (int i = 0; i < face[0]; i++)
                    {
                        int v2_index = (int)indices[i];
                        facenorm.X += (vertices[v1_index].position.Y - vertices[v2_index].position.Y) * (vertices[v1_index].position.Z + vertices[v1_index].position.Z);
                        facenorm.Y += (vertices[v1_index].position.Z - vertices[v2_index].position.Z) * (vertices[v1_index].position.X + vertices[v2_index].position.X);
                        facenorm.Z += (vertices[v1_index].position.X - vertices[v2_index].position.X) * (vertices[v1_index].position.Y + vertices[v2_index].position.Y);
                        v1_index = v2_index;
                    }

                    // Normalize normal for face
                    double squared_normal_length = 0.0f;
                    squared_normal_length += facenorm.X * facenorm.X;
                    squared_normal_length += facenorm.Y * facenorm.Y;
                    squared_normal_length += facenorm.Z * facenorm.Z;
                    float normal_length = (float)Math.Sqrt(squared_normal_length);
                    if (normal_length > 1.0E-6)
                    {
                        facenorm /= normal_length;
                    }

                    faces.Add(new Face(indices, facenorm));

                    //Compute normal for vertices:
                    for (int i = 0; i < face[0]; i++)
                    {
                        vertices[(int)face[i + 1]].normal += facenorm;
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
            if (totalfaces != readfaces)
            {
                throw new Exception($"Expected {totalfaces} faces, but read only {readfaces} faces in file {filepath}");
            }

            // Finish calculating vertex normals:
            foreach(Vertex v in vertices)
            {
                v.normal = v.normal.Normalized();
            }

            // Return mesh 
            return new Mesh(vertices, faces);
        }
    }

}
