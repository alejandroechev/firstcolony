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

    public class Intro2 : AbstractGame
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

        Random rand = new Random();

        Objeto2D blurSprite;
        ResolveTexture2D renderTargetTexture;

        bool motionBlur = true;

        List<string> texts = new List<string>();
        List<int> lengths = new List<int>();
        int currentText = 0;
       
        // Game constructor
        public Intro2()
        {            
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1270;
            graphics.PreferredBackBufferHeight = 720;
            graphics.PreferMultiSampling = true;
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
            texts.Add("Tras décadas de viaje, la nave finalmente llegó a su destino: el planeta bautizado como Tierra 2.");
            lengths.Add(texts[1].Length);
            texts.Add("Los colonos se instalaron a lo largo y ancho del nuevo mundo y comenzaron \n" +
             "la difícil tarea de acomodarlo para permitir la supervivencia de la colonia.");
            lengths.Add(texts[2].Length / 2);
            //texts.Add("y los recursos disponibles para abastecerlas se hacían cada vez más escasos.");
            texts.Add("La tarea se fue haciendo año a año cada vez más difícil \n" +
               "debido a las escasas fuentes de energía disponibles en el planeta.");
            lengths.Add(texts[3].Length / 2);
            //texts.Add("su objetivo: enviar una nave interestelar a colonizar el primer planeta habitable detectado");
            texts.Add("Si no se lograba descubrir alguna fuente energética en el corto plazo, la colonia moriría, \n" +
                "y junto con ella, las esperanzas de supervivencia de toda la humanidad.");
            lengths.Add(texts[4].Length / 2);
            //texts.Add("y permitiría finalmente lograr expandir a la humanidad fuera de los confines de la Tierra.");
            texts.Add("El futuro se veía cada día más oscuro. ");
            lengths.Add(texts[5].Length);
            texts.Add("");
            lengths.Add(texts[6].Length);
            texts.Add("Un descubrimiento fortuito arrojó una luz de esperanza a la misión.");
            lengths.Add(texts[7].Length);
            texts.Add("Una expedición al cinturón de asteroides que orbitaba el planeta descubrió la presencia de unos extraños cristales, \n" +
                "con la increible propiedad de almacenar energía eléctrica.");
            lengths.Add(texts[8].Length / 2);
            texts.Add("Si se lograba obtener la electricidad de estos fragiles cristales, \n" +
                "la colonia tendría energía suficiente para subsistir.");
            lengths.Add(texts[9].Length / 2);
            texts.Add("");
            lengths.Add(texts[10].Length);

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
            info = new SplineInfo("path.sp");
            info.Load();

            spline = new CatmullRom();
            spline.AddPoints(info.Points);
            interpolated = spline.Interpolate(0.05f);

            fontMenu = Content.Load<SpriteFont>("Fonts//SpriteFont1");
            
            //Cargar fondo
            fondoEstrellas = new Objeto2D(Content.Load<Texture2D>("Sprites//Textures//EstrellasPlaneta"), 
                new Vector2(Window.ClientBounds.Width/2, Window.ClientBounds.Height/2), 1);
            objectList.Add(fondoEstrellas);
                
            //Cargar Nave
            nave = new Objeto3D(Content.Load<Model>("Sprites//Models//UnaNave"),
                new Vector3(-80, 410, 1270), new Vector3(1, 1f, 1)*2,
                new Vector3(-(float)Math.PI / 2, 0, (float)(Math.PI / 8)));
            nave.enableLight = true;
            nave.enableDirectionalLight = true;
            nave.lightDirection = new Vector3(-10, 1, 1);
            nave.lightDirection.Normalize();
            objectList.Add(nave);

            tierra = new Objeto3D(Content.Load<Model>("Sprites//Models//Planet"), new Vector3(0, -4000, -7000), new Vector3(1, 1f, 1) * 20f,
                new Vector3(0, 0, 0));
            tierra.enableDirectionalLight = true;
            tierra.enableLight = true;
            tierra.lightDirection = new Vector3(-10, 1, 1);
            tierra.lightDirection.Normalize();
            objectList.Add(tierra);

            Vector3 v = tierra.position + new Vector3(0, 1000, 11000);
            for (int i = 0; i < 30; i++)
            {
                Objeto3D a = new Objeto3D(Content.Load<Model>("Sprites//Models//TiberiumRojo"), v +
                    new Vector3((float)((rand.NextDouble() - 0.5) * 6000 - 3000), (float)((rand.NextDouble() - 0.5) * 3000),
                        (float)((rand.NextDouble() - 0.5) * 1000)),
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
                    new Vector3((float)((rand.NextDouble() - 0.5) * 6000 - 1000), (float)((rand.NextDouble() - 0.5) * 3000), 
                        (float)((rand.NextDouble() - 0.5) * 1000)),
                    new Vector3(1, 1, 1) * 0.001f,
                    new Vector3(0, 0, 0));
                a.enableDirectionalLight = true;
                a.enableLight = true;
                a.lightDirection = new Vector3(-10, 1, 1);
                a.lightDirection.Normalize();
                a.ambient = new Vector3(0.7f, 0.7f, 0.7f);
                objectList.Add(a);
            }

            v = tierra.position + new Vector3(0, 2000, 7000);

            for (int i = 0; i < 300; i++)
            {
                Objeto3D a = new Objeto3D(Content.Load<Model>("Sprites//Models//asteroid"), v +
                    new Vector3((float)((rand.NextDouble() - 0.5) * 10000 - 2000), (float)((rand.NextDouble() - 0.5) * 1000), 
                        (float)((rand.NextDouble() - 0.5) * 1000)),
                    new Vector3(1, 1, 1) * 0.1f,
                    new Vector3(0, 0, 0));
                a.enableDirectionalLight = true;
                a.enableLight = true;
                a.lightDirection = new Vector3(-10, 1, 1);
                a.lightDirection.Normalize();
                a.ambient = new Vector3(0.7f, 0.7f, 0.7f);
                objectList.Add(a);
            }

            renderTargetTexture = new ResolveTexture2D(
                graphics.GraphicsDevice,
                graphics.GraphicsDevice.PresentationParameters.BackBufferWidth,
                graphics.GraphicsDevice.PresentationParameters.BackBufferHeight,
                1,
                graphics.GraphicsDevice.PresentationParameters.BackBufferFormat);

            blurSprite = new Objeto2D(renderTargetTexture, new Vector2(Window.ClientBounds.Width / 2, Window.ClientBounds.Height / 2), 1);

            Camara.cameraPosition = new Vector3(0.0f, 350.0f, 700.0f);
            Camara.cameraLookAt = tierra.position + new Vector3(-500, 700, 0);

            //sol = new Objeto3D(Content.Load<Model>("Sprites//Models//redsun"), new Vector3(600, 0, -500), new Vector3(1, 1f, 1) * 1f,
            //    new Vector3(0, 0, 0));
            //sol.enableLight = true;
            ////sol.diffuse = new Vector3(0.2f, 0 , 0);
            //sol.emmisive = new Vector3(0.5f, 0, 0);
            //sol.diffuse = new Vector3(0.5f, 0.5f, 0.5f); 

            //objectList.Add(sol);
            
            
          
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
            if ((currentKeyboardState.IsKeyDown(Keys.Enter) &&
                 lastKeyboardState.IsKeyUp(Keys.Enter)))
            {
                currentText++;
            }
            if ((currentKeyboardState.IsKeyDown(Keys.W)))
            {
                Vector3 v = Camara.cameraLookAt - Camara.cameraPosition;
                v.Normalize();
                Camara.cameraPosition += 5 * v;
                fondoEstrellas.scale *= 1.0002f;
            }
            if ((currentKeyboardState.IsKeyDown(Keys.S)))
            {
                Camara.cameraPosition += new Vector3(0, 0, 30);
                fondoEstrellas.scale /= 1.0002f;
                motionBlur = true;
            }
            if ((currentKeyboardState.IsKeyDown(Keys.A)))
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
                fondoEstrellas.position += new Vector2(0.2f, 0);
            }
            if ((currentKeyboardState.IsKeyDown(Keys.D)))
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
                fondoEstrellas.position -= new Vector2(0.4f, 0);
            }
            if ((currentKeyboardState.IsKeyDown(Keys.Space)))
            {
                startDark = true;
                motionBlur = false;
            }

            
            //if ((currentKeyboardState.IsKeyDown(Keys.Space) &&
            //     lastKeyboardState.IsKeyUp(Keys.Space)))
            //{
            //    Console.WriteLine(nave.position.ToString());
            //}

            //if ((currentKeyboardState.IsKeyDown(Keys.W)))
            //{
            //    nave.position += new Vector3(0, 0, -10);
            //}
            //else if ((currentKeyboardState.IsKeyDown(Keys.S)))
            //{
            //    nave.position += new Vector3(0, 0, 10);
            //}
            //else if ((currentKeyboardState.IsKeyDown(Keys.Down)))
            //{
            //    nave.position += new Vector3(0, -10, 0);
            //}
            //else if ((currentKeyboardState.IsKeyDown(Keys.Up)))
            //{
            //    nave.position += new Vector3(0, 10, 0);
            //}
            //else if ((currentKeyboardState.IsKeyDown(Keys.Right)))
            //{
            //    nave.position += new Vector3(10, 0, 0);
            //}
            //else if ((currentKeyboardState.IsKeyDown(Keys.Left)))
            //{
            //    nave.position += new Vector3(-10, 0, 0);
            //}

            //splineIndex++;
            //if (splineIndex < interpolated.Count)
            //{
            //    nave.position = new Vector3(interpolated[splineIndex].x / 10, interpolated[splineIndex].y / 10,
            //        interpolated[splineIndex].z / 10);
            //    explosionParticles.AddParticle(nave.position + new Vector3(-30, 30, 120), Vector3.Zero);
            //    explosionParticles2.AddParticle(nave.position + new Vector3(30, 30, 120), Vector3.Zero);
            //}

            Vector3 naveDir = Camara.cameraLookAt - nave.position;
            naveDir.Normalize();
            nave.position += 10 * naveDir;
           
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








            
     

  

