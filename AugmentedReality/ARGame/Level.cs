using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using ARUtils;
using GameModel;

using GoblinXNA;
using GoblinXNA.Graphics;
using GoblinXNA.Graphics.Geometry;
using GoblinXNA.Physics;
using GoblinXNA.SceneGraph;
using GoblinXNA.UI.UI2D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Model = GoblinXNA.Graphics.Model;

namespace ARGame
{
    public class Level : AbstractLevel
    {
        public CultureInfo CultureInfo { get; set; }
        public LightNode lightNode;
       
        public MarkerNode markerNode;
        protected List<MarkerNode> markers;
        protected List<GameObject> objects;
        public override Dictionary<int, GameObject> ObjectsTable { get { return null; } }
        public override List<GameObject> ObjectsList { get { return objects; } }
        protected ARAstronaut player;
        public override List<ARAstronaut> Players { get { return new List<ARAstronaut> { player }; } }
        public CoulombPhysics physics;
        
        public override event Action<bool> LevelEnded;
        public override event Action<bool> SubLevelEnded;

        public int levelCompleted; 
        protected float time; //para manejar tiempo de las teclas, eliminar despues

        protected string file;
        public override string File { get { return file; } set { file = value; } }

        protected GameGUI.GameGUI gameGUI;
        protected float playtime;

        protected bool ArrowMode;

        protected bool shooting = false;
        protected bool passSub = false;
        protected float passSubTimer = 0;

        protected float xmin = 3*-16.394729f;
        protected float xmax = 3 * 16.394729f;
        protected float ymin = 3 * -10.676758f;
        protected float ymax = 3 * 14.676758f;

        protected List<Vector3> respawnPositions;
        protected int currentRespawnPosition = 0;

        protected Random rand = new Random();


        public Level(AbstractGame game) : base(game)
        {
            CultureInfo = new CultureInfo("en-US");
            markers = new List<MarkerNode>();
            objects = new List<GameObject>();
            physics = new CoulombPhysics();
            respawnPositions = new List<Vector3>();
            gameGUI = new GameGUI.GamePlayGUI();
            this.ArrowMode = true;
        }

        public override void Close() 
        {
            game.sceneGraph.RootNode.RemoveChildren();
            this.markerNode.RemoveChildren();
        }
        public override void Init() 
        {
            Load(this.File);
            //game.sceneGraph.RootNode.AddChild(this.lightNode);
            game.sceneGraph.RootNode.AddChild(this.markerNode);
            (gameGUI as GameGUI.GamePlayGUI).ChangeCharge += this.player.SetCharge;
            (gameGUI as GameGUI.GamePlayGUI).Activate += this.player.Activate;
            (gameGUI as GameGUI.GamePlayGUI).Shoot += this.ShootRay;
            gameGUI.Init(game.sceneGraph,game.contentManager, game.font);
            levelCompleted = 0;
            playtime = 0;
            this.player.Bullet.Destroy += this.DestroyRay;
        }

        public override void Draw()
        {
            UI2DRenderer.WriteText(new Vector2(509, 0), "Puntos: " + levelCompleted, Color.White, game.font);
            string timestring;
            
            int min = (int)(playtime/60);
            if(min < 10) timestring = "0"+min+":";
            else timestring = min+":";

            int seg = (int)(playtime%60);
            if(seg<10) timestring+="0"+seg;
            else timestring+=seg;

            
            UI2DRenderer.WriteText(new Vector2(509, 20), "Tiempo: " + timestring , Color.White, game.font);
            if (passSub && levelCompleted > 1)
            {
                UI2DRenderer.WriteText(new Vector2(250, 350), "Subnivel Completado. Sigue asi!!!", Color.Yellow, game.bigFont != null ? game.bigFont : game.font);
                (gameGUI as GameGUI.GamePlayGUI).messagePanel.RelativeSize = new Vector2(0.8f, 0.15f);

            }
            else { (gameGUI as GameGUI.GamePlayGUI).messagePanel.RelativeSize = new Vector2(0.8f, 0.1f); }
            gameGUI.Draw();      
            
        }

        private void UpdateArrow()
        {
            foreach (IPhysicBody body in objects)
            {
                if (body.MaterialName != null && body.MaterialName.Equals("crystal"))
                {

                    Vector3 force = physics.CalculateForce(body, objects.ToArray() as IPhysicBody[]);

                    float lenght = force.Length();
                    force.Normalize();
                    int acosFix = force.Y < 0 ? -1 : 1;
                    int playerOn = ArrowMode ? 1 : 0;

                    (body as ARCrystal).DrawArrow(playerOn * lenght, acosFix * (float)Math.Acos(Vector3.Dot(force, Vector3.UnitX)));


                    // UI2DRenderer.WriteText(new Vector2(0, 60), "F:" + lenght + " A:" + force.ToString(), Color.Red, game.font);
                }

            }
        }

