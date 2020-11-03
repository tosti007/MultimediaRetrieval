using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;

namespace MultimediaRetrieval
{
    class MainClass
    {
        public static int Main(string[] args)
        {
            string workdir = Directory.GetCurrentDirectory().TrimEnd(new char[] { '/', '\\' });
            if (workdir.EndsWith("bin/Debug", StringComparison.InvariantCulture) && File.Exists("MultimediaRetrieval.exe"))
            {
                workdir = Path.GetDirectoryName(Path.GetDirectoryName(workdir));
                Console.WriteLine("Visual Studio execution detected, changing workdirectory to {0}", workdir);
                Directory.SetCurrentDirectory(workdir);

                
                FeatureOptions options = new FeatureOptions {
                    InputFile = "../database/step1/output.mr",
                    InputDir = "../database/step4/"
                };

                return options.Execute();
            }

            return Parser.Default.ParseArguments<DistanceMethodOptions, ViewOptions, FeatureOptions, NormalizeOptions, QueryOptions, EvaluateOptions>(args)
                .MapResult(
                    (DistanceMethodOptions opts) => opts.Execute(),
                    (ViewOptions opts) => opts.Execute(),
                    (FeatureOptions opts) => opts.Execute(),
                    (NormalizeOptions opts) => opts.Execute(),
                    (QueryOptions opts) => opts.Execute(),
                    (EvaluateOptions opts) => opts.Execute(),
                errs => 1);
        }
    }

    [Verb("distancemethods", HelpText = "Print the possible distance methods")]
    class DistanceMethodOptions
    {
        public int Execute()
        {
            foreach (DistanceFunction f in Enum.GetValues(typeof(DistanceFunction)))
                Console.WriteLine(f);
            return 0;
        }
    }

    [Verb("view", HelpText = "View a Mesh file.")]
    class ViewOptions
    {
        [Value(0, MetaName = "path", Required = false, HelpText = "Mesh file path to view.")]
        public string MeshFile { get; set; }

        public int Execute()
        {
            using (MeshViewer view = new MeshViewer(800, 600, "MultimediaRetrieval - " + MeshFile, MeshFile))
                view.Run(60.0);

            return 0;
        }
    }

    [Verb("feature", HelpText = "Generate a feature file.")]
    class FeatureOptions
    {
        [Option('i', "input",
            Default = "database/step1/output.mr",
            HelpText = "File path to read the class features from.")]
        public string InputFile { get; set; }

        [Option('o', "output",
            HelpText = "(Default: [DIRECTORY]/output.mr) File path to write the features to.")]
        public string OutputFile { get; set; }

        [Option('d', "directory",
            Default = "database/step4/",
            HelpText = "Folder name to read the meshes from.")]
        public string InputDir { get; set; }

        public int Execute()
        {
            if (string.IsNullOrWhiteSpace(InputFile))
                InputFile = Path.Combine(InputDir, "output.mr");

            if (string.IsNullOrWhiteSpace(OutputFile))
                OutputFile = Path.Combine(InputDir, "output.mr");

            DatabaseReader classes = DatabaseReader.ReadFromFile(InputFile);
            FeatureDatabase db = new FeatureDatabase(classes, InputDir);
            db.WriteToFile(OutputFile);

            return 0;
        }
    }

    [Verb("normalize", HelpText = "Normalize a feature file for later querying.")]
    class NormalizeOptions
    {
        [Option('d', "database",
            Default = "database/step1/",
            HelpText = "Directory to filter the possible meshes with.")]
        public string InputDir { get; set; }

        [Option('i', "input",
            Default = "database/step4/output.mr",
            HelpText = "File to read the features from.")]
        public string InputFile { get; set; }

        [Option('o', "output",
            Default = "database/output.mr",
            HelpText = "File path to write the normalized features to.")]
        public string OutputFile { get; set; }

        [Option('v', "vector",
            Default = false,
            HelpText = "Print the feature vectors for the given matches.")]
        public bool Vectors { get; set; }

        // Good options:
        //   100, 2.5
        //    30, 1,5
        [Option("tsne",
            Default = null,
            HelpText = "(Default: if on 30,1.5) Use the tSNE algorithm to reduce the feature vector dimensionallity.")]
        public string TSNE { get; set; }

