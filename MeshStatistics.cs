using System;
using System.Collections.Generic;
using OpenTK;
// For documentation check:
// http://accord-framework.net/docs/html/R_Project_Accord_NET.htm
// https://github.com/accord-net/framework/wiki
using Accord.Math.Decompositions;
using Accord.Statistics;

namespace MultimediaRetrieval
{
    public class MeshStatistics
    {
        public uint ID;
        public string Classification;
        public int vertexCount, faceCount;
        public FaceType faceType;
        public AABB boundingBox;
        public float surface_area;
        float diameter;
        float eccentricity;
        // TODO: compactness(with respect to a sphere)

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
        }

        public const string Headers = 
            "ID;" +
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
            "Surface_Area" + 
            "Diameter" +
            "Eccentricity";

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
                eccentricity
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
