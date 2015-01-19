using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Xml.Linq;
using GameModel;
using System.Globalization;

namespace AstroLib
{
    
    public abstract class AbstractLevel
    {
        private CultureInfo englishCulture = new CultureInfo("en-US");
        public CultureInfo CultureInfo { get { return englishCulture; } }

        public List<IPhysicBody> ObjectsList { get; private set; }
        public Dictionary<int, IPhysicBody> ObjectsTable { get; private set; }
        public List<Astronaut> Players { get; private set; }
        public virtual event Action<bool> LevelEnded;
        public virtual event Action<bool> SubLevelEnded;
        public abstract string File {get;set;}

        public CoulombPhysics physics;

        public AbstractLevel()
        {
            ObjectsList = new List<IPhysicBody>();
            ObjectsTable = new Dictionary<int, IPhysicBody>();
            Players = new List<Astronaut>();

            physics = new CoulombPhysics();
        }

        public abstract void Init();
        public virtual void Close() { }
        public virtual void Draw() { }
        public abstract void Update(float elpasedTime);
        public virtual void ResetSubLevel() { }
        public virtual void PassSubLevel() { }
        public virtual void Pause(bool pause) { }
        protected abstract void BulletCrystalCollision(IPhysicBody physObj1, IPhysicBody physObj2);
        protected abstract void AsteroidCrystalCollision(IPhysicBody physObj1, IPhysicBody physObj2);
        protected abstract void CrystalPortalCollision(IPhysicBody physObj1, IPhysicBody physObj2);

        protected virtual void CreatePhysicsRelations()
        {
            PhysicMaterial physicsMaterial = new PhysicMaterial();
            physicsMaterial.MaterialName1 = "crystal";
            physicsMaterial.MaterialName2 = "asteroid";
            physicsMaterial.ContactBeginCallback = AsteroidCrystalCollision;

            physics.AddPhysicMaterial(physicsMaterial);

            PhysicMaterial physicsMaterial2 = new PhysicMaterial();
            physicsMaterial2.MaterialName1 = "crystal";
            physicsMaterial2.MaterialName2 = "portal";
            physicsMaterial2.ContactBeginCallback = CrystalPortalCollision;

            physics.AddPhysicMaterial(physicsMaterial2);

            PhysicMaterial physicsMaterial3 = new PhysicMaterial();
            physicsMaterial3.MaterialName1 = "crystal";
            physicsMaterial3.MaterialName2 = "bullet";
            physicsMaterial3.ContactBeginCallback = BulletCrystalCollision;

            physics.AddPhysicMaterial(physicsMaterial3);
        }       

        public virtual void Load(string file)
        {
            XDocument xmlDoc = XDocument.Load(file);
            XElement xmlScene = xmlDoc.Elements("scene").First();
            XElement xmlObjects = xmlScene.Elements("object_list").First();
                        
            LoadAsteroids(xmlObjects);
            LoadCrystals(xmlObjects);
            LoadPortals(xmlObjects);
            LoadAstronauts(xmlObjects);

            CreatePhysicsRelations();           
        }
       
        protected virtual void LoadAstronauts(XElement xmlObjects)
        {
            foreach (XElement astronautNode in xmlObjects.Elements("astronaut"))
            {
                float mass = LoadFloat(astronautNode, "mass");
                Vector3 position = 3 * LoadXYZ(astronautNode.Elements("position").First());
                Astronaut astronaut = new Astronaut(mass, position);
                ObjectsList.Add(astronaut);
                Players.Add(astronaut);
            }
        }

        protected virtual void LoadPortals(XElement xmlObjects)
        {
            foreach (XElement portalNode in xmlObjects.Elements("portal"))
            {
                Vector3 position = 3 * LoadXYZ(portalNode.Elements("position").First());
                Vector3 scale = LoadXYZ(portalNode.Elements("scale").First());
                Portal portal = new Portal(scale, position);
                ObjectsList.Add(portal);
            }
        }

        protected virtual void LoadCrystals(XElement xmlObjects)
        {
            foreach (XElement crystalNode in xmlObjects.Elements("crystal"))
            {
                float mass = 100 * LoadFloat(crystalNode, "mass");
                float charge = LoadFloat(crystalNode, "charge");
                bool intertial = LoadBool(crystalNode, "inertial");
                Vector3 position = 3 * LoadXYZ(crystalNode.Elements("position").First());
                Vector3 velocity = 3 * LoadXYZ(crystalNode.Elements("velocity").First());
                Vector3 scale = LoadXYZ(crystalNode.Elements("scale").First());
                bool isDivisible = LoadBool(crystalNode, "divisible");

                Crystal crystal = new Crystal(scale, mass, position, velocity, intertial, charge, true, isDivisible);
                ObjectsList.Add(crystal);
            }
        }

        protected virtual void LoadAsteroids(XElement xmlObjects)
        {
            foreach (XElement asteroidNode in xmlObjects.Elements("asteroid"))
            {
                float mass = LoadFloat(asteroidNode, "mass");
                bool intertial = LoadBool(asteroidNode, "inertial");
                Vector3 position = LoadXYZ(asteroidNode.Elements("position").First());
                position = 3 * position;
                Vector3 velocity = 3 * LoadXYZ(asteroidNode.Elements("velocity").First());
                Vector3 scale = LoadXYZ(asteroidNode.Elements("scale").First());

                Asteroid asteroid = new Asteroid(scale, mass, position, velocity, intertial);

                asteroid.isDestroyable = LoadBool(asteroidNode, "destroyable");
                asteroid.isGravitational = LoadBool(asteroidNode, "gravitational");
                asteroid.isSpawnable = LoadBool(asteroidNode, "spawnable");
                if (asteroid.isSpawnable)
                    asteroid.spawnPosition = 3 * LoadXYZ(asteroidNode.Elements("spawnposition").First());
                if (asteroid.isGravitational)
                    asteroid.sunPosition = 3 * LoadXYZ(asteroidNode.Elements("sunposition").First());
                ObjectsList.Add(asteroid);
            }
        }

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
        

    }
}
