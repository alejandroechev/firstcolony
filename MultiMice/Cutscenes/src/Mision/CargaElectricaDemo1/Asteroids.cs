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
using RawInputSharp;
using XNAnimation;
using XNAnimation.Controllers;
using XNAnimation.Effects;
using System.Threading;

// TO DO: Avoid Spanglish!

namespace CargaElectricaDemo1
{

    public class Asteroids : AbstractGame
    {
        // Things used by the game
        public GraphicsDeviceManager graphics;
        public SpriteFont fontMenu;        

        public Plane planoJuego;
        List<IGameObject> objectList;
        
        private int numPlayer;
        public int[] level;
        
        // Objects Game
        Astronauta[] astroPlayers;
        Objeto3D[] masMenos;
        Objeto3D nave;
        Objeto3D mapa;             
        Objeto2D fondoEstrellas;
        Objeto2D[] reminders;
        Objeto2D texto;

        List<Objeto3D> cristales;

        List<Objeto3D> asteroidesReales = new List<Objeto3D>();
        
        List<Objeto3D> campos = new List<Objeto3D>();
       
        // Sistemas de Particulas
        ParticleSystem explosionParticles;
        ParticleSystem explosionSmokeParticles;
    
        // Controlador de Pantallas
        GameState state;
        enum GameState {GS_START, GS_MENU,GS_PAUSE}

        Random rand = new Random();

        Objeto2D blurSprite;
        ResolveTexture2D renderTargetTexture;

        bool motionBlur = false;

        List<string> texts = new List<string>();
        List<int> lengths = new List<int>();
        int currentText = 0;
       
        // Game constructor
        public Asteroids()
        {            
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1270;
            graphics.PreferredBackBufferHeight = 720;
            graphics.IsFullScreen = false;
            Content.RootDirectory = "Content";

            // Cargar Particulas
            explosionParticles = new ExplosionParticleSystem(this, Content);
            explosionSmokeParticles = new ExplosionSmokeParticleSystem(this, Content);      

            // Set the draw order so the explosions and fire
            // will appear over the top of the smoke.
            explosionSmokeParticles.DrawOrder = 0;        
            explosionParticles.DrawOrder = 2;
           
            // Register the particle system components.
            Components.Add(explosionParticles);
            Components.Add(explosionSmokeParticles);         

            state = GameState.GS_MENU;
            cristales = new List<Objeto3D>();

            texts.Add("");
            lengths.Add(texts[0].Length);
            texts.Add("Alerta! Alerta! Se aproxima una lluvia de asteroides. \n" +
             "Deben recolectar los últimos cristales antes de que sean destruidos!");
            lengths.Add(texts[1].Length / 2);
            texts.Add("");
            lengths.Add(texts[2].Length);
            

        }

        // Initialize important things
        protected override void Initialize()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Camara = new CamaraAjustable(graphics);
            objectList = new List<IGameObject>(); 
            base.Initialize();            
        }

        Vector3 baseCristal = new Vector3(0, 170, 2100);


