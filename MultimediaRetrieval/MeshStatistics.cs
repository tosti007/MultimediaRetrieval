using System;
using OpenTK;

namespace MultimediaRetrieval
{
    public class MeshStatistics
    {
        public uint ID;
        public string Classification;
        public int VertexCount, FaceCount;
        public FaceType FaceType;
        public AABB BoundingBox;
        public FeatureVector Features;

        private MeshStatistics()
        {
            // Nothing
        }

        public MeshStatistics(Mesh mesh)
        {
            //The constructor for Query meshes:
            ID = 0;
            Classification = "?";

            // Generate more features
            GenerateFeatures(mesh);
        }

        public MeshStatistics(DatabaseReader reader, string filepath)
        {
            ID = DatabaseReader.GetId(filepath);
            Classification = reader[ID];

            // Generate more features
            GenerateFeatures(Mesh.ReadMesh(filepath));
        }

        void GenerateFeatures(Mesh mesh)
        {
            VertexCount = mesh.vertices.Count;
            FaceCount = mesh.faces.Count;

            FaceType = Face.CalculateType(mesh.faces);
            BoundingBox = mesh.BoundingBox;

            Features = new FeatureVector(mesh);
        }

        public static string Headers()
        {
            return string.Join(";",
                "ID",
                "Class",
                "#Vertices",
                "#Faces",
                "FaceType",
                AABB.Headers(),
                FeatureVector.Headers()
                );
        }

        public override string ToString()
        {
            return string.Join(";", 
                ID, 
                Classification, 
                VertexCount, 
                FaceCount, 
                FaceType, 
                BoundingBox.ToString(),
                Features.ToString()
                );
        }

        public static MeshStatistics Parse(string input)
        {
            string[] data = input.Split(new char[] { ';' }, 13);
            var stats = new MeshStatistics();

            stats.ID = uint.Parse(data[0]);
            stats.Classification = data[1];
            stats.VertexCount = int.Parse(data[2]);
            stats.FaceCount = int.Parse(data[3]);
            Enum.TryParse(data[4], out stats.FaceType);
            stats.BoundingBox = new AABB(
                new Vector3(float.Parse(data[5]), float.Parse(data[6]), float.Parse(data[7])),
                new Vector3(float.Parse(data[8]), float.Parse(data[9]), float.Parse(data[10]))
                );
            // AABB Volume 
            stats.Features = FeatureVector.Parse(data[12]);

            return stats;
        }
    }
}
