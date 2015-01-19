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
using GameNetwork;
using GoblinXNA.UI;
using GoblinXNA.UI.UI2D;
using ARGame.GameGUI;

using Bnoerj.Winshoked;

namespace ARGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class PlayerGame : AbstractGame
    {
        private const int WIDTH = 800;
        private const int HEIGHT = 600;

        private TrainingLevelManager TrainingMan;
        private PlayerLevelManager LevelMan;
        private PlayerInfo myPlayer;

        private enum NetworkState { NONE, P_NOTCONNECTED, P_CONNECTED, P_TRAININGSTARTED, P_GAMESTARTED, P_RECOVERING };
        private PlayerNetworkManager playerNetwork;
        private string textMessage = "";
        private NetworkState state = NetworkState.NONE;

        //GUI
        private GameInitGUI initGui;

        private bool gamePaused = false;

        private TimeSpan lastTime = new TimeSpan();

        private WinshokedComponent keyboardBlock;

        
        public PlayerGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            contentManager = new ContentManager();

            TrainingMan = new TrainingLevelManager(this);
            TrainingMan.SubLevelEnded += SubLevelEndedHandler;

            LevelMan = new PlayerLevelManager(this);

            initGui = new GameInitGUI();

            textMessage = "Esperando conexion...";

            keyboardBlock = new WinshokedComponent(this);
            this.Components.Add(keyboardBlock);

            //if (Configuration.Instance.GetBoolParam("main", "highPriority"))
            //    System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Highest;
            
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

            playerNetwork = new PlayerNetworkManager();
            playerNetwork.Init();
            playerNetwork.ServerConnected += ServerConnected;
            playerNetwork.PlayerInfoReceived += PlayerInfoReceived;
            playerNetwork.GameStateReceived += GameStateReceived;
            playerNetwork.LevelStateReceived += LevelStateReceived;
            playerNetwork.GoToLevelReceived += GoToLevelReceived;
            playerNetwork.GameStarted += GameStarted;
            playerNetwork.ResetLevel += ResetLevel;
            playerNetwork.PauseGame += PauseGame;
            playerNetwork.ResumeGame += ResumeGame;
            playerNetwork.RecoverToTraining += RecoverToTraining;
            playerNetwork.RecoverToMission += RecoverToMission;

            state = NetworkState.P_NOTCONNECTED;

            font = Content.Load<SpriteFont>("myFont");
            bigFont = Content.Load<SpriteFont>("myFontPlayer");

            IsMouseVisible = true;

            if (Configuration.Instance.GetBoolParam("main", "handleRecovery"))
            {
                PlayerInfo pinfo = playerNetwork.RecoverConnection();
                if (pinfo != null)
                    myPlayer = pinfo;
                initGui.Connect += Recover;
                textMessage = "Recuperando...";
                state = NetworkState.P_RECOVERING;
            }

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

            initGui.Init(sceneGraph, contentManager, font);
            if (state == NetworkState.P_RECOVERING)
                initGui.EnableConnectPanel();

            TrainingMan.Load("XMLLevel/TrainingPlayer.xml");
            LevelMan.Load("XMLLevel/LevelsPlayer.xml");
            
        }

        
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            UpdateNetwork(gameTime);

            if (!gamePaused)
            {
                if (state == NetworkState.P_TRAININGSTARTED)
                    TrainingMan.CurrentLevel.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
                else if (state == NetworkState.P_GAMESTARTED)
                    LevelMan.CurrentLevel.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            }
            base.Update(gameTime);
        }

        private void UpdateNetwork(GameTime gameTime)
        {
            if (state == NetworkState.P_NOTCONNECTED)
            {
                playerNetwork.HandleConnectionMessages();
            }
            else if (state == NetworkState.P_CONNECTED)
            {
                playerNetwork.HandleNetworkMessages();                
            }
            else if (state == NetworkState.P_TRAININGSTARTED)
            {
                playerNetwork.HandleNetworkMessages();
            }
            else if (state == NetworkState.P_GAMESTARTED)
            {
                playerNetwork.HandleNetworkMessages();
                if (gameTime.TotalGameTime.Subtract(lastTime).Milliseconds > 10)
                {
                    SendPlayerState();
                    lastTime = gameTime.TotalGameTime;
                }
            }
            else if (state == NetworkState.P_RECOVERING)
            {
                playerNetwork.HandleNetworkMessages();  
            }

        }

        private bool waitForActive = true;
        private void SendPlayerState()
        {
            ARAstronaut player = LevelMan.CurrentLevel.Players[0];
            PlayerState pState = new PlayerState(myPlayer.Id, myPlayer.GroupId, player.IsActive, player.Position, player.PotentialCharge);
            if(player.IsActive)
            {
                playerNetwork.SendPlayerState(pState);
                waitForActive = false;
            }
            else if (!waitForActive)
            {
                playerNetwork.SendPlayerState(pState);
                waitForActive = true;
            }

        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            if (state != NetworkState.P_TRAININGSTARTED && state != NetworkState.P_GAMESTARTED)
            {
                UI2DRenderer.WriteText(new Vector2(10, 0), ">>" + textMessage, Color.White, font);
            }
            else if (state == NetworkState.P_TRAININGSTARTED)
            {
                UI2DRenderer.WriteText(new Vector2(10, 70), "Jugador " + (myPlayer.Id + 3 * myPlayer.GroupId + 1), Color.White, this.font);
                TrainingMan.CurrentLevel.Draw();
                if (gamePaused)
                    UI2DRenderer.WriteText(new Vector2(350, 300), "Juego Pausado", Color.Yellow, this.font);
            }
            else if (state == NetworkState.P_GAMESTARTED)
            {
                UI2DRenderer.WriteText(new Vector2(10, 70), "Grupo " + (myPlayer.GroupId + 1), Color.White, this.font);
                UI2DRenderer.WriteText(new Vector2(10, 90), "Jugador " + (myPlayer.Id + 3 * myPlayer.GroupId + 1), Color.White, this.font);
                LevelMan.CurrentLevel.Draw();
                if (gamePaused)
                    UI2DRenderer.WriteText(new Vector2(350, 300), "Juego Pausado", Color.Yellow, this.font);
            }

            
            base.Draw(gameTime);
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            playerNetwork.Close();
            base.OnExiting(sender, args);
        }

        #endregion

        #region Network handlers
        private void ServerConnected()
        {
            state = NetworkState.P_CONNECTED;
            textMessage = "Presiona el boton para conectarte";
            initGui.Connect += Connect;
            initGui.EnableConnectPanel();
        }

        private void PlayerInfoReceived(PlayerInfo obj)
        {
            myPlayer = obj;
            LevelMan.PlayerId = obj.Id;

            ARManager.SetupMarkerTracking(sceneGraph, "calib.xml", Configuration.Instance.GetIntParam("main", "videoDevice"));

            //mando el id del jugador considerando q parten en 1.
            ARManager.SetupMarkers(sceneGraph, obj.Id + 3*obj.GroupId + 1);

            playerNetwork.SaveRecoveryInfo();
            
            //TrainingMan.Load("XMLLevel/TrainingPlayer.xml");
            TrainingMan.Init();
            TrainingMan.CurrentLevel.Players[0].Id = obj.Id;

            state = NetworkState.P_TRAININGSTARTED;
            
        }

        private void GameStateReceived(List<ObjectState> arg1)
        {
            if (state == NetworkState.P_GAMESTARTED)
            {
                foreach (ObjectState obj in arg1)
                {
                    int key = obj.Id;
                    if (LevelMan.CurrentLevel.ObjectsTable.ContainsKey(key))
                    {
                        GameObject updateObj = LevelMan.CurrentLevel.ObjectsTable[key];
                        updateObj.SetPosition(obj.Position);
                        
                        if ((obj is MetaDataObjectState))
                        {
                            MetaDataObjectState metaState = (MetaDataObjectState)obj;
                            ARCrystal crystal = (ARCrystal)updateObj;
                            crystal.Charge = metaState.Charge;
                        }
                    }
                }
            }
        }

        private void LevelStateReceived(LevelState obj)
        {
            if (state == NetworkState.P_GAMESTARTED)
            {
                LevelMan.LevelStateReceived(obj.Success);
            }
        }

        private void GoToLevelReceived(LevelInfo obj)
        {
            if (obj.GroupId == 1)
            {
                TrainingMan.GoToLevel(obj.Id);
                state = NetworkState.P_TRAININGSTARTED;
                PauseGame();
                
            }
            else if (obj.GroupId == 2)
            {
                TrainingMan.EndTraining();
                LevelMan.Init();
                LevelMan.GoToLevel(obj.Id);
                state = NetworkState.P_GAMESTARTED;
                PauseGame();
                
            }
            else if (state == NetworkState.P_TRAININGSTARTED)
            {
                TrainingMan.GoToLevel(obj.Id);
                PauseGame();
            }
            else if (state == NetworkState.P_GAMESTARTED)
            {
                LevelMan.GoToLevel(obj.Id);
                PauseGame();
            }
        }

        private void GameStarted()
        {
            TrainingMan.EndTraining();
            state = NetworkState.P_GAMESTARTED;
            //LevelMan.Load("XMLLevel/LevelsPlayer.xml");
            LevelMan.Init();
            PauseGame();
        }

        private void ResetLevel()
        {
            if (state == NetworkState.P_TRAININGSTARTED)
            {
                TrainingMan.CurrentLevel.ResetSubLevel();
            }
            else if (state == NetworkState.P_GAMESTARTED)
            {
                LevelMan.CurrentLevel.ResetSubLevel();
            }
        }

        private void PauseGame()
        {
            gamePaused = true;
            
            if (state == NetworkState.P_TRAININGSTARTED)
            {
                TrainingMan.CurrentLevel.Pause(gamePaused);
            }
            else if (state == NetworkState.P_GAMESTARTED)
            {
                LevelMan.CurrentLevel.Pause(gamePaused);
            }            
                
        }

        private void ResumeGame()
        {
            gamePaused = false;

            if (state == NetworkState.P_TRAININGSTARTED)
            {
                TrainingMan.CurrentLevel.Pause(gamePaused);
            }
            else if (state == NetworkState.P_GAMESTARTED)
            {
                LevelMan.CurrentLevel.Pause(gamePaused);
            }

        }

        private void RecoverToTraining(LevelInfo linfo)
        {
            if (state == NetworkState.P_RECOVERING)
            {
                ARManager.SetupMarkerTracking(sceneGraph, "calib.xml", Configuration.Instance.GetIntParam("main", "videoDevice"));
                ARManager.SetupMarkers(sceneGraph, myPlayer.Id + 3 * myPlayer.GroupId + 1);

                state = NetworkState.P_TRAININGSTARTED;
                //TrainingMan.Load("XMLLevel/TrainingPlayer.xml");
                TrainingMan.Init();
                TrainingMan.CurrentLevel.Players[0].Id = myPlayer.Id;
                if(linfo.Id > 0)
                    TrainingMan.GoToLevel(linfo.Id);
                ResumeGame();
            }
        }

        private void RecoverToMission(LevelInfo linfo)
        {
            if (state == NetworkState.P_RECOVERING)
            {
                ARManager.SetupMarkerTracking(sceneGraph, "calib.xml", Configuration.Instance.GetIntParam("main", "videoDevice"));
                ARManager.SetupMarkers(sceneGraph, myPlayer.Id + 3 * myPlayer.GroupId + 1);

                state = NetworkState.P_GAMESTARTED;
                //LevelMan.Load("XMLLevel/LevelsPlayer.xml");
                LevelMan.Init();
                if(linfo.Id > 0)
                    LevelMan.GoToLevel(linfo.Id);
                ResumeGame();
            }
        }

        #endregion

        #region Level Handlers
        private void SubLevelEndedHandler(int level, bool success)
        {
            if (state == NetworkState.P_TRAININGSTARTED)
            {
                LevelState levState = new LevelState(myPlayer.Id, myPlayer.GroupId, success);
                playerNetwork.SendLevelEnded(levState);
            }
        }
        #endregion

        #region GUI Handlers
        private void Connect()
        {
            if (playerNetwork != null && state == NetworkState.P_CONNECTED)
            {
                playerNetwork.SendPlayerConnected();
                textMessage = "Esperando inicio del juego...";
                initGui.DisableConnectPanel();
            }
        }

        private void Recover()
        {
            if (playerNetwork != null)
            {
                playerNetwork.SendReconnect();                
                initGui.DisableConnectPanel();
            }
        }
        #endregion
        
    }
}
