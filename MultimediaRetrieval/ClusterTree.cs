using System;
using System.Linq;
using System.Collections.Generic;

namespace MultimediaRetrieval
{
    public class ClusterTree
    {
        List<ClusterGroup> Clusters;
        List<ClusterNode> Nodes;
        DistanceFunction[] Functions;

        public ClusterTree(DistanceFunction[] f, List<MeshStatistics> meshes, int k)
        {
            if (meshes.Count < k)
                throw new Exception("Cannot create a clustertree with more clusters than elements!");

            Functions = f;
            Nodes = meshes.Select((m) => new ClusterNode(m)).ToList();
            Clusters = new List<ClusterGroup>(k)
            {
                new ClusterGroup(Nodes[0])
            };

            while (Clusters.Count < k)
            {
                float max = float.NegativeInfinity;
                ClusterNode largest = null;
                foreach (var n in Nodes)
                {
                    float min = float.PositiveInfinity;
                    foreach (var c in Clusters)
                    {
                        if (n == c.Center)
                        {
                            min = float.NegativeInfinity;
                            break;
                        }
                        float d = ClusterNode.Distance(Functions, n, c.Center);
                        if (d < min)
                            d = min;
                    }
                    if (min > max)
                    {
                        max = min;
                        largest = n;
                    }
                }

                if (largest == null)
                    throw new Exception("ClusterNode is null where it shouldn't be!");

                Clusters.Add(new ClusterGroup(largest));
            }

            foreach (var n in Nodes)
                n.UpdateCluster(Functions, Clusters);

            bool changed = true;
            while (changed)
            {
                changed = false;
                foreach (var c in Clusters)
                    changed |= c.UpdateCenter(Functions);
                foreach (var n in Nodes)
                    changed |= n.UpdateCluster(Functions, Clusters);
            }
        }

        public void Print()
        {
            Console.WriteLine("Cluster with {0} clusters and {1} nodes", Clusters.Count, Nodes.Count);
            for (int i = 0; i < Clusters.Count; i++)
            {
                Console.WriteLine("Cluster {0} with center {1}", i + 1, Clusters[i].Center.Mesh.ID);
                foreach (var n in Clusters[i].Items)
                {
                    Console.WriteLine("\tMesh {0}", n.Mesh.ID);
                }
            }
        }

        public IEnumerable<MeshStatistics> Search(FeatureVector query, int k)
        {
            var sorted = Clusters.AsParallel()
                .OrderBy((c) => query.Distance(Functions, c.Center.Mesh.Features))
                .AsSequential();

            foreach (var g in sorted)
            {
                foreach (var n in g.Items)
                    yield return n.Mesh;
                k -= g.Items.Count;
                if (k <= 0)
                    break;
            }
        }
    }

    public class ClusterGroup
    {
        public ClusterNode Center;
        public List<ClusterNode> Items;

        public ClusterGroup() : this(null) { }
        public ClusterGroup(ClusterNode n)
        {
            Items = new List<ClusterNode>();
            Center = n;
            if (n != null)
            {
                n.Cluster = this;
                Items.Add(n);
            }
        }

        public bool UpdateCenter(DistanceFunction[] functions)
        {
            float distance = float.PositiveInfinity;
            ClusterNode smallest = null;
            foreach (var n1 in Items)
            {
                float sum = 0;
                foreach (var n2 in Items)
                    sum += ClusterNode.Distance(functions, n1, n2);
                if (distance > sum)
                {
                    distance = sum;
                    smallest = n1;
                }
            }
            if (smallest != Center)
            {
                //if (smallest == null)
                //    Console.Error.WriteLine("UPDATECENTER SETTING TO NULL {0}", nodes.Count());
                Center = smallest;
                return true;
            }
            return false;
        }
    }

    public class ClusterNode
    {
        public MeshStatistics Mesh;
        public ClusterGroup Cluster;
        private Dictionary<uint, float> _distances;

        public ClusterNode(MeshStatistics m) : this(m, null) { }
        public ClusterNode(MeshStatistics m, ClusterGroup cluster)
        {
            Mesh = m;
            Cluster = cluster;
            if (cluster != null)
                Cluster.Items.Add(this);
            _distances = new Dictionary<uint, float>();
        }

        public bool UpdateCluster(DistanceFunction[] functions, List<ClusterGroup> clusters)
        {
            ClusterGroup smallest = null;
            float distance = float.PositiveInfinity;
            foreach (var c in clusters)
            {
                //if (c.Center == null)
                //    Console.Error.WriteLine("OHBOI");
                float d = Distance(functions, this, c.Center);
                if (d < distance)
                {
                    smallest = c;
                    distance = d;
                }
            }
            if (smallest != Cluster)
            {
                if (smallest != null)
                    smallest.Items.Remove(this);
                //if (smallest == null)
                //    Console.Error.WriteLine("UPDATECLUSTER SETTING TO NULL");
                Cluster = smallest;
                Cluster.Items.Add(this);
                return true;
            }
            return false;
        }

        public static float Distance(DistanceFunction[] functions, ClusterNode a, ClusterNode b)
        {
            if (a.Mesh.ID > b.Mesh.ID) return Distance(functions, b, a);

            if (!a._distances.ContainsKey(b.Mesh.ID))
                a._distances.Add(b.Mesh.ID, FeatureVector.Distance(functions, a.Mesh.Features, b.Mesh.Features));

            return a._distances[b.Mesh.ID];
        }
    }
}
