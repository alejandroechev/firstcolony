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
using SplineLib;

// TO DO: Avoid Spanglish!

namespace CargaElectricaDemo1
{

    public class Intro1 : AbstractGame
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
        Objeto3D sol;
        Objeto2D fondoEstrellas;
        
        // Sistemas de Particulas
        ParticleSystem explosionParticles;
        ParticleSystem explosionParticles2;
        ParticleSystem explosionSmokeParticles;
    
        
        KeyboardState lastKeyboardState;
        KeyboardState currentKeyboardState;

        Spline spline;
        SplineInfo info;
        List<MathLib.Vector> interpolated;

        int splineIndex = 0;

        Objeto2D blurSprite;
        ResolveTexture2D renderTargetTexture;
        bool motionBlur = false;

        Vector3 fixedDir = new Vector3();

        List<string> texts = new List<string>();
        List<int> lengths = new List<int>();
        int currentText = 0;
       
        // Game constructor
        public Intro1()
        {            
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1270;
            graphics.PreferredBackBufferHeight = 720;
            graphics.PreferMultiSampling = true;
            graphics.PreferredBackBufferFormat = SurfaceFormat.Bgr32;
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

            texts.Add("");
            lengths.Add(texts[0].Length);
            texts.Add("A finales del Siglo 21, el planeta Tierra estaba en crisis.");
            lengths.Add(texts[1].Length);
            texts.Add("La población mundial había superado la barrera de 12 mil millones de personas, \n" +
             "y los recursos disponibles para abastecerlas se hacían cada vez más escasos.");
            lengths.Add(texts[2].Length/2);
            //texts.Add("y los recursos disponibles para abastecerlas se hacían cada vez más escasos.");
            texts.Add("En un esfuerzo conjunto entre todas las naciones, se organizó la misión 'Primera Colonia', \n" +
               "su objetivo: enviar una nave interestelar a colonizar el primer planeta habitable detectado");
            lengths.Add(texts[3].Length/2);
            //texts.Add("su objetivo: enviar una nave interestelar a colonizar el primer planeta habitable detectado");
            texts.Add("Todas las esperanzas para solucionar la crisis se pusieron en esta misión.");
            lengths.Add(texts[4].Length);
            texts.Add("Su éxito abriría las puertas a futuras misiones hacia otras estrellas y planetas, \n" +
                "y permitiría finalmente lograr expandir a la humanidad fuera de los confines de la Tierra.");
            lengths.Add(texts[5].Length/2);
            //texts.Add("y permitiría finalmente lograr expandir a la humanidad fuera de los confines de la Tierra.");
            texts.Add("");
            lengths.Add(texts[6].Length);
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
            info = new SplineInfo("path1.sp");
            info.Load();

            spline = new CatmullRom();
            spline.AddPoints(info.Points);
            interpolated = spline.Interpolate(0.009f);
            splineIndex = interpolated.Count - 1;

            fontMenu = Content.Load<SpriteFont>("Fonts//SpriteFont1");
            
            //Cargar fondo
            fondoEstrellas = new Objeto2D(Content.Load<Texture2D>("Sprites//Textures//EstrellasTierra"), 
                new Vector2(4*Window.ClientBounds.Width/5, Window.ClientBounds.Height/2), 1.3f);
            objectList.Add(fondoEstrellas);
                

            tierra = new Objeto3D(Content.Load<Model>("Sprites//Models//earth"), new Vector3(-900, -4500, -7000), 
                new Vector3(1, 1f, 1) * 80f,
                new Vector3(0, 0, 0));
            tierra.enableDirectionalLight = true;
            tierra.enableLight = true;
            tierra.lightDirection = new Vector3(10, 1, 1);
            tierra.lightDirection.Normalize();
            tierra.ambient = new Vector3(0.9f);
            objectList.Add(tierra);

            //Cargar Nave-4100 Y:-3700 Z:-7000
            nave = new Objeto3D(Content.Load<Model>("Sprites//Models//UnaNave"), new Vector3(-1810, -3700, -6500), 
                new Vector3(1, 1f, 1) *2,
                new Vector3(8.552112f, 0, 0));
            nave.enableLight = true;
            nave.enableDirectionalLight = true;
            nave.ambient = new Vector3(0.8f, 0.8f, 0.8f);
            nave.lightDirection = new Vector3(10, 1, 1);
            nave.lightDirection.Normalize();
            objectList.Add(nave);

            Camara.cameraLookAt = nave.position;//tierra.position + new Vector3(0,850,0);
            Camara.cameraPosition = new Vector3(12212, tierra.position.Y, -5039);

            //sol = new Objeto3D(Content.Load<Model>("Sprites//Models//redsun"), new Vector3(600, 0, -500), new Vector3(1, 1f, 1) * 1f,
            //    new Vector3(0, 0, 0));
            //sol.enableLight = true;
            ////sol.diffuse = new Vector3(0.2f, 0 , 0);
            //sol.emmisive = new Vector3(0.5f, 0, 0);
            //sol.diffuse = new Vector3(0.5f, 0.5f, 0.5f); 

            //objectList.Add(sol);

           
            renderTargetTexture = new ResolveTexture2D(
                graphics.GraphicsDevice,
                graphics.GraphicsDevice.PresentationParameters.BackBufferWidth,
                graphics.GraphicsDevice.PresentationParameters.BackBufferHeight,
                1,
                graphics.GraphicsDevice.PresentationParameters.BackBufferFormat);

            blurSprite = new Objeto2D(renderTargetTexture, new Vector2(Window.ClientBounds.Width / 2, Window.ClientBounds.Height / 2), 1);
            
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

            
            if ((currentKeyboardState.IsKeyDown(Keys.Space) &&
                 lastKeyboardState.IsKeyUp(Keys.Space)))
            {
                Console.WriteLine(Camara.cameraPosition.ToString());
                
                fixedDir = nave.position - Camara.cameraPosition;
                fixedDir.Normalize();

                motionBlur = true;
            }
            if ((currentKeyboardState.IsKeyDown(Keys.Enter) &&
                 lastKeyboardState.IsKeyUp(Keys.Enter)))
            {
                currentText++;
            }
            
            //Camera position
            if ((currentKeyboardState.IsKeyDown(Keys.W)))
            {
                Vector3 v = Camara.cameraLookAt - Camara.cameraPosition;
                v.Normalize();
                Camara.cameraPosition += 15 * v;
                fondoEstrellas.scale *= 1.0002f;

            }
            else if ((currentKeyboardState.IsKeyDown(Keys.S)))
            {
                Vector3 v = Camara.cameraLookAt - Camara.cameraPosition;
                v.Normalize();
                Camara.cameraPosition -= 20 * v;
                fondoEstrellas.scale /= 1.0002f;
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
                fondoEstrellas.position -= new Vector2(0.5f, 0);

                Vector3 v = Camara.cameraLookAt - Camara.cameraPosition;
                v.Normalize();
                Camara.cameraPosition +=  v;
                fondoEstrellas.scale *= 1.00002f;
            }
            else if ((currentKeyboardState.IsKeyDown(Keys.D)))
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
                fondoEstrellas.position += new Vector2(0.5f, 0);
            }

