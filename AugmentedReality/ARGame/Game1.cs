using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

using GoblinXNA;
using GoblinXNA.Graphics;
using GoblinXNA.SceneGraph;
using Model = GoblinXNA.Graphics.Model;
using GoblinXNA.Graphics.Geometry;
using GoblinXNA.Device.Capture;
using GoblinXNA.Device.Vision;
using GoblinXNA.Device.Vision.Marker;
using GoblinXNA.Device.Util;
using GoblinXNA.Physics;
using GoblinXNA.Helpers;
using ARUtils;
using GameModel;
using ARGame.Levels;
using ARGame.Utils;
using Bnoerj.Winshoked;

namespace ARGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : AbstractGame
    {
        const int WIDTH = 800;
        const int HEIGHT = 600;
                
        LevelManager LevelMan;

        private WinshokedComponent keyboardBlock;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            contentManager = new ContentManager();            
            LevelMan = new LevelManager(this);
            this.IsMouseVisible = true;

            keyboardBlock = new WinshokedComponent(this);
            this.Components.Add(keyboardBlock);
        }

        #region XNA GameMethods

        protected override void Initialize()
        {
            // Initialize the GoblinXNA framework
           State.InitGoblin(graphics, Content, "");

            // Initialize the scene graph
            sceneGraph = new Scene(this);
            sceneGraph.PreferPerPixelLighting = false;
            sceneGraph.EnableLighting = false;
            
   
            graphics.IsFullScreen = Configuration.Instance.GetBoolParam("main", "fullScreen");
            graphics.ApplyChanges();
            ARManager.SetupMarkerTracking(sceneGraph, "calib.xml", Configuration.Instance.GetIntParam("main", "videoDevice"));
            
            //mando el id del jugador
            ARManager.SetupMarkers(sceneGraph,1);
            //actualLevel = new Level1(this);


            font = Content.Load<SpriteFont>("myFont");

            base.Initialize();
        }

       
       
        protected override void LoadContent()
        {
            DirectoryInfo textureDir = new DirectoryInfo("Content\\Textures");
            if (textureDir.Exists)
            {
                foreach (FileInfo finfo in textureDir.GetFiles())
                {
                    string name = finfo.Name.Replace(finfo.Extension, "");
                    contentManager.AddTexture(name, Content.Load<Texture2D>("Textures\\" + name));
                }
            }
            //cargo los modelos
            DirectoryInfo modelsDir = new DirectoryInfo("Content\\Models");
            if (modelsDir.Exists)
            {
                foreach (FileInfo finfo in modelsDir.GetFiles())
                {
                    string name = finfo.Name.Replace(finfo.Extension, "");
                    ModelLoader loader = new ModelLoader();
                    Model model = (Model)loader.Load("", "Models\\" + name);
                    //model.CastShadows = true;
                    //model.ReceiveShadows = true;
                    model.UseInternalMaterials = true;
                    contentManager.AddModel(name, model);
                }
            }

            LevelMan.Load("XMLLevel/Levels.xml");
            //LevelMan.Load("XMLLevel/LevelsTest.xml");
            LevelMan.Init();
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
                LevelMan.NextLevel();
            
            LevelMan.CurrentLevel.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            
            base.Update(gameTime);
        }

        string fps = "";
        string triangles = "";

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            triangles  = (sceneGraph.TriangleCount) + "\n";
            fps = sceneGraph.FPS + "\n";
            // TODO: Add your drawing code here
            LevelMan.CurrentLevel.Draw();
            base.Draw(gameTime);
        }

        #endregion

        
    }
}
