using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using GameNetwork;
using GameUtils;

namespace ServerGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class ServerGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private enum NetworkState { NONE, S_SENDINGDISCOVERY, S_INDIVGAMESTARTED, S_GROUPGAMESTARTED };

        private NetworkState state = NetworkState.NONE;
        private ServerNetworkManager serverNetwork;

        private GroupManager groupManager;

        private TimeSpan lastTime = new TimeSpan();

        private string textMessage = "";
        private string statusMessage = "";
        private List<ResultData> dataMessage = new List<ResultData>();

        private bool showVideos = false;

        private SpriteFont font;
        private SpriteFont smallFont;

        private KeyboardState previousState;

        Configuration config;

        private string levelListPath = "XMLLevels\\_LevelsList.xml";
        
        List<Video> videos = new List<Video>();
        int currentVideo = 0;
        VideoPlayer player;
        Texture2D videoTexture;

        bool firstScene = true;
        ResolveTexture2D backTexture;
        Texture2D displayTex;

        int indivLevels = 0;
        int groupLevels = 0;
        bool loadLevelsFile = false;
        
        public ServerGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            state = NetworkState.S_SENDINGDISCOVERY;

            serverNetwork = new ServerNetworkManager();
            serverNetwork.Init();
            serverNetwork.NewGroup += NewGroup;
            serverNetwork.LevelEndedReceived += LevelEndedReceived;
            serverNetwork.GroupDisconnected += GroupDisconnected;
            serverNetwork.GroupRecovered += GroupRecovered;

            statusMessage = "Cargando...";

            config = Configuration.Instance;
            config.Load("configuration.xml");
            graphics.IsFullScreen = config.GetBoolParam("server", "fullscreen");
            indivLevels = config.GetIntParam("server", "numIndividualLevels");
            groupLevels = config.GetIntParam("server", "numGroupLeveles");
            loadLevelsFile = config.GetBoolParam("server", "loadLevelsFile");
            
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            font = Content.Load<SpriteFont>("myFont");
            smallFont = Content.Load<SpriteFont>("smallFont");

            
            backTexture = new ResolveTexture2D(
               graphics.GraphicsDevice,
               graphics.GraphicsDevice.PresentationParameters.BackBufferWidth,
               graphics.GraphicsDevice.PresentationParameters.BackBufferHeight,
               1,
               graphics.GraphicsDevice.PresentationParameters.BackBufferFormat);

            groupManager = new GroupManager(indivLevels, groupLevels);
            if(loadLevelsFile)
                groupManager.Load(levelListPath);
            
            videos.Add(Content.Load<Video>("Intro"));
            videos.Add(Content.Load<Video>("MissionStart"));
            videos.Add(Content.Load<Video>("Asteroides"));
            videos.Add(Content.Load<Video>("End"));

            player = new VideoPlayer();
            
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            serverNetwork.Close();
            base.OnExiting(sender, args);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
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
                StartGame();
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
                Reset(0);
            }
            else if (KeyDownOnce(Keyboard.GetState(), previousState, Keys.D2))
            {
                Reset(1);
            }
            else if (KeyDownOnce(Keyboard.GetState(), previousState, Keys.D3))
            {
                Reset(2);
            }
            else if (KeyDownOnce(Keyboard.GetState(), previousState, Keys.D4))
            {
                Reset(3);
            }
            else if (KeyDownOnce(Keyboard.GetState(), previousState, Keys.D5))
            {
                Reset(4);
            }
            else if (KeyDownOnce(Keyboard.GetState(), previousState, Keys.D6))
            {
                Reset(5);
            }
            else if (KeyDownOnce(Keyboard.GetState(), previousState, Keys.D7))
            {
                Reset(6);
            }
            else if (KeyDownOnce(Keyboard.GetState(), previousState, Keys.D8))
            {
                Reset(7);
            }
            else if (KeyDownOnce(Keyboard.GetState(), previousState, Keys.D9))
            {
                Reset(8);
            }

            previousState = Keyboard.GetState();

            UpdateNetwork(gameTime);

            base.Update(gameTime);
        }

        private bool KeyDownOnce(KeyboardState newState, KeyboardState oldState, Keys key)
        {
            return newState.IsKeyDown(key) && !oldState.IsKeyDown(key);
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
            else if (state == NetworkState.S_INDIVGAMESTARTED || state == NetworkState.S_GROUPGAMESTARTED)
            {
                serverNetwork.HandleNetworkMessages();
            }

        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
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
                    spriteBatch.Draw(backTexture, screen, new Color(255,255,255,255));
                //if (state == NetworkState.S_INDIVGAMESTARTED || state == NetworkState.S_GROUPGAMESTARTED)
                //    spriteBatch.Draw(displayTex, new Rectangle(30,120,200, displayTex.Height), new Color(255, 255, 255, 255));
                spriteBatch.DrawString(font, textMessage, new Vector2(50, 150), Color.White);
                spriteBatch.DrawString(font, statusMessage, new Vector2(10, Window.ClientBounds.Height - 50), Color.White);
                int i = 0;
                foreach (ResultData r in dataMessage)
                {
                    spriteBatch.DrawString(smallFont, r.text, new Vector2(50, 180 + 20*i), r.success ? new Color(95, 216, 109) : new Color(255, 120, 120));
                    i++;
                }

            }
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void StartGame()
        {
            serverNetwork.SendGroupInfo(groupManager.AllGroups);
            groupManager.NextTraining();
            state = NetworkState.S_INDIVGAMESTARTED;
            textMessage = "Entrenamiento " + (groupManager.CurrentTraining + 1);
            dataMessage = groupManager.GetCurrentPlayerAllData();
            statusMessage = "";
        }

        private void NextLevel()
        {
            if (state == NetworkState.S_INDIVGAMESTARTED)
            {
                if (groupManager.NextTraining())
                {
                    LevelInfo info = new LevelInfo(groupManager.CurrentTraining, 0);
                    serverNetwork.SendGoToLevel(info);
                    textMessage = "Entrenamiento " + (groupManager.CurrentTraining + 1);
                    dataMessage = groupManager.GetCurrentPlayerAllData();
                }
                else
                {
                    state = NetworkState.S_GROUPGAMESTARTED;
                }
            }
            if (state == NetworkState.S_GROUPGAMESTARTED)
            {
                if (groupManager.NextLevel())
                {
                    LevelInfo info = new LevelInfo(groupManager.CurrentTraining + groupManager.CurrentLevel, 0);
                    serverNetwork.SendGoToLevel(info);
                    if (loadLevelsFile)
                    {
                        if (groupManager.CurrentLevel > 1)
                            textMessage = "Misión " + (groupManager.CurrentLevel - 1);
                        else if (groupManager.CurrentLevel <= 1)
                            textMessage = "Entrenamiento grupal";
                    }
                    else 
                        textMessage = "Misión " + (groupManager.CurrentLevel);

                    dataMessage = groupManager.GetCurrentGroupIterationsData();
                }
                else
                {
                    statusMessage = "Juego terminado";
                    LevelInfo info = new LevelInfo(groupManager.CurrentTraining + groupManager.CurrentLevel + 1, 0);
                    serverNetwork.SendGoToLevel(info);
                    
                }
            }
            groupManager.PrintData();

        }

        private void Reset(int group)
        {            
            serverNetwork.SendResetLevelGroup(group);            
        }

        #region Network Handlers
        private void NewGroup()
        {
            groupManager.NewGroup();
            statusMessage = "Conexiones establecidas: " + groupManager.AllGroups.Count;

        }

        private void LevelEndedReceived(LevelState obj)
        {
            int playerId = obj.Id;
            int groupId = obj.GroupId;

            if (state == NetworkState.S_INDIVGAMESTARTED)
            {
                if (obj.Success)
                    groupManager.IncreasePlayerIterations(playerId);
                else
                    groupManager.IncreasePlayerTries(playerId);

                dataMessage = groupManager.GetCurrentPlayerAllData();
            }
            else if (state == NetworkState.S_GROUPGAMESTARTED)
            {
                if (obj.Success)
                    groupManager.IncreaseGroupIterations(groupId);
                else
                    groupManager.IncreaseGroupTries(groupId);

                dataMessage = groupManager.GetCurrentGroupIterationsData();
            }

        }

        private void GroupDisconnected(string sIp, GroupInfo group)
        {
            if (group != null)
                statusMessage = " Grupo " + (group.GroupId+1) + " desconectado";
            else
                statusMessage = " Grupo desconectado";
        }

        private void GroupRecovered(GroupInfo group)
        {
            LevelInfo info = new LevelInfo(0, 0);
            if (state == NetworkState.S_INDIVGAMESTARTED)
                info = new LevelInfo(groupManager.CurrentTraining, 0);
            else if (state == NetworkState.S_GROUPGAMESTARTED)
                info = new LevelInfo(groupManager.CurrentTraining + groupManager.CurrentLevel, 0);
            
            serverNetwork.SendRecoverToMission(info, group);
            statusMessage = " Grupo " + (group.GroupId+1) + " recuperado";
        }
        #endregion

        #region NetworkCommands
        private void PauseGame()
        {
            statusMessage = "Juego pausado";
            serverNetwork.SendPauseGame();
        }

        private void ResumeGame()
        {
            statusMessage = "" ;
            serverNetwork.SendResumeGame();
        }
        #endregion

    }
}
