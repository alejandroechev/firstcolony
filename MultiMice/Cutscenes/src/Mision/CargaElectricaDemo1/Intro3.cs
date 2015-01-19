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

    public class Intro3 : AbstractGame
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
        
       
        // Game constructor
        public Intro3()
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

            //Camara.cameraPosition = new Vector3(1000, -2000, 700);
            Camara.cameraLookAt = tierra.position + new Vector3(0,700,0);
          
        }

       


        protected override void UnloadContent()
        {
            Content.Dispose();
        }

        bool startDark = false;

        // Update
        protected override void Update(GameTime gameTime)
        {

            lastKeyboardState = currentKeyboardState;
            
            currentKeyboardState = Keyboard.GetState();

            if (tierra.diffuse.Length() >= 0.1f && startDark)
            {
                tierra.diffuse *= 0.998f;
                tierra.ambient *= 0.998f;

            }
            if ((currentKeyboardState.IsKeyDown(Keys.W)))
            {
                Vector3 v = Camara.cameraLookAt - Camara.cameraPosition;
                v.Normalize();
                Camara.cameraPosition += 10*v;
                fondoEstrellas.scale *= 1.0005f;
            }
            if ((currentKeyboardState.IsKeyDown(Keys.S)))
            {
                Camara.cameraPosition += new Vector3(0, 0, 10);
                fondoEstrellas.scale /= 1.0005f;
            }
            if ((currentKeyboardState.IsKeyDown(Keys.A)))
            {
                Vector3 dir = Camara.cameraLookAt - Camara.cameraPosition;
                float tar_posDist = dir.Length();
                dir.Normalize();
                Vector3 right = -Vector3.Cross(dir, new Vector3(0, 1, 0));
                Vector3 tempVec;
                float turnSpeed = 0.001f;
                tempVec = dir - right * turnSpeed;
                tempVec.Normalize();
                Camara.cameraPosition = Camara.cameraLookAt - tempVec * tar_posDist;
                fondoEstrellas.position -= new Vector2(0.5f,0);
            }
            if ((currentKeyboardState.IsKeyDown(Keys.D)))
            {
                Vector3 dir = Camara.cameraLookAt - Camara.cameraPosition;
                float tar_posDist = dir.Length();
                dir.Normalize();
                Vector3 right = Vector3.Cross(dir, new Vector3(0, 1, 0));
                Vector3 tempVec;
                float turnSpeed = 0.001f;
                tempVec = dir - right * turnSpeed;
                tempVec.Normalize();
                Camara.cameraPosition = Camara.cameraLookAt - tempVec * tar_posDist;
                fondoEstrellas.position -= new Vector2(0.5f, 0);
            }
            if ((currentKeyboardState.IsKeyDown(Keys.Q)))
            {
                startDark = true;
            }
            if ((currentKeyboardState.IsKeyDown(Keys.Down)))
            {
                Camara.cameraPosition += new Vector3(0, -10, 0);
            }
            if ((currentKeyboardState.IsKeyDown(Keys.Up)))
            {
                Camara.cameraPosition += new Vector3(0, 10, 0);
            }
            if ((currentKeyboardState.IsKeyDown(Keys.Right)))
            {
                Camara.cameraPosition += new Vector3(10, 0, 0);
            }
            if ((currentKeyboardState.IsKeyDown(Keys.Left)))
            {
                Camara.cameraPosition += new Vector3(-10, 0, 0);
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








            
     

  

