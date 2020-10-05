using System;
using OpenTK;
// For documentation check:
// http://accord-framework.net/docs/html/R_Project_Accord_NET.htm
// https://github.com/accord-net/framework/wiki
using Accord.Math.Decompositions;
using Accord.Statistics;
using System.Linq;

namespace MultimediaRetrieval
{
    public class MeshStatistics
    {
        public const int NUMBER_OF_SAMPLES = 1000;

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
        public Histogram_A3 a3;
        public Histogram_D1 d1;
        public Histogram_D2 d2;
        public Histogram_D3 d3;
        public Histogram_D4 d4;

        private MeshStatistics()
        {
            // Nothing
        }

        public MeshStatistics(Mesh mesh)
        {
            //The constructor for Query meshes:
            ID = 0;
            Classification = "?";

            // Generate more features
            GenerateFeatures(mesh);
        }

        public MeshStatistics(DatabaseReader reader, string filepath)
        {
            ID = DatabaseReader.GetId(filepath);
            Classification = reader[ID];

            // Generate more features
            GenerateFeatures(Mesh.ReadMesh(filepath));
        }

        void GenerateFeatures(Mesh mesh)
        {
            vertexCount = mesh.vertices.Count;
            faceCount = mesh.faces.Count;

            faceType = Face.CalculateType(mesh.faces);
            boundingBox = new AABB(mesh);

            foreach (Face f in mesh.faces)
            {
                surface_area += f.CalculateArea(ref mesh.vertices);
            }

            double[,] cov = new double[vertexCount, 3];
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
            for (int i = 0; i < faceCount; i++)
            {
                volume += mesh.faces[i].CalculateSignedVolume(ref mesh.vertices);
            }
            this.volume = Math.Abs(this.volume);

            this.compactness = (float)(Math.Pow(surface_area, 3) / (36 * Math.PI * Math.Pow(volume, 2)));

            //The shape property discriptors:
            Random rand = new Random();

            a3 = new Histogram_A3();
            a3.Sample(mesh, rand, NUMBER_OF_SAMPLES);

            d1 = new Histogram_D1();
            d1.Sample(mesh, rand, NUMBER_OF_SAMPLES);

            d2 = new Histogram_D2();
            d2.Sample(mesh, rand, NUMBER_OF_SAMPLES);

            d3 = new Histogram_D3();
            d3.Sample(mesh, rand, NUMBER_OF_SAMPLES);

            d4 = new Histogram_D4();
            d4.Sample(mesh, rand, NUMBER_OF_SAMPLES);
        }

        public static string Headers()
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
            Histogram_A3.ToCSVHeader() + ";" +
            Histogram_D1.ToCSVHeader() + ";" +
            Histogram_D2.ToCSVHeader() + ";" +
            Histogram_D3.ToCSVHeader() + ";" +
            Histogram_D4.ToCSVHeader();
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
            stats.compactness = float.Parse(data[15]);
            stats.volume = float.Parse(data[16]);

            //TODO: Might want to handle this differently.
            //There are meshes with infinite eccentricity/compactness, fix this:
            if (float.IsInfinity(stats.compactness))
                stats.compactness = 0;
            if (float.IsInfinity(stats.eccentricity))
                stats.eccentricity = 0;

            //The histograms:
            int histoIndex = 17;
            stats.a3 = new Histogram_A3();
            stats.a3.LoadData(data, histoIndex);
            histoIndex += Histogram_A3.BIN_SIZE;

            stats.d1 = new Histogram_D1();
            stats.d1.LoadData(data, histoIndex);
            histoIndex += Histogram_D1.BIN_SIZE;

            stats.d2 = new Histogram_D2();
            stats.d2.LoadData(data, histoIndex);
            histoIndex += Histogram_D2.BIN_SIZE;

            stats.d3 = new Histogram_D3();
            stats.d3.LoadData(data, histoIndex);
            histoIndex += Histogram_D3.BIN_SIZE;

            stats.d4 = new Histogram_D4();
            stats.d4.LoadData(data, histoIndex);
            histoIndex += Histogram_D4.BIN_SIZE;

            return stats;
        }
    }

    public class FeatureVector
    {
        public float[] data;

        private FeatureVector(float[] data)
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

            m.a3.Data.CopyTo(data, histoIndex);
            histoIndex += m.a3.bins;

            m.d1.Data.CopyTo(data, histoIndex);
            histoIndex += m.d1.bins;

            m.d2.Data.CopyTo(data, histoIndex);
            histoIndex += m.d2.bins;

            m.d3.Data.CopyTo(data, histoIndex);
            histoIndex += m.d3.bins;

            m.d4.Data.CopyTo(data, histoIndex);
        }

        public static FeatureVector operator +(FeatureVector a, FeatureVector b)
        {
            if (a.data.Length != b.data.Length)
                throw new Exception("Attempted to add two FeatureVectors of different length.");

            for(int i = 0; i < a.data.Length; i++)
                a.data[i] += b.data[i];

            return a;
        }

        public static FeatureVector operator -(FeatureVector a, FeatureVector b)
        {
            if (a.data.Length != b.data.Length)
                throw new Exception("Attempted to subtract two FeatureVectors of different length.");

            for (int i = 0; i < a.data.Length; i++)
                a.data[i] -= b.data[i];

            return a;
        }

        public void Map(Func<float, float> f)
        {
            for(int i = 0; i < data.Length; i++)
            {
                data[i] = f(data[i]);
            }
        }

        //Normalize the feature vector using the average and a standard deviation from a featuredatabase:
        public void Normalize(FeatureDatabase db)
        {
            if (data.Length != db.Average.data.Length || data.Length != db.StandardDev.data.Length)
                throw new Exception("Attempted to normalize with FeatureVectors of different length.");

            for(int i = 0; i < data.Length; i++)
            {
                if(db.StandardDev.data[i] != 0)
                    data[i] = (data[i] - db.Average.data[i]) / db.StandardDev.data[i];
                else
                {
                    data[i] = (data[i] - db.Average.data[i]);
                    //Console.WriteLine($"Sdev was 0 at index {i}! Did not use it for normalization."); 
                    //TODO: Make sure this doesn't happen by changing histo-bins for example.
                }
            }
        }

        public static float EuclidianDistance(FeatureVector a, FeatureVector b)
        {
            if (a.data.Length != b.data.Length)
                throw new Exception("Attempted to calculate distance between two FeatureVectors of different length.");

            float result = 0;
            for (int i = 0; i < a.data.Length; i++)
            {
                float value = a.data[i] - b.data[i];
                result += value * value;
            }

            return (float)Math.Sqrt(result);
        }
    }
}
