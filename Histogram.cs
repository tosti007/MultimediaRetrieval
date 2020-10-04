using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultimediaRetrieval
{
    //A histogram class for the distribution features in the MeshStatistics
    class Histogram
    {
        string title;
        int bins;
        float min, max;

        int[] data;
        public Histogram(string title, float min, float max, int bins)
        {
            this.title = title;
            this.min = min;
            this.max = max;
            this.bins = bins;

            data = new int[bins];
        }

        //Loads in a Histogram from string data, starting at the given start index.
        public void LoadData(string[] sdata, int start)
        {
            for(int i = 0; i < bins; i++)
            {
                data[i] = int.Parse(sdata[start + i]);
            }
        }

        public string ToCSV()
        {
            string[] result = new string[bins];
            for (int i = 0; i < bins; i++)
            {
                result[i] = data[i].ToString();
            }
            return result.Aggregate((pstring, csv) => $"{pstring};{csv}");
        }

        public string ToCSVHeader()
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
                throw new Exception($"Data {f} was out of range of min: {min} and max: {max} of Histogram {title}.");

            float diff = max - min;
            float step = diff / bins;
            int index = (int)Math.Floor((f - min) / step);

            if (index == data.Length)
                index--;

            data[index]++;
        }
    }
}
