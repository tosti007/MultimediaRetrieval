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
using OpenTK.Graphics.ES11;

namespace MultimediaRetrieval
{
    public class MeshStatistics
    {
        public const int NUMBER_OF_SAMPLES = 1000;
        public const int A3_BIN_SIZE = 10;
        public const float A3_MIN = 0;
        public const float A3_MAX = (float)Math.PI;
        public const int D1_BIN_SIZE = 10;
        public const float D1_MIN = 0;
        public const float D1_MAX = 1;
        public const int D2_BIN_SIZE = 10;
        public const float D2_MIN = 0;
        public const float D2_MAX = 1;
        public const int D3_BIN_SIZE = 10;
        public const float D3_MIN = 0;
        public const float D3_MAX = 1;
        public const int D4_BIN_SIZE = 10;
        public const float D4_MIN = 0;
        public const float D4_MAX = 1;

        public uint ID;
        public string Classification;
        public int vertexCount, faceCount;
        public FaceType faceType;

        //The global discriptors:
        public AABB boundingBox;
        public float surface_area;
        public float diameter;
        public float eccentricity;
        public float compactness;
        public float volume;

        //The shape property discriptors:
        public Histogram a3, d1, d2, d3, d4;

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
                Vector3 pos = mesh.vertices[i].position;
                cov[i, 0] = pos.X;
                cov[i, 1] = pos.Y;
                cov[i, 2] = pos.Z;

                foreach (Vertex v in mesh.vertices)
                {
                    this.diameter = Math.Max(this.diameter, Vector3.DistanceSquared(pos, v.position));
                }
            }
            this.diameter = (float)Math.Sqrt(this.diameter);
            cov = cov.Covariance(0);
            // Eigenvectors are normalized, so retrieve from diagonalmatrix
            double[,] eig = new EigenvalueDecomposition(cov).DiagonalMatrix;
            float eig_max = (float)Math.Max(Math.Max(eig[0, 0], eig[1, 1]), eig[2, 2]);
            float eig_min = (float)Math.Min(Math.Min(eig[0, 0], eig[1, 1]), eig[2, 2]);
            this.eccentricity = eig_max / eig_min;

            //For compactness, we need the volume:
            this.volume = 0;
            for(int i = 0; i < faceCount; i++)
            {
                volume += mesh.faces[i].CalculateSignedVolume(ref mesh.vertices);
            }
            this.volume = Math.Abs(this.volume);

            this.compactness = (float)(Math.Pow(surface_area,3)/(36*Math.PI*Math.Pow(volume, 2)));

            //The shape property discriptors:
            Random rand = new Random();

            //For A3, sample the angle between 3 random vertices a hundred times.
            a3 = new Histogram("A3", A3_MIN, A3_MAX, A3_BIN_SIZE);
            for (int i = 0; i < NUMBER_OF_SAMPLES; i++)
            {
                //https://math.stackexchange.com/questions/361412/finding-the-angle-between-three-points
                Vector3 v1 = Sample(mesh, rand);
                Vector3 v2 = Sample(mesh, rand);
                Vector3 v3 = Sample(mesh, rand);
                Vector3 ab = v2 - v1;
                Vector3 bc = v3 - v2;
                if (ab.Length * bc.Length == 0)
                {
                    a3.AddData(0);
                    continue;
                }

                a3.AddData(Vector3.CalculateAngle(ab, bc));
            }

            //For D1, sample the distance between the barycentre and a random vertex a hundred times.
            //The barycenter is normalized! It is always at (0,0,0)!
            d1 = new Histogram("D1", D1_MIN, D1_MAX, D1_BIN_SIZE);
            for (int i = 0; i < NUMBER_OF_SAMPLES; i++)
            {
                Vector3 v = Sample(mesh, rand);
                d1.AddData(v.Length);
            }

            //For D2, sample the distance between two vertices a hundred times.
            d2 = new Histogram("D2", D2_MIN, D2_MAX, D2_BIN_SIZE);
            for (int i = 0; i < NUMBER_OF_SAMPLES; i++)
            {
                Vector3 v1 = Sample(mesh, rand);
                Vector3 v2 = Sample(mesh, rand);
                Vector3 ab = v2 - v1;
                d2.AddData(ab.Length);
            }

            //For D3, sample the  square root of area of triangle given by 3 vertices a hundred times.
            d3 = new Histogram("D3", D3_MIN, D3_MAX, D3_BIN_SIZE);
            for(int i = 0; i < NUMBER_OF_SAMPLES; i++)
            {
                Vector3 v1 = Sample(mesh, rand);
                Vector3 v2 = Sample(mesh, rand);
                Vector3 v3 = Sample(mesh, rand);
                d3.AddData((float)Math.Sqrt(Face.CalculateArea(v1, v2, v3)));
            }