        public override void Update(float elpasedTime)
        {
            if (passSub && passSubTimer < 1.5)
                passSubTimer += elpasedTime;
            else if (passSub && passSubTimer >= 1.5)
            {
                passSub = false;
                passSubTimer = 0;
            }

            UpdatePlayerPosition();
            UpdateMarkerFound();
            UpdateArrow();
            //Update world
            physics.Update(elpasedTime, this.objects.ToArray() as IPhysicBody[]);
          
            //codigo de por mientras para pasar niveles
            //KeyboardState keyState = Keyboard.GetState();
            //if ((time += elpasedTime) > 0.2f)
            //{
            //    time = 0;
            //    if (keyState.IsKeyDown(Keys.Right))
            //        this.Win();
            //}
            playtime += elpasedTime;
        }

        private void UpdateMarkerFound()
        {
            if (!markerNode.MarkerFound)
            {

                foreach (MarkerNode marker in markers)
                {
                    if (marker.MarkerFound)
                    {
                        this.game.sceneGraph.RootNode.RemoveChild(markerNode);
                        foreach (GameObject obj in objects)
                        {
                            if (obj.ObjectNode != null)
                            {
                                markerNode.RemoveChild(obj.ObjectNode);
                                marker.AddChild(obj.ObjectNode);
                            }
                        }
                        this.markerNode = marker;
                        break;
                    }
                }
            }
        }

        public void UpdatePlayerPosition()
        {

            if (markerNode.MarkerFound)
            {
                Vector3 cameraPos = new Vector3(0, 0, 0);
                Vector3 playerPosition = this.game.sceneGraph.GraphicsDevice.Viewport.Unproject(cameraPos,
                State.ProjectionMatrix, State.ViewMatrix, markerNode.WorldTransformation);
                
                player.Position = new Vector3(playerPosition.X,playerPosition.Y,0);
               
            }

        }

        public override void Pause(bool pause)
        {
            gameGUI.Pause(pause);           
        }

        #region Level Events
        
        public override void ResetSubLevel() 
        {
            ResetObjects();
            this.gameGUI.Reset();
            if (SubLevelEnded != null)
                SubLevelEnded(false);
            
        }

        protected void ResetObjects()
        {
            List<GameObject> objsAux = new List<GameObject>(objects);

            foreach (GameObject obj in objsAux)
                obj.Reset();
            
        }
        public override void PassSubLevel()
        {
            passSub = true;
            ResetObjects();
            this.gameGUI.Reset();
            if (SubLevelEnded != null)
                SubLevelEnded(true);            
        }
        
        //este metodo no existe despues
        public void Win()
        {
            if (LevelEnded != null)
                LevelEnded(true);
           
        }

        
        protected virtual void BulletCrystalCollision(IPhysicBody physObj1, IPhysicBody physObj2)
        {
            Vector3 dir = (physObj2 as ARBullet).Direction;
            dir = Vector3.Cross(Vector3.UnitZ, dir);
            dir.Normalize();
            this.CloneCrystal(physObj1 as ARCrystal, dir);
            this.DestroyRay((physObj2 as ARBullet));
        }

        private void CloneCrystal(ARCrystal crystal, Vector3 dir)
        {
            ARCrystal newCrystal = crystal.Divide(dir);
            objects.Add(newCrystal);
            markerNode.AddChild(newCrystal.ObjectNode);

        }
        protected virtual void AsteroidCrystalCollision(IPhysicBody physObj1, IPhysicBody physObj2)
        {
           ResetSubLevel();
        }
        protected virtual void CrystallPortalCollision(IPhysicBody physObj1, IPhysicBody physObj2)
        {
            /*depende del nivel*/
            //Win();
            
        }

        protected virtual void OutOfBoundsHandler(IPhysicBody body)
        {
            if (body.MaterialName.Equals("crystal"))
            {
                ((GameObject)body).Reset();
            }
            else if (body.MaterialName.Equals("asteroid"))
            {
                ((GameObject)body).Reset();
            }
        }

