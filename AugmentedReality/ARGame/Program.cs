//#define red
using System;
using Microsoft.Xna.Framework;
using ARGame.Utils;

namespace ARGame
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            Configuration.Instance.Load("config.xml");
            
            Game game = null;
            //Se elige el modo del juego, segun archivo de configuracion: 0 = single player, 1 = network player, 2 = server player
            int mode = Configuration.Instance.GetIntParam("main", "gameMode");
            if (mode == 0)
                game = new Game1();
            else if (mode == 1)
                game = new PlayerGame();
            else if (mode == 2)
                game = new ServerGame();
            else
                return;
            
            game.Run();
            game.Dispose();
        }
    }
}