            //For D4, sample cube root of volume of tetrahedron formed by 4 random vertices a hundred times
            d4 = new Histogram("D4", D4_MIN, D4_MAX, D4_BIN_SIZE);
            for(int i = 0; i < NUMBER_OF_SAMPLES; i++)
            {
                // https://math.stackexchange.com/questions/3616760/how-to-calculate-the-volume-of-tetrahedron-given-by-4-points
                Vector4 v1 = new Vector4(Sample(mesh, rand), 1);
                Vector4 v2 = new Vector4(Sample(mesh, rand), 1);
                Vector4 v3 = new Vector4(Sample(mesh, rand), 1);
                Vector4 v4 = new Vector4(Sample(mesh, rand), 1);
                Matrix4 m = new Matrix4(v1, v2, v3, v4);
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
            "Volume;" +
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
                volume,
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

            //The histograms:
            int histoIndex = 15;
            stats.a3 = new Histogram("A3", A3_MIN, A3_MAX, A3_BIN_SIZE);
            stats.a3.LoadData(data, histoIndex);
            histoIndex += A3_BIN_SIZE;

            stats.d1 = new Histogram("D1", D1_MIN, D1_MAX, D1_BIN_SIZE);
            stats.d1.LoadData(data, histoIndex);
            histoIndex += D1_BIN_SIZE;

            stats.d2 = new Histogram("D2", D2_MIN, D2_MAX, D2_BIN_SIZE);
            stats.d2.LoadData(data, histoIndex);
            histoIndex += D2_BIN_SIZE;

            stats.d3 = new Histogram("D3", D3_MIN, D3_MAX, D3_BIN_SIZE);
            stats.d3.LoadData(data, histoIndex);
            histoIndex += D3_BIN_SIZE;

            stats.d4 = new Histogram("D4", D4_MIN, D4_MAX, D4_BIN_SIZE);
            stats.d4.LoadData(data, histoIndex);
            histoIndex += D4_BIN_SIZE;

            return stats;
        }

        private static Vector3 Sample(Mesh m, Random r)
        {
            return m.vertices[r.Next(m.vertices.Count)].position;
        }       
    }

    public struct FeatureVector
    {
        public float[] data;

        FeatureVector(float[] data)
        {
            this.data = data;
        }

        public FeatureVector(MeshStatistics m)
        {
            data = new float[5 + m.a3.bins + m.d1.bins + m.d2.bins + m.d3.bins + m.d4.bins];
            data[0] = m.surface_area;
            data[1] = m.diameter;
            data[2] = m.eccentricity;
            data[3] = m.compactness;
            data[4] = m.volume;

            int histoIndex = 5;
            int[] a3data = m.a3.GetData();
            for (int i = 0; i < m.a3.bins; i++)
            {
                data[i] = a3data[i];
            }
            histoIndex += m.a3.bins;

            int[] d1data = m.d1.GetData();
            for (int i = 0; i < m.d1.bins; i++)
            {
                data[i] = d1data[i];
            }
            histoIndex += m.d1.bins;

            int[] d2data = m.d2.GetData();
            for (int i = 0; i < m.d2.bins; i++)
            {
                data[i] = d2data[i];
            }
            histoIndex += m.d2.bins;

            int[] d3data = m.d3.GetData();
            for (int i = 0; i < m.d3.bins; i++)
            {
                data[i] = d3data[i];
            }
            histoIndex += m.d3.bins;

            int[] d4data = m.d4.GetData();
            for (int i = 0; i < m.d4.bins; i++)
            {
                data[i] = d4data[i];
            }
        }

        public static FeatureVector operator +(FeatureVector a, FeatureVector b)
        {
            if (a.data.Length != b.data.Length)
                throw new Exception("Attempted to add two FeatureVectors of different length.");
            else
            {
                float[] resultData = new float[a.data.Length];
                for(int i = 0; i < a.data.Length; i++)
                {
                    resultData[i] = a.data[i] + b.data[i];
                }
                return new FeatureVector(resultData);
            }
        }

        public static FeatureVector operator -(FeatureVector a, FeatureVector b)
        {
            if (a.data.Length != b.data.Length)
                throw new Exception("Attempted to subtract two FeatureVectors of different length.");
            else
            {
                float[] resultData = new float[a.data.Length];
                for (int i = 0; i < a.data.Length; i++)
                {
                    resultData[i] = a.data[i] - b.data[i];
                }
                return new FeatureVector(resultData);
            }
        }

        public void Map(Func<float, float> f)
        {
            for(int i = 0; i < data.Length; i++)
            {
                data[i] = f(data[i]);
            }
        }
    }
}
