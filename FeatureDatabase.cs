using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace MultimediaRetrieval
{
    public class FeatureDatabase
    {
        List<MeshStatistics> meshes;

        public FeatureDatabase(DatabaseReader classes)
        {
            meshes = new List<MeshStatistics>(classes.Items.Count);

            foreach (var item in classes.Items)
            {
                meshes.Add(MeshStatistics.Parse(item));
            }
        }

        private FeatureDatabase()
        {
            meshes = new List<MeshStatistics>();
        }

        public void WriteToFile(string filepath)
        {
            // check for endswith .mr
            if (!filepath.EndsWith(".mr", System.StringComparison.InvariantCulture))
                filepath += ".mr";

            File.WriteAllLines(filepath, new string[] {MeshStatistics.Headers}.Concat(
                meshes.OrderBy((cls) => cls.ID).Select((cls) => cls.ToString())
                ));
        }

        public static FeatureDatabase ReadFromFile(string filepath)
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

            string dirpath = Path.GetDirectoryName(filepath);
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
    }
}
