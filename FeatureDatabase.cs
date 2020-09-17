using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace MultimediaRetrieval
{
    public class FeatureDatabase
    {
        List<MeshStatistics> meshes;

        private FeatureDatabase()
        {
            meshes = new List<MeshStatistics>();
        }
        public FeatureDatabase(DatabaseReader classes, string dirpath)
        {
            meshes = new List<MeshStatistics>(classes.Items.Count);

            foreach (var item in classes.Items)
            {
                meshes.Add(new MeshStatistics(item.Key, item.Value, dirpath));
            }
        }

        public void WriteToFile(string filepath)
        {
            DatabaseReader.WriteToMRFile(filepath, MeshStatistics.Headers, meshes.OrderBy((cls) => cls.ID));
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
    }
}
