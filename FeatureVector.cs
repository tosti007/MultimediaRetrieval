using System;

namespace MultimediaRetrieval
{
    public class FeatureVector
    {
        public float[] _data;

        private FeatureVector(float[] data)
        {
            this._data = data;
        }

        public FeatureVector(MeshStatistics m)
        {
            _data = new float[5 + m.a3.Bins + m.d1.Bins + m.d2.Bins + m.d3.Bins + m.d4.Bins];
            _data[0] = m.SurfaceArea;
            _data[1] = m.Diameter;
            _data[2] = m.Eccentricity;
            _data[3] = m.Compactness;
            _data[4] = m.Volume;

            int histoIndex = 5;

            m.a3.AsPercentage().CopyTo(_data, histoIndex);
            histoIndex += m.a3.Bins;

            m.d1.AsPercentage().CopyTo(_data, histoIndex);
            histoIndex += m.d1.Bins;

            m.d2.AsPercentage().CopyTo(_data, histoIndex);
            histoIndex += m.d2.Bins;

            m.d3.AsPercentage().CopyTo(_data, histoIndex);
            histoIndex += m.d3.Bins;

            m.d4.AsPercentage().CopyTo(_data, histoIndex);
        }

        public static FeatureVector operator +(FeatureVector a, FeatureVector b)
        {
            if (a._data.Length != b._data.Length)
                throw new Exception("Attempted to add two FeatureVectors of different length.");

            for (int i = 0; i < a._data.Length; i++)
                a._data[i] += b._data[i];

            return a;
        }

        public static FeatureVector operator -(FeatureVector a, FeatureVector b)
        {
            if (a._data.Length != b._data.Length)
                throw new Exception("Attempted to subtract two FeatureVectors of different length.");

            for (int i = 0; i < a._data.Length; i++)
                a._data[i] -= b._data[i];

            return a;
        }

        public void Map(Func<float, float> f)
        {
            for (int i = 0; i < _data.Length; i++)
            {
                _data[i] = f(_data[i]);
            }
        }

        //Normalize the feature vector using the average and a standard deviation from a featuredatabase:
        public void Normalize(FeatureDatabase db)
        {
            if (_data.Length != db.Average._data.Length || _data.Length != db.StandardDev._data.Length)
                throw new Exception("Attempted to normalize with FeatureVectors of different length.");

            for (int i = 0; i < _data.Length; i++)
            {
                if (db.StandardDev._data[i] != 0)
                    _data[i] = (_data[i] - db.Average._data[i]) / db.StandardDev._data[i];
                else
                {
                    _data[i] = (_data[i] - db.Average._data[i]);
                    //Console.WriteLine($"Sdev was 0 at index {i}! Did not use it for normalization."); 
                    //TODO: Make sure this doesn't happen by changing histo-bins for example.
                }
            }
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
    }
}
