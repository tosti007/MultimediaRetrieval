using System;

namespace MultimediaRetrieval
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            using (Game game = new Game(800, 600, "MultimediaRetrieval"))
            {
                game.Run(60.0);
            }
        }
    }
}
