using System;
using System.Collections.Generic;
using OpenTK;
// For documentation check:
// http://accord-framework.net/docs/html/R_Project_Accord_NET.htm
// https://github.com/accord-net/framework/wiki
using Accord.Math.Decompositions;
using Accord.Statistics;
using System.Diagnostics.SymbolStore;
using System.Security.Cryptography;

namespace MultimediaRetrieval
{
    public class MeshStatistics
    {
        public uint ID;
        public string Classification;
        public int vertexCount, faceCount;
        public FaceType faceType;

        //The global discriptors:
        public AABB boundingBox;
        public float surface_area;
        float diameter;
        float eccentricity;
        float compactness;

        //The shape property discriptors:
        Histogram a3, d1, d2, d3, d4;

        private MeshStatistics()
        {
            // Nothing
        }

        public MeshStatistics(DatabaseReader reader, string filepath)
        {
            ID = DatabaseReader.GetId(filepath);
            Classification = reader[ID];

            // Generate more features
            Mesh mesh = Mesh.ReadMesh(filepath);
            vertexCount = mesh.vertices.Count;
            faceCount = mesh.faces.Count;

            faceType = Face.CalculateType(mesh.faces);
            boundingBox = new AABB(mesh);

            foreach (Face f in mesh.faces)
            {
                surface_area += f.CalculateArea(ref mesh.vertices);
            }

            double[,] cov = new double[vertexCount,3];
            this.diameter = float.NegativeInfinity;
            for (int i = 0; i < mesh.vertices.Count; i++)
            {
                cov[i, 0] = mesh.vertices[i].position.X;
                cov[i, 1] = mesh.vertices[i].position.Y;
                cov[i, 2] = mesh.vertices[i].position.Z;

                foreach (Vertex v in mesh.vertices)
                {
                    this.diameter = Math.Max(this.diameter, Vector3.DistanceSquared(mesh.vertices[i].position, v.position));
                }
            }
            this.diameter = (float)Math.Sqrt(this.diameter);
            cov = cov.Covariance(0);
            // Eigenvectors are normalized, so retrieve from diagonalmatrix
            double[,] eig = new EigenvalueDecomposition(cov).DiagonalMatrix;
            float eig_max = (float)Math.Max(Math.Max(eig[0, 0], eig[1, 1]), eig[2, 2]);
            float eig_min = (float)Math.Min(Math.Min(eig[0, 0], eig[1, 1]), eig[2, 2]);
            this.eccentricity = eig_max / eig_min;

            //TODO: Compactness
            compactness = 0;

            //The shape property discriptors:
            Random rand = new Random();

            //For A3, sample the angle between 3 random vertices a hundred times.
            a3 = new Histogram("A3", 0, (float)(Math.PI), 10);
            for (int i = 0; i < 100; i++)
            {
                //https://math.stackexchange.com/questions/361412/finding-the-angle-between-three-points
                Vertex v1 = mesh.vertices[rand.Next(vertexCount)];
                Vertex v2 = mesh.vertices[rand.Next(vertexCount)];
                Vertex v3 = mesh.vertices[rand.Next(vertexCount)];
                Vector3 ab = v2.position - v1.position;
                Vector3 bc = v3.position - v2.position;
                if (ab.Length * bc.Length == 0)
                {
                    a3.AddData(0);
                    continue;
                }

                //For some reason Math.Acos cannot handle -1 and 1.
                double dot = Vector3.Dot(ab, bc) / (ab.Length * bc.Length);
                if(dot < -1)
                {
                    a3.AddData((float)Math.PI);
                    continue;
                }
                if(dot > 1)
                {
                    a3.AddData(0);
                    continue;
                }

                float angle = (float)Math.Acos(Vector3.Dot(ab, bc)/(ab.Length * bc.Length));
                if (float.IsNaN(angle))
                {
                    Console.WriteLine(dot);
                }
                a3.AddData(angle);
            }

            //For D1, sample the distance between the barycentre and a random vertex a hundred times.
            //The barycenter is normalized! It is always at (0,0,0)!
            d1 = new Histogram("D1", 0, 1, 10);
            for (int i = 0; i < 100; i++)
            {
                Vertex v = mesh.vertices[rand.Next(vertexCount)];
                d1.AddData(v.position.Length);
            }

            //For D2, sample the distance between two vertices a hundred times.
            d2 = new Histogram("D2", 0, 1, 10);
            for (int i = 0; i < 100; i++)
            {
                Vertex v1 = mesh.vertices[rand.Next(vertexCount)];
                Vertex v2 = mesh.vertices[rand.Next(vertexCount)];
                Vector3 ab = v2.position - v1.position;
                d2.AddData(ab.Length);
            }

            //For D3, sample the  square root of area of triangle given by 3 vertices a hundred times.
            d3 = new Histogram("D3", 0, 1, 10);
            for(int i = 0; i < 100; i++)
            {
                Vertex v1 = mesh.vertices[rand.Next(vertexCount)];
                Vertex v2 = mesh.vertices[rand.Next(vertexCount)];
                Vertex v3 = mesh.vertices[rand.Next(vertexCount)];
                d3.AddData((float)Math.Sqrt(Vector3.Cross(v2.position - v1.position, v3.position - v1.position).Length / 2.0));
            }

            //For D4, sample cube root of volume of tetrahedron formed by 4 random vertices a hundred times
            d4 = new Histogram("D4", 0, 1, 10);
            for(int i = 0; i < 100; i++)
            {
                // https://math.stackexchange.com/questions/3616760/how-to-calculate-the-volume-of-tetrahedron-given-by-4-points
                Vertex v1 = mesh.vertices[rand.Next(vertexCount)];
                Vertex v2 = mesh.vertices[rand.Next(vertexCount)];
                Vertex v3 = mesh.vertices[rand.Next(vertexCount)];
                Vertex v4 = mesh.vertices[rand.Next(vertexCount)];
                Matrix4 m = new Matrix4(new Vector4(v1.position, 1), new Vector4(v2.position, 1), new Vector4(v3.position, 1), new Vector4(v4.position, 1));
                double area = Math.Abs(m.Determinant / 6.0);
                d4.AddData((float)Math.Pow(Math.Abs(area), 1.0 / 3.0));
            }
        }

