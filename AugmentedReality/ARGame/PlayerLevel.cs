using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Globalization;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using GoblinXNA;
using GoblinXNA.Graphics;
using GoblinXNA.SceneGraph;
using Model = GoblinXNA.Graphics.Model;
using GoblinXNA.Graphics.Geometry;
using GoblinXNA.Physics;
using GoblinXNA.UI.UI2D;

using ARUtils;
using GameModel;
using GoblinXNA.UI;

namespace ARGame
{
    public class PlayerLevel : AbstractLevel
    {
        public CultureInfo CultureInfo { get; set; }
        public LightNode lightNode;
       
        public MarkerNode markerNode;
        protected List<MarkerNode> markers;
        protected Dictionary<int, GameObject> objects;
        protected Dictionary<GameObject, int> invObjects;
        public override Dictionary<int, GameObject> ObjectsTable { get { return objects; } }
        public override List<GameObject> ObjectsList { get { return objects.Values.ToList<GameObject>(); } }
        protected ARAstronaut player;
        public override List<ARAstronaut> Players { get { return new List<ARAstronaut>{player}; } }

       

        protected string file;
        public override string File { get { return file; } set { file = value; } }

        protected int currentId;

        protected GameGUI.GameGUI gameGUI;

        protected float subleveltime;
        protected float maxtime;
        protected int levelCompleted;
        protected float playtime;

        protected CoulombPhysics physics;

        public PlayerLevel(AbstractGame game) : base(game)
        {
            CultureInfo = new CultureInfo("en-US");
            markers = new List<MarkerNode>();
            objects = new Dictionary<int, GameObject>();
            invObjects = new Dictionary<GameObject, int>();
           
            currentId = 0;
            player = new ARAstronaut(0, 1, new Vector3(0, 0, 0));
            physics = new CoulombPhysics();
            gameGUI = new GameGUI.GamePlayGUI();
            
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
            gameGUI.Init(game.sceneGraph, game.contentManager, game.font);
            (gameGUI as GameGUI.GamePlayGUI).RayShooterButton.Enabled = false;
            levelCompleted = 0;
            playtime = 0;
        }

        public override void Draw()
        {
            UI2DRenderer.WriteText(new Vector2(509, 0), "Puntos: " + levelCompleted, Color.White, game.font);
            string timestring;

            int min = (int)(playtime / 60);
            if (min < 10) timestring = "0" + min + ":";
            else timestring = min + ":";

            int seg = (int)(playtime % 60);
            if (seg < 10) timestring += "0" + seg;
            else timestring += seg;


            UI2DRenderer.WriteText(new Vector2(509, 20), "Tiempo: " + timestring, Color.White, game.font);
            gameGUI.Draw();

        }

        public override void Update(float elpasedTime)
        {
            UpdateMarkerFound();
            UpdatePlayerPosition();
            //subleveltime += elpasedTime;
            //if (subleveltime > maxtime)
            //    player.ArrowMode = false;
            //UpdateArrow();
            //playtime += elpasedTime;
        }

        protected void UpdateArrow()
        {
            List<IPhysicBody> chargedObjects = new List<IPhysicBody>();
            chargedObjects.Add(player);
            foreach (IPhysicBody body in ObjectsList)
            {
                if (body.MaterialName != null && body.MaterialName.Equals("crystal"))
                {
                    chargedObjects.Add(body);
                    Vector3 force = physics.CalculateForce(body, chargedObjects.ToArray() as IPhysicBody[]);

                    float length = force.Length();
                    force.Normalize();
                    int acosFix = force.Y < 0 ? -1 : 1;
                    int playerOn = player.ArrowMode ? 1 : 0;

                    (body as ARCrystal).DrawArrow(playerOn * length, acosFix * (float)Math.Acos(Vector3.Dot(force, Vector3.UnitX)));
                }

            }
            //foreach (IPhysicBody body in ObjectsList)
            //{
            //    if (body.MaterialName != null && body.MaterialName.Equals("crystal"))
            //    {

            //        int playerOn = player.ArrowMode ? 1 : 0;
            //        ARCrystal crystal = (ARCrystal)body;
            //        crystal.DrawArrow(playerOn * crystal.ArrowLength, crystal.ArrowAngle);
            //    }

            //}
        }

