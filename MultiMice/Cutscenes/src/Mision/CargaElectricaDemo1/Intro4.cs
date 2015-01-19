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

    public class Intro4 : AbstractGame
    {
        // Things used by the game
        public AnimationController[] animationController;
        public GraphicsDeviceManager graphics;
        public SpriteFont fontMenu;        

        private List<IGameObject> objectList;
        
        private int numPlayer;
        public int[] level;
        
        // Objects Game
        Objeto3D nave;
        Objeto3D mapa;
        Objeto3D tierra;     
        Objeto2D fondoEstrellas;
        
        // Sistemas de Particulas
        ParticleSystem explosionParticles;
        ParticleSystem explosionParticles2;
        ParticleSystem explosionSmokeParticles;
    
        
        KeyboardState lastKeyboardState;
        KeyboardState currentKeyboardState;

        Random rand = new Random();

        
        
       
        // Game constructor
        public Intro4()
        {            
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1270;
            graphics.PreferredBackBufferHeight = 720;
            graphics.IsFullScreen = false;
            Content.RootDirectory = "Content";

            // Cargar Particulas
            explosionParticles = new FireParticleSystemBlue(this, Content);
            explosionParticles2 = new FireParticleSystemBlue(this, Content);
            explosionSmokeParticles = new ExplosionSmokeParticleSystem(this, Content);      

            // Set the draw order so the explosions and fire
            // will appear over the top of the smoke.
            explosionSmokeParticles.DrawOrder =1;        
            explosionParticles.DrawOrder = 2;
            explosionParticles2.DrawOrder = 2;
           
            // Register the particle system components.
            Components.Add(explosionParticles);
            Components.Add(explosionParticles2);
            Components.Add(explosionSmokeParticles);

            this.Window.Title = "Mission";

        }

        // Initialize important things
        protected override void Initialize()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Camara = new CamaraAjustable(graphics);
            objectList = new List<IGameObject>(); 
            base.Initialize();            
        }



        // Initialize Objects and add them to a draw/Update list in order of draw.
        protected override void LoadContent()
        {

            //Cargar fondo
            fondoEstrellas = new Objeto2D(Content.Load<Texture2D>("Sprites//Textures//EstrellasPlaneta"),
                new Vector2(Window.ClientBounds.Width / 2, Window.ClientBounds.Height / 2), 1);
            objectList.Add(fondoEstrellas);

            tierra = new Objeto3D(Content.Load<Model>("Sprites//Models//Planet"), new Vector3(1000, -4000, -7000), new Vector3(1, 1f, 1) * 20f,
                new Vector3(0, 0, 0));
            tierra.enableDirectionalLight = true;
            tierra.enableLight = true;
            tierra.lightDirection = new Vector3(-10, 1, 1);
            tierra.lightDirection.Normalize();
            objectList.Add(tierra);

            Camara.cameraLookAt = tierra.position;
            Camara.cameraPosition =  new Vector3(0.0f, -2000.0f, -2000.0f);

            Vector3 v = tierra.position + new Vector3(0,1000,11000);
            for (int i = 0; i < 30; i++)
            {
                Objeto3D a = new Objeto3D(Content.Load<Model>("Sprites//Models//TiberiumRojo"), v +
                    new Vector3((float)((rand.NextDouble() - 0.5) * 6000 - 1000), (float)((rand.NextDouble() - 0.5) * 2000), 
                        (float)((rand.NextDouble() - 0.5) * 100)),
                    new Vector3(1, 1, 1) * 0.001f,
                    new Vector3(0, 0, 0));
                a.enableDirectionalLight = true;
                a.enableLight = true;
                a.lightDirection = new Vector3(-10, 1, 1);
                a.lightDirection.Normalize();
                a.ambient = new Vector3(0.7f, 0.7f, 0.7f);
                objectList.Add(a);
            }

            for (int i = 0; i < 30; i++)
            {
                Objeto3D a = new Objeto3D(Content.Load<Model>("Sprites//Models//TiberiumAzul"), v +
                    new Vector3((float)((rand.NextDouble() - 0.5) * 6000 - 1000), (float)((rand.NextDouble() - 0.5) * 2000), 0),
                    new Vector3(1, 1, 1) * 0.001f,
                    new Vector3(0, 0, 0));
                a.enableDirectionalLight = true;
                a.enableLight = true;
                a.lightDirection = new Vector3(-10, 1, 1);
                a.lightDirection.Normalize();
                a.ambient = new Vector3(0.7f, 0.7f, 0.7f);
                objectList.Add(a);
            }

            v = tierra.position + new Vector3(0, 1000, 7000);

            for (int i = 0; i < 30; i++)
            {
                Objeto3D a = new Objeto3D(Content.Load<Model>("Sprites//Models//asteroid"), v +
                    new Vector3((float)((rand.NextDouble() - 0.5) * 3000 - 1000), (float)((rand.NextDouble() - 0.5) * 1000), 0),
                    new Vector3(1, 1, 1) * 0.1f,
                    new Vector3(0, 0, 0));
                a.enableDirectionalLight = true;
                a.enableLight = true;
                a.lightDirection = new Vector3(-10, 1, 1);
                a.lightDirection.Normalize();
                a.ambient = new Vector3(0.7f, 0.7f, 0.7f);
                objectList.Add(a);
            }

            //for(int i=0; i<30; i++)
            //{
            //    Asteroide a = new Asteroide(Content.Load<Model>("Sprites//Models//TiberiumRojo"), v + 
            //        new Vector3((float)((rand.NextDouble()-0.5)*100),(float)((rand.NextDouble()-0.5)*100),0), 
            //        new Vector3(1, 1, 1) * 0.0001f,
            //        new Vector3(0, 0, 0), i);
            //    objectList.Add(a);
            //}

            //v = tierra.position += new Vector3(3000);

            //for (int i = 0; i < 30; i++)
            //{
            //    Asteroide a = new Asteroide(Content.Load<Model>("Sprites//Models//asteroid"), v +
            //        new Vector3((float)((rand.NextDouble() - 0.5) * 100), (float)((rand.NextDouble() - 0.5) * 100), 0),
            //        new Vector3(1, 1, 1) * 0.0001f,
            //        new Vector3(0, 0, 0), i);
            //    objectList.Add(a);
            //}  
            
          
        }

        
        protected override void UnloadContent()
        {
            Content.Dispose();
        }

        // Update
        protected override void Update(GameTime gameTime)
        {

            lastKeyboardState = currentKeyboardState;
            
            currentKeyboardState = Keyboard.GetState();

            
            if ((currentKeyboardState.IsKeyDown(Keys.A) &&
                 lastKeyboardState.IsKeyUp(Keys.A)))
            {
                
            }
            if ((currentKeyboardState.IsKeyDown(Keys.S)))
            {
                Camara.cameraPosition += new Vector3(0, 0, 20);
                fondoEstrellas.scale /= 1.0005f;
            }
            if ((currentKeyboardState.IsKeyDown(Keys.W)))
            {
                Camara.cameraPosition += new Vector3(0, 0, -20);
                fondoEstrellas.scale *= 1.0005f;
            }
            

           
            Camara.cameraViewMatrix = Matrix.CreateLookAt(Camara.cameraPosition, Camara.cameraLookAt, Vector3.Up);

            explosionParticles.SetCamera(Camara.cameraViewMatrix, Camara.cameraProjectionMatrix);
            explosionParticles2.SetCamera(Camara.cameraViewMatrix, Camara.cameraProjectionMatrix);
            explosionSmokeParticles.SetCamera(Camara.cameraViewMatrix, Camara.cameraProjectionMatrix);
            

          
            
            base.Update(gameTime);
        }

  
        // Draw
        protected override void Draw(GameTime gameTime)
        {
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
            
                                       
            base.Draw(gameTime);
        }


    }
}








            
     

  

