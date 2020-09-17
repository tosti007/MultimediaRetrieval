using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace MultimediaRetrieval
{
    public class DatabaseReader
    {
        // There are 1815 items in the db, but LPSB starts with 1 so...
        private const int NR_PRINCETON_MESHES = 1814;
        private static readonly string[] UNSUPPORTED_EXTENSIONS = { 
            ".txt", ".jpg", ".cla", ".gitkeep", ".mr", ".zip", ".7z"
        };

        public Dictionary<uint, string> Items = new Dictionary<uint, string>();

        public static DatabaseReader operator +(DatabaseReader a, DatabaseReader b)
        {
            if (a.Items.Count < b.Items.Count)
                return b + a;

            foreach(var item in b.Items)
                a.Items.Add(item.Key, item.Value);

            return a;
        }

        public void WriteToFile(string filepath)
        {
            WriteToMRFile(filepath, "ID,Class", Items.OrderBy((cls) => cls.Key).Select((cls) => cls.Key+","+cls.Value));
        }

        public static DatabaseReader ReadFromFile(string filepath)
        {
            DatabaseReader total = new DatabaseReader();

            using (StreamReader file = new StreamReader(filepath))
            {
                string line = file.ReadLine();
                while ((line = file.ReadLine()) != null)
                {
                    if (line == string.Empty)
                        continue;

                    string[] split = line.Split(new char[] { ',' });

                    total.Items.Add(uint.Parse(split[0]), split[1]);
                }
            }

            return total;
        }

        public static DatabaseReader ParseClassification(string dirpath, string outpath)
        {
            DatabaseReader total = new DatabaseReader();

            string[] files = Directory.GetFiles(dirpath, "*.mr");
            if (files.Length > 0)
            {
                throw new ArgumentException("This folder contains already a statistic file");
            }

            files = Directory.GetFiles(dirpath, "*.cla");
            if (files.Length > 0)
            {
                foreach (string filepath in files)
                {
                    ParseClassificationPrinceton(total, dirpath, filepath, outpath);
                }
                return total;
            }

            // Assuming it's LPSB since we could not find any class file
            ParseClassificationLPSB(total, dirpath, outpath);
            return total; 
        }

        private static void ParseClassificationLPSB(DatabaseReader total, string dirpath, string outpath)
        {
            foreach(string filename in ListMeshes(dirpath))
            {
                string cls = Path.GetFileName(Path.GetDirectoryName(filename)).ToLower();
                if (cls == string.Empty)
                    continue;

                uint id = GetId(filename) + NR_PRINCETON_MESHES;
                total.Items.Add(id, cls);

                MoveFile(filename, outpath, id);
            }
        }

        private static void ParseClassificationPrinceton(DatabaseReader total, string dirpath, string filepath, string outpath)
        {
            Dictionary<uint, string> tmp = new Dictionary<uint, string>();

            using (StreamReader file = new StreamReader(filepath))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    if (line == string.Empty)
                        continue;

                    if (line.StartsWith("PSB", System.StringComparison.InvariantCulture))
                        continue;

                    string[] split = line.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);

                    if (split.Length != 3)
                        continue;

                    string cls = split[0].ToLower();
                    int amount = int.Parse(split[2]);

                    for(int i = 0; i < amount; i++)
                    {
                        line = file.ReadLine();
                        tmp.Add(uint.Parse(line), cls);
                    }
                }
            }


            foreach (string filename in ListMeshes(dirpath))
            {
                uint id = GetId(filename);

                if (!tmp.TryGetValue(id, out string cls))
                    continue;

                total.Items.Add(id, cls);

                MoveFile(filename, outpath, id);
            }
        }

        private static void MoveFile(string filename, string outpath, uint id)
        {
            string newfile = outpath + Path.DirectorySeparatorChar + id + Path.GetExtension(filename);
            if (!File.Exists(newfile))
                File.Copy(filename, Path.GetFullPath(newfile), false);
        }

        public static IEnumerable<string> ListMeshes(string dirpath)
        {
            foreach (string filename in Directory.EnumerateFiles(dirpath, "*", SearchOption.AllDirectories))
            {
                string extension = Path.GetExtension(filename);
                if (extension.EndsWith("~", StringComparison.InvariantCulture))
                    continue;

                if (UNSUPPORTED_EXTENSIONS.Any((s) => string.Equals(s, extension)))
                    continue;

                yield return Path.GetFullPath(filename);                
            }
        }

        public static uint GetId(string filename)
        {
            return uint.Parse(Path.GetFileNameWithoutExtension(filename).TrimStart(new char[] { 'm' }));
        }

        public static void WriteToMRFile<T>(string filepath, string headers, IEnumerable<T> data)
        {
            // check for endswith .mr
            if (!filepath.EndsWith(".mr", StringComparison.InvariantCulture))
                filepath += ".mr";

            File.WriteAllLines(filepath, new string[] { headers }.Concat(
                data.Select((cls) => cls.ToString())
                ));
        }
    }
}
