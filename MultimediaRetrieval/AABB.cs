using OpenTK;
using System.Linq;

namespace MultimediaRetrieval
{
    public struct AABB
    {
        public Vector3 min;
        public Vector3 max;

        public AABB(Vector3 min, Vector3 max)
        {
            this.min = min;
            this.max = max;
        }

        public AABB(Mesh m)
        {
            this.min = new Vector3(float.PositiveInfinity);
            this.max = new Vector3(float.NegativeInfinity);
            foreach (Vector3 v in m.vertices.Select((a) => a.position))
            {
                if (v.X < min.X)
                    min.X = v.X;
                if (v.Y < min.Y)
                    min.Y = v.Y;
                if (v.Z < min.Z)
                    min.Z = v.Z;

                if (v.X > max.X)
                    max.X = v.X;
                if (v.Y > max.Y)
                    max.Y = v.Y;
                if (v.Z > max.Z)
                    max.Z = v.Z;
            }
        }

        public float DiagonalLength()
        {
            return (min - max).Length;
        }

        public float Volume()
        {
            Vector3 v = max - min;
            return v.X * v.Y * v.Z;
        }

        public static string Headers()
        {
            return string.Join(";",
                "AABB_min_X",
                "AABB_min_Y",
                "AABB_min_Z",
                "AABB_max_X",
                "AABB_max_Y",
                "AABB_max_Z"
                );
        }

        public override string ToString()
        {
            return string.Join(";",
                min.X,
                min.Y,
                min.Z,
                max.X,
                max.Y,
                max.Z
                );
        }
    }

}
