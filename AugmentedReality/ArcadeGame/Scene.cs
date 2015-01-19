using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Globalization;
using Microsoft.Xna.Framework;

namespace ArcadeGame
{
    class Scene
    {
        enum Text { BACKGROUND, ASTEROID };
        public CultureInfo CultureInfo { get; set; }    
     
        private List<GameObject> objects;
        public List<GameObject> Objects { get { return objects; } }
        private List<SceneAstronaut> astronauts;
        public List<SceneAstronaut> Astronauts { get { return astronauts; } }       

        
        private int width, height;
        public Scene(int width, int height)
        {
            objects = new List<GameObject>();
            astronauts = new List<SceneAstronaut>();
            CultureInfo = new CultureInfo("en-US");
            this.width = width;
            this.height = height;
        }

        public void AddObject(GameObject obj)
        {
            objects.Add(obj);
        }

        public void Load(string file)
        {
            XDocument xmlDoc = XDocument.Load(file);
            XElement xmlScene = xmlDoc.Elements("scene").First();
            
            XElement xmlObjects = xmlScene.Elements("object_list").First();
            foreach (XElement asteroidNode in xmlObjects.Elements("asteroid"))
            {
                float mass = LoadFloat(asteroidNode, "mass");
                bool intertial = LoadBool(asteroidNode, "inertial");
                Vector3 position = LoadXYZ(asteroidNode.Elements("position").First());
                position = new Vector3(width / 2, height / 2, 0) + 18*position;
                Vector3 velocity = 40*LoadXYZ(asteroidNode.Elements("velocity").First());
                Vector3 scale = LoadXYZ(asteroidNode.Elements("scale").First());

                SceneAsteroid asteroid = new SceneAsteroid(mass,position,velocity, intertial);
                asteroid.isAlive = true;
                asteroid.Scale = new Vector2(scale.X / 1.75f, scale.Y / 1.75f);
                asteroid.isDestroyable = LoadBool(asteroidNode, "destroyable");
                asteroid.isGravitational = LoadBool(asteroidNode, "gravitational");
                asteroid.isSpawnable = LoadBool(asteroidNode, "spawnable");
                asteroid.spawnPosition = new Vector3(width / 2, height / 2, 0) + 18 * LoadXYZ(asteroidNode.Elements("spawnposition").First());
                if(asteroid.isGravitational)
                    asteroid.sunPosition = asteroid.spawnPosition = new Vector3(width / 2, height / 2, 0) + 18 * LoadXYZ(asteroidNode.Elements("sunposition").First());
                asteroid.SpriteId = (int)ArcadeGame.Game1.Text.ASTEROID;
                objects.Add(asteroid);             
            }
            foreach (XElement crystalNode in xmlObjects.Elements("crystal"))
            {
                float mass = LoadFloat(crystalNode, "mass");
                float charge = LoadFloat(crystalNode, "charge");
                bool intertial = LoadBool(crystalNode, "inertial");
                Vector3 position = LoadXYZ(crystalNode.Elements("position").First());
                position = new Vector3(width / 2, height / 2, 0) + 18 * position;
                Vector3 velocity = 40 * LoadXYZ(crystalNode.Elements("velocity").First());
                Vector3 scale = LoadXYZ(crystalNode.Elements("scale").First());

                SceneCrystal crystal = new SceneCrystal(mass, position, velocity, intertial, charge);
                crystal.isAlive = true;
                crystal.Scale = new Vector2(scale.X / 1.75f, scale.Y / 1.75f);
                crystal.isDivisible = LoadBool(crystalNode, "divisible");                
                crystal.SpriteId = (int)ArcadeGame.Game1.Text.CRYSTAL;
                objects.Add(crystal);
            }
            foreach (XElement portalNode in xmlObjects.Elements("portal"))
            {
                Vector3 position = LoadXYZ(portalNode.Elements("position").First());
                position = new Vector3(width / 2, height / 2, 0) + 18 * position;
                
                Vector3 scale = LoadXYZ(portalNode.Elements("scale").First());

                ScenePortal portal = new ScenePortal(position);               
                portal.Scale = new Vector2(scale.X / 1.75f, scale.Y / 1.75f);                
                portal.SpriteId = (int)ArcadeGame.Game1.Text.PORTAL;
                objects.Add(portal);
            }
            foreach (XElement astronautNode in xmlObjects.Elements("astronaut"))
            {
                float mass = LoadFloat(astronautNode, "mass");
                Vector3 position = LoadXYZ(astronautNode.Elements("position").First());
                position = new Vector3(width / 2, height / 2, 0) + 18 * position;
                Vector3 scale = LoadXYZ(astronautNode.Elements("scale").First());

                SceneAstronaut astronaut = new SceneAstronaut(mass, position);
                astronaut.Scale = new Vector2(scale.X / 1.75f, scale.Y / 1.75f);
                astronaut.SpriteId = (int)ArcadeGame.Game1.Text.ASTRONAUT;
                objects.Add(astronaut);
                astronauts.Add(astronaut);
            }    
        }
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

       
    }
}
