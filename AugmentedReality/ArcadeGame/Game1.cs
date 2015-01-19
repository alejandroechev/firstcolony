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
using GameModel;
using GameNetwork;

namespace ArcadeGame
{
    
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public enum Text { BACKGROUND,ASTEROID, ASTRONAUT, CRYSTAL, PORTAL };
        const int WIDTH = 800;
        const int HEIGHT = 600;
        GraphicsDeviceManager graphics;
        Texture2D[] textures;        
        Rectangle viewportRect;
        SpriteBatch spriteBatch;
        SpriteFont font;
        Scene scene;
        float inputTime;

        //Network test
        public enum NetworkState { NONE, P_NOTCONNECTED, P_CONNECTED, S_SENDINGDISCOVERY, S_GAMESTARTED, P_GAMESTARTED };
        ServerNetworkManager serverNetwork;
        PlayerNetworkManager playerNetwork;
        string textMessage = "";
        string textSubMessage = "";
        NetworkState state = NetworkState.NONE;
        List<PlayerInfo> players = new List<PlayerInfo>();

        Dictionary<int, GameObject> objectTable = new Dictionary<int, GameObject>();
        Dictionary<int, SceneAstronaut> playerTable = new Dictionary<int, SceneAstronaut>();

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = WIDTH;
            graphics.PreferredBackBufferHeight = HEIGHT;
           
