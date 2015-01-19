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
using GameNetwork;

namespace ARGame
{
    public class ServerLevel : AbstractLevel
    {
        public CultureInfo CultureInfo { get; set; }
        protected Dictionary<int, GameObject> objects;
        protected Dictionary<GameObject, int> invObjects;        
        public override Dictionary<int, GameObject> ObjectsTable { get { return objects; } }
        public override List<GameObject> ObjectsList { get { return objects.Values.ToList<GameObject>(); } }
        private List<ARAstronaut> players;
        public override List<ARAstronaut> Players { get { return players; } }
        public CoulombPhysics physics;

        protected List<Vector3> respawnPositions;
        protected int currentRespawnPosition = 0;

        public override event Action<bool> SubLevelEnded;

        protected string file;
        public override string File { get { return file; } set { file = value; } }

        protected float xmin = 3 * -16.394729f;
        protected float xmax = 3 * 16.394729f;
        protected float ymin = 3 * -10.676758f;
        protected float ymax = 3 * 14.676758f;

        protected int currentId;

        public ARCrystal Crystal { get; private set; }

        public ServerLevel(AbstractGame game):base(game)
        {
            CultureInfo = new CultureInfo("en-US");
            objects = new Dictionary<int, GameObject>();
            invObjects = new Dictionary<GameObject, int>();
            players = new List<ARAstronaut>();
            physics = new CoulombPhysics();
            currentId = 0;
            respawnPositions = new List<Vector3>();
            
        }

        public override void Init() 
        {
            Load(this.File);
        }

        #region Level Initilization
        protected void CreatePhysicsRelations()
        {
            PhysicMaterial physicsMaterial = new PhysicMaterial();
            physicsMaterial.MaterialName1 = "crystal";
            physicsMaterial.MaterialName2 = "asteroid";
            physicsMaterial.ContactBeginCallback = AsteroidCrystalCollision;

            physics.AddPhysicMaterial(physicsMaterial);

            PhysicMaterial physicsMaterial2 = new PhysicMaterial();
            physicsMaterial2.MaterialName1 = "crystal";
            physicsMaterial2.MaterialName2 = "portal";
            physicsMaterial2.ContactBeginCallback = PortalCrystallCollision;

            physics.AddPhysicMaterial(physicsMaterial2);
        }

