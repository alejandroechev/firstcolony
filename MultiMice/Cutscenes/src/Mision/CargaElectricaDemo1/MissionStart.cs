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

    public class MissionStart : AbstractGame
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
        Objeto2D fondoEstrellas;
        
        // Sistemas de Particulas
        ParticleSystem explosionParticles;
        ParticleSystem explosionParticles2;
        ParticleSystem explosionSmokeParticles;
    
        
        KeyboardState lastKeyboardState;
        KeyboardState currentKeyboardState;

        Objeto2D blurSprite;
        ResolveTexture2D renderTargetTexture;

        bool motionBlur = false;

        List<string> texts = new List<string>();
        List<int> lengths = new List<int>();
        int currentText = 0;
        
       
        // Game constructor
        public MissionStart()
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

            texts.Add("");
            lengths.Add(texts[0].Length);
            texts.Add("Felicitaciones reclutas, han completado su entrenamiento! \n" +
             "El momento de la verdad ha llegado, preparense para el despegue.");
            lengths.Add(texts[1].Length / 2);
            //texts.Add("y los recursos disponibles para abastecerlas se hacían cada vez más escasos.");
            texts.Add("");
            lengths.Add(texts[2].Length);

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
            fontMenu = Content.Load<SpriteFont>("Fonts//SpriteFont1");

            //Cargar fondo
            fondoEstrellas = new Objeto2D(Content.Load<Texture2D>("Sprites//Textures//Estrellas"), new Vector2(635, 50), 1 / 1.3f);
            objectList.Add(fondoEstrellas);

            //Cargar Mapa
            mapa = new Objeto3D(Content.Load<Model>("Sprites//Models//mapa"), new Vector3(0, 0, -450), new Vector3(1f, 0.8f, 1f) * 4, 
                new Vector3(-MathHelper.Pi, -MathHelper.Pi / 2, 0));
            mapa.boneTransforms = false;
            objectList.Add(mapa);
                
            //Cargar Nave
            nave = new Objeto3D(Content.Load<Model>("Sprites//Models//UnaNave"), new Vector3(0, 70, 200), new Vector3(1, 1f, 1) * 2f,
                new Vector3(-(float)Math.PI / 2, 0, 0));
            nave.isAlive = true;
            objectList.Add(nave);

            //Effect cartoonEffect = Content.Load<Effect>("CartoonEffect");            //foreach (IGameObject obj in objectList)
            ////{
            ////    if(obj is Objeto3D)
            ////        ChangeEffectUsedByModel(((Objeto3D)obj).model, cartoonEffect);
            ////}

            renderTargetTexture = new ResolveTexture2D(
                graphics.GraphicsDevice,
                graphics.GraphicsDevice.PresentationParameters.BackBufferWidth,
                graphics.GraphicsDevice.PresentationParameters.BackBufferHeight,
                1,
                graphics.GraphicsDevice.PresentationParameters.BackBufferFormat);

            blurSprite = new Objeto2D(renderTargetTexture, new Vector2(Window.ClientBounds.Width / 2, Window.ClientBounds.Height / 2), 1);

          
        }

        static void ChangeEffectUsedByModel(Model model, Effect replacementEffect)
        {
            // Table mapping the original effects to our replacement versions.
            Dictionary<Effect, Effect> effectMapping = new Dictionary<Effect, Effect>();

            foreach (ModelMesh mesh in model.Meshes)
            {
                // Scan over all the effects currently on the mesh.
                foreach (BasicEffect oldEffect in mesh.Effects)
                {
                    // If we haven't already seen this effect...
                    if (!effectMapping.ContainsKey(oldEffect))
                    {
                        // Make a clone of our replacement effect. We can't just use
                        // it directly, because the same effect might need to be
                        // applied several times to different parts of the model using
                        // a different texture each time, so we need a fresh copy each
                        // time we want to set a different texture into it.
                        Effect newEffect = replacementEffect.Clone(
                                                    replacementEffect.GraphicsDevice);

                        // Copy across the texture from the original effect.
                        newEffect.Parameters["Texture"].SetValue(oldEffect.Texture);

                        newEffect.Parameters["TextureEnabled"].SetValue(
                                                            oldEffect.TextureEnabled);

                        effectMapping.Add(oldEffect, newEffect);
                    }
                }

                // Now that we've found all the effects in use on this mesh,
                // update it to use our new replacement versions.
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = effectMapping[meshPart.Effect];
                }
            }
        }


        protected override void UnloadContent()
        {
            Content.Dispose();
        }

        Vector3 previousPosition;
        
        // Update
        protected override void Update(GameTime gameTime)
        {

            lastKeyboardState = currentKeyboardState;
            
            currentKeyboardState = Keyboard.GetState();

            if ((currentKeyboardState.IsKeyDown(Keys.A) &&
                 lastKeyboardState.IsKeyUp(Keys.A)))
                explosionParticles.AddParticle(nave.position + new Vector3(-10,0,120), Vector3.Zero);

            if ((currentKeyboardState.IsKeyDown(Keys.Enter) &&
                 lastKeyboardState.IsKeyUp(Keys.Enter)))
            {
                currentText++;
            }

            if (Camara.cameraPosition.Y > 260 )
            {
                Camara.cameraPosition.Y -= 0.1f;
                Camara.cameraPosition.Z -= 0.3f;
                Camara.cameraLookAt.Z = Camara.cameraPosition.Z - 700;                
            }

            if (Camara.cameraPosition.Y < 300 && Camara.cameraPosition.Y > 261)
            {
                nave.position += new Vector3(0, 0.1f, 0);
                previousPosition = Camara.cameraPosition;
                
            }
            else if (Camara.cameraPosition.Y < 261 && nave.position.Y < 200)
            {
                nave.position += new Vector3(0, 0.5f, 0);
                explosionParticles.AddParticle(nave.position + new Vector3(-30, 0, 120), Vector3.Zero);
                explosionParticles2.AddParticle(nave.position + new Vector3(30, 0, 120), Vector3.Zero);

                Camara.cameraPosition.X = Camara.RandomPointOnCircle(previousPosition).X;
                Camara.cameraPosition.Y = Camara.RandomPointOnCircle(previousPosition).Y;
                
            }
            else if (Camara.cameraPosition.Y < 261 && nave.position.Y >= 200)
            {
                motionBlur = true;
                nave.position += new Vector3(-0.5f, 0, -5f);
                explosionParticles.AddParticle(nave.position + new Vector3(-30, 0, 120), Vector3.Zero);
                explosionParticles2.AddParticle(nave.position + new Vector3(30, 0, 120), Vector3.Zero);

                //Camara.cameraPosition.X = Camara.RandomPointOnCircle(previousPosition).X;
                //Camara.cameraPosition.Y = Camara.RandomPointOnCircle(previousPosition).Y;
                //fondoEstrellas.position.X = Camara.RandomPointOnCircle(previousPosition).X + 635;
                //fondoEstrellas.position.Y = Camara.RandomPointOnCircle(previousPosition).Y + 50;
                
            }
            /*else
                explosionSmokeParticles.AddParticle(new Vector3(0, 10, -400), Vector3.Zero);
                
          */
            Camara.cameraViewMatrix = Matrix.CreateLookAt(Camara.cameraPosition, Camara.cameraLookAt, Vector3.Up);

            explosionParticles.SetCamera(Camara.cameraViewMatrix, Camara.cameraProjectionMatrix);
            explosionParticles2.SetCamera(Camara.cameraViewMatrix, Camara.cameraProjectionMatrix);
            explosionSmokeParticles.SetCamera(Camara.cameraViewMatrix, Camara.cameraProjectionMatrix);
            

          
            
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








            
     

  