            graphics.ApplyChanges();
            inputTime = 0;
            
        }

       
        protected override void Initialize()
        {
            
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            textures = new Texture2D[5];

            textures[(int)Text.BACKGROUND] = Content.Load<Texture2D>("Sprites\\background");
            textures [(int)Text.ASTEROID]= Content.Load<Texture2D>("Sprites\\asteroid2");
            textures[(int)Text.ASTRONAUT] = Content.Load<Texture2D>("Sprites\\astronauta");
            textures[(int)Text.CRYSTAL] = Content.Load<Texture2D>("Sprites\\sun");
            textures[(int)Text.PORTAL] = Content.Load<Texture2D>("Sprites\\portal");
            font = Content.Load<SpriteFont>("myFont");
            scene = new Scene(WIDTH, HEIGHT);
            scene.Load("Scene/Incoming.xml");

            //Create a Rectangle that represents the full
            //drawable area of the game screen.
            viewportRect = new Rectangle(0, 0,
                graphics.GraphicsDevice.Viewport.Width,
                graphics.GraphicsDevice.Viewport.Height);


            InitPlayerObjectTables();
          
            //asteroid.Sprite = asteroidTexture;
            base.Initialize();
        }

        private void InitPlayerObjectTables()
        {
            int id = 0;
            foreach (GameObject obj in scene.Objects)
            {
                if (!(obj is SceneAstronaut))
                {
                    objectTable.Add(id, obj);
                    id++;
                }
            }

            id = 0;
            foreach (SceneAstronaut obj in scene.Astronauts)
            {
                playerTable.Add(id, obj);
                id++;
            }
        }
                
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here

        }

       
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        
       
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            if(serverNetwork != null)
                PhysicsSolver.Update((float)gameTime.ElapsedGameTime.TotalSeconds, scene.Objects.ToArray() as IPhysicBody []);

            KeyboardState keyState = Keyboard.GetState();
            if ((inputTime += (float)gameTime.ElapsedGameTime.TotalSeconds) > 0.14f)
            {
                inputTime = 0;
                if (keyState.IsKeyDown(Keys.D1))
                {
                    bool status = scene.Astronauts.ElementAt(0).selected;
                    DeselectAstronauts();
                    scene.Astronauts.ElementAt(0).selected = !status;
                }
                if (keyState.IsKeyDown(Keys.D2))
                {
                    bool status = scene.Astronauts.ElementAt(1).selected;
                    DeselectAstronauts();
                    scene.Astronauts.ElementAt(1).selected = !status;
                }
                if (keyState.IsKeyDown(Keys.D3))
                {
                    bool status = scene.Astronauts.ElementAt(2).selected;
                    DeselectAstronauts();
                    scene.Astronauts.ElementAt(2).selected = !status;
                }
                if (keyState.IsKeyDown(Keys.D0))
                {
                    DeselectAstronauts();
                }

                if (keyState.IsKeyDown(Keys.S))
                {
                    state = NetworkState.S_SENDINGDISCOVERY;
                    if (serverNetwork == null)
                    {
                        serverNetwork = new ServerNetworkManager();
                        serverNetwork.Init();
                        serverNetwork.NewPlayer += new Action(serverNetwork_NewPlayer);
                    }
                }
                else if (keyState.IsKeyDown(Keys.P))
                {
                    state = NetworkState.P_NOTCONNECTED;
                    if (playerNetwork == null)
                    {
                        playerNetwork = new PlayerNetworkManager();
                        playerNetwork.Init();
                        playerNetwork.ServerConnected += new Action(playerNetwork_ServerConnected);
                        playerNetwork.PlayerInfoReceived += new Action<PlayerInfo>(playerNetwork_PlayerInfoReceived);
                        playerNetwork.GameStateReceived += new Action<List<ObjectState>>(playerNetwork_GameStateReceived);
                    }
                }
                else if (keyState.IsKeyDown(Keys.V))
                {
                    if (serverNetwork != null && state == NetworkState.S_SENDINGDISCOVERY)
                    {
                        serverNetwork.SendPlayerInfo(players);
                        state = NetworkState.S_GAMESTARTED;

                    }
                }
                else if (keyState.IsKeyDown(Keys.N))
                {
                    if (playerNetwork != null && state == NetworkState.P_CONNECTED)
                    {
                        playerNetwork.SendPlayerConnected();
                        textSubMessage = "New Player Sent";
                    }
                }
            }
            UpdateNetwork();
            base.Update(gameTime);
        }

        void playerNetwork_GameStateReceived(List<ObjectState> arg1)
        {
            foreach (ObjectState obj in arg1)
            {
                objectTable[obj.Id].SetPosition(obj.Position);
                objectTable[obj.Id].isAlive = obj.IsAlive;
            }
        }

        void playerNetwork_PlayerInfoReceived(PlayerInfo obj)
        {
            state = NetworkState.P_GAMESTARTED;
        }

        void playerNetwork_ServerConnected()
        {
            state = NetworkState.P_CONNECTED;
        }

        void serverNetwork_NewPlayer()
        {
            textSubMessage = "New Player";
            players.Add(new PlayerInfo(players.Count, 0));
        }

        private void DeselectAstronauts()
        {
            foreach (SceneAstronaut astronaut in scene.Astronauts)
            astronaut.selected = false;
        }

        private void UpdateNetwork()
        {
            if (state == NetworkState.P_NOTCONNECTED)
            {
                playerNetwork.HandleConnectionMessages();
                textMessage = "Player Not Connected";
            }
            else if (state == NetworkState.P_CONNECTED)
            {
                playerNetwork.HandleNetworkMessages();
                textMessage = "Player Connected";
            }
            else if (state == NetworkState.P_GAMESTARTED)
            {
                playerNetwork.HandleNetworkMessages();
                textMessage = "Player Game Started";
            }
            else if (state == NetworkState.S_SENDINGDISCOVERY)
            {
                serverNetwork.HandleNetworkMessages();
                serverNetwork.SendDiscovery();
                textMessage = "Sever Sending Discovery";
            }
            else if (state == NetworkState.S_GAMESTARTED)
            {
                serverNetwork.HandleNetworkMessages();
                serverNetwork.SendGameState(GetObjectStates(), null);
                textMessage = "Sever Game Started";
            }
            else if (state == NetworkState.NONE)
            {
                textMessage = "Choose Server(S) or Player(P)";
            }
            
        }

        private List<ObjectState> GetObjectStates()
        {
            List<ObjectState> states = new List<ObjectState>();
            foreach (KeyValuePair<int, GameObject> obj in objectTable)
            {

                ObjectState state = new ObjectState(obj.Key, 0, obj.Value.isAlive, obj.Value.Position);
                states.Add(state);
                
            }
            return states;
        }


        private List<PlayerState> GetPlayerStates()
        {
            List<PlayerState> states = new List<PlayerState>();
            foreach (KeyValuePair<int, SceneAstronaut> obj in playerTable)
            {
                PlayerState state = new PlayerState(obj.Key, 0, obj.Value.isAlive, obj.Value.Position, obj.Value.Charge);
                states.Add(state);
                
            }
            return states;
        }


       
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);

            
            spriteBatch.Draw(textures[(int)Text.BACKGROUND], viewportRect,
                Color.White);

            foreach (GameObject obj in scene.Objects)
            {
                if (obj.isAlive)
                {

                    obj.Draw(textures[obj.SpriteId], spriteBatch, font);
                        
                }
            }

            spriteBatch.DrawString(font, textMessage, new Vector2(10, 10), Color.Yellow);

            spriteBatch.DrawString(font, textSubMessage, new Vector2(10, 30), Color.Yellow);
            
            spriteBatch.End();


            base.Draw(gameTime);
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            if (serverNetwork != null)
                serverNetwork.Close();
            else if (playerNetwork != null)
                playerNetwork.Close();
            base.OnExiting(sender, args);
        }
    }
}
