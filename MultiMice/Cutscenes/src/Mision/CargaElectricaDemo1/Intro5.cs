using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAnimation;
using XNAnimation.Controllers;
using System;



namespace CargaElectricaDemo1
{

    public class Intro5 : AbstractGame
    {
        // Cosas nativas del juego
        public GraphicsDeviceManager graphics;
        public SpriteFont font;
        
        // Cosas agregadas por mi
        public Plane planoJuego;
        List<IGameObject> objectList;
        
        // Objects Game
        Objeto3D mapa;
        Objeto2D fondoEstrellas;

        Objeto3D campo;
        
        KeyboardState lastKeyboardState;
        KeyboardState currentKeyboardState;

        Random rand = new Random();

        List<string> texts = new List<string>();
        List<int> lengths = new List<int>();
        int currentText = 0;
              

        // Game constructor
        public Intro5()
        {            
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1270;
            graphics.PreferredBackBufferHeight = 720;
            graphics.IsFullScreen = false;

            texts.Add("");
            lengths.Add(texts[0].Length);
            texts.Add("Para recolectar los cristales se creo un grupo especializado, que recibiría una preparación exhaustiva \n" +
             "para lograr la captura y el transporte de estas valiosas fuentes de energía.");
            lengths.Add(texts[1].Length / 2);
            //texts.Add("y los recursos disponibles para abastecerlas se hacían cada vez más escasos.");
            texts.Add("Año a año son reclutados nuevos miembros a este grupo, quienes deben sufrir un entrenamiento riguroso \n" +
               "para llevar a cabo la misión, poniendo en riesgo sus vidas por el bien de la colonia.");
            lengths.Add(texts[2].Length / 2);
            //texts.Add("su objetivo: enviar una nave interestelar a colonizar el primer planeta habitable detectado");

            texts.Add("");
            lengths.Add(texts[3].Length);
            
            Content.RootDirectory = "Content";                 
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
            planoJuego= new Plane(Vector3.Up, 0);
            //font = Content.Load<SpriteFont>("SpriteFont1");
            font = Content.Load<SpriteFont>("Fonts//SpriteFont1");
            //Cargar fondo
            fondoEstrellas = new Objeto2D(Content.Load<Texture2D>("Sprites//Textures//EstrellasPlaneta"),
                new Vector2(Window.ClientBounds.Width / 2, Window.ClientBounds.Height / 2), 1);
            objectList.Add(fondoEstrellas);

            // Cargar Mapa
            mapa = new Objeto3D(Content.Load<Model>("Sprites//Models//training_lab2"), new Vector3(0, 0, 0), new Vector3(1.3f, 1f,1.3f)*0.35f, new Vector3(0,0,0));
            objectList.Add(mapa);
            mapa.enableDirectionalLight = true;
            mapa.enableLight = true;
            mapa.lightDirection = new Vector3(-10, 1, 1);
            mapa.lightDirection.Normalize();
            mapa.ambient = new Vector3(0.5f, 0.5f, 0.5f);

            Camara.cameraPosition = new Vector3(0, 100, 250);
            Camara.cameraProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(50.0f),
                                     graphics.GraphicsDevice.Viewport.AspectRatio, 1.0f, 1500.0f);

            Astronauta a = new Astronauta(Content.Load<Model>("Sprites//Models//Astro"), mapa.position,
                    new Vector3(2.5f, 2.5f, 2.5f),
                    new Vector3(0, 0, 0),0);
            a.skinnedModel = Content.Load<SkinnedModel>("Sprites/Models/PlayerMarine");
            a.enableDirectionalLight = true;
            a.enableLight = true;
            a.lightDirection = new Vector3(-10, 1, 1);
            a.lightDirection.Normalize();
            a.ambient = new Vector3(0.1f, 0.1f, 0.1f);
            objectList.Add(a);

            //campo = new Objeto3D(Content.Load<Model>("Sprites//Models//campoRojo"), a.position, 
            //    new Vector3(1, 1, 1) * 1.5f, Vector3.Zero);
            //campo.enableDirectionalLight = true;
            //campo.enableLight = true;
            //campo.lightDirection = new Vector3(-10, 1, 1);
            //campo.lightDirection.Normalize();
            //campo.ambient = new Vector3(0.1f, 0.1f, 0.1f);
            //objectList.Add(campo);    