        protected virtual void DestroyRay(ARBullet bullet) 
        {
            objects.Remove(bullet);
            markerNode.RemoveChild(bullet.ObjectNode);
            shooting = false;
           
        }
        protected virtual void DestroyCrystral(ARCrystal crystal)
        {
            objects.Remove(crystal);
            markerNode.RemoveChild(crystal.ObjectNode);

        }
        protected virtual void ShootRay()
        {
            if (shooting) return;
            shooting = true;

            Vector3 nearSource = new Vector3(400, 300, 0);
            Vector3 farSource = new Vector3(400, 300, 1);

            Matrix viewMatrix = markerNode.WorldTransformation * State.ViewMatrix;

            Vector3 nearPoint = this.game.sceneGraph.GraphicsDevice.Viewport.Unproject(nearSource,
                State.ProjectionMatrix, viewMatrix, Matrix.Identity);
            Vector3 farPoint = this.game.sceneGraph.GraphicsDevice.Viewport.Unproject(farSource,
                State.ProjectionMatrix, viewMatrix, Matrix.Identity);


            Vector3 linVel = farPoint - nearPoint;
            
            Vector3 position = nearPoint + Vector3.UnitX;
            Vector3 velocity = linVel/3; 
           

            //ARBullet bullet = new ARBullet(this.game.contentManager.GetModel("asteroid"), scale, mass, position, velocity);            
            
            this.player.Bullet.Reset(position, velocity);
            objects.Add(this.player.Bullet);
            markerNode.AddChild(this.player.Bullet.ObjectNode);
           
 
        }
        #endregion

        #region Level Initilization
        private void CreatePhysicsRelations()
        {
            PhysicMaterial physicsMaterial = new PhysicMaterial();
            physicsMaterial.MaterialName1 = "crystal";
            physicsMaterial.MaterialName2 = "asteroid";
            physicsMaterial.ContactBeginCallback = AsteroidCrystalCollision;

            physics.AddPhysicMaterial(physicsMaterial);

            PhysicMaterial physicsMaterial2 = new PhysicMaterial();
            physicsMaterial2.MaterialName1 = "crystal";
            physicsMaterial2.MaterialName2 = "portal";
            physicsMaterial2.ContactBeginCallback = CrystallPortalCollision;

            physics.AddPhysicMaterial(physicsMaterial2);

            PhysicMaterial physicsMaterial3 = new PhysicMaterial();
            physicsMaterial3.MaterialName1 = "crystal";
            physicsMaterial3.MaterialName2 = "bullet";
            physicsMaterial3.ContactBeginCallback = BulletCrystalCollision;

            physics.AddPhysicMaterial(physicsMaterial3);
        }

        #region Load XML
        public override void Load(string file)
        {
            XDocument xmlDoc = XDocument.Load(file);
            XElement xmlScene = xmlDoc.Elements("scene").First();

            XElement xmlObjects = xmlScene.Elements("object_list").First();
          
            //llamado a cargar los marcadores del nivel.
            this.markerNode = ARManager.markerNode;

            markers.Add(markerNode);

            if (xmlScene.Elements("positions").Count() > 0)
            {
                XElement xmlPositions = xmlScene.Elements("positions").First();
                if (xmlPositions != null)
                    LoadPositions(xmlPositions);
            }
            LoadAsteroids(xmlObjects);
            LoadCrystals(xmlObjects);
            LoadPortals(xmlObjects);
            LoadAstronauts(xmlObjects);

            //CreateLights();
            CreatePhysicsRelations();
            //CreateGround();


            physics.SetBounds(xmin, xmax, ymin, ymax);
            physics.OutOfBounds += OutOfBoundsHandler;
           
        }

        private void LoadPositions(XElement xmlPositions)
        {
            foreach (XElement respawn in xmlPositions.Elements("respawn"))
            {
                respawnPositions.Add(3 * LoadXYZ(respawn));
            }
        }

        
        private void CreateLights()
        {
            // Create a directional light source
            LightSource lightSource = new LightSource();
            lightSource.Direction = new Vector3(0, -1, -1);
            lightSource.Diffuse = Color.White.ToVector4();
            lightSource.Specular = new Vector4(0.6f, 0.6f, 0.6f, 1);

            // Create a light node to hold the light source
            this.lightNode = new LightNode();
            this.lightNode.LightSources.Add(lightSource);

            //game.sceneGraph.EnableShadowMapping = true;
        }

