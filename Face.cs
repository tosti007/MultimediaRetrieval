using System;
using System.Collections.Generic;
using OpenTK;

namespace MultimediaRetrieval
{
    public enum FaceType
    {
        Tris,
        Quads,
        Mixed
    }

    public struct Face
    {
        public List<uint> indices;
        public Vector3 normal;

        public Face(List<uint> indices, Vector3 normal)
        {
            this.indices = indices;
            this.normal = normal;
        }

        public float CalculateArea(ref List<Vertex> vertices)
        {
            // https://math.stackexchange.com/a/1951650
            switch (indices.Count)
            {
                case 3:
                    {
                        Vector3 p1 = vertices[(int)indices[0]].position;
                        Vector3 p2 = vertices[(int)indices[1]].position;
                        Vector3 p3 = vertices[(int)indices[2]].position;
                        return CalculateArea(p1, p2, p3);
                    }
                default:
                    throw new NotImplementedException($"The area for a face with {indices.Count} indices has not been implemented");
            }
        }

        public static float CalculateArea(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            return Vector3.Cross(p2 - p1, p3 - p1).Length / 2;
        }

        public float CalculateSignedVolume(ref List<Vertex> vertices)
        {
            //https://stackoverflow.com/questions/1406029/how-to-calculate-the-volume-of-a-3d-mesh-object-the-surface-of-which-is-made-up
            switch (indices.Count)
            {
                case 3:
                    {
                        Vector3 p1 = vertices[(int)indices[0]].position;
                        Vector3 p2 = vertices[(int)indices[1]].position;
                        Vector3 p3 = vertices[(int)indices[2]].position;
                        var v321 = p3.X * p2.Y * p1.Z;
                        var v231 = p2.X * p3.Y * p1.Z;
                        var v312 = p3.X * p1.Y * p2.Z;
                        var v132 = p1.X * p3.Y * p2.Z;
                        var v213 = p2.X * p1.Y * p3.Z;
                        var v123 = p1.X * p2.Y * p3.Z;
                        return (1.0f / 6.0f) * (-v321 + v231 + v312 - v132 - v213 + v123);
                    }
                default:
                    throw new NotImplementedException($"The signed volume for a face with {indices.Count} indices has not been implemented");
            }
        }

        public static FaceType CalculateType(List<Face> faces)
        {
            bool tris = false;
            bool quads = false;

            foreach (Face f in faces)
            {
                switch (f.indices.Count)
                {
                    case 3:
                        if (quads)
                            return FaceType.Mixed;
                        tris = true;
                        break;
                    case 4:
                        if (tris)
                            return FaceType.Mixed;
                        quads = true;
                        break;
                    default:
                        return FaceType.Mixed;
                }
            }

            if (tris)
                return FaceType.Tris;
            if (quads)
                return FaceType.Quads;
            return FaceType.Mixed;
        }

        public static Vector3 CalculateNormal(List<uint> indices, ref List<Vertex> vertices)
        {
            Vector3 facenorm = new Vector3(0);
            int v1_index = (int)indices[indices.Count - 1];
            for (int i = 0; i < indices.Count; i++)
            {
                int v2_index = (int)indices[i];
                facenorm.X += (vertices[v1_index].position.Y - vertices[v2_index].position.Y) * (vertices[v1_index].position.Z + vertices[v1_index].position.Z);
                facenorm.Y += (vertices[v1_index].position.Z - vertices[v2_index].position.Z) * (vertices[v1_index].position.X + vertices[v2_index].position.X);
                facenorm.Z += (vertices[v1_index].position.X - vertices[v2_index].position.X) * (vertices[v1_index].position.Y + vertices[v2_index].position.Y);
                v1_index = v2_index;
            }
            return facenorm.Normalized();
        }
    }

}
