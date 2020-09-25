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

        public float CalculateArea(ref List<Vertex> vertices)
        {
            // https://math.stackexchange.com/a/1951650
            switch (indices.Count)
            {
                case 3: {
                        Vector3 p1 = vertices[(int)indices[0]].position;
                        Vector3 p2 = vertices[(int)indices[1]].position;
                        Vector3 p3 = vertices[(int)indices[2]].position;
                        return Vector3.Cross(p2 - p1, p3 - p1).Length / 2;
                    }
                default: 
                    throw new NotImplementedException($"The area for a face with {indices.Count} indices has not been implemented");
            }
        }
    }

    public struct AABB
    {
        public Vector3 min;
        public Vector3 max;
        public AABB(Vector3 min, Vector3 max)
        {
            this.min = min;
            this.max = max;
        }

        public float DiagonalLength()
        {
            return (min - max).Length;
        }
    }

    public enum FaceType
    {
        Tris,
        Quads,
        Mixed
    }

    public class Mesh
    {
        public List<Vertex> vertices;
        public List<Face> faces;

        public Matrix4 Model = Matrix4.Identity;

        public AABB boundingBox;

        public FaceType faceType;

        public Mesh(List<Vertex> vertices, List<Face> faces)
        {
            this.vertices = vertices;
            this.faces = faces;

            boundingBox = calculateAABB();
            faceType = calculateFaceType();
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

        private AABB calculateAABB()
        {
            float xmin = float.PositiveInfinity, ymin = float.PositiveInfinity, zmin = float.PositiveInfinity;
            float xmax = float.NegativeInfinity, ymax = float.NegativeInfinity, zmax = float.NegativeInfinity; 
            for(int i = 0; i < vertices.Count; i++)
            {
                if (vertices[i].position.X < xmin)
                    xmin = vertices[i].position.X;
                if (vertices[i].position.Y < ymin)
                    ymin = vertices[i].position.Y;
                if (vertices[i].position.Z < zmin)
                    zmin = vertices[i].position.Z;

                if (vertices[i].position.X > xmax)
                    xmax = vertices[i].position.X;
                if (vertices[i].position.Y > ymax)
                    ymax = vertices[i].position.Y;
                if (vertices[i].position.Z > zmax)
                    zmax = vertices[i].position.Z;
            }
            return new AABB(new Vector3(xmin, ymin, zmin), new Vector3(xmax, ymax, zmax));
        }

        private FaceType calculateFaceType()
        {
            bool tris = false;
            bool quads = false;
            for(int i = 0; i < faces.Count; i++)
            {
                if (!tris && faces[i].indices.Count == 3)
                    tris = true;

                if (!quads && faces[i].indices.Count == 4)
                    if (tris)
                        return FaceType.Mixed;
                    else
                        quads = true;
            }
            if (quads)
                return FaceType.Quads;
            else
                return FaceType.Tris;
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

                faces.Add(new Face(indices, facenorm.Normalized()));

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
