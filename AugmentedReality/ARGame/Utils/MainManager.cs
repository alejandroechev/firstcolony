using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;

namespace ARGame.Utils
{
    public abstract class MainManager
    {
        protected readonly string GAME_CONFIG_FILE = "GameConfig.xml";
        protected readonly string MANAGER_CONFIG_FILE = "ManagerConfig.xml";

        public string GameFile { get; protected set; }
        public string PlayersFile { get; protected set; }

        protected MainManager()
        {
            
        }

        public abstract void Load(Game game);

        public abstract void Init();

        public virtual void Close()
        {
            //NetworkMan.Close();

        }


    }
}