        // Initialize Objects and add them to a draw/Update list in order of draw.
        protected override void LoadContent()
        {
            
            //Cargar fondo
            fondoEstrellas = new Objeto2D(Content.Load<Texture2D>("Sprites//Textures//Estrellas"),
                new Vector2(Window.ClientBounds.Width / 2, Window.ClientBounds.Height / 2), 1 );
            objectList.Add(fondoEstrellas);

            //Cargar Mapa
            mapa = new Objeto3D(Content.Load<Model>("Sprites//Models//mapa"), new Vector3(0, 0, -450), new Vector3(1f, 0.8f, 1f) * 4, 
                new Vector3(-MathHelper.Pi, -MathHelper.Pi / 2, 0));
            //objectList.Add(mapa);
                
            //Cargar Nave
            nave = new Objeto3D(Content.Load<Model>("Sprites//Models//UnaNave"), new Vector3(0, 170, -1200), new Vector3(1, 1f, 1) * 5f,
                
                new Vector3(0, -MathHelper.Pi / 2, MathHelper.Pi / 2));
            nave.isAlive = true;
            //objectList.Add(nave);

            
            for (int i = 0; i < 30; i++)
            {
                Vector3 v = baseCristal + new Vector3(rand.Next(0, 200) - 100, rand.Next(0, 200) - 100, -rand.Next(0, 150));
                Objeto3D campo = new Objeto3D(Content.Load<Model>("Sprites//Models//campoRojo"), v, new Vector3(1, 1, 1) / 1.5f, Vector3.Zero);
                campo.boneTransforms = false;
                Objeto3D a = new Objeto3D(Content.Load<Model>("Sprites//Models//TiberiumRojo"), v, new Vector3(1, 1, 1) * 0.0001f, 
                    new Vector3(0, 0, 0));
                a.boneTransforms = false;
                cristales.Add(a);
                objectList.Add(a);
            }

            for (int i = 0; i < 30; i++)
            {
                Vector3 v = baseCristal + new Vector3(rand.Next(0, 200) - 100, rand.Next(0, 200) - 100, -rand.Next(0, 150));
                Objeto3D campo = new Objeto3D(Content.Load<Model>("Sprites//Models//campoAzul"), v, new Vector3(1, 1, 1) / 1.5f, Vector3.Zero);
                campo.boneTransforms = false;
                Objeto3D a = new Objeto3D(Content.Load<Model>("Sprites//Models//TiberiumAzul"), v, new Vector3(1, 1, 1) * 0.0001f, 
                    new Vector3(0, 0, 0));
                a.boneTransforms = false;
                cristales.Add(a);
                objectList.Add(a);
            }

            for (int i = 0; i < 200; i++)
            {
                Vector3 v = nave.position + new Vector3(rand.Next(0, 400) - 200, rand.Next(0, 100) - 50, -rand.Next(250, 4750));
                Objeto3D a = new Objeto3D(Content.Load<Model>("Sprites//Models//asteroid"), v, new Vector3(1, 1, 1) * 0.01f * rand.Next(1, 4),
                    new Vector3(0, 0, 0));
                a.boneTransforms = false;
                asteroidesReales.Add(a);
                objectList.Add(a);
            }

            renderTargetTexture = new ResolveTexture2D(
               graphics.GraphicsDevice,
               graphics.GraphicsDevice.PresentationParameters.BackBufferWidth,
               graphics.GraphicsDevice.PresentationParameters.BackBufferHeight,
               1,
               graphics.GraphicsDevice.PresentationParameters.BackBufferFormat);

            blurSprite = new Objeto2D(renderTargetTexture, new Vector2(Window.ClientBounds.Width / 2, Window.ClientBounds.Height / 2), 1);


            fontMenu = Content.Load<SpriteFont>("Fonts//SpriteFont1");
          
        }


        protected override void UnloadContent()
        {
            Content.Dispose();
        }

        KeyboardState lastKeyboardState;
        KeyboardState currentKeyboardState;

        private float speed = 3;
        // Update
        protected override void Update(GameTime gameTime)
        {
            lastKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            
                    //else 
            if ((currentKeyboardState.IsKeyDown(Keys.Space) &&
                  lastKeyboardState.IsKeyUp(Keys.Space)))
                    {
                        Camara.cameraLookAt = new Vector3(0.0f, 0.0f, 1500.0f);
                        Camara.cameraPosition = new Vector3(400.0f, 520.0f, 2500.0f);
                        speed = 0.2f;
                        fondoEstrellas.rotation = (float)Math.PI;
                       
                    }

            
                    if ((currentKeyboardState.IsKeyDown(Keys.Enter) &&
                         lastKeyboardState.IsKeyUp(Keys.Enter)))
                    {
                        currentText++;
                    }




                    //nave.position += new Vector3(0, 0, +3f);
                    foreach (Objeto3D a in asteroidesReales)
                    {
                        a.position += new Vector3(0, 0, speed);
                    }


                    Camara.cameraViewMatrix = Matrix.CreateLookAt(Camara.cameraPosition, Camara.cameraLookAt, Vector3.Up);
                    
                  
              
            
            base.Update(gameTime);
        }

  
        // Draw
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.ResolveBackBuffer(renderTargetTexture);
            GraphicsDevice.Clear(Color.Black);

            // Draw all 2d & 3d objects
            foreach (IGameObject s in objectList)
            {
                if (s.isAlive)
                {
                    if (s is Objeto2D)
                    {
                        spriteBatch.Begin(SpriteBlendMode.AlphaBlend);
                        s.Draw(this);
                        spriteBatch.End();
                    }
                    else
                    {
                        graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
                        s.Draw(this);

                    }
                }
            }

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            spriteBatch.DrawString(fontMenu, texts[currentText], new Vector2(Window.ClientBounds.Width / 2 - lengths[currentText] * 5,
                9 * Window.ClientBounds.Height / 10), Color.White);
            spriteBatch.End();

            if (motionBlur)
            {
                spriteBatch.Begin(SpriteBlendMode.AlphaBlend);
                blurSprite.Draw(this, new Color(255, 255, 255, 200));
                spriteBatch.End();
            }
            base.Draw(gameTime);
        }


    }
}








            
     

  

