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

        [Option("medoids",
            Default = null,
            HelpText = "Generate a K-Mediods cluster tree with [ARG] clusters and safe it to \"[OUTPUT]kmed\" file. If no k is given, it is set to the number of classes.")]
        public string KMedoids { get; set; }

        // Good options:
        //   100, 2.5
        //    30, 1,5
        [Option("tsne",
            Default = null,
            HelpText = "(Default: if on 80,0.5) Use the tSNE algorithm to reduce the feature vector dimensionallity.")]
        public string TSNE { get; set; }

        [Option('m', "method",
            Default = new[] { DistanceFunction.Euclidian, DistanceFunction.Earthmovers },
            HelpText = "The distance function to use, does not work for tSNE.")]
        public IEnumerable<DistanceFunction> DistanceFuncs { get; set; }

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

            if (KMedoids != null)
            {
                int k;
                if (string.IsNullOrWhiteSpace(KMedoids))
                    k = db.meshes.Select((m) => m.Classification).Distinct().Count();
                else
                    k = int.Parse(KMedoids);
                Console.WriteLine("Using {0} K-Medoids Clusters");
                var tree = new ClusterTree(DistanceFuncs.Parse(), db.meshes, k);
                tree.WriteToFile(OutputFile + "kmed");
            }

            if (TSNE != null)
            {
                double p, t;
                if (string.IsNullOrWhiteSpace(TSNE))
                {
                    p = 80;
                    t = 0.5;
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

        [Option("medoids",
            Default = false,
            HelpText = "Use the KMediods tree file at \"[INPUT]kmed\" to search for meshes.")]
        public bool KMedoids { get; set; }

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

        public virtual bool ParseInput()
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

            if (KMedoids && !File.Exists(InputFile + "kmed"))
            {
                Console.Error.WriteLine("K-Mediods file does not exist yet, use normalize first.");
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

        public virtual bool ParseInput(out FeatureDatabase db)
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

        public virtual bool ParseInput(FeatureDatabase db, out FeatureVector query)
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

            Console.Error.WriteLine("Cannot read inputmesh!");
            Console.Error.WriteLine("Should be a meshfile or a id of a mesh in the FeatureDatabase.");
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
            return Search(db, query, InputK);
        }

        public IEnumerable<(MeshStatistics, float)> Search(FeatureDatabase db, FeatureVector query, int? k)
        {
#if Windows
            //Do the same with KDTree:
            if (WithANN)
            {
                ANN ann = new ANN(db, k.Value);
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
            IEnumerable<MeshStatistics> selected;

            if (KMedoids)
            {
                var tree = ClusterTree.ReadFrom(db, InputFile + "kmed");
                if (!AsCSV)
                    Console.WriteLine("Using K-Medoids for searching with distance method: {0}", string.Join(" ", tree.Functions.Select((f) => f.ToString())));
                DistanceFuncs = tree.Functions;
                selected = tree.Search(query);
            }
            else
                selected = db.meshes;

            IEnumerable<(MeshStatistics, float)> meshes = GetDistanceAndSort(selected, query);

            if (k.HasValue)
                meshes = meshes.Take(k.Value);

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
        [Option("firsttier",
            Default = true,
            HelpText = "Choose the K size automatically depending on the number of meshes with that class")]
        public bool FirstTier { get; set; }

        public override bool ParseInput()
        {
            var r = base.ParseInput();

            if (FirstTier)
            {
                if (InputT.HasValue)
                {
                    Console.Error.WriteLine("Cannot use T value if firsttier is set.");
                    return false;
                }

                if (KMedoids)
                {
                    Console.WriteLine("KMedoids is on. Query results will be extended to correct length.");
                }

#if Windows
                if (WithANN)
                {
                    NewTree = true;
                }
#endif
            }

            return r;
        }

        public override int Execute()
        {
            // No need for checking the query input, as we don't use it anyhow.
            if (!ParseInput() || !ParseInput(out FeatureDatabase db))
                return 1;

            Measure.Init(db.meshes.Select((m) => m.Classification));

            // Foreach mesh, search the database with that mesh in parallel.
            var results = db.meshes.AsParallel().Select((m) => {
                if (!AsCSV)
                    Console.WriteLine("Handling {0}", m.ID);
                int? k = FirstTier ? (int?)Measure.ClassesCount[m.Classification] : null;
                var answers = Search(db, m.Features, k).Select((r) => r.Item1.Classification);
                if (KMedoids)
                {
                    var nr_missing = Measure.ClassesCount[m.Classification] - answers.Count();
                    if (nr_missing > 0)
                        answers = answers.Concat(Enumerable.Repeat("", nr_missing));
                }
                var Performance = new Measure(m.Classification, answers);
                return (m.Classification, Performance);
            }).AsSequential();

            // We need both total and per class performance.
            var r_class = new Dictionary<string, Measure>();
            var r_total = new Measure();
            foreach (var (c, perf) in results)
            {
                if (!r_class.ContainsKey(c))
                    r_class[c] = new Measure();

                r_class[c] += perf;
                r_total += perf;
            }

            List<string> func_names = new List<string>() {
                "Precision",
                "Recall",
                "Accuracy",
                "F1Score",
                "Specificity",
            };
            List<Func<Measure, float>> funcs = new List<Func<Measure, float>>() {
                (x) => x.Precision,
                (x) => x.Recall,
                (x) => x.Accuracy,
                (x) => x.F1Score,
                (x) => x.Specificity,
            };

            #region PrettyPrintTable
            string format;
            int minlen;
            Func<Measure, string> method;
            if (AsCSV)
            {
                format = "{0},{1}";
                minlen = 1;
                method = (x) => string.Join(",", funcs.Select((f) => f(x)));
                Console.WriteLine("Classification,{0}", string.Join(",", func_names));
            }
            else
            {
                const int columnwidth = 12;

                format = "{0} : {1}";
                minlen = Math.Max("total".Length, r_class.Keys.Select((c) => c.Length).Max());
                method = (x) => string.Join(" ", funcs.Select((f, i) => f(x).ToString().PadRight(columnwidth)));

                // Sort by largest classes first
                r_class.OrderByDescending((p) => p.Value.TP + p.Value.FN);

                // Pretty print the title and header
                string title = "FeatureDatabase Performance Table";
                string header = "Classification".PadLeft(minlen).PadRight(minlen + format.Length - 6); 
                header += string.Join(" ", func_names.Select((s) => s.PadRight(columnwidth)));
                Console.WriteLine(title.PadLeft((header.Length - title.Length) / 2 + title.Length));
                Console.WriteLine(header);
            }

            Console.WriteLine(format, "total".PadLeft(minlen), method(r_total));
            foreach(var p in r_class)
                Console.WriteLine(format, p.Key.PadLeft(minlen), method(p.Value));
            #endregion

            return 0;
        }

        public struct Measure
        {
            public static Dictionary<string, int> ClassesCount;
            public static int NumberOfElements;

            public int TP, FP, FN, TN;

            public Measure(string c, IEnumerable<string> results)
            {
                TP = 0;
                FP = 0;
                // Wanneer je voor C# kiest in je project en alsnog python aan het schrijven bent.
                foreach (var x in results)
                    if (x == c)
                        TP++;
                    else
                        FP++;
                FN = ClassesCount[c] - TP;
                TN = NumberOfElements - ClassesCount[c] - FP;
            }

            public static void Init(IEnumerable<string> classes)
            {
                ClassesCount = new Dictionary<string, int>();
                NumberOfElements = 0;
                foreach (var c in classes)
                {
                    if (!ClassesCount.ContainsKey(c))
                        ClassesCount[c] = 0;

                    ClassesCount[c] += 1;
                    NumberOfElements += 1;
                }
            }

            public static Measure operator +(Measure a, Measure b)
            {
                // To find a total of measures we have two options:
                // 1) Calculate the function for each measure, average the results
                // 2) Add the measures and calculate the function
                // This _should_ be the same, and thus I added the + operator for easy totalling.
                return new Measure
                {
                    TP = a.TP + b.TP,
                    FP = a.FP + b.FP,
                    FN = a.FN + b.FN,
                    TN = a.TN + b.TN
                };
            }

            public float Precision   => (float)                       TP / (TP + FP);
            public float Recall      => (float)                       TP / (TP + FN);
            public float Accuracy    => (float)                (TP + TN) / (TP + FN + FP + TN);
            public float F1Score     => (float) 2 * (Precision * Recall) / (Precision + Recall);
            public float Specificity => (float)                       TN / (FP + TN);
        }
    }
}
