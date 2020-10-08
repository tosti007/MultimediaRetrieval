using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using OpenTK.Graphics.OpenGL;

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
            Mesh mesh = Mesh.ReadMesh(MeshFile);
            Camera camera = new Camera(1.5f, 30f, 45f);
            using (MeshViewer view = new MeshViewer(800, 600, "MultimediaRetrieval", mesh, camera))
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

        [Option('k', "k_parameter",
            Default = 5,
            HelpText = "The number of top matching meshes to return.")]
        public int InputK { get; set; }

        [Option('v', "vector",
            Default = false,
            HelpText = "Print the feature vectors for the given matches.")]
        public bool Vectors { get; set; }

        public int Execute()
        {
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
            query.Normalize(db.Average, db.StandardDev);

            //Fill a list of ID's to distances between the input feature vector and the database feature vectors:
            List<(MeshStatistics, float)> distance = new List<(MeshStatistics, float)>();
            foreach(MeshStatistics m in db.meshes)
            {
                distance.Add((m, FeatureVector.EuclidianDistance(m.Features, query)));
            }

            //Sort the meshes in the database by distance and return the top:
            distance.Sort((a, b) => a.Item2.CompareTo(b.Item2));
            for (int i = 0; i < InputK; i++)
            {
                Console.Write($"Close match: {distance[i].Item1.ID}, with distance {distance[i].Item2}");
                if (Vectors)
                {
                    Console.Write(" and ");
                    Console.WriteLine(distance[i].Item1.Features.PrettyPrint());
                }
                else
                {
                    Console.WriteLine(".");
                }
            }

            return 0;
        }
    }
}
