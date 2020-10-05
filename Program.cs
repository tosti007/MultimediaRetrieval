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
                    InputDir = "database/step4/"
                };
                return options.Execute();
            }

            return Parser.Default.ParseArguments<ViewOptions, FeatureOptions, QueryOptions>(args)
                .MapResult(
                    (ViewOptions opts) => opts.Execute(),
                    (FeatureOptions opts) => opts.Execute(),
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
            HelpText = "(Default: [DIRECTORY]/output.mr) File path to read the class features from.")]
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

    [Verb("query", HelpText = "Query a mesh, given a feature file.")]
    class QueryOptions
    {
        [Option('i', "input",
            HelpText = "(Default: mesh.off) File path to read the mesh from.")]
        public string InputFile { get; set; }

        [Option('d', "database",
            Default = "database/step4/",
            HelpText = "(Default: database/step4/) Directory to read the features from.")]
        public string InputDir { get; set; }

        [Option('k', "k_parameter",
            Default = "5",
            HelpText = "(Default: 5) The number of top matching meshes to return.")]
        public string InputK { get; set; }

        public int Execute()
        {
            int k = int.Parse(InputK);
            FeatureDatabase db = FeatureDatabase.ReadFrom(Path.Combine(InputDir, "output.mr"), InputDir);

            Mesh inputmesh = Mesh.ReadMesh(InputFile);
            MeshStatistics inputms = new MeshStatistics(inputmesh);
            FeatureVector inputfv = new FeatureVector(inputms);

            inputfv.Normalize(db.Average, db.StandardDev);

            //Fill a list of ID's to distances between the input feature vector and the database feature vectors:
            List<(uint, float)> distance = new List<(uint, float)>();
            foreach(MeshStatistics m in db.meshes)
            {
                FeatureVector fv = new FeatureVector(m);
                fv.Normalize(db.Average, db.StandardDev);
                distance.Add((m.ID, FeatureVector.EuclidianDistance(fv, inputfv)));
            }

            //Sort the meshes in the database by distance and return the top:
            distance.Sort((a, b) => a.Item2.CompareTo(b.Item2));
            for (int i = 0; i < k; i++)
            {
                Console.WriteLine($"Close match: {distance[i].Item1}, with distance {distance[i].Item2}.");
            }

            return 0;
        }
    }
}
