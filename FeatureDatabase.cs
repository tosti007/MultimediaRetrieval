using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Accord.MachineLearning.Clustering;

namespace MultimediaRetrieval
{
    public class FeatureDatabase
    {
        public List<MeshStatistics> meshes;
        private FeatureVector _avg;
        private FeatureVector _std;
        public bool Normalized { get; private set; }

        private FeatureDatabase()
        {
            Normalized = false;
            meshes = new List<MeshStatistics>();
        }
        public FeatureDatabase(DatabaseReader classes, string dirpath)
        {
            Normalized = false;
            meshes = DatabaseReader.ListMeshes(dirpath).AsParallel().Select((file) => {
                Console.WriteLine("Generating features for {0}", file);
                return new MeshStatistics(classes, file);
            }).ToList();
        }

        public void WriteToFile(string filepath)
        {
            // check for endswith .mr
            if (!filepath.EndsWith(".mr", StringComparison.InvariantCulture))
                filepath += ".mr";

            List<string> headers = new List<string>(3);
            if (Normalized)
            {
                headers.Add(_avg.ToString());
                headers.Add(_std.ToString());
            }
            headers.Add(MeshStatistics.Headers());

            WriteToFile(filepath, string.Join("\n", headers), (m) => m.ToString());
        }

        public void WriteToFile(string filepath, string header, Func<MeshStatistics, string> tostring)
        {
            File.WriteAllLines(filepath, new[] { header }.Concat(
                meshes.OrderBy((cls) => cls.ID).Select(tostring)
                ));
        }

        public static FeatureDatabase ReadFrom(string filepath)
        {
            FeatureDatabase result = new FeatureDatabase();

            using (StreamReader file = new StreamReader(filepath))
            {
                // Ignore the first line with headers
                string line = file.ReadLine();

                if (line.Count((c) => c == ';') == FeatureVector.Headers().Count((c) => c == ';'))
                {
                    // Normalized featuredatabase
                    result._avg = FeatureVector.Parse(line);
                    line = file.ReadLine();
                    result._std = FeatureVector.Parse(line);
                    line = file.ReadLine();
                    result.Normalized = true;
                }

                while ((line = file.ReadLine()) != null)
                {
                    if (line == string.Empty)
                        continue;

                    var item = MeshStatistics.Parse(line);
                    result.meshes.Add(item);
                }
            }

            return result;
        }

        public void FilterNanAndInf(bool print)
        {
            List<MeshStatistics> nm = new List<MeshStatistics>(meshes.Count);
            foreach (MeshStatistics stats in meshes)
            {
                if (stats.Features.HasValue((x) => float.IsNaN(x) || float.IsInfinity(x)))
                {
                    Console.Error.Write($"Bad value found for {stats.ID}");
                    if (print)
                    {
                        Console.Error.Write(" with ");
                        Console.Error.WriteLine(stats.Features.PrettyPrint());
                    }
                    else
                    {
                        Console.Error.WriteLine(".");
                    }
                } else
                {
                    nm.Add(stats);
                }
            }
            meshes = nm;
            meshes.TrimExcess();
        }

        public void Filter(string dirpath)
        {
            HashSet<uint> ids = new HashSet<uint>(DatabaseReader.ListMeshes(dirpath).Select(DatabaseReader.GetId));
            List<MeshStatistics> nm = new List<MeshStatistics>(meshes.Count);
            foreach (MeshStatistics stats in meshes)
            {
                if (ids.Contains(stats.ID))
                {
                    nm.Add(stats);
                    ids.Remove(stats.ID);
                }
            }
            meshes = nm;
            meshes.TrimExcess();
        }

        public void Normalize()
        {
            foreach (MeshStatistics m in meshes)
                m.Features.HistogramsAsPercentages();

            FeatureVector avg = Average;
            FeatureVector std = StandardDev;

            foreach (MeshStatistics m in meshes)
                m.Features.Normalize(avg, std);

            Normalized = true;
        }

        public FeatureVector Average
        {
            get
            {
                if (_avg != null)
                    return _avg;

                //Create the average FeatureVector.
                _avg = new FeatureVector();
                for (int i = 0; i < meshes.Count; i++)
                {
                    _avg += meshes[i].Features;
                }

                _avg.Map(f => f / meshes.Count);
                return _avg;
            }
        }

        public FeatureVector StandardDev
        {
            get
            {
                if (_std != null)
                    return _std;

                _std = new FeatureVector();
                for (int i = 0; i < meshes.Count; i++)
                {
                    FeatureVector x = meshes[i].Features - _avg;
                    x.Map(f => f * f);
                    _std += x;
                }
                _std.Map(f => f / (meshes.Count - 1));
                _std.Map(f => (float)Math.Sqrt(f));
                return _std;
            }
        }

        public float[] ToArray()
        {
            int dim = meshes[0].Features.Size;
            float[] result = new float[dim * meshes.Count];
            for (int i = 0; i < meshes.Count; i++)
                meshes[i].Features.ToArray().CopyTo(result, i * dim);

            return result; 
        }

        public double[][] ToArrayDouble()
        {
            double[][] result = new double[meshes.Count][];

            for (int i = 0; i < meshes.Count; i++)
                result[i] = meshes[i].Features.ToArrayDouble();

            return result;
        }

        public void ReduceDimensions(int NumberOfOutputs = 2, double Perplexity = 1.5, double Theta = 0.5)
        {
            // Accord.Math.Random.Generator.Seed = 0;

            // Declare some observations
            double[][] observations = ToArrayDouble();

            // Create a new t-SNE algorithm 
            TSNE tSNE = new TSNE()
            {
                NumberOfInputs = observations[0].Length,
                NumberOfOutputs = NumberOfOutputs,
                Perplexity = Perplexity,
                Theta = Theta
            };

            // Transform to a reduced dimensionality space
            double[][] output = tSNE.Transform(observations);

            for (int i = 0; i < meshes.Count; i++)
                meshes[i].Features.FromArray(output[i]);
        }
    }
}
