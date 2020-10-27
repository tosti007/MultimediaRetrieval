using System;
using System.IO;
using System.Collections.Generic;

namespace MultimediaRetrieval
{
    public class DatabaseReader : Dictionary<uint, string>
    {
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

                    string[] split = line.Split(new char[] { ';' });

                    total.Add(uint.Parse(split[0]), split[1]);
                }
            }

            return total;
        }

        public static IEnumerable<string> ListMeshes(string dirpath)
        {
            foreach (string filename in Directory.EnumerateFiles(dirpath, "*", SearchOption.AllDirectories))
            {
                string extension = Path.GetExtension(filename);
                if (extension.EndsWith("~", StringComparison.InvariantCulture))
                    continue;

                if (string.Equals(extension, ".mr"))
                    continue;

                yield return Path.GetFullPath(filename);                
            }
        }

        public static uint GetId(string filename)
        {
            return uint.Parse(Path.GetFileNameWithoutExtension(filename).TrimStart(new char[] { 'm' }));
        }
    }
}
