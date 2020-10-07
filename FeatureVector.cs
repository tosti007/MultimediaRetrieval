using System;

namespace MultimediaRetrieval
{
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

            for (int i = 0; i < a.data.Length; i++)
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
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = f(data[i]);
            }
        }

        //Normalize the feature vector using the average and a standard deviation from a featuredatabase:
        public void Normalize(FeatureDatabase db)
        {
            if (data.Length != db.Average.data.Length || data.Length != db.StandardDev.data.Length)
                throw new Exception("Attempted to normalize with FeatureVectors of different length.");

            for (int i = 0; i < data.Length; i++)
            {
                if (db.StandardDev.data[i] != 0)
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
