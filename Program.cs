using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CommandLine;
using OpenTK.Graphics.OpenGL;
#if Windows
using wrapper;
#endif

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
                    InputFile = "database/step1/output.mr",
                    InputDir = "database/step4/"
                };

                return options.Execute();
            }

            return Parser.Default.ParseArguments<ViewOptions, FeatureOptions, NormalizeOptions, QueryOptions>(args)
                .MapResult(
                    (ViewOptions opts) => opts.Execute(),
                    (FeatureOptions opts) => opts.Execute(),
                    (NormalizeOptions opts) => opts.Execute(),
                    (QueryOptions opts) => opts.Execute(),
                errs => 1);
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

            return 0;
        }
    }

    [Verb("query", HelpText = "Query a mesh, given a feature file.")]
    class QueryOptions
    {
        [Value(0, MetaName = "path", Required = false, HelpText = "Mesh file path to view.")]
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

#if Windows
        [Option("ann",
            Default = false,
            HelpText = "Output the results of an ANN k-nearest neighbour search. This will not execute if no k is given.")]
        public bool WithANN { get; set; }

        [Option("newtree",
            Default = false,
            HelpText = "If ANN is performed, generate a new tree to the kdtree.tree file. Otherwise, this file will be read to obtain the tree.")]
        public bool NewTree { get; set; }
#endif

        public int Execute()
        {
            if (InputT != null && InputK != null)
            {
                Console.Error.WriteLine("T and K cannot both be set.");
                return 1;
            }

            if (InputT == null && InputK == null)
                InputK = 5;

            if (string.IsNullOrWhiteSpace(InputFile))
                InputFile = Path.Combine(InputDir, "output.mr");

            FeatureDatabase db = FeatureDatabase.ReadFrom(InputFile);

            if (!string.IsNullOrWhiteSpace(InputDir))
                db.Filter(InputDir);

            if (!db.Normalized)
            {
                Console.Error.WriteLine("Featuredatabase is not yet normalized!");
                Console.Error.WriteLine("Normalized in memory now, but for future cases use the `normalize` command first.");
                db.Normalize();
                db.FilterNanAndInf(Vectors);
            }

            FeatureVector query = new FeatureVector(Mesh.ReadMesh(InputMesh));
            query.HistogramsAsPercentages();
            query.Normalize(db.Average, db.StandardDev);

            //Fill a list of ID's to distances between the input feature vector and the database feature vectors.
            //Sort the meshes in the database by distance and return the selected.
            IEnumerable<(MeshStatistics, float)> meshes = db.meshes.AsParallel()
                .Select((m) => (m, query.Distance(m.Features)))
                .OrderBy((arg) => arg.Item2).AsSequential();

            if (InputK != null)
                meshes = meshes.Take(InputK.Value);

            if (InputT != null)
                meshes = meshes.TakeWhile((arg) => arg.Item2 <= InputT.Value);

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

#if Windows
            //Do the same with KDTree:
            if (WithANN)
            {
                if (InputK == null)
                {
                    Console.WriteLine("Unable to perform ANN, no k specified.");
                }
                else
                {
                    int dim = query.Size;
                    float eps = 0.0f;
                    int npts = db.meshes.Count;
                    wrapper.KDTree instance = new wrapper.KDTree();
                    if (NewTree || !File.Exists("kdtree.tree"))
                    {
                        Console.WriteLine("Creating new ANN KDTree.");
                        unsafe
                        {
                            float[] dataArr = db.Flattened();
                            fixed (float* dataArrPtr = dataArr)
                            {
                                instance.CreateKDTree(dim, npts, InputK.Value, dataArrPtr);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Loading existing ANN KDTree.");
                        instance.LoadKDTree();
                    }

                    float[] queryArr = query.Flattened();

                    unsafe
                    {
                        fixed (float* queryArrPtr = queryArr)
                        {
                            Console.WriteLine("Results from ANN:");
                            int* topIndicesPtr = instance.SearchKDTree(dim, InputK.Value, queryArrPtr, eps);
                            for (int i = 0; i < InputK; i++)
                                Console.WriteLine($"Close match: {db.meshes[topIndicesPtr[i]].ID} with class {db.meshes[topIndicesPtr[i]].Classification}");
                        }
                    }
                }
            }
#endif
            return 0;
        }
    }
}
