using System;
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
        public const int NUMBER_OF_SAMPLES = 1000;

        public uint ID;
        public string Classification;
        public int VertexCount, FaceCount;
        public FaceType FaceType;

        //The global discriptors:
        public AABB BoundingBox;
        public float SurfaceArea;
        public float Diameter;
        public float Eccentricity;
        public float Compactness;
        public float Volume;

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
            VertexCount = mesh.vertices.Count;
            FaceCount = mesh.faces.Count;

            FaceType = Face.CalculateType(mesh.faces);
            BoundingBox = new AABB(mesh);

            foreach (Face f in mesh.faces)
            {
                SurfaceArea += f.CalculateArea(ref mesh.vertices);
            }

            double[,] cov = new double[VertexCount, 3];
            this.Diameter = float.NegativeInfinity;
            for (int i = 0; i < mesh.vertices.Count; i++)
            {
                Vector3 pos = mesh.vertices[i].position;
                cov[i, 0] = pos.X;
                cov[i, 1] = pos.Y;
                cov[i, 2] = pos.Z;

                foreach (Vertex v in mesh.vertices)
                {
                    this.Diameter = Math.Max(this.Diameter, Vector3.DistanceSquared(pos, v.position));
                }
            }
            this.Diameter = (float)Math.Sqrt(this.Diameter);
            cov = cov.Covariance(0);
            // Eigenvectors are normalized, so retrieve from diagonalmatrix
            double[,] eig = new EigenvalueDecomposition(cov).DiagonalMatrix;
            float eig_max = (float)Math.Max(Math.Max(eig[0, 0], eig[1, 1]), eig[2, 2]);
            float eig_min = (float)Math.Min(Math.Min(eig[0, 0], eig[1, 1]), eig[2, 2]);
            this.Eccentricity = eig_max / eig_min;

            //For compactness, we need the volume:
            this.Volume = 0;
            for (int i = 0; i < FaceCount; i++)
            {
                Volume += mesh.faces[i].CalculateSignedVolume(ref mesh.vertices);
            }
            this.Volume = Math.Abs(this.Volume);

            this.Compactness = (float)(Math.Pow(SurfaceArea, 3) / (36 * Math.PI * Math.Pow(Volume, 2)));

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
                VertexCount, 
                FaceCount, 
                FaceType, 
                BoundingBox.min.X, 
                BoundingBox.min.Y, 
                BoundingBox.min.Z, 
                BoundingBox.max.X, 
                BoundingBox.max.Y, 
                BoundingBox.max.Z, 
                BoundingBox.Volume(), 
                SurfaceArea,
                Diameter,
                Eccentricity,
                Compactness,
                Volume,
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
            stats.VertexCount = int.Parse(data[2]);
            stats.FaceCount = int.Parse(data[3]);
            Enum.TryParse(data[4], out stats.FaceType);
            stats.BoundingBox = new AABB(
                new Vector3(float.Parse(data[5]), float.Parse(data[6]), float.Parse(data[7])),
                new Vector3(float.Parse(data[8]), float.Parse(data[9]), float.Parse(data[10]))
                );
            // AABB Volume 
            stats.SurfaceArea = float.Parse(data[12]);
            stats.Diameter = float.Parse(data[13]);
            stats.Eccentricity = float.Parse(data[14]);
            stats.Compactness = float.Parse(data[15]);
            stats.Volume = float.Parse(data[16]);

            //TODO: Might want to handle this differently.
            //There are meshes with infinite eccentricity/compactness, fix this:
            if (float.IsInfinity(stats.Compactness))
                stats.Compactness = 0;
            if (float.IsInfinity(stats.Eccentricity))
                stats.Eccentricity = 0;

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
}