        public int Execute()
        {
            FeatureDatabase db = FeatureDatabase.ReadFrom(InputFile);

            if (db.Normalized)
            {
                Console.Error.WriteLine($"Featurefile {InputFile} is already normalized!");
                return 1;
            }

            if (!string.IsNullOrWhiteSpace(InputDir))
                db.Filter(InputDir);

            db.Normalize();
            db.FilterNanAndInf(Vectors);
            db.WriteToFile(OutputFile);

            if (TSNE != null)
            {
                double p, t;
                if (string.IsNullOrWhiteSpace(TSNE))
                {
                    p = 100;
                    t = 2.5;
                }
                else
                {
                    double[] data = TSNE.Split(',').Select(double.Parse).ToArray();
                    p = data[0];
                    t = data[1];
                }
                Console.WriteLine("Reducing dimensionality using TSNE...");
                db.ReduceDimensions(NumberOfOutputs: 2, Perplexity: p, Theta: t);
                db.WriteToFile(OutputFile + "tsne", "Class;X;Y", (m) => m.Classification + ";" + m.Features);
            }

            return 0;
        }
    }

    [Verb("query", HelpText = "Query a mesh, given a feature file.")]
    class QueryOptions
    {
        [Value(0, MetaName = "path", Required = false, HelpText = "Mesh file path or Mesh ID in database to use.")]
        public string InputMesh { get; set; }

        [Option('d', "database",
            HelpText = "Directory to filter the possible meshes with.")]
        public string InputDir { get; set; }

        [Option('i', "input",
            Default = "database/output.mr",
            HelpText = "File to read the features from.")]
        public string InputFile { get; set; }

        [Option('v', "vector",
            Default = false,
            HelpText = "Print the feature vectors for the given matches.")]
        public bool Vectors { get; set; }

        [Option('k', "k_parameter",
            HelpText = "(Default: 5) The number of top matching meshes to return, this will be null if t is given.")]
        public int? InputK { get; set; }

        [Option('t', "t_parameter",
            HelpText = "(Default: null) The maximal distance for matching meshes to return, this will be null if k is given.")]
        public float? InputT { get; set; }

        [Option("csv",
            Default = false, 
            HelpText = "Output the matches as CSV.")]
        public bool AsCSV { get; set; }

        [Option('m', "method",
            Default = new[] { DistanceFunction.Euclidian, DistanceFunction.Earthmovers },
            HelpText = "The distance function to use.")]
        public IEnumerable<DistanceFunction> DistanceFuncs { get; set; }

#if Windows
        [Option("ann",
            Default = false,
            HelpText = "Output the results of an ANN k-nearest neighbour search. This will not execute if no k is given.")]
        public bool WithANN { get; set; }

        // TODO: Move this to normalize
        [Option("newtree",
            Default = false,
            HelpText = "If ANN is performed, generate a new tree to the kdtree.tree file. Otherwise, this file will be read to obtain the tree.")]
        public bool NewTree { get; set; }
#endif

        public bool ParseInput()
        {
            if (InputT.HasValue && InputK.HasValue)
            {
                Console.Error.WriteLine("T and K cannot both be set.");
                return false;
            }

            if (!InputT.HasValue && !InputK.HasValue)
                InputK = 5;

            if (string.IsNullOrWhiteSpace(InputFile))
                InputFile = Path.Combine(InputDir, "output.mr");

            if (!DistanceFuncs.Any() || DistanceFuncs.Count() > 2)
            {
                Console.Error.WriteLine("Method option can only contain 1 or 2 values!");
                return false;
            }

#if Windows
            if (WithANN && !InputK.HasValue)
            {
                Console.WriteLine("Unable to perform ANN, no k specified.");
                return false;
            }
#endif
            return true;
        }

        public bool ParseInput(out FeatureDatabase db)
        {
            db = FeatureDatabase.ReadFrom(InputFile);

            if (!string.IsNullOrWhiteSpace(InputDir))
                db.Filter(InputDir);

            if (InputK.HasValue)
            {
                if (InputK.Value == 0)
                    InputK = db.meshes.Count;
                else
                    InputK = Math.Min(InputK.Value, db.meshes.Count);
            }

            if (!db.Normalized)
            {
                Console.Error.WriteLine("Featuredatabase is not yet normalized!");
                Console.Error.WriteLine("Normalized in memory now, but for future cases use the `normalize` command first.");
                db.Normalize();
                db.FilterNanAndInf(Vectors);
            }
            return true;
        }

