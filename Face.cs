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
                        return Vector3.Cross(p2 - p1, p3 - p1).Length / 2;
                    }
                default:
                    throw new NotImplementedException($"The area for a face with {indices.Count} indices has not been implemented");
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
    }

}
