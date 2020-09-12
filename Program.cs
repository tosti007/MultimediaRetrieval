using System;

namespace MultimediaRetrieval
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Mesh mesh = Mesh.ReadMesh("m0.off");
            Camera camera = new Camera(1.5f, 30f, 45f);
            using (Game game = new Game(800, 600, "MultimediaRetrieval", mesh, camera))
            {
                game.Run(60.0);
            }
        }
    }
}