        private void CreateGround()
        {
            GeometryNode groundNode = new GeometryNode();           
                groundNode.Model = new Box(140, 90, 0.1f);
            
            // Set this ground model to act as an occluder so that it appears transparent
            groundNode.IsOccluder = true;

            // Make the ground model to receive shadow casted by other objects with
            // CastShadows set to true
            groundNode.Model.ReceiveShadows = true;

            Material groundMaterial = new Material();
            groundMaterial.Diffuse = Color.Gray.ToVector4();
            groundMaterial.Specular = Color.White.ToVector4();
            groundMaterial.SpecularPower = 20;

            groundNode.Material = groundMaterial;
            TransformNode ground = new TransformNode();
            ground.Translation = new Vector3(-10, 0, -10);
            markerNode.AddChild(ground);
            ground.AddChild(groundNode);
        }
        #region Load Objects
        private void LoadAstronauts(XElement xmlObjects)
        {
            int id = 0;
            foreach (XElement astronautNode in xmlObjects.Elements("astronaut"))
            {
                float mass = LoadFloat(astronautNode, "mass");
                Vector3 position = 3 * LoadXYZ(astronautNode.Elements("position").First());
                ARAstronaut astronaut = new ARAstronaut(id++, mass, position);   
                objects.Add(astronaut);               
                //markerNode.AddChild(astronaut.ObjectNode);
                astronaut.ShootRay += this.ShootRay;
                player = astronaut;
            }
        }

        private void LoadPortals(XElement xmlObjects)
        {
            foreach (XElement portalNode in xmlObjects.Elements("portal"))
            {
                Vector3 position = 3 * LoadXYZ(portalNode.Elements("position").First());                

                Vector3 scale = LoadXYZ(portalNode.Elements("scale").First());

                ARPortal portal = new ARPortal(this.game.contentManager.GetModel("portal"), scale, position);
                
                objects.Add(portal);
                markerNode.AddChild(portal.ObjectNode);
            }
        }

        private void LoadCrystals(XElement xmlObjects)
        {
            foreach (XElement crystalNode in xmlObjects.Elements("crystal"))
            {
                float mass = 100*LoadFloat(crystalNode, "mass");
                float charge = LoadFloat(crystalNode, "charge");
                bool intertial = LoadBool(crystalNode, "inertial");
                Vector3 position = 3 * LoadXYZ(crystalNode.Elements("position").First());
                //position = new Vector3(width / 2, height / 2, 0) + 18 * position;
                Vector3 velocity = 3 * LoadXYZ(crystalNode.Elements("velocity").First());
                Vector3 scale = LoadXYZ(crystalNode.Elements("scale").First());

                ARCrystal crystal = new ARCrystal(this.game.contentManager, scale, mass, position, velocity, intertial, charge);
                crystal.isAlive = true;               
                crystal.isDivisible = LoadBool(crystalNode, "divisible");
                crystal.Destroy += this.DestroyCrystral;
                objects.Add(crystal);
                markerNode.AddChild(crystal.ObjectNode);
            }
        }

        private void LoadAsteroids(XElement xmlObjects)
        {
            foreach (XElement asteroidNode in xmlObjects.Elements("asteroid"))
            {
                float mass = LoadFloat(asteroidNode, "mass");
                bool intertial = LoadBool(asteroidNode, "inertial");
                Vector3 position = LoadXYZ(asteroidNode.Elements("position").First());
                position = 3 * position;
                Vector3 velocity = 3*LoadXYZ(asteroidNode.Elements("velocity").First());
                Vector3 scale = LoadXYZ(asteroidNode.Elements("scale").First());

                ARAsteroid asteroid = new ARAsteroid(this.game.contentManager.GetModel("asteroid"), scale, mass, position, velocity, intertial);

                asteroid.isDestroyable = LoadBool(asteroidNode, "destroyable");
                asteroid.isGravitational = LoadBool(asteroidNode, "gravitational");
                asteroid.isSpawnable = LoadBool(asteroidNode, "spawnable");
                if (asteroid.isSpawnable)
                asteroid.spawnPosition = 3 * LoadXYZ(asteroidNode.Elements("spawnposition").First());
                if (asteroid.isGravitational)
                    asteroid.sunPosition = 3 * LoadXYZ(asteroidNode.Elements("sunposition").First());

                
                objects.Add(asteroid);
                markerNode.AddChild(asteroid.ObjectNode);
            }
        }
        #endregion

        #region Load Utils
        private bool LoadBool(XElement node, string attribute)
        {
            if (node == null)
                return false;
            bool value;
            value = node.Attribute(attribute).Value.Equals("true");
            return value;
        }
        protected float LoadFloat(XElement node, string attribute)
        {
            if (node == null)
                return 0;
            float value = 0;
            float.TryParse(node.Attribute(attribute).Value, NumberStyles.Number, CultureInfo, out value);
            return value;
        }

        protected Vector3 LoadXYZ(XElement node)
        {
            return new Vector3(LoadFloat(node, "x"), LoadFloat(node, "y"), LoadFloat(node, "z"));
        }
        #endregion

        #endregion
        #endregion
    }

}
