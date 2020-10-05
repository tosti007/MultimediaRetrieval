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

            File.WriteAllLines(filepath, new string[] { meshes[0].Headers() }.Concat(
                meshes.OrderBy((cls) => cls.ID).Select((cls) => cls.ToString())
                ));
        }

        public static FeatureDatabase ReadFrom(string filepath, string dirpath)
        {
            Dictionary<uint, MeshStatistics> tmp = new Dictionary<uint, MeshStatistics>();

            using (StreamReader file = new StreamReader(filepath))
            {
                // Ignore the first line with headers
                string line = file.ReadLine();
                while ((line = file.ReadLine()) != null)
                {
                    if (line == string.Empty)
                        continue;

                    var item = MeshStatistics.Parse(line);
                    tmp.Add(item.ID, item);
                }
            }

            FeatureDatabase result = new FeatureDatabase();

            foreach (string filename in DatabaseReader.ListMeshes(dirpath))
            {
                uint id = DatabaseReader.GetId(filename);

                if (!tmp.TryGetValue(id, out var item))
                    continue;

                result.meshes.Add(item);
            }

            return result;
        }

        public FeatureVector Average
        {
            get
            {
                if (_avg != null)
                    return _avg;

                //Create the average FeatureVector.
                _avg = new FeatureVector(meshes[0]);
                for (int i = 1; i < meshes.Count; i++)
                {
                    _avg += new FeatureVector(meshes[i]);
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

                _std = new FeatureVector(meshes[0]) - Average;
                _std.Map(f => f * f);
                for (int i = 1; i < meshes.Count; i++)
                {
                    FeatureVector x = new FeatureVector(meshes[0]) - _avg;
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
