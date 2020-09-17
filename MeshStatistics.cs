using System;
using System.Collections.Generic;
using OpenTK;

namespace MultimediaRetrieval
{
    public class MeshStatistics
    {
        public uint ID;
        public string Classification;
        public int vertexCount, faceCount;
        public FaceType faceType;
        public AABB boundingBox;

        private MeshStatistics()
        {
            // Nothing
        }

        public MeshStatistics(uint id, string classification, string dirpath)
        {
            ID = id;
            Classification = classification;

            // Generate more features
            Mesh mesh = Mesh.ReadMesh(id, dirpath);
            vertexCount = mesh.vertices.Count;
            faceCount = mesh.faces.Count;

            faceType = mesh.faceType;
            boundingBox = mesh.boundingBox;
        }

        public const string Headers = "ID;Class;#Vertices;#Faces;FaceType;AABB min;AABB max";

        public override string ToString()
        {
            return string.Join(";", ID, Classification, vertexCount, faceCount, faceType, boundingBox.min, boundingBox.max);
        }

        public static MeshStatistics Parse(string input)
        {
            string[] data = input.Split(new char[] { ';' });
            var stats = new MeshStatistics();
            stats.ID = uint.Parse(data[0]);
            stats.Classification = data[1];
            stats.vertexCount = int.Parse(data[2]);
            stats.faceCount = int.Parse(data[3]);
            Enum.TryParse(data[4], out stats.faceType);
            stats.boundingBox = new AABB(Utils.ParseVector3(data[5]), Utils.ParseVector3(data[6]));

            return stats;
        }
    }
}
