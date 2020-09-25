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
        public float surface_area;

        private MeshStatistics()
        {
            // Nothing
        }

        public MeshStatistics(DatabaseReader reader, string filepath)
        {
            ID = DatabaseReader.GetId(filepath);
            Classification = reader.Items[ID];

            // Generate more features
            Mesh mesh = Mesh.ReadMesh(filepath);
            vertexCount = mesh.vertices.Count;
            faceCount = mesh.faces.Count;

            faceType = Face.CalculateType(mesh.faces);
            boundingBox = new AABB(mesh);

            foreach (Face f in mesh.faces)
            {
                surface_area += f.CalculateArea(ref mesh.vertices);
            }
        }

        public const string Headers = 
            "ID;" +
            "Class;" + 
            "#Vertices;" + 
            "#Faces;" + 
            "FaceType;" + 
            "AABB_min_X;" + 
            "AABB_min_Y;" + 
            "AABB_min_Z;" + 
            "AABB_max_X;" + 
            "AABB_max_Y;" + 
            "AABB_max_Z;" + 
            "AABB_Volume;" + 
            "Surface_Area";

        public override string ToString()
        {
            return string.Join(";", 
                ID, 
                Classification, 
                vertexCount, 
                faceCount, 
                faceType, 
                boundingBox.min.X, 
                boundingBox.min.Y, 
                boundingBox.min.Z, 
                boundingBox.max.X, 
                boundingBox.max.Y, 
                boundingBox.max.Z, 
                boundingBox.Volume(), 
                surface_area
                );
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
            stats.boundingBox = new AABB(
                new Vector3(float.Parse(data[5]), float.Parse(data[6]), float.Parse(data[7])),
                new Vector3(float.Parse(data[8]), float.Parse(data[9]), float.Parse(data[10]))
                );
            // AABB Volume 
            stats.surface_area = float.Parse(data[12]);

            return stats;
        }
    }
}
