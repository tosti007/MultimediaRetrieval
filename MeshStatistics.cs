using System.Collections.Generic;

namespace MultimediaRetrieval
{
    public class MeshStatistics
    {
        public uint ID;
        public string Classification;

        private MeshStatistics()
        {
            // Nothing
        }

        public MeshStatistics(uint id, string classification, string dirpath)
        {
            ID = id;
            Classification = classification;

            // Generate more features
            //Mesh m = Mesh.ReadMesh(id, dirpath);
        }

        public const string Headers = "ID,Class";

        public override string ToString()
        {
            return string.Join(",", ID, Classification);
        }

        public static MeshStatistics Parse(string input)
        {
            string[] data = input.Split(new char[] { ',' });
            var stats = new MeshStatistics();
            stats.ID = uint.Parse(data[0]);
            stats.Classification = data[1];

            return stats;
        }
    }
}
