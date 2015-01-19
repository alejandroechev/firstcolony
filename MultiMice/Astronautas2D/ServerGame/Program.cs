using System;

namespace ServerGame
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (ServerGame game = new ServerGame())
            {
                game.Run();
            }
        }
    }
}

