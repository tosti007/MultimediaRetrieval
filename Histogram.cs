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
            for(int i = 0; i < bins; i++)
            {
                if(f >= (i * ((max - min) / bins) + min) && f < ((i + 1) * ((max - min) / bins) + min))
                {
                    data[i]++;
                    return;
                }
            }
        }
    }
}