            if ((currentKeyboardState.IsKeyDown(Keys.I)))
            {
                nave.position += new Vector3(0, 10, 0);
            }
            else if ((currentKeyboardState.IsKeyDown(Keys.K)))
            {
                nave.position += new Vector3(0, -10, 0);
            }
            if ((currentKeyboardState.IsKeyDown(Keys.J)))
            {
                nave.position += new Vector3(-10, 0, 0);
            }
            else if ((currentKeyboardState.IsKeyDown(Keys.L)))
            {
                nave.position += new Vector3(10, 0, 0);
            }

            //Camera lookat
            if ((currentKeyboardState.IsKeyDown(Keys.Down)))
            {
                Vector3 right = -Vector3.Cross(fixedDir, new Vector3(0, 1, 0));
                Vector3 spaceDir = 5 * fixedDir + right;
                spaceDir.Normalize();
                nave.position += 10 * spaceDir;
                Camara.cameraLookAt = nave.position;
            }
            else if ((currentKeyboardState.IsKeyDown(Keys.Up)))
            {
                Vector3 right = -Vector3.Cross(fixedDir, new Vector3(0, 1, 0));
                Vector3 spaceDir = 5 * fixedDir + right;
                spaceDir.Normalize();
                nave.position -= 20 * spaceDir;
                tierra.position -= 10 * spaceDir;
                Camara.cameraLookAt = nave.position;
                fondoEstrellas.position -= new Vector2(0.5f, 0);
                (explosionParticles as FireParticleSystemBlue).SetGravity(spaceDir);
                explosionParticles.AddParticle(nave.position + 60 * right, Vector3.Zero);
                (explosionParticles2 as FireParticleSystemBlue).SetGravity(spaceDir);
                explosionParticles2.AddParticle(nave.position - 60 * right, Vector3.Zero);
            }

