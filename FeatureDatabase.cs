﻿using System;
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
    }
}
