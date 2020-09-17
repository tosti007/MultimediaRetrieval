using System.Collections.Generic;

namespace MultimediaRetrieval
{
    public class MeshStatistics
    {
        public uint ID;
        public string Classification;

        public MeshStatistics(uint id, string classification)
        {
            ID = id;
            Classification = classification;
        }

        public const string Headers = "ID,Class";

        public override string ToString()
        {
            return string.Join(",", ID, Classification);
        }

        public static MeshStatistics Parse(string input)
        {
            string[] data = input.Split(new char[] { ',' });
            return new MeshStatistics(uint.Parse(data[0]), data[1]);
        }

        public static MeshStatistics Parse(KeyValuePair<uint, string> item)
        {
            return new MeshStatistics(item.Key, item.Value);
        }
    }
}
