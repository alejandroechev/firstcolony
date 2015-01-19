using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using GoblinXNA;
using Microsoft.Xna.Framework.Content;
using System.Drawing;
using GoblinXNA.SceneGraph;
using System.Xml.Linq;
using System.Reflection;
using GoblinXNA.Device.Capture;



namespace ARGame.Utils
{
    /// <summary>
    /// Clase que contiene todos los elementos basicos para el funcionamiento de un juego
    /// </summary>
    public class ClientMainManager: MainManager
    {

        //private readonly string CONTENT_DIRECTORY = "Content";

        protected static ClientMainManager instance;
        public static ClientMainManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new ClientMainManager();
                return instance;
            }
        }

        public ContentManager ContentMan { get; private set; }

        private ClientMainManager()
        {
            ContentMan = new ContentManager();
        }


        public override void Init()
        {
           
        }


        public override void Load(Game game)
        {
            
        }


    }
}
