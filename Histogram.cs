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
    public enum HistogramType
    {
        A3,
        D1, 
        D2, 
        D3,
        D4
    }

    public struct Histogram
    {
        public HistogramType Name;
        public int StartIndex;
        public int Bins;
        public float Min, Max;

        public Histogram(HistogramType name, int start, int bins, float min, float max)
        {
            this.Name = name;
            this.StartIndex = start;
            this.Bins = bins;
            this.Min = min;
            this.Max = max;
        }

        public string ToCSV(ref float[] data)
        {
            return string.Join(";", data.Skip(StartIndex).Take(Bins));
        }

        public string Headers()
        {
            string[] result = new string[Bins];
            float diff = (Max - Min) / Bins;
            for (int i = 0; i < Bins; i++)
            {
                float start = i * diff + Min;
                float end = (i + 1) * diff + Min;
                result[i] = $"{Name}({start},{end})";
            }
            return string.Join(";", result);
        }

        public void AsPercentage(ref float[] data)
        {
            float total = 0;

            for (int i = 0; i < Bins; i++)
                total += data[StartIndex + i];

            if (total == 0)
                return;

            for (int i = 0; i < Bins; i++)
                data[StartIndex + i] /= total;
        }

        public void AddData(ref float[] data, float f)
        {
            if (f < Min || f > Max)
                throw new Exception($"Data {f} was out of range of min: {Min} and max: {Max}.");

            float diff = Max - Min;
            float step = diff / Bins;
            int index = (int)Math.Floor((f - Min) / step);

            if (index == Bins)
                index--;

            data[StartIndex + index]++;
        }

        public void Sample(ref float[] data, Mesh mesh, Random rand, int nr_samples)
        {
            for (int i = 0; i < nr_samples; i++)
                Sample(ref data, mesh, rand);
        }

        public void Sample(ref float[] data, Mesh mesh, Random rand)
        {
            float sample;
            switch (Name)
            {
                case HistogramType.A3:
                    sample = Sample_A3(mesh, rand);
                    break;
                case HistogramType.D1:
                    sample = Sample_D1(mesh, rand);
                    break;
                case HistogramType.D2:
                    sample = Sample_D2(mesh, rand);
                    break;
                case HistogramType.D3:
                    sample = Sample_D3(mesh, rand);
                    break;
                case HistogramType.D4:
                    sample = Sample_D4(mesh, rand);
                    break;
                default:
                    throw new NotImplementedException($"Sample not implemented for histogram {Name}");
            }
            AddData(ref data, sample);
        }

        private float Sample_A3(Mesh mesh, Random rand)
        {
            Vector3 v1 = mesh.Sample(rand);
            Vector3 v2 = mesh.Sample(rand);
            Vector3 v3 = mesh.Sample(rand);
            Vector3 ab = v2 - v1;
            Vector3 bc = v3 - v2;
            if (ab.Length * bc.Length == 0)
                return 0;
            return Vector3.CalculateAngle(ab, bc);
        }

        private float Sample_D1(Mesh mesh, Random rand)
        {
            //For D1, sample the distance between the barycentre and a random vertex.
            //The barycenter is normalized! It is always at (0,0,0)!
            Vector3 v = mesh.Sample(rand);
            return v.Length;
        }

        private float Sample_D2(Mesh mesh, Random rand)
        {
            //For D2, sample the distance between two vertices.
            Vector3 v1 = mesh.Sample(rand);
            Vector3 v2 = mesh.Sample(rand);
            Vector3 ab = v2 - v1;
            return ab.Length;
        }

        private float Sample_D3(Mesh mesh, Random rand)
        {
            //For D3, sample the  square root of area of triangle given by 3 vertices.
            Vector3 v1 = mesh.Sample(rand);
            Vector3 v2 = mesh.Sample(rand);
            Vector3 v3 = mesh.Sample(rand);
            return (float)Math.Sqrt(Face.CalculateArea(v1, v2, v3));
        }

        private float Sample_D4(Mesh mesh, Random rand)
        {
            //For D4, sample cube root of volume of tetrahedron formed by 4 random vertices
            // https://math.stackexchange.com/questions/3616760/how-to-calculate-the-volume-of-tetrahedron-given-by-4-points
            Vector4 v1 = new Vector4(mesh.Sample(rand), 1);
            Vector4 v2 = new Vector4(mesh.Sample(rand), 1);
            Vector4 v3 = new Vector4(mesh.Sample(rand), 1);
            Vector4 v4 = new Vector4(mesh.Sample(rand), 1);
            Matrix4 m = new Matrix4(v1, v2, v3, v4);
            double area = Math.Abs(m.Determinant / 6.0);
            return (float)Math.Pow(Math.Abs(area), 1.0 / 3.0);
        }
    }
}