        protected void UpdateMarkerFound()
        {
            if (!markerNode.MarkerFound)
            {

                foreach (MarkerNode marker in markers)
                {
                    if (marker.MarkerFound)
                    {
                        this.game.sceneGraph.RootNode.RemoveChild(markerNode);
                        foreach (GameObject obj in ObjectsList)
                        {
                            markerNode.RemoveChild(obj.ObjectNode);
                            marker.AddChild(obj.ObjectNode);
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
        public override void PassSubLevel()
        {
            if (levelCompleted == 0)
            {
                gameGUI.WriteMessage("\n\n\n\n\nMuy bien! pasaron la mision.\n Recogan mas cristales mientras el comandante les asigna\n una nueva mision");
            }
            this.gameGUI.Reset();
            levelCompleted++;
        }

        public override void ResetSubLevel() 
        {
            player.ArrowMode = true;
            subleveltime = 0;
            maxtime += 30;
            gameGUI.WriteMessage("\n\n\n\n\nREINICIANDO MISION.\n Cuidado con los asteroides, no deben chocar con ellos");
            this.gameGUI.Reset();
            foreach (GameObject obj in ObjectsList)
                obj.Reset();
        }

        protected virtual void DestroyCrystral(ARCrystal crystal)
        {
            objects.Remove(invObjects[crystal]);
            markerNode.RemoveChild(crystal.ObjectNode);

        }
        
        #endregion

        #region Level Initilization

        #region Load XML
        public override void Load(string file)
        {
            XDocument xmlDoc = XDocument.Load(file);
            XElement xmlScene = xmlDoc.Elements("scene").First();

            XElement xmlObjects = xmlScene.Elements("object_list").First();

            

            //llamado a cargar los marcadores del nivel.
            this.markerNode = ARManager.markerNode;
            markers.Add(markerNode);

            LoadAsteroids(xmlObjects);
            LoadCrystals(xmlObjects);
            LoadPortals(xmlObjects);
            LoadAstronauts(xmlObjects);

            CreateLights();
            CreateGround();           
           
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

            game.sceneGraph.EnableShadowMapping = false;
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
                objects.Add(currentId, astronaut);
                invObjects.Add(astronaut, currentId);
                currentId++;
            }
        }

        private void LoadPortals(XElement xmlObjects)
        {
            foreach (XElement portalNode in xmlObjects.Elements("portal"))
            {
                Vector3 position = 3 * LoadXYZ(portalNode.Elements("position").First());                

                Vector3 scale = LoadXYZ(portalNode.Elements("scale").First());

                ARPortal portal = new ARPortal(this.game.contentManager.GetModel("portal"), scale, position);

                objects.Add(currentId, portal);
                invObjects.Add(portal, currentId);
                markerNode.AddChild(portal.ObjectNode);
                currentId++;
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
                objects.Add(currentId, crystal);
                invObjects.Add(crystal, currentId);
                markerNode.AddChild(crystal.ObjectNode);
                currentId++;
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


                objects.Add(currentId, asteroid);
                invObjects.Add(asteroid, currentId);
                markerNode.AddChild(asteroid.ObjectNode);
                currentId++;
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
        private float LoadFloat(XElement node, string attribute)
        {
            if (node == null)
                return 0;
            float value = 0;
            float.TryParse(node.Attribute(attribute).Value, NumberStyles.Number, CultureInfo, out value);
            return value;
        }

        private Vector3 LoadXYZ(XElement node)
        {
            return new Vector3(LoadFloat(node, "x"), LoadFloat(node, "y"), LoadFloat(node, "z"));
        }
        #endregion

        #endregion
        #endregion

        
    }

}
