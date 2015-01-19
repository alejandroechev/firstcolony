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
using GoblinXNA.Device.Generic;
using ARGame.GameGUI;

namespace ARGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class ServerGame : AbstractGame
    {
        private const int WIDTH = 800;
        private const int HEIGHT = 600;

        SpriteBatch spriteBatch;

        private Dictionary<int, ServerLevelManager> LevelManTable;
        private Dictionary<ServerLevelManager, int> InvLevelManTable;
        private Dictionary<int, ServerLevelManager> TrainingManTable;
        private Dictionary<ServerLevelManager, int> InvTrainingManTable;
        private GroupManager groupManager;

        private enum NetworkState { NONE, S_SENDINGDISCOVERY, S_TRAININGSTARTED, S_GAMESTARTED };
        private ServerNetworkManager serverNetwork;
        private string textMessage = "";
        private string statusMessage = "";
        private NetworkState state = NetworkState.NONE;

        //GUIs
        //private ServerInitGUI initGui;
        //private ServerIndivGUI indGui;
        //private ServerGroupGUI groupGui;

        private bool gamePaused = false;

        private TimeSpan lastTime = new TimeSpan();

        private List<ResultData> dataMessage = new List<ResultData>();

        private bool showVideos = false;

        private SpriteFont font;
        private SpriteFont smallFont;

        private KeyboardState previousState;

        List<Video> videos = new List<Video>();
        int currentVideo = 0;
        VideoPlayer player;
        Texture2D videoTexture;

        bool firstScene = true;
        ResolveTexture2D backTexture;
        Texture2D displayTex;


        public ServerGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            contentManager = new ContentManager();

            LevelManTable = new Dictionary<int, ServerLevelManager>();
            InvLevelManTable = new Dictionary<ServerLevelManager, int>();
            TrainingManTable = new Dictionary<int, ServerLevelManager>();
            InvTrainingManTable = new Dictionary<ServerLevelManager, int>();
            groupManager = new GroupManager();

            state = NetworkState.S_SENDINGDISCOVERY;
            
            serverNetwork = new ServerNetworkManager();
            serverNetwork.Init();
            serverNetwork.NewPlayer += NewPlayer;
            serverNetwork.PlayerStateReceived += PlayerStateReceived;
            serverNetwork.LevelEndedReceived += LevelEndedReceived;
            serverNetwork.PlayerDisconnected += PlayerDisconnected;
            serverNetwork.PlayerRecovered += PlayerRecovered;

            IsMouseVisible = true;

            textMessage = "Server Sending Discovery";

        }

        

        #region XNA GameMethods

        protected override void Initialize()
        {
            // Initialize the GoblinXNA framework
            State.InitGoblin(graphics, Content, "");

            // Initialize the scene graph
            sceneGraph = new Scene(this);
            sceneGraph.PreferPerPixelLighting = false;

            //ARManager.SetupMarkerTracking(sceneGraph, "calib.xml", Configuration.Instance.GetIntParam("main", "videoDevice"));

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
                    model.CastShadows = true;
                    model.ReceiveShadows = true;
                    model.UseInternalMaterials = true;
                    contentManager.AddModel(name, model);
                }
            }

            spriteBatch = new SpriteBatch(GraphicsDevice);
            

            font = Content.Load<SpriteFont>("myFont");
            smallFont = Content.Load<SpriteFont>("smallFont");


            backTexture = new ResolveTexture2D(
               graphics.GraphicsDevice,
               graphics.GraphicsDevice.PresentationParameters.BackBufferWidth,
               graphics.GraphicsDevice.PresentationParameters.BackBufferHeight,
               1,
               graphics.GraphicsDevice.PresentationParameters.BackBufferFormat);

            
            videos.Add(Content.Load<Video>("Intro"));
            videos.Add(Content.Load<Video>("MissionStart"));
            videos.Add(Content.Load<Video>("Asteroides"));
            videos.Add(Content.Load<Video>("End"));

            player = new VideoPlayer();

            //initGui = new ServerInitGUI();
            //initGui.Init(sceneGraph, contentManager, font);
            //initGui.Start += Start;
        }


        protected override void Update(GameTime gameTime)
        {
            if (KeyDownOnce(Keyboard.GetState(), previousState, Keys.Enter))
            {
                showVideos = true;
                player.IsLooped = false;
                player.Play(videos[currentVideo]);
                currentVideo++;
            }
            else if (KeyDownOnce(Keyboard.GetState(), previousState, Keys.Escape))
            {
                player.Stop();
                showVideos = false;
                if (firstScene)
                {
                    GraphicsDevice.ResolveBackBuffer(backTexture);
                    firstScene = false;
                    statusMessage = "Empezar Juego";
                }
            }
            else if (KeyDownOnce(Keyboard.GetState(), previousState, Keys.Space) && state == NetworkState.S_SENDINGDISCOVERY)
            {
                Start();
            }
            else if (KeyDownOnce(Keyboard.GetState(), previousState, Keys.Right))
            {
                NextLevel();
            }
            else if (KeyDownOnce(Keyboard.GetState(), previousState, Keys.P))
            {
                PauseGame();
            }
            else if (KeyDownOnce(Keyboard.GetState(), previousState, Keys.R))
            {
                ResumeGame();
            }
            else if (KeyDownOnce(Keyboard.GetState(), previousState, Keys.D1))
            {
                if(state == NetworkState.S_TRAININGSTARTED)
                    ResetPlayerLevel(0);
                else
                    ResetGroupLevel(0);
            }
            else if (KeyDownOnce(Keyboard.GetState(), previousState, Keys.D2))
            {
                if (state == NetworkState.S_TRAININGSTARTED)
                    ResetPlayerLevel(1);
                else
                    ResetGroupLevel(1);
            }
            else if (KeyDownOnce(Keyboard.GetState(), previousState, Keys.D3))
            {
                if (state == NetworkState.S_TRAININGSTARTED)
                    ResetPlayerLevel(2);
                else
                    ResetGroupLevel(2);
            }
            else if (KeyDownOnce(Keyboard.GetState(), previousState, Keys.D4))
            {
                ResetPlayerLevel(3);
            }
            else if (KeyDownOnce(Keyboard.GetState(), previousState, Keys.D5))
            {
                ResetPlayerLevel(4);
            }
            else if (KeyDownOnce(Keyboard.GetState(), previousState, Keys.D6))
            {
                ResetPlayerLevel(5);
            }
            else if (KeyDownOnce(Keyboard.GetState(), previousState, Keys.D7))
            {
                ResetPlayerLevel(6);
            }
            else if (KeyDownOnce(Keyboard.GetState(), previousState, Keys.D8))
            {
                ResetPlayerLevel(7);
            }
            else if (KeyDownOnce(Keyboard.GetState(), previousState, Keys.D9))
            {
                ResetPlayerLevel(8);
            }

            previousState = Keyboard.GetState();

            UpdateNetwork(gameTime);

            if(!gamePaused)
                UpdateLevels(gameTime);

            base.Update(gameTime);
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            serverNetwork.Close();
            base.OnExiting(sender, args);
        }

        private void UpdateLevels(GameTime gameTime)
        {
            if (state == NetworkState.S_TRAININGSTARTED)
            {
                foreach (LevelManager levelMan in TrainingManTable.Values)
                    levelMan.CurrentLevel.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            }
            else if (state == NetworkState.S_GAMESTARTED)
            {
                foreach (LevelManager levelMan in LevelManTable.Values)
                    levelMan.CurrentLevel.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            }
        }

        private void UpdateNetwork(GameTime gameTime)
        {
            if (state == NetworkState.S_SENDINGDISCOVERY)
            {
                serverNetwork.HandleNetworkMessages();
                if (gameTime.TotalGameTime.Subtract(lastTime).Milliseconds > 100)
                {
                    serverNetwork.SendDiscovery();
                    lastTime = gameTime.TotalGameTime;
                }
            }
            else if (state == NetworkState.S_TRAININGSTARTED)
            {
                serverNetwork.HandleNetworkMessages();
            }
            else if (state == NetworkState.S_GAMESTARTED)
            {
                serverNetwork.HandleNetworkMessages();
                if (gameTime.TotalGameTime.Subtract(lastTime).Milliseconds > 100)
                {
                    SendObjectStates();
                    lastTime = gameTime.TotalGameTime;
                }
            }

        }

        private bool KeyDownOnce(KeyboardState newState, KeyboardState oldState, Keys key)
        {
            return newState.IsKeyDown(key) && !oldState.IsKeyDown(key);
        }


        private void SendObjectStates()
        {
            List<ObjectState> objStates = new List<ObjectState>();
            List<MetaDataObjectState> metaStates = new List<MetaDataObjectState>();
            GetObjectAndPlayerStates(objStates, metaStates);
            if (objStates.Count > 0 || metaStates.Count > 0)
                serverNetwork.SendGameState(objStates, metaStates);
        }

        private void GetObjectAndPlayerStates(List<ObjectState> states, List<MetaDataObjectState> metaStates)
        {
            foreach (KeyValuePair<int, ServerLevelManager> levelManPair in LevelManTable)
            {
                Dictionary<int, GameObject> levelObjects = levelManPair.Value.CurrentLevel.ObjectsTable;
                foreach (KeyValuePair<int, GameObject> obj in levelObjects)
                {
                    if (obj.Value.Position.X != obj.Value.LastPosition.X || obj.Value.Position.Y != obj.Value.LastPosition.Y
                            || obj.Value.Position.Z != obj.Value.LastPosition.Z)
                    {
                        if (!(obj.Value is ARAstronaut) && !(obj.Value is ARCrystal))
                        {
                            
                                ObjectState state = new ObjectState(obj.Key, levelManPair.Key, true, obj.Value.Position);
                                states.Add(state);
                            
                        }
                        else if ((obj.Value is ARCrystal))
                        {
                            ARCrystal crystal = (ARCrystal)obj.Value;
                            MetaDataObjectState metaState = new MetaDataObjectState(obj.Key, levelManPair.Key, true, obj.Value.Position,
                                crystal.Charge);
                            metaStates.Add(metaState);
                        }
                    }
                }
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(SpriteBlendMode.Additive);
            Rectangle screen = new Rectangle(GraphicsDevice.Viewport.X,
                    GraphicsDevice.Viewport.Y,
                    GraphicsDevice.Viewport.Width,
                    GraphicsDevice.Viewport.Height);

            if (showVideos)
            {
                if (player.State != MediaState.Stopped)
                    videoTexture = player.GetTexture();

                if (videoTexture != null)
                    spriteBatch.Draw(videoTexture, screen, Color.White);
            }
            else
            {
                if (backTexture != null)
                    spriteBatch.Draw(backTexture, screen, new Color(255, 255, 255, 255));
                //if (state == NetworkState.S_INDIVGAMESTARTED || state == NetworkState.S_GROUPGAMESTARTED)
                //    spriteBatch.Draw(displayTex, new Rectangle(30,120,200, displayTex.Height), new Color(255, 255, 255, 255));
                spriteBatch.DrawString(font, textMessage, new Vector2(50, 150), Color.White);
                spriteBatch.DrawString(font, statusMessage, new Vector2(10, Window.ClientBounds.Height - 50), Color.White);
                int i = 0;
                foreach (ResultData r in dataMessage)
                {
                    spriteBatch.DrawString(smallFont, r.text, new Vector2(50, 180 + 20 * i), r.success ? new Color(95, 216, 109) : new Color(255, 120, 120));
                    i++;
                }

            }
            spriteBatch.End();
            
            base.Draw(gameTime);
        }

        #endregion

        #region Network Handlers
        private void PlayerStateReceived(PlayerState obj)
        {
            if (state == NetworkState.S_GAMESTARTED)
            {
                int groupId = obj.GroupId;
                LevelManager levelMan = LevelManTable[groupId];
                ServerLevel currLevel = (ServerLevel)levelMan.CurrentLevel;
                currLevel.SetPlayerInfo(obj, groupManager.PlayersInGroup(groupId));
                //groupManager.LogPlayerState(obj.Id, obj.GroupId, obj.Charge, obj.IsAlive, currLevel.Crystal.PotentialCharge);
            }
        }

        private void NewPlayer()
        {
            groupManager.NewPlayer();
            statusMessage = "Conexiones establecidas: "  +groupManager.AllPlayers.Count;
            
        }

        private void LevelEndedReceived(LevelState obj)
        {
            if (state == NetworkState.S_TRAININGSTARTED)
            {
                int playerId = obj.Id;
                int groupId = obj.GroupId;
                
                if (obj.Success)
                    groupManager.IncreasePlayerIterations(playerId + 3*groupId);
                else
                    groupManager.IncreasePlayerTries(playerId + 3 * groupId);

                dataMessage = groupManager.GetCurrentPlayerAllData();
            }
            
        }

        private void PlayerDisconnected(string sIp, PlayerInfo player)
        {
            if (player != null)
                statusMessage = " Jugador desconectado. Id: " + (player.Id + 1) + " Grupo: " + (player.GroupId+1);
            else
                statusMessage = " Jugador desconocido desconectado ";
        }

        private void PlayerRecovered(PlayerInfo player)
        {
            if (state == NetworkState.S_TRAININGSTARTED)
            {
                LevelInfo info = new LevelInfo(TrainingManTable[0].CurrentLevelIndex , 0);
                serverNetwork.SendRecoverToTraining(info, player);
                groupManager.ResetPlayerData(player.Id + 3 * player.GroupId);
            }
            else if (state == NetworkState.S_GAMESTARTED)
            {
                LevelInfo info = new LevelInfo(LevelManTable[0].CurrentLevelIndex, 0);
                serverNetwork.SendRecoverToMission(info, player);
            }
            statusMessage = " Jugador recuperado. Id: " + (player.Id + 1) + " Grupo: " + (player.GroupId + 1);
        }
        #endregion

        #region LevelManager Handlers
        private void SubLevelEnded(ServerLevelManager level, bool endState)
        {
            if (state == NetworkState.S_GAMESTARTED)
            {
                int groupId = InvLevelManTable[level];
                LevelState info = new LevelState(level.CurrentLevelIndex, groupId, endState);
                serverNetwork.SendLevelEnded(info);

                if (endState)
                    groupManager.IncreaseGroupIterations(groupId);
                else
                    groupManager.IncreaseGroupTries(groupId);
                SendObjectStates();
                dataMessage = groupManager.GetCurrentGroupIterationsData();
            }
        }
        #endregion

        #region GUI Handlers
        private void Start()
        {
            if (Configuration.Instance.GetBoolParam("main", "hasTraining"))
            {
                if (serverNetwork != null && state == NetworkState.S_SENDINGDISCOVERY)
                {
                    foreach (PlayerInfo info in groupManager.AllPlayers)
                    {
                        ServerLevelManager levelMan = new ServerLevelManager(this);
                        levelMan.Load("XMLLevel/TrainingServer.xml");
                        levelMan.Init();

                        TrainingManTable.Add(info.Id + 3 * info.GroupId, levelMan);
                        InvTrainingManTable.Add(levelMan, info.Id + 3 * info.GroupId);
                    }

                    serverNetwork.SendPlayerInfo(groupManager.AllPlayers);
                    state = NetworkState.S_TRAININGSTARTED;

                    
                    //indGui = new ServerIndivGUI(groupManager.PlayersData);
                    //indGui.Init(sceneGraph, contentManager, font);
                    //indGui.NextLevel += NextLevel;
                    //indGui.ResetLevel += ResetPlayerLevel;
                    //indGui.PauseGame += PauseGame;
                    //indGui.ResumeGame += ResumeGame;
                    //indGui.PauseButton.DoClick();

                    if (Configuration.Instance.GetIntParam("main", "firstLevel") == 0)
                        groupManager.NextTraining();
                    else
                        GoToLevel(Configuration.Instance.GetIntParam("main", "firstLevel"));


                    textMessage = "Entrenamiento " + (groupManager.CurrentTraining + 1);
                    dataMessage = groupManager.GetCurrentPlayerAllData();
                    statusMessage = "";

                    PauseGame();
                }
            }
            else
            {
                if (serverNetwork != null && state == NetworkState.S_SENDINGDISCOVERY)
                {
                    foreach (int groupId in groupManager.PlayersByGroup.Keys)
                    {
                        ServerLevelManager levelMan = new ServerLevelManager(this);
                        levelMan.Load("XMLLevel/LevelsServer.xml");
                        levelMan.Init();
                        levelMan.SubLevelEnded += SubLevelEnded;

                        LevelManTable.Add(groupId, levelMan);
                        InvLevelManTable.Add(levelMan, groupId);
                    }


                    serverNetwork.SendPlayerInfo(groupManager.AllPlayers);
                    state = NetworkState.S_GAMESTARTED;
                    
                    //groupGui = new ServerGroupGUI(groupManager.GroupsData);
                    //groupGui.Init(sceneGraph, contentManager, font);
                    //groupGui.NextLevel += NextLevel;
                    //groupGui.ResetLevel += ResetGroupLevel;
                    //groupGui.PauseGame += PauseGame;
                    //groupGui.ResumeGame += ResumeGame;
                    //groupGui.PauseButton.DoClick();

                    if (Configuration.Instance.GetIntParam("main", "firstLevel") == 0)
                        groupManager.NextLevel();
                    else
                        GoToLevel(Configuration.Instance.GetIntParam("main", "firstLevel"));
                }
            }
        }

        private void StartMissions()
        {
            if (state == NetworkState.S_TRAININGSTARTED)
            {
                foreach (int groupId in groupManager.PlayersByGroup.Keys)
                {
                    ServerLevelManager levelMan = new ServerLevelManager(this);
                    levelMan.Load("XMLLevel/LevelsServer.xml");
                    levelMan.Init();
                    levelMan.SubLevelEnded += SubLevelEnded;

                    LevelManTable.Add(groupId, levelMan);
                    InvLevelManTable.Add(levelMan, groupId);
                }

                serverNetwork.SendGameStart();
                state = NetworkState.S_GAMESTARTED;
                groupManager.NextLevel();

                textMessage = "Misión " + (groupManager.CurrentLevel);
                dataMessage = groupManager.GetCurrentGroupIterationsData();

                PauseGame();
                //groupGui = new ServerGroupGUI(groupManager.GroupsData);
                //groupGui.Init(sceneGraph, contentManager, font);
                //groupGui.NextLevel += NextLevel;
                //groupGui.ResetLevel += ResetGroupLevel;
                //groupGui.PauseGame += PauseGame;
                //groupGui.ResumeGame += ResumeGame;
                //groupGui.PauseButton.DoClick();
            }
        }

        private void ResetPlayerLevel(int player)
        {
            if (state == NetworkState.S_TRAININGSTARTED)
            {
                if (TrainingManTable.Count > 0)
                {
                    TrainingManTable[player].CurrentLevel.ResetSubLevel();
                    int group = player / 3;
                    int playerId = player % 3;
                    serverNetwork.SendResetLevelPlayer(playerId, group);
                }
            }
        }

        private void ResetGroupLevel(int group)
        {
            if (state == NetworkState.S_GAMESTARTED)
            {
                if (LevelManTable.Count > 0)
                {
                    LevelManTable[group].CurrentLevel.ResetSubLevel();
                    serverNetwork.SendResetLevelGroup(group);
                                        
                }
            }
        }

        private void NextLevel()
        {

            if (state == NetworkState.S_TRAININGSTARTED)
            {
                if (TrainingManTable.Count > 0)
                {
                    if (TrainingManTable[0].GameEndedFlag)
                    {
                        StartMissions();
                    }
                    else
                    {
                        LevelInfo info = new LevelInfo(TrainingManTable[0].CurrentLevelIndex + 1, 0);
                        serverNetwork.SendGoToLevel(info);
                        foreach (LevelManager levelMan in TrainingManTable.Values)
                        {
                            levelMan.NextLevel();
                        }
                        groupManager.NextTraining();
                        textMessage = "Entrenamiento " + (groupManager.CurrentTraining + 1);
                        dataMessage = groupManager.GetCurrentPlayerAllData();

                        PauseGame();
                    }                    
                }
                groupManager.PrintData();
                
            }
            else if (state == NetworkState.S_GAMESTARTED)
            {
                if (LevelManTable.Count > 0)
                {
                    if (!LevelManTable[0].GameEndedFlag)
                    {
                        LevelInfo info = new LevelInfo(LevelManTable[0].CurrentLevelIndex + 1, 0);
                        serverNetwork.SendGoToLevel(info);
                        foreach (LevelManager levelMan in LevelManTable.Values)
                        {
                            levelMan.NextLevel();
                        }
                        groupManager.NextLevel();

                        textMessage = "Misión " + (groupManager.CurrentLevel);
                        dataMessage = groupManager.GetCurrentGroupIterationsData();
                        
                        PauseGame();
                    }
                    
                }
                groupManager.PrintData();
                //groupManager.PrintLogs();
            }
        }

        private void GoToLevel(int level)
        {

            if (state == NetworkState.S_TRAININGSTARTED)
            {
                if (TrainingManTable.Count > 0)
                {
                    LevelInfo info = new LevelInfo(level, 1);
                    serverNetwork.SendGoToLevel(info);
                    foreach (LevelManager levelMan in TrainingManTable.Values)
                    {
                        levelMan.GoToLevel(level);
                    }
                    groupManager.NextTraining();
                    PauseGame();

                }

            }
            else if (state == NetworkState.S_GAMESTARTED)
            {
                if (LevelManTable.Count > 0)
                {
                    if (!LevelManTable[0].GameEndedFlag)
                    {
                        LevelInfo info = new LevelInfo(level, 2);
                        serverNetwork.SendGoToLevel(info);
                        foreach (LevelManager levelMan in LevelManTable.Values)
                        {
                            levelMan.GoToLevel(level);
                        }
                        groupManager.NextLevel();
                        PauseGame();
                    }

                }
            }
        }

        private void PauseGame()
        {
            gamePaused = true;
            statusMessage = "Juego pausado";
            serverNetwork.SendPauseGame();
        }

        private void ResumeGame()
        {
            gamePaused = false;
            statusMessage = "";
            serverNetwork.SendResumeGame();
        }

        
        #endregion

    }
}
