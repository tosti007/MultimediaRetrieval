using System;
using System.IO;

namespace MultimediaRetrieval
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            string workdir = Directory.GetCurrentDirectory().TrimEnd(new char[] { '/', '\\' });
            if (workdir.EndsWith("bin/Debug", StringComparison.InvariantCulture) && File.Exists("MultimediaRetrieval.exe"))
            {
                workdir = Path.GetDirectoryName(Path.GetDirectoryName(workdir));
                Console.WriteLine("Visual Studio execution detected, changing workdirectory to {0}", workdir);
                Directory.SetCurrentDirectory(workdir);
            }

            DatabaseReader classes = new DatabaseReader();
            classes += DatabaseReader.ReadClassification("database/step0/LPSB", "database/step1/");
            classes += DatabaseReader.ReadClassification("database/step0/Princeton", "database/step1");

            FeatureDatabase db = new FeatureDatabase(classes);
            db.WriteToFile("database/step1/output");
            //Mesh mesh = Mesh.ReadMesh("m0.off");
            //Camera camera = new Camera(1.5f, 30f, 45f);
            //using (MeshViewer view = new MeshViewer(800, 600, "MultimediaRetrieval", mesh, camera)) view.Run(60.0);
        }
    }
}