            a = new Astronauta(Content.Load<Model>("Sprites//Models//Astro"), mapa.position + new Vector3(50,0,50),
                    new Vector3(2.5f, 2.5f, 2.5f),
                    new Vector3(0, 0, 0), 1);
            a.skinnedModel = Content.Load<SkinnedModel>("Sprites/Models/PlayerMarine");
            a.enableDirectionalLight = true;
            a.enableLight = true;
            a.lightDirection = new Vector3(-10, 1, 1);
            a.lightDirection.Normalize();
            a.ambient = new Vector3(0.1f, 0.1f, 0.1f);
            objectList.Add(a);


            a = new Astronauta(Content.Load<Model>("Sprites//Models//Astro"), mapa.position + new Vector3(-50, 0, 50),
                    new Vector3(2.5f, 2.5f, 2.5f),
                    new Vector3(0, 0, 0), 2);
            a.skinnedModel = Content.Load<SkinnedModel>("Sprites/Models/PlayerMarine");
            a.enableDirectionalLight = true;
            a.enableLight = true;
            a.lightDirection = new Vector3(-10, 1, 1);
            a.lightDirection.Normalize();
            a.lightDiffuse = new Vector3(0.3f, 0.3f, 0.3f);
            a.lightSpecular = new Vector3(0.3f, 0.3f, 0.3f);
            a.ambient = new Vector3(0.1f, 0.1f, 0.1f);
            objectList.Add(a);


            int numPlayer = 3;

            animationController = new AnimationController[numPlayer];

            for (int i = 0; i < numPlayer; i++)
            {
                animationController[i] = new AnimationController(a.skinnedModel.SkeletonBones);
                animationController[i].StartClip(a.skinnedModel.AnimationClips["Idle"]);
            }
            

            
            // Cargar Cuadro de Texto
            //texto = new Objeto2D(Content.Load<Texture2D>("Sprites//Textures//texto"), new Vector2(635, 625), 1.3f);
            //objectList.Add(texto);

        }


        protected override void UnloadContent()
        {
            Content.Dispose();
        }


        
        // Update
        protected override void Update(GameTime gameTime)
        {                        
            
            // Update Camara
            
            lastKeyboardState = currentKeyboardState;
            
            currentKeyboardState = Keyboard.GetState();
            if ((currentKeyboardState.IsKeyDown(Keys.Enter) &&
                 lastKeyboardState.IsKeyUp(Keys.Enter)))
            {
                currentText++;
            }
            
            //if ((currentKeyboardState.IsKeyDown(Keys.A) &&
            //     lastKeyboardState.IsKeyUp(Keys.A)))
            //{
                
            //}
            if ((currentKeyboardState.IsKeyDown(Keys.S)))
            {
                Vector3 dir = Camara.cameraLookAt - Camara.cameraPosition;
                float tar_posDist = dir.Length();
                dir.Normalize();
                Camara.cameraPosition -= dir;
            }
            if ((currentKeyboardState.IsKeyDown(Keys.W)))
            {
                Vector3 dir = Camara.cameraLookAt - Camara.cameraPosition;
                float tar_posDist = dir.Length();
                dir.Normalize();
                Camara.cameraPosition += dir;
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
                fondoEstrellas.scale *= 1.0005f;
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
                fondoEstrellas.scale *= 1.0005f;
            }
            
            Camara.cameraViewMatrix = Matrix.CreateLookAt(Camara.cameraPosition, Camara.cameraLookAt, Vector3.Up);

            animationController[0].Update(gameTime.ElapsedGameTime, Matrix.Identity);
            animationController[1].Update(gameTime.ElapsedGameTime, Matrix.Identity);
            animationController[2].Update(gameTime.ElapsedGameTime, Matrix.Identity);
           
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

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            spriteBatch.DrawString(font, texts[currentText], new Vector2(Window.ClientBounds.Width / 2 - lengths[currentText] * 5,
                9 * Window.ClientBounds.Height / 10), Color.White);
            spriteBatch.End();
       

            base.Draw(gameTime);
        }

    }
}








            
     

  

