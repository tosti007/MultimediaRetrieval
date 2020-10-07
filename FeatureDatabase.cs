using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Accord.Statistics.Testing;

namespace MultimediaRetrieval
{
    public class FeatureDatabase
    {
        public List<MeshStatistics> meshes;
        private FeatureVector _avg;
        private FeatureVector _std;

        private FeatureDatabase()
        {
            meshes = new List<MeshStatistics>();
        }
        public FeatureDatabase(DatabaseReader classes, string dirpath)
        {
            meshes = DatabaseReader.ListMeshes(dirpath).AsParallel().Select((file) => {
                Console.WriteLine("Generating features for {0}", file);
                return new MeshStatistics(classes, file);
            }).ToList();
        }

        public void WriteToFile(string filepath)
        {
            // check for endswith .mr
            if (!filepath.EndsWith(".mr", StringComparison.InvariantCulture))
                filepath += ".mr";

            File.WriteAllLines(filepath, new string[] { MeshStatistics.Headers() }.Concat(
                meshes.OrderBy((cls) => cls.ID).Select((cls) => cls.ToString())
                ));
        }

        public static FeatureDatabase ReadFrom(string filepath)
        {
            FeatureDatabase result = new FeatureDatabase();

            using (StreamReader file = new StreamReader(filepath))
            {
                // Ignore the first line with headers
                string line = file.ReadLine();
                while ((line = file.ReadLine()) != null)
                {
                    if (line == string.Empty)
                        continue;

                    var item = MeshStatistics.Parse(line);
                    result.meshes.Add(item);
                }
            }

            return result;
        }

        public void Filter(string dirpath)
        {
            HashSet<uint> ids = new HashSet<uint>(DatabaseReader.ListMeshes(dirpath).Select(DatabaseReader.GetId));
            List<MeshStatistics> nm = new List<MeshStatistics>(meshes.Count);
            foreach (MeshStatistics stats in meshes)
            {
                if (ids.Contains(stats.ID))
                {
                    nm.Add(stats);
                    ids.Remove(stats.ID);
                }
            }
            meshes = nm;
            meshes.TrimExcess();
        }

        public FeatureVector Average
        {
            get
            {
                if (_avg != null)
                    return _avg;

                //Create the average FeatureVector.
                _avg = new FeatureVector();
                for (int i = 0; i < meshes.Count; i++)
                {
                    _avg += meshes[i].Features;
                }

                _avg.Map(f => f / meshes.Count);
                return _avg;
            }
        }

        public FeatureVector StandardDev
        {
            get
            {
                if (_std != null)
                    return _std;

                _std = new FeatureVector();
                for (int i = 0; i < meshes.Count; i++)
                {
                    FeatureVector x = meshes[i].Features - _avg;
                    x.Map(f => f * f);
                    _std += x;
                }
                _std.Map(f => f / (meshes.Count - 1));
                _std.Map(f => (float)Math.Sqrt(f));
                return _std;
            }
        }
    }
}
