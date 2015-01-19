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
using Astronautas2D.Factories;
using Astronautas2D.Levels;
using AstroLib;
using Astronautas2D.Utils;
using Astronautas2D.GameObjects;
using Astronautas2D.Visual_Components;
using GameNetwork;

namespace Astronautas2D
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        // Elementos del juegos
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        GameObjectFactory objectFactory;
        GUIElementFactory guiFactory;
        GameLevelManager levelManager;
        MiceManager miceManager;
        Writer fontWriter;
        Writer messageWriter;
        SoundManager soundManager;

        Texture2D loadingBack;
        Texture2D startMissionBack;
        Texture2D mission1Back;
        Texture2D mission2Back;
        Texture2D mission3Back;

        Configuration configuration;

        // Variables para dibujar la pantalla
        private int BackBufferWidth;
        private int BackBufferHeight;
        private Rectangle viewRect;
        
        // Constantes a usar
        string levelListPath = "XMLLevels\\_LevelsList.xml";
        string configurationPath = "configuration.xml";

        private bool gamePaused = false;
        private bool narrative;
        private bool allTogether;
        private bool isFullScreen;

        private enum NetworkState { NONE, P_NOTCONNECTED, P_CONNECTED, P_WAITING, P_GAMESTARTED, P_RECOVERING };
        private PlayerNetworkManager playerNetwork;
        private string textMessage = "";
        private List<string> messageQueue = new List<string>();
        private NetworkState state = NetworkState.NONE;
        private GroupInfo myGroup;

        private KeyboardState previousState;

        private bool isSinglePlayer = true;
        private bool recovery = true;

        public Game1()
        {
            // Elementos de contenido
            Content.RootDirectory = "Content";

            playerNetwork = new PlayerNetworkManager();
            playerNetwork.Init();
            playerNetwork.ServerConnected += ServerConnected;
            playerNetwork.GroupInfoReceived += GroupInfoReceived;
            playerNetwork.GoToLevelReceived += GoToLevelReceived;
            playerNetwork.PauseGame += PauseGame;
            playerNetwork.ResumeGame += ResumeGame;
            playerNetwork.RecoverToMission += RecoverToMission;

            state = NetworkState.P_NOTCONNECTED;

            // Elementos graficos
            configuration = Configuration.Instance;
            // Cargamos el archivo de configuración y le asignamos la propiedad de fullScreen
            configuration.Load(configurationPath);
            isFullScreen = configuration.GetBoolParam("player", "fullscreen");
            narrative = configuration.GetBoolParam("player", "narrative");
            allTogether = configuration.GetBoolParam("player", "allTogether");
            BackBufferWidth = configuration.GetIntParam("player", "width");
            BackBufferHeight = configuration.GetIntParam("player", "height");
            viewRect = new Rectangle(0, 0, BackBufferWidth, BackBufferHeight);

            // Elementos de input
            miceManager = new MiceManager(viewRect);

            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = BackBufferWidth;
            graphics.PreferredBackBufferHeight = BackBufferHeight;
            graphics.IsFullScreen = isFullScreen;

            
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // Se inicializan las fabricas de objetos
            soundManager = SoundManager.Instance;
            objectFactory = new GameObjectFactory(miceManager, viewRect);
            guiFactory = new GUIElementFactory(viewRect);
            levelManager = new GameLevelManager(miceManager, viewRect);           
            levelManager.IndividualSubLevelEnded += IndividualSubLevelEndedHandler;
            levelManager.GroupSubLevelEnded += GroupSubLevelEndedHandler; 

            base.Initialize();
        }

        private void IndividualSubLevelEndedHandler(bool endState, int id)
        {
            if (!isSinglePlayer && myGroup != null)
            {
                LevelState state = new LevelState(id + 3 * myGroup.GroupId, myGroup.GroupId, endState);
                playerNetwork.SendLevelEnded(state);
            }
        }

        private void GroupSubLevelEndedHandler(bool endState)
        {
            if (!isSinglePlayer && myGroup != null)
            {
                LevelState state = new LevelState(myGroup.GroupId, myGroup.GroupId, endState);
                playerNetwork.SendLevelEnded(state);
            }
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
                        
            isSinglePlayer = configuration.GetBoolParam("player", "isSinglePlayer");
            if (!isSinglePlayer)
            {
                if(narrative)
                    messageQueue.Add("Esperando comunicacion con comando central...");
                else
                    messageQueue.Add("Esperando conexión con el profesor...");
            }
            

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            fontWriter = new Writer(spriteBatch);
            messageWriter = new Writer(spriteBatch);

            soundManager.Load(this.Content);
            objectFactory.LoadContent(this.Content);
            guiFactory.LoadContent(this.Content);
            fontWriter.loadContent(Content.Load<SpriteFont>("Fonts\\myFont"));
            if(narrative)
                messageWriter.loadContent(Content.Load<SpriteFont>("Fonts\\bigFontNarrative"));
            else
                messageWriter.loadContent(Content.Load<SpriteFont>("Fonts\\bigFontNoNarrative"));

            if (narrative)
            {
                loadingBack = Content.Load<Texture2D>("Sprites\\Narrative\\Backgrounds\\loading");
                startMissionBack = Content.Load<Texture2D>("Sprites\\Narrative\\Backgrounds\\mapaEspacial2");
                mission1Back = Content.Load<Texture2D>("Sprites\\Narrative\\Backgrounds\\mapaEspacialZona1");
                mission2Back = Content.Load<Texture2D>("Sprites\\Narrative\\Backgrounds\\mapaEspacialZona2");
                mission3Back = Content.Load<Texture2D>("Sprites\\Narrative\\Backgrounds\\mapaEspacialZona3");
            }
            else
                loadingBack = Content.Load<Texture2D>("Sprites\\NoNarrative\\Backgrounds\\White");

            //Cargamos los niveles
            levelManager.Load(this.levelListPath, objectFactory, guiFactory, viewRect);
            miceManager.loadContent(this.Content);
            levelManager.EndingScreen = objectFactory.getBackground(backgrounds.ending);

            // Cargamos el nivel actual
            if (isSinglePlayer)
            {
                TeamLevel sl = (TeamLevel)levelManager.CurrentLevel;
                sl.Load(levelManager.CurrentLevel.File);
                levelManager.Init();
            }

            recovery = configuration.GetBoolParam("player", "recovery");
            if (recovery)
            {
                GroupInfo pinfo = playerNetwork.RecoverConnection();
                if (pinfo != null)
                {
                    myGroup = pinfo;
                    levelManager.GroupNumber = myGroup.GroupId + 1;
                }
                if (narrative)
                    messageQueue.Add("Presionen Space para continuar con la mision");
                else
                    messageQueue.Add("Presionen Space para recuperar el juego");
                state = NetworkState.P_RECOVERING;
            }
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            playerNetwork.Close();
            base.OnExiting(sender, args);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            UpdateNetwork();
            // Allows the game to exit
            /*
            if (KeyDownOnce(Keyboard.GetState(), previousState, Keys.Escape))
                this.Exit();
            else*/
            if (KeyDownOnce(Keyboard.GetState(), previousState, Keys.R) && isSinglePlayer)
                levelManager.ResetLevel(true);
            else if (KeyDownOnce(Keyboard.GetState(), previousState, Keys.Right) && isSinglePlayer)
                levelManager.NextLevel();
            else if (KeyDownOnce(Keyboard.GetState(), previousState, Keys.Up) && isSinglePlayer)
                levelManager.GroupNumber = 1;
            else if (KeyDownOnce(Keyboard.GetState(), previousState, Keys.P) && isSinglePlayer)
                PauseGame();
            else if (KeyDownOnce(Keyboard.GetState(), previousState, Keys.Q) && isSinglePlayer)
                ResumeGame();
            else if (KeyDownOnce(Keyboard.GetState(), previousState, Keys.Space) && state == NetworkState.P_CONNECTED)
                Connect();
            else if (KeyDownOnce(Keyboard.GetState(), previousState, Keys.Space) && state == NetworkState.P_RECOVERING)
                Recover();


            if (!gamePaused && !levelManager.EndingReached)
            {
                miceManager.Update((float)gameTime.ElapsedGameTime.Milliseconds);
                levelManager.Update((float)gameTime.ElapsedGameTime.Milliseconds);
            }

            previousState = Keyboard.GetState();
            base.Update(gameTime);
        }

        private bool KeyDownOnce(KeyboardState newState, KeyboardState oldState, Keys key)
        {
            return newState.IsKeyDown(key) && !oldState.IsKeyDown(key);
        }

        private void UpdateNetwork()
        {
            if (state == NetworkState.P_NOTCONNECTED)
                playerNetwork.HandleConnectionMessages();
            else
                playerNetwork.HandleNetworkMessages();
        }

        private void Connect()
        {
            if (playerNetwork != null && state == NetworkState.P_CONNECTED)
            {
                playerNetwork.SendPlayerConnected();
                if(narrative)
                    messageQueue.Add("Esperando inicio del entrenamiento...");
                else
                    messageQueue.Add("Esperando inicio del juego...");
                state = NetworkState.P_WAITING;
            }
        }

        private void Recover()
        {
            if (playerNetwork != null && state == NetworkState.P_RECOVERING)
            {
                playerNetwork.SendReconnect();
                messageQueue.Add("Esperando reconexion...");
                state = NetworkState.P_WAITING;
            }
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            GraphicsDevice.Clear(Color.Black);
            if (isSinglePlayer || state == NetworkState.P_GAMESTARTED)
            {
                this.levelManager.Draw(gameTime, spriteBatch, fontWriter);
                /*
                TeamLevel sl = (TeamLevel)levelManager.CurrentLevel;
                sl.Draw(gameTime, spriteBatch, fontWriter);
                miceManager.DrawMice(spriteBatch);
                */
            }
            
            if (gamePaused || (!isSinglePlayer && state != NetworkState.P_GAMESTARTED))
                spriteBatch.Draw(loadingBack, new Rectangle(0, 0, BackBufferWidth, BackBufferHeight), Color.White);
            int i = 0;
            foreach (string s in messageQueue)
            {
                messageWriter.DrawText(s, new Vector2(50, 100 + 50*i), narrative ? Color.LightSeaGreen : Color.Black);
                i++;
            }
            base.Draw(gameTime);
            spriteBatch.End();
        }


        #region Network Handlers
        private void ServerConnected()
        {
            if (!recovery)
            {
                state = NetworkState.P_CONNECTED;
                if (narrative)
                    messageQueue.Add("Presiona space para conectarte al comando central");
                else
                    messageQueue.Add("Presiona space para conectarte al profesor");
            }
        }

        private void GroupInfoReceived(GroupInfo obj)
        {
            myGroup = obj;
            levelManager.GroupNumber = myGroup.GroupId +1;
            state = NetworkState.P_GAMESTARTED;
            TeamLevel sl = (TeamLevel)levelManager.CurrentLevel;
            sl.Load(levelManager.CurrentLevel.File);
            messageQueue.Clear();
            playerNetwork.SaveRecoveryInfo();
            levelManager.Init();
            PauseGame();
        }


        private void GoToLevelReceived(LevelInfo obj)
        {
            
            bool b = levelManager.GoToLevel(obj.Id);
            //levelManager.GroupNumber = myGroup.GroupId + 1;
            
            if(b) PauseGame();
        }

        private void PauseGame()
        {
            messageQueue.Clear();
            if (narrative)
            {
                messageQueue.Add("Mision pausada");
                messageQueue.Add("Esperando proximas instrucciones");
            }
            else
            {
                messageQueue.Add("Juego pausado");
                messageQueue.Add("Esperando próximas instrucciones");
            }
            gamePaused = true;
        }

        private void ResumeGame()
        {
            messageQueue.Clear();
            gamePaused = false;
        }

        private void RecoverToMission(LevelInfo obj)
        {
            messageQueue.Clear();
            state = NetworkState.P_GAMESTARTED;
            //levelManager.Init();            
            levelManager.GoToLevel(obj.Id);
            //PauseGame();
        }
        #endregion
    }
}
