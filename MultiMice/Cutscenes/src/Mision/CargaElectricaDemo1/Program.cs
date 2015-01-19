using System;

namespace CargaElectricaDemo1
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (AbstractGame game = new Intro3())
            {
                game.Run();
            }
        }
    }
}

