using System;

namespace MultimediaRetrieval
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            DatabaseReader classes = new DatabaseReader();
            //classes += DatabaseReader.ReadClassification("../../database/step0/LPSB", "../../database/step1/");
            classes += DatabaseReader.ReadClassification("../../database/step0/Princeton", "../../database/step1");

            FeatureDatabase db = new FeatureDatabase(classes);
            db.WriteToFile("../../database/step1/output");
            //Mesh mesh = Mesh.ReadMesh("m0.off");
            //Camera camera = new Camera(1.5f, 30f, 45f);
            //using (Game game = new Game(800, 600, "MultimediaRetrieval", mesh, camera)) game.Run(60.0);
        }
    }
}
