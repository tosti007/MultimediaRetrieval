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
            ".txt", ".jpg", ".cla", ".gitkeep", ".mr" 
        };

        private Dictionary<uint, string> classes = new Dictionary<uint, string>();

        public static DatabaseReader operator +(DatabaseReader a, DatabaseReader b)
        {
            if (a.classes.Count < b.classes.Count)
                return b + a;

            foreach(var item in b.classes)
                a.classes.Add(item.Key, item.Value);

            return a;
        }


        public static DatabaseReader ReadClassification(string dirpath, string outpath)
        {
            DatabaseReader total = new DatabaseReader();

            string[] files = Directory.GetFiles(dirpath, "*.mr", SearchOption.TopDirectoryOnly);
            if (files.Length > 0)
            {
                throw new System.ArgumentException("This folder contains already a statistic file");
            }

            files = Directory.GetFiles(dirpath, "*.cla", SearchOption.TopDirectoryOnly);
            if (files.Length > 0)
            {
                foreach (string filepath in files)
                {
                    ReadClassificationPrinceton(total, dirpath, dirpath + filepath, outpath);
                }
                return total;
            }

            // Assuming it's LPSB since we could not find any class file
            ReadClassificationLPSB(total, dirpath, outpath);
            return total; 
        }

        private static void ReadClassificationLPSB(DatabaseReader total, string dirpath, string outpath)
        {
            foreach(string filename in ListMeshes(dirpath))
            {
                string cls = Path.GetFileName(Path.GetDirectoryName(filename)).ToLower();
                if (cls == string.Empty)
                    continue;

                uint id = GetId(filename) + NR_PRINCETON_MESHES;
                total.classes.Add(id, cls);

                MoveFile(filename, outpath, id);
            }
        }

        private static void ReadClassificationPrinceton(DatabaseReader total, string dirpath, string filepath, string outpath)
        {
            Dictionary<uint, string> tmp = new Dictionary<uint, string>();

            using (StreamReader file = new StreamReader(filepath))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    if (line == string.Empty)
                        continue;

                    string[] split = line.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);

                    // If it is any other line, such as header, ignore it.
                    if (split.Length != 3)
                        continue;

                    string cls = split[0];
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

                total.classes.Add(id, cls);

                MoveFile(filename, outpath, id);
            }
        }

        public static IEnumerable<string> ListMeshes(string dirpath)
        {
            foreach (string filename in Directory.EnumerateFiles(dirpath))
            {
                string fullname = dirpath + filename;
                string extension = Path.GetExtension(fullname);

                if (UNSUPPORTED_EXTENSIONS.Any((s) => s == extension))
                    continue;

                yield return fullname;                
            }
        }

        public static uint GetId(string filename)
        {
            return uint.Parse(Path.GetFileNameWithoutExtension(filename).TrimStart(new char[] { 'm' }));
        }

        private static void MoveFile(string filename, string outpath, uint id)
        {
            string newfile = outpath + Path.DirectorySeparatorChar + id + Path.GetExtension(filename);
            File.Copy(filename, newfile, false);
        }
    }
}