            if ((currentKeyboardState.IsKeyDown(Keys.Right)))
            {
                Vector3 dir = Camara.cameraLookAt - Camara.cameraPosition;
                float tar_posDist = dir.Length();
                dir.Normalize();
                Vector3 right = -Vector3.Cross(dir, new Vector3(0, 1, 0));
                Vector3 tempVec;
                float turnSpeed = 0.01f;
                tempVec = dir - right * turnSpeed;
                tempVec.Normalize();
                Camara.cameraLookAt = Camara.cameraPosition + tempVec * tar_posDist;
                fondoEstrellas.position += new Vector2(0.5f, 0);
            }
            else if ((currentKeyboardState.IsKeyDown(Keys.Left)))
            {
                Vector3 dir = Camara.cameraLookAt - Camara.cameraPosition;
                float tar_posDist = dir.Length();
                dir.Normalize();
                Vector3 right = Vector3.Cross(dir, new Vector3(0, 1, 0));
                Vector3 tempVec;
                float turnSpeed = 0.01f;
                tempVec = dir - right * turnSpeed;
                tempVec.Normalize();
                Camara.cameraLookAt = Camara.cameraPosition + tempVec * tar_posDist;
                fondoEstrellas.position -= new Vector2(0.5f, 0);
            }

            //splineIndex--;
            //if (splineIndex >= 0)// interpolated.Count)
            //{
            //    nave.position = new Vector3(interpolated[splineIndex].x / 10, interpolated[splineIndex].y / 10,
            //        interpolated[splineIndex].z / 10);
            //    explosionParticles.AddParticle(nave.position + new Vector3(-30, 0, -50), Vector3.Zero);
            //    explosionParticles2.AddParticle(nave.position + new Vector3(30, 0, -50), Vector3.Zero);
            //}

           
            Camara.cameraViewMatrix = Matrix.CreateLookAt(Camara.cameraPosition, Camara.cameraLookAt, Vector3.Up);

            explosionParticles.SetCamera(Camara.cameraViewMatrix, Camara.cameraProjectionMatrix);
            explosionParticles2.SetCamera(Camara.cameraViewMatrix, Camara.cameraProjectionMatrix);
            //explosionSmokeParticles.SetCamera(Camara.cameraViewMatrix, Camara.cameraProjectionMatrix);
            

          
            
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
            spriteBatch.DrawString(fontMenu, texts[currentText], new Vector2(Window.ClientBounds.Width / 2 - lengths[currentText]*5,
                9 * Window.ClientBounds.Height / 10), Color.White);
            spriteBatch.End();

            if (motionBlur)
            {
                spriteBatch.Begin(SpriteBlendMode.AlphaBlend);
                blurSprite.Draw(this, new Color(255, 255, 255, 200));
                spriteBatch.End();
            }
            
            //renderTargetTexture.GenerateMipMaps(TextureFilter.Linear);
                                       
            base.Draw(gameTime);
        }


    }
}








            
     

  