        #region Load XML
        public override void Load(string file)
        {
            XDocument xmlDoc = XDocument.Load(file);
            XElement xmlScene = xmlDoc.Elements("scene").First();

            XElement xmlObjects = xmlScene.Elements("object_list").First();

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

            CreatePhysicsRelations();

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


        #region Load Objects
        protected void LoadAstronauts(XElement xmlObjects)
        {
            int id = 0;
            foreach (XElement astronautNode in xmlObjects.Elements("astronaut"))
            {
                float mass = LoadFloat(astronautNode, "mass");
                Vector3 position = 3 * LoadXYZ(astronautNode.Elements("position").First());
                ARAstronaut astronaut = new ARAstronaut(id++, mass, position);
                objects.Add(currentId, astronaut);
                invObjects.Add(astronaut, currentId);
                players.Add(astronaut);
                currentId++;
            }
        }

        protected void LoadPortals(XElement xmlObjects)
        {
            foreach (XElement portalNode in xmlObjects.Elements("portal"))
            {
                Vector3 position = 3 * LoadXYZ(portalNode.Elements("position").First());

                Vector3 scale = LoadXYZ(portalNode.Elements("scale").First());

                ARPortal portal = new ARPortal(this.game.contentManager.GetModel("portal"), scale, position);

                objects.Add(currentId, portal);
                invObjects.Add(portal, currentId);
                currentId++;
            }
        }

        protected void LoadCrystals(XElement xmlObjects)
        {
            foreach (XElement crystalNode in xmlObjects.Elements("crystal"))
            {
                float mass = 100 * LoadFloat(crystalNode, "mass");
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
                Crystal = crystal;
                currentId++;
            }
        }

        protected void LoadAsteroids(XElement xmlObjects)
        {
            foreach (XElement asteroidNode in xmlObjects.Elements("asteroid"))
            {
                float mass = LoadFloat(asteroidNode, "mass");
                bool intertial = LoadBool(asteroidNode, "inertial");
                Vector3 position = LoadXYZ(asteroidNode.Elements("position").First());
                position = 3 * position;
                Vector3 velocity = 3 * LoadXYZ(asteroidNode.Elements("velocity").First());
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
                currentId++;
            }
        }
        #endregion

        #region Load Utils
        protected bool LoadBool(XElement node, string attribute)
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

        public override void Draw()
        {
            base.Draw();
            UI2DRenderer.WriteText(new Vector2(10, 0), ">> Lleven el cristal al portal \n>> Todos deben participar", Color.White, game.font);
        }


        public override void Update(float elpasedTime)
        {
            //Update world
            physics.Update(elpasedTime, this.ObjectsList.ToArray() as IPhysicBody[]);
            //UpdateArrow();          
        }

        protected void UpdateArrow()
        {
            foreach (IPhysicBody body in ObjectsList)
            {
                if (body.MaterialName != null && body.MaterialName.Equals("crystal"))
                {

                    Vector3 force = physics.CalculateForce(body, ObjectsList.ToArray() as IPhysicBody[]);

                    float length = force.Length();
                    force.Normalize();
                    int acosFix = force.Y < 0 ? -1 : 1;
                    float angle = acosFix * (float)Math.Acos(Vector3.Dot(force, Vector3.UnitX));
                    ARCrystal crystal = (ARCrystal)body;
                    crystal.ArrowAngle = angle;
                    crystal.ArrowLength = length;

                }
            }
        }

        public virtual void SetPlayerInfo(PlayerState playerState, int numPlayers)
        {
            Players[playerState.Id].SetPosition(playerState.Position);
            Players[playerState.Id].Charge = playerState.Charge;
            Players[playerState.Id].IsActive = playerState.IsAlive;
        }



        #region Level Events
        public override void ResetSubLevel() 
        {
            foreach (GameObject obj in ObjectsList)
            {
                if (obj.MaterialName != null && obj.MaterialName.Equals("crystal"))
                {
                    obj.Reset();
                }
                else
                    obj.Reset();
            }
            if (SubLevelEnded != null)
                SubLevelEnded(false);
        }

        public override void PassSubLevel()
        {
            foreach (GameObject obj in ObjectsList)
            {
                if (obj.MaterialName != null && obj.MaterialName.Equals("crystal"))
                {
                    if (respawnPositions.Count == 0)
                    {
                        int aleat0 = (DateTime.Now.Millisecond % 2 == 0 ? -1 : 1) * (10 + DateTime.Now.Millisecond % 5);
                        int aleat1 = (DateTime.Now.Millisecond % 2 == 0 ? -1 : 1) * (20 + DateTime.Now.Millisecond % 15);
                        obj.SetPosition(obj.Position + new Vector3(aleat0, aleat1, 0));
                    }
                    else
                    {
                        obj.SetPosition(respawnPositions[currentRespawnPosition]);
                        currentRespawnPosition++;
                        if (currentRespawnPosition >= respawnPositions.Count)
                            currentRespawnPosition = 0;
                    }
                    obj.Charge = obj.Charge * -1;
                }
            }
            if (SubLevelEnded != null)
                SubLevelEnded(true);
        }

        protected virtual void DestroyCrystral(ARCrystal crystal)
        {
            objects.Remove(invObjects[crystal]);
        }


        protected virtual void AsteroidCrystalCollision(IPhysicBody physObj1, IPhysicBody physObj2)
        {
            ResetSubLevel();
        }

        protected virtual void PortalCrystallCollision(IPhysicBody physObj1, IPhysicBody physObj2)
        {
            PassSubLevel();
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

        
        #endregion

        
    }

}