        public string Headers()
        {
            return "ID;" +
            "Class;" +
            "#Vertices;" +
            "#Faces;" +
            "FaceType;" +
            "AABB_min_X;" +
            "AABB_min_Y;" +
            "AABB_min_Z;" +
            "AABB_max_X;" +
            "AABB_max_Y;" +
            "AABB_max_Z;" +
            "AABB_Volume;" +
            "Surface_Area;" +
            "Diameter;" +
            "Eccentricity;" +
            "Compactness;" +
            a3.ToCSVHeader() + ";" +
            d1.ToCSVHeader() + ";" +
            d2.ToCSVHeader() + ";" +
            d3.ToCSVHeader() + ";" +
            d4.ToCSVHeader();
        }
            

        public override string ToString()
        {
            return string.Join(";", 
                ID, 
                Classification, 
                vertexCount, 
                faceCount, 
                faceType, 
                boundingBox.min.X, 
                boundingBox.min.Y, 
                boundingBox.min.Z, 
                boundingBox.max.X, 
                boundingBox.max.Y, 
                boundingBox.max.Z, 
                boundingBox.Volume(), 
                surface_area,
                diameter,
                eccentricity,
                compactness,
                a3.ToCSV(),
                d1.ToCSV(),
                d2.ToCSV(),
                d3.ToCSV(),
                d4.ToCSV()
                );
        }

        public static MeshStatistics Parse(string input)
        {
            string[] data = input.Split(new char[] { ';' });
            var stats = new MeshStatistics();

            stats.ID = uint.Parse(data[0]);
            stats.Classification = data[1];
            stats.vertexCount = int.Parse(data[2]);
            stats.faceCount = int.Parse(data[3]);
            Enum.TryParse(data[4], out stats.faceType);
            stats.boundingBox = new AABB(
                new Vector3(float.Parse(data[5]), float.Parse(data[6]), float.Parse(data[7])),
                new Vector3(float.Parse(data[8]), float.Parse(data[9]), float.Parse(data[10]))
                );
            // AABB Volume 
            stats.surface_area = float.Parse(data[12]);
            stats.diameter = float.Parse(data[13]);
            stats.eccentricity = float.Parse(data[14]);

            return stats;
        }
    }
}
