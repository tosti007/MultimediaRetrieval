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
    public class FeatureVector
    {
        public const int NUMBER_OF_SAMPLES = 1000;
        public const int HISTOGRAM_START_INDEX = 5;
        private static readonly Histogram[] HISTOGRAMS = {
                new Histogram(HistogramType.A3, HISTOGRAM_START_INDEX +  0, 10, 0, (float)Math.PI),
                new Histogram(HistogramType.D1, HISTOGRAM_START_INDEX + 10, 10, 0, 0.8f),
                new Histogram(HistogramType.D2, HISTOGRAM_START_INDEX + 20, 10, 0, 1),
                new Histogram(HistogramType.D3, HISTOGRAM_START_INDEX + 30, 10, 0, 0.6f),
                new Histogram(HistogramType.D4, HISTOGRAM_START_INDEX + 40, 10, 0, 0.4f),
            };

        private float[] _data;

        public float SurfaceArea { get => _data[0]; set => _data[0] = value; }
        public float Diameter { get => _data[1]; set => _data[1] = value; }
        public float Eccentricity { get => _data[2]; set => _data[2] = value; }
        public float Compactness { get => _data[3]; set => _data[3] = value; }
        public float Volume { get => _data[4]; set => _data[4] = value; }

        public FeatureVector()
        {
            int length = HISTOGRAM_START_INDEX + HISTOGRAMS.Select((h) => h.Bins).Sum();
            _data = new float[length];
        }

        private FeatureVector(float[] data)
        {
            this._data = data;
        }

        public FeatureVector(Mesh mesh) : this()
        {
            foreach (Face f in mesh.faces)
            {
                SurfaceArea += f.CalculateArea(ref mesh.vertices);
            }

            double[,] cov = new double[mesh.vertices.Count, 3];
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
            for (int i = 0; i < mesh.faces.Count; i++)
            {
                Volume += mesh.faces[i].CalculateSignedVolume(ref mesh.vertices);
            }
            this.Volume = Math.Abs(this.Volume);

            this.Compactness = (float)(Math.Pow(SurfaceArea, 3) / (36 * Math.PI * Math.Pow(Volume, 2)));

            //The shape property discriptors:
            // TODO: Remove this seed later on, but for testing purposes keep it.
            Random rand = new Random(1234);

            foreach (Histogram hist in HISTOGRAMS)
                hist.Sample(ref _data, mesh, rand, NUMBER_OF_SAMPLES);
        }

        public static FeatureVector operator +(FeatureVector a, FeatureVector b)
        {
            if (a._data.Length != b._data.Length)
                throw new Exception("Attempted to add two FeatureVectors of different length.");

            FeatureVector c = new FeatureVector();
            for (int i = 0; i < a._data.Length; i++)
                c._data[i] = a._data[i] + b._data[i];

            return c;
        }

        public static FeatureVector operator -(FeatureVector a, FeatureVector b)
        {
            if (a._data.Length != b._data.Length)
                throw new Exception("Attempted to subtract two FeatureVectors of different length.");

            FeatureVector c = new FeatureVector();
            for (int i = 0; i < a._data.Length; i++)
                c._data[i] = a._data[i] - b._data[i];

            return c;
        }

        public void Map(Func<float, float> f)
        {
            for (int i = 0; i < _data.Length; i++)
            {
                _data[i] = f(_data[i]);
            }
        }

        public bool HasValue(Func<float, bool> f)
        {
            return _data.Any(f);
        }

        //Normalize the feature vector using the average and a standard deviation from a featuredatabase:
        public void Normalize(FeatureVector avg, FeatureVector std)
        {
            if (_data.Length != avg._data.Length || _data.Length != std._data.Length)
                throw new Exception("Attempted to normalize with FeatureVectors of different length.");

            for (int i = 0; i < _data.Length; i++)
            {
                _data[i] = _data[i] - avg._data[i];

                if (std._data[i] != 0)
                    _data[i] /= std._data[i];

                // Else
                //Console.WriteLine($"Sdev was 0 at index {i}! Did not use it for normalization."); 
                //TODO: Make sure this doesn't happen by changing histo-bins for example.
            }
        }

        public void HistogramsAsPercentages()
        {
            foreach (Histogram hist in HISTOGRAMS)
                hist.AsPercentage(ref _data);
        }

        public static float EuclidianDistance(FeatureVector a, FeatureVector b)
        {
            if (a._data.Length != b._data.Length)
                throw new Exception("Attempted to calculate distance between two FeatureVectors of different length.");

            float result = 0;
            for (int i = 0; i < a._data.Length; i++)
            {
                float value = a._data[i] - b._data[i];
                result += value * value;
            }

            return (float)Math.Sqrt(result);
        }

        //TODO: Test this, might be bad because the featurevectors are generally not normalized (normalized in this context means length == 1)
        public static float CosineDistance(FeatureVector a, FeatureVector b)
        {
            if (a._data.Length != b._data.Length)
                throw new Exception("Attempted to calculate distance between two FeatureVectors of different length.");

            float result = 0;
            float alen = 0;
            float blen = 0;
            for(int i = 0; i < a._data.Length; i++)
            {
                alen += a._data[i];
                blen += b._data[i];
                result += a._data[i] * b._data[i];
            }
            return 1 - (result / (alen * blen));
        }

        public static string Headers()
        {
            return string.Join(";",
                "Surface_Area",
                "Diameter",
                "Eccentricity",
                "Compactness",
                "Volume",
                string.Join(";",
                    HISTOGRAMS.Select((h) => h.Headers())
                ));
        }

        public override string ToString()
        {
            return string.Join(";", _data);
        }

        public string PrettyPrint()
        {
            return "Feature Vector\n" + string.Join("\n", Headers().Split(';').Zip(ToString().Split(';'), (a, b) => $"    {a}: {b}"));
        }

        public static FeatureVector Parse(string input)
        {
            FeatureVector v = new FeatureVector(input.Split(new char[] { ';' }).Select(float.Parse).ToArray());

            //There are meshes with infinite eccentricity/compactness, fix this:
            if (float.IsInfinity(v.Compactness))
                v.Compactness = 0;
            if (float.IsInfinity(v.Eccentricity))
                v.Eccentricity = 0;

            return v;
        }
    }
}