        public bool ParseInput(FeatureDatabase db, out FeatureVector query)
        {
            if (File.Exists(InputMesh))
            {
                query = new FeatureVector(Mesh.ReadMesh(InputMesh));
                query.HistogramsAsPercentages();
                query.Normalize(db.Average, db.StandardDev);
                return true;
            }

            if (uint.TryParse(InputFile, out uint meshid))
            {
                query = db.meshes.Find((m) => m.ID == meshid).Features;
                return true;
            }

            query = null;
            return false;
        }

        public virtual int Execute()
        {
            if (!ParseInput() || !ParseInput(out FeatureDatabase db) || !ParseInput(db, out FeatureVector query))
                return 1;

            PrintResults(Search(db, query));

            return 0;
        }

        public IEnumerable<(MeshStatistics, float)> Search(FeatureDatabase db, FeatureVector query)
        {
#if Windows
            //Do the same with KDTree:
            if (WithANN)
            {
                ANN ann = new ANN(db, InputK.Value);
                if (NewTree || !ANN.FileExists())
                {
                    Console.WriteLine("Creating new ANN KDTree.");
                    ann.Create();
                }
                else
                {
                    Console.WriteLine("Loading existing ANN KDTree.");
                    ann.Load();
                }

                MeshStatistics[] results = ann.Search(query);

                Console.WriteLine("Results from ANN (with our distance metric):");
                return GetDistanceAndSort(results, query);
            }
#endif

            //Fill a list of ID's to distances between the input feature vector and the database feature vectors.
            //Sort the meshes in the database by distance and return the selected.
            IEnumerable<(MeshStatistics, float)> meshes = GetDistanceAndSort(db.meshes, query);

            if (InputK.HasValue)
                meshes = meshes.Take(InputK.Value);

            if (InputT.HasValue)
                meshes = meshes.TakeWhile((arg) => arg.Item2 <= InputT.Value);

            return meshes;
        }

        public IEnumerable<(MeshStatistics, float)> GetDistanceAndSort(IEnumerable<MeshStatistics> meshes, FeatureVector query)
        {
            return meshes.AsParallel()
                .Select((m) => (m, query.Distance(DistanceFuncs.Parse(), m.Features)))
                .OrderBy((arg) => arg.Item2).AsSequential();
        }

        public void PrintResults(IEnumerable<(MeshStatistics, float)> meshes)
        {
            string printformat;
            if (AsCSV)
            {
                Console.WriteLine("ID,Class,Distance");
                printformat = "{0},{1},{2}";
            }
            else
            {
                printformat = "Close match: {0} ({1}), with distance {2}";
            }

            foreach (var (match, distance) in meshes)
            {
                Console.Write(printformat, match.ID, match.Classification, distance);

                if (!AsCSV && Vectors)
                {
                    Console.Write(" and ");
                    Console.Write(match.Features.PrettyPrint());
                }

                Console.WriteLine();
            }
        }
    }

    [Verb("evaluate", HelpText = "Evaluate a mesh database for performance.")]
    class EvaluateOptions : QueryOptions
    {
        public override int Execute()
        {
            if (!ParseInput() || !ParseInput(out FeatureDatabase db))
                return 1;

            var results = db.meshes.AsParallel().Select((m) => {
                Console.WriteLine("Handling {0}", m.ID);
                var answers = Search(db, m.Features).Select((r) => r.Item1.Classification);
                var perf = Evaluate(m.Classification, answers);
                return (m.Classification, perf);
            }).AsSequential();

            var r_class = new Dictionary<string, ValueCounter>();
            var r_total = new ValueCounter();
            foreach (var (c, perf) in results)
            {
                if (!r_class.ContainsKey(c))
                    r_class[c] = new ValueCounter();

                r_class[c] += perf;
                r_total += perf;
            }

            Console.WriteLine("Performance total: {0}", r_total.Percentage);
            foreach(var p in r_class.OrderByDescending((p) => p.Value.Percentage))
                Console.WriteLine("Performance {0}: {1}", p.Key, p.Value.Percentage);

            return 0;
        }

        public float Evaluate(string c, IEnumerable<string> results)
        {
            int nrcorrect = results.Count((s) => s == c);
            return nrcorrect / results.Count();
        }

        struct ValueCounter
        {
            public float Total { get; private set; }
            public int Count { get; private set; }
            public float Percentage { get => Total / Count; }

            public static ValueCounter operator +(ValueCounter a, float b)
            {
                return new ValueCounter
                {
                    Total = a.Total + b,
                    Count = a.Count + 1
                };
            }
        }
    }
}
