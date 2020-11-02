using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace MultimediaRetrieval
{
    public class ClusterTree
    {
        public List<ClusterGroup<MeshStatistics>> Clusters;
        public DistanceFunction[] Functions;

        private ClusterTree()
        {
            Clusters = new List<ClusterGroup<MeshStatistics>>();
            Functions = null;
        }
        public ClusterTree(DistanceFunction[] f, List<MeshStatistics> meshes, int k)
        {
            var builder = new ClusterTreeBuilder(f, meshes, k);
            this.Functions = f;
            this.Clusters = builder.ClustersAsStats();
        }

        public void Print()
        {
            Console.WriteLine("Cluster with {0} clusters", Clusters.Count);
            for (int i = 0; i < Clusters.Count; i++)
            {
                Console.WriteLine("Cluster {0} with center {1}", i + 1, Clusters[i].Center.ID);
                foreach (var n in Clusters[i].Items)
                {
                    Console.WriteLine("\tMesh {0}", n.ID);
                }
            }
        }

        public IEnumerable<MeshStatistics> Search(FeatureVector query, int k = 0)
        {
            var sorted = Clusters.AsParallel()
                .OrderBy((c) => query.Distance(Functions, c.Center.Features))
                .AsSequential();

            foreach (var g in sorted)
            {
                foreach (var n in g.Items)
                    yield return n;
                k -= g.Items.Count;
                if (k <= 0)
                    break;
            }
        }

        public void WriteToFile(string filepath)
        {
            File.WriteAllLines(filepath, new[] { "KMediods " + string.Join(" ", Functions) }.Concat(
                Clusters.Select((c) => c.Center.ID + ";" +
                    string.Join(";", c.Items.Select((n) => n.ID)))
                ));
        }

        public static ClusterTree ReadFrom(FeatureDatabase db, string filepath)
        {
            var items = new Dictionary<uint, MeshStatistics>();
            foreach (var m in db.meshes)
                items.Add(m.ID, m);

            ClusterTree result = new ClusterTree();

            using (StreamReader file = new StreamReader(filepath))
            {
                string line = file.ReadLine();
                result.Functions = line.Split(' ').Skip(1).Select((f) => (DistanceFunction)Enum.Parse(typeof(DistanceFunction), f)).Parse();

                while ((line = file.ReadLine()) != null)
                {
                    var data = line.Split(';').Select(uint.Parse);
                    var item = new ClusterGroup<MeshStatistics>(items[data.First()], data.Skip(1).Select((i) => items[i]).ToList());
                    result.Clusters.Add(item);
                }
            }

            return result;
        }
    }

    public class ClusterGroup<T> where T : class
    {
        public T Center;
        public List<T> Items;

        public ClusterGroup() : this(null) { }
        public ClusterGroup(T n) : this (n, new List<T>()) { }
        public ClusterGroup(T n, List<T> items)
        {
            Items = items;
            Center = n;
        }
    }

    public class ClusterNode
    {
        public MeshStatistics Value;
        public ClusterGroup<ClusterNode> Cluster { get; private set; }

        public ClusterNode(MeshStatistics v) : this(v, null) { }
        public ClusterNode(MeshStatistics v, ClusterGroup<ClusterNode> cluster)
        {
            Value = v;
            SetCluster(cluster);
        }

        public void SetCluster(ClusterGroup<ClusterNode> c)
        {
            if (Cluster != c)
            {
                if (Cluster != null)
                    Cluster.Items.Remove(this);
                Cluster = c;
                if (Cluster != null)
                    Cluster.Items.Add(this);
            }
        }
    }

    public class ClusterTreeBuilder
    {
        public List<ClusterGroup<ClusterNode>> Clusters;
        public List<ClusterNode> Nodes;
        public DistanceFunction[] Functions;
        public Dictionary<(uint, uint), float> Distances;

        public ClusterTreeBuilder(DistanceFunction[] f, List<MeshStatistics> meshes, int k)
        {
            if (meshes.Count < k)
                throw new Exception("Cannot create a ClusterTree with more clusters than elements!");

            Console.WriteLine("Building ClusterTree - Start");

            Functions = f;
            Distances = new Dictionary<(uint, uint), float>((meshes.Count - 1) * meshes.Count / 2);
            Nodes = meshes.Select((m) => new ClusterNode(m)).ToList();
            Clusters = new List<ClusterGroup<ClusterNode>>(k)
            {
                new ClusterGroup<ClusterNode>(Nodes[0])
            };

            Console.WriteLine("Building ClusterTree - Init");
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
                        float d = Distance(n, c.Center);
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

                var newgroup = new ClusterGroup<ClusterNode>(largest);
                largest.SetCluster(newgroup);
                Clusters.Add(newgroup);
            }

            foreach (var n in Nodes)
                UpdateCluster(n, Clusters);

            bool changed = true;
            while (changed)
            {
                Console.WriteLine("Building ClusterTree - Iter");
                changed = false;
                foreach (var c in Clusters)
                    changed |= UpdateCenter(c, Functions);
                foreach (var n in Nodes)
                    changed |= UpdateCluster(n, Clusters);
            }
            Console.WriteLine("Building ClusterTree - Done");
        }

        private bool UpdateCenter(ClusterGroup<ClusterNode> c, DistanceFunction[] functions)
        {
            float distance = float.PositiveInfinity;
            ClusterNode smallest = null;
            foreach (var n1 in c.Items)
            {
                float sum = 0;
                foreach (var n2 in c.Items)
                    sum += Distance(n1, n2);
                if (distance > sum)
                {
                    distance = sum;
                    smallest = n1;
                }
            }
            if (smallest != c.Center)
            {
                //if (smallest == null)
                //    Console.Error.WriteLine("UPDATECENTER SETTING TO NULL {0}", nodes.Count());
                c.Center = smallest;
                return true;
            }
            return false;
        }

        private bool UpdateCluster(ClusterNode n, List<ClusterGroup<ClusterNode>> clusters)
        {
            ClusterGroup<ClusterNode> smallest = null;
            float distance = float.PositiveInfinity;
            foreach (var c in clusters)
            {
                //if (c.Center == null)
                //    Console.Error.WriteLine("OHBOI");
                float d = Distance(n, c.Center);
                if (d < distance)
                {
                    smallest = c;
                    distance = d;
                }
            }
            if (smallest != n.Cluster)
            {
                if (smallest != null)
                    smallest.Items.Remove(n);
                //if (smallest == null)
                //    Console.Error.WriteLine("UPDATECLUSTER SETTING TO NULL");
                n.SetCluster(smallest);
                return true;
            }
            return false;
        }

        private float Distance(ClusterNode a, ClusterNode b)
        {
            if (a.Value.ID > b.Value.ID) return Distance(b, a);

            var key = (a.Value.ID, b.Value.ID);

            if (!Distances.ContainsKey(key))
                Distances.Add(key, FeatureVector.Distance(Functions, a.Value.Features, b.Value.Features));

            return Distances[key];
        }

        public List<ClusterGroup<MeshStatistics>> ClustersAsStats()
        {
            return Clusters.Select((c) =>
                new ClusterGroup<MeshStatistics>(c.Center.Value,
                    c.Items.Select((n) => n.Value).ToList()
                )).ToList();
        }
    }
}
