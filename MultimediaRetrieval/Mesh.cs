using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using System.Globalization;

namespace MultimediaRetrieval
{
    public class Mesh
    {
        public List<Vertex> vertices;
        public List<Face> faces;
        private AABB? _bbox;

        public AABB BoundingBox
        {
            get
            {
                if (!_bbox.HasValue)
                    _bbox = new AABB(this);
                return _bbox.Value;
            }
        }

        public Mesh(List<Vertex> vertices, List<Face> faces)
        {
            this.vertices = vertices;
            this.faces = faces;
        }

        public Vector3 Sample(Random r)
        {
            return vertices[r.Next(vertices.Count)].position;
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

        public static Mesh ReadMesh(uint id, string dirpath)
        {
            return ReadMesh(Path.Combine(dirpath, id + ".off"));
        }

        public static Mesh ReadMesh(string filepath)
        {
            if (string.IsNullOrWhiteSpace(filepath))
            {
                Console.WriteLine("No filepath detected, reading from stdin");
                return ReadOffMesh(Console.In, "from stdin");
            }

            if (File.Exists(filepath))
            {
                //Check if the file is an .off file:
                string ext = Path.GetExtension(filepath);
                if (ext == ".off")
                {
                    return ReadOffMesh(File.OpenText(filepath), $"in file {filepath}");
                }

                throw new Exception("Unknown filetype: " + ext);
            }

            throw new Exception("Invalid view input data, should be either file path or mesh data");
        }

        //OFF mesh reader, based on work by Philip Shilane, Patrick Min, Michael Kazhdan, and Thomas Funkhouser (Princeton Shape Benchmark)
        private static Mesh ReadOffMesh(TextReader reader, string source)
        {
            // Read file
            List<Vertex> vertices = null;
            List<Face> faces = null;
            uint random_data = 0;

            uint[] vertexAdjFaces = new uint[0];

            string line;
            uint line_count = 0;
            while ((line = reader.ReadLine()) != null)
            {
                // Increment line counter
                line_count++;

                // Skip blank lines and comments
                if (line[0] == '#') 
                    continue;
                if (String.IsNullOrEmpty(line)) 
                    continue;

                // Read the header first:
                if (vertices == null)
                {
                    // Read header 
                    if (!(line == "OFF"))
                    {
                        // Read mesh counts
                        string[] counts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (counts.Length != 3)
                        {
                            throw new Exception($"Syntax error reading header on line {line_count} {source}");
                        }

                        // Set count of vertices/faces.
                        vertices = new List<Vertex>(int.Parse(counts[0]));
                        faces = new List<Face>(int.Parse(counts[1]));
                        random_data = uint.Parse(counts[2]);

                        vertexAdjFaces = new uint[int.Parse(counts[0])];
                    }
                    continue;
                }

                if (vertices.Count < vertices.Capacity)
                {
                    // Read vertex coordinates
                    float[] vertex = Array.ConvertAll(line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries), (s) => float.Parse(s, CultureInfo.InvariantCulture));
                    if (vertex.Length != 3)
                    {
                        throw new Exception($"Syntax error reading vertex on line {line_count} {source}");
                    }

                    vertices.Add(new Vertex(new Vector3(vertex[0], vertex[1], vertex[2]), new Vector3(0)));
                    continue;
                }

                // Read face:
                uint[] face = Array.ConvertAll(line.Split(new char[]{ ' ' }, StringSplitOptions.RemoveEmptyEntries), uint.Parse);
                if (face.Length < face[0] + 1)
                    throw new Exception($"Syntax error reading face on line {line_count} {source}");

                if (face[0] < 3)
                    throw new Exception($"Face found containing only 2 vertices on line {line_count} {source}");

                if (face[0] > 3)
                    throw new NotImplementedException($"Only triangles currently supported");

                List<uint> indices = new List<uint>();
                for (int i = 0; i < face[0]; i++)
                    indices.Add(face[i + 1]);

                // Compute normal for face
                Vector3 facenorm = Face.CalculateNormal(indices, ref vertices);
                faces.Add(new Face(indices, facenorm));

                //Compute normal for vertices:
                for (int i = 0; i < face[0]; i++)
                {
                    vertices[(int)face[i + 1]].normal += facenorm;
                }

                if (faces.Count < faces.Capacity)
                {
                    continue;
                }

                // We are done reading vertices and faces, rest is random data
                for (uint i = 0; i < random_data; i++)
                {
                    // Throw away any other data we have left
                    reader.ReadLine();
                }
            }

            // Finish calculating vertex normals:
            foreach (Vertex v in vertices)
            {
                v.normal.Normalize();
            }

            // Return mesh 
            return new Mesh(vertices, faces);
        }
    }

}
