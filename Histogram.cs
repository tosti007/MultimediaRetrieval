using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace MultimediaRetrieval
{
    //A histogram class for the distribution features in the MeshStatistics
    public abstract class AbstractHistogram
    {
        public int nr_items;
        float min, max;
        public int[] Data { get; }

        public int Bins => Data.Length;

        public AbstractHistogram(float min, float max, int bins)
        {
            this.min = min;
            this.max = max;

            Data = new int[bins];
        }

        //Loads in a Histogram from string data, starting at the given start index.
        public void LoadData(string[] sdata, int start)
        {
            for(int i = 0; i < Bins; i++)
            {
                Data[i] = int.Parse(sdata[start + i]);
            }
        }

        public float[] AsPercentage()
        {
            float[] result = new float[Bins];
            for (int i = 0; i < Bins; i++)
                result[i] = (float)Data[i] / nr_items;
            return result;
        }

        public string ToCSV()
        {
            string[] result = new string[Bins];
            for (int i = 0; i < Bins; i++)
            {
                result[i] = Data[i].ToString();
            }
            return result.Aggregate((pstring, csv) => $"{pstring};{csv}");
        }

        protected static string ToCSVHeader(string title, float min, float max, int bins)
        {
            string[] result = new string[bins];
            for(int i = 0; i < bins; i++)
            {
                result[i] = title + " (" + (i * ((max - min)/bins) + min).ToString() + "," + ((i + 1) * ((max - min) / bins) + min).ToString() + ")";
            }
            return result.Aggregate((pstring, csv) => $"{pstring};{csv}");
        }

        public void AddData(float f)
        {
            if (f < min || f > max)
                throw new Exception($"Data {f} was out of range of min: {min} and max: {max}.");

            float diff = max - min;
            float step = diff / Bins;
            int index = (int)Math.Floor((f - min) / step);

            if (index == Data.Length)
                index--;

            Data[index]++;
            nr_items++;
        }

        protected virtual void Sample(Mesh mesh, Random rand)
        {
            throw new NotImplementedException("Sample not implemented for this histogram");
        }

        public void Sample(Mesh mesh, Random rand, int nr_samples)
        {
            for (int i = 0; i < nr_samples; i++)
                Sample(mesh, rand);
        }

        public static void AsPercentage(ref float[] data, int start, int nr_bins)
        {
            float total = 0;

            for (int i = 0; i < nr_bins; i++)
                total += data[start + i];

            if (total == 0)
                return;

            for (int i = 0; i < nr_bins; i++)
                data[start + i] /= total;
        }
    }

    public class Histogram_A3 : AbstractHistogram
    {
        public const string NAME = "A3";
        public const int BIN_SIZE = 10;
        public const float MIN = 0;
        public const float MAX = (float)Math.PI;

        public Histogram_A3() : base(MIN, MAX, BIN_SIZE) { }

        public static string ToCSVHeader() =>
            ToCSVHeader(NAME, MIN, MAX, BIN_SIZE);

        protected override void Sample(Mesh mesh, Random rand)
        {
            //For A3, sample the angle between 3 random vertices.
            Vector3 v1 = mesh.Sample(rand);
            Vector3 v2 = mesh.Sample(rand);
            Vector3 v3 = mesh.Sample(rand);
            Vector3 ab = v2 - v1;
            Vector3 bc = v3 - v2;
            if (ab.Length * bc.Length == 0)
            {
                AddData(0);
                return;
            }

            AddData(Vector3.CalculateAngle(ab, bc));
        }
    }

    public class Histogram_D1 : AbstractHistogram
    {
        public const string NAME = "D1";
        public const int BIN_SIZE = 10;
        public const float MIN = 0;
        public const float MAX = 1;

        public Histogram_D1() : base(MIN, MAX, BIN_SIZE) { }

        public static string ToCSVHeader() =>
            ToCSVHeader(NAME, MIN, MAX, BIN_SIZE);

        protected override void Sample(Mesh mesh, Random rand)
        {
            //For D1, sample the distance between the barycentre and a random vertex.
            //The barycenter is normalized! It is always at (0,0,0)!
            Vector3 v = mesh.Sample(rand);
            AddData(v.Length);
        }
    }

    public class Histogram_D2 : AbstractHistogram
    {
        public const string NAME = "D2";
        public const int BIN_SIZE = 10;
        public const float MIN = 0;
        public const float MAX = 1;

        public Histogram_D2() : base(MIN, MAX, BIN_SIZE) { }

        public static string ToCSVHeader() =>
            ToCSVHeader(NAME, MIN, MAX, BIN_SIZE);

        protected override void Sample(Mesh mesh, Random rand)
        {
            //For D2, sample the distance between two vertices.
            Vector3 v1 = mesh.Sample(rand);
            Vector3 v2 = mesh.Sample(rand);
            Vector3 ab = v2 - v1;
            AddData(ab.Length);
        }
    }

    public class Histogram_D3 : AbstractHistogram
    {
        public const string NAME = "D3";
        public const int BIN_SIZE = 10;
        public const float MIN = 0;
        public const float MAX = 1;

        public Histogram_D3() : base(MIN, MAX, BIN_SIZE) { }

        public static string ToCSVHeader() =>
            ToCSVHeader(NAME, MIN, MAX, BIN_SIZE);

        protected override void Sample(Mesh mesh, Random rand)
        {
            //For D3, sample the  square root of area of triangle given by 3 vertices.
            Vector3 v1 = mesh.Sample(rand);
            Vector3 v2 = mesh.Sample(rand);
            Vector3 v3 = mesh.Sample(rand);
            AddData((float)Math.Sqrt(Face.CalculateArea(v1, v2, v3)));
        }
    }

    public class Histogram_D4 : AbstractHistogram
    {
        public const string NAME = "D4";
        public const int BIN_SIZE = 10;
        public const float MIN = 0;
        public const float MAX = 1;

        public Histogram_D4() : base(MIN, MAX, BIN_SIZE) { }

        public static string ToCSVHeader() =>
            ToCSVHeader(NAME, MIN, MAX, BIN_SIZE);

        protected override void Sample(Mesh mesh, Random rand)
        {
            //For D4, sample cube root of volume of tetrahedron formed by 4 random vertices
            // https://math.stackexchange.com/questions/3616760/how-to-calculate-the-volume-of-tetrahedron-given-by-4-points
            Vector4 v1 = new Vector4(mesh.Sample(rand), 1);
            Vector4 v2 = new Vector4(mesh.Sample(rand), 1);
            Vector4 v3 = new Vector4(mesh.Sample(rand), 1);
            Vector4 v4 = new Vector4(mesh.Sample(rand), 1);
            Matrix4 m = new Matrix4(v1, v2, v3, v4);
            double area = Math.Abs(m.Determinant / 6.0);
            AddData((float)Math.Pow(Math.Abs(area), 1.0 / 3.0));
        }
    }
}
