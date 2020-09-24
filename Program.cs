using System;
using System.IO;
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
                    InputDir = "database/step1/"
                };
                return options.Execute();
            }

            return Parser.Default.ParseArguments<ParseOptions, ViewOptions, FeatureOptions>(args)
                .MapResult(
                    (ParseOptions opts) => opts.Execute(),
                    (ViewOptions opts) => opts.Execute(),
                    (FeatureOptions opts) => opts.Execute(),
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

    [Verb("parse", HelpText = "Parse database folders into output folder.")]
    class ParseOptions
    {
        [Option('i', "input", 
            Default = "database/step0", 
            HelpText = "Folder name to read the raw databases from.")]
        public string InputDir { get; set; }

        [Option('o', "output",
            Default = "database/step1",
            HelpText = "Folder name to write the raw databases into.")]
        public string OutputDir { get; set; }

        [Option('f', "file",
            HelpText = "(Default: [OUTPUT]/output.mr) File path to write the feature list to.")]
        public string OutputFile { get; set; }

        public int Execute()
        {
            if (string.IsNullOrWhiteSpace(OutputFile))
                OutputFile = Path.Combine(OutputDir, "output.mr");

            DatabaseReader classes = new DatabaseReader();
            foreach (var dir in Directory.EnumerateDirectories(InputDir))
                classes += DatabaseReader.ParseClassification(dir, OutputDir);

            classes.WriteToFile(OutputFile);

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
            Default = "database/step1/",
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
}
