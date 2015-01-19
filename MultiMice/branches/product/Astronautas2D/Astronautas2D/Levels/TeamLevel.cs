using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework;
using Astronautas2D.GameObjects;
using System.Xml.Linq;
using Astronautas2D.GameObjects.Entities;
using Astronautas2D.Factories;
using Astronautas2D.Utils;
using System.Linq;
using GameModel;
using Astronautas2D.Visual_Components;
using Astronautas2D;
using Astronautas2D.GUI;
using Astronautas2D.GUI.GUIElements;
using Astronautas2D.Objectives;

namespace Astronautas2D.Levels
{
    public enum levelState {created ,loading, appearing, playing, paused};

    public class TeamLevel : AstroLib.AbstractLevel
    {
        protected string filepath;
        protected bool loaded;
        public bool Loaded { get { return loaded; } }
        
        public virtual event Action<bool> GroupSubLevelEnded;

        // Fábrica de objetos
        protected GameObjectFactory objectFactory;
        public GameObjectFactory ObjectFactory { set { objectFactory = value;} }
        protected GUIElementFactory guiFactory;
        public GUIElementFactory GUIFactory { set { guiFactory = value; } }
        protected PlayerManager playerManager;
        public PlayerManager PlayerManager { get { return playerManager; } set { playerManager = value; } }
        protected GUIManager guiManager;
        public GUIManager GuiManager { get { return guiManager; } set { guiManager = value; } }
        protected TimeManager timeManager;
        public TimeManager TimeManager { get { return timeManager; } set { timeManager = value; } }
        protected ScoreManager scoreManager;
        public ScoreManager ScoreManager { get { return scoreManager; } set { scoreManager = value; } }
        protected Rectangle boundries;
        public Rectangle Boundries { get { return boundries; } set { boundries = value; } }
        protected MiceManager miceManager;


        public int GroupNumber
        {
            set 
            {
                playerManager.GroupNumber = value;
            }
        }

        // Estado del nivel
        protected levelState currentState;

        // Elementos del nivel en una lista enumerada
        IGameObject[] gameObjectList;

        // Lista de elementos iniciales del nivel
        protected List<IPhysicBody> InitialLevelObjects;

        // Lista de elementos iniciales del subnivel
        protected List<IPhysicBody> InitialSublevelObjects;

        // Lista de elementos agregados del juego
        protected List<IPhysicBody> AddedObjects;

        //Elementos gráficos
        protected Texture2D background;

        //Crystales a romper
        protected List<Crystal2D> shatterList;
 
        // Si se debe reiniciar o no
        protected bool restart = false;
        // Si el nivel tiene o no narrativa
        protected bool narrative;
        // Si es tutorial
        protected bool tutorial;

        // Puntaje acumulado en el nivel
        protected int totalScore;

        // Objetivo del nivel
        protected Objective objective;
        // objetivo del nivel en texto
        protected String objectiveDescription;

        // Posiciones de respawn
        List<Vector3> respawnPositions;

        // Lista de crystales
        protected List<Crystal2D> crystals;
        protected List<Astronaut2D> astronauts;
        protected float chargeValue;
        
        //condiciones del nivel
        protected bool[] conditions;
        // Indice del nivel
        protected int levelIndex;


        // Constructor del GameLevel
        public TeamLevel(int levelIndex, Objective o, bool narrative, bool tutorial, MiceManager mM, GameObjectFactory of, GUIElementFactory gf, Rectangle boundries, bool[] conditions, float chargeValue, bool allAtOnce)
            :base()
        {
            this.levelIndex = levelIndex;
            this.chargeValue = chargeValue;
            this.crystals = new List<Crystal2D>();
            this.astronauts = new List<Astronaut2D>();
            this.miceManager = mM;
            this.loaded = false;
            this.narrative = narrative;
            this.tutorial = tutorial;
            this.objective = o;
            this.currentState = levelState.created;
            this.gameObjectList = null;
            this.InitialLevelObjects = new List<IPhysicBody>();
            this.InitialSublevelObjects = new List<IPhysicBody>();
            this.AddedObjects = new List<IPhysicBody>();
            this.respawnPositions = new List<Vector3>();
            this.totalScore = 0;
            this.conditions = conditions;

            // De todo el juego
            this.objectFactory = of;
            this.guiFactory = gf;
            this.boundries = boundries;
            // Propias del nivel
            this.guiManager = new GUIManager(boundries, guiFactory, narrative);
            this.timeManager = new TimeManager();
            this.scoreManager = new ScoreManager(timeManager, narrative);
            this.playerManager = new PlayerManager(o, mM, guiManager, timeManager, scoreManager, conditions, allAtOnce, -1);
        }

        #region AbstractLevel methods
        // Propiedad para obtener el path del archivo del Level
        public override string File { get { return filepath; } set { filepath = value; } }

        public override void Init()
        {
            this.playerManager.ActivateMouseEvents();
            this.timeManager.Start();
            this.shatterList = new List<Crystal2D>();
            this.currentState = levelState.appearing;
            this.SaveInitialLevelObjects();
            this.objectFactory.initializeRandomPositions(this.respawnPositions);
        }
        public override void Close()
        {
            this.playerManager.DeactivateMouseEvents();
        }
        public override void Update(float elapsedTime)
        {
            if (currentState == levelState.playing)
            {
                if (restart)
                {
                    this.Reset(false);
                    restart = false;
                }

                timeManager.Update(elapsedTime);
                playerManager.Update(elapsedTime);
                guiManager.Update(elapsedTime);

                foreach (Crystal2D c in crystals)
                {
                    foreach (Astronaut2D a in astronauts)
                    {
                        IPhysicBody[] aux = new IPhysicBody[1];
                        aux[0] = a;
                        Vector3 force = base.physics.CalculateForce(c, aux);
                        bool active = a.IsGlowing;
                        c.changeOtherForce(a.PlayerId, new Vector2(force.X, force.Y), active);
                    }
                    foreach (Crystal2D otherCrystal in crystals)
                    {
                        if (c.ListIndex == otherCrystal.ListIndex && c != otherCrystal)
                        {
                            IPhysicBody[] aux = new IPhysicBody[1];
                            aux[0] = otherCrystal;
                            Vector3 force = base.physics.CalculateForce(c, aux);
                            c.changeOtherForce(playerId.team, new Vector2(force.X, force.Y),true);
                        }
                    }
                }

                foreach (IPhysicBody g in base.ObjectsList)
                {
                    IGameObject aux = (IGameObject)g;

                    if (aux.Alive && !(aux is Astronaut2D) && !(aux is Bullet2D))
                    {
                        aux.Update(elapsedTime);
                    }

                }
                if (shatterList.Count > 0)
                {
                    foreach (Crystal2D c in shatterList)
                    {
                        this.CreateCrystals(c, false);
                    }
                    this.shatterList = new List<Crystal2D>();
                }

                base.physics.Update(elapsedTime, base.ObjectsList);
                UpdateObjectList();
                CleanObjectsList();
            }
            else if (currentState == levelState.appearing)
            {
                currentState = levelState.playing;
            }
        }

        #region AbstractLevel Collisions
        protected override void CreatePhysicsRelations()
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

            PhysicMaterial physicsMaterial4 = new PhysicMaterial();
            physicsMaterial4.MaterialName1 = "crystal";
            physicsMaterial4.MaterialName2 = "astronaut";
            physicsMaterial4.ContactBeginCallback = AstronautCrystalCollision;

            physics.AddPhysicMaterial(physicsMaterial4);

            PhysicMaterial physicsMaterial5 = new PhysicMaterial();
            physicsMaterial5.MaterialName1 = "asteroid";
            physicsMaterial5.MaterialName2 = "astronaut";
            physicsMaterial5.ContactBeginCallback = AstronautAsteroidCollision;

            physics.AddPhysicMaterial(physicsMaterial5);


        }
        protected override void BulletCrystalCollision(GameModel.IPhysicBody physObj1, GameModel.IPhysicBody physObj2)
        {
            IGameObject aux = this.findObjectByType("Astronautas2D.GameObjects.Entities.Crystal2D", physObj1, physObj2);
            IGameObject aux2 = this.findObjectByType("Astronautas2D.GameObjects.Entities.Bullet2D", physObj1, physObj2);
            Crystal2D crystal = (Crystal2D)aux;
            Bullet2D bullet = (Bullet2D)aux2;
            crystal.ShatterDirection = bullet.ShootDirection;
            this.tryToShatter(crystal);
            crystal.Crash(false);
            bullet.Explode();
        }
        protected override void AsteroidCrystalCollision(GameModel.IPhysicBody physObj1, GameModel.IPhysicBody physObj2)
        {
            IGameObject aux = this.findObjectByType("Astronautas2D.GameObjects.Entities.Crystal2D", physObj1, physObj2);
            Crystal2D crystal = (Crystal2D)aux;
            crystal.Crash(false);
            SoundManager.Instance.Play(Sounds.crystalDeath);
            this.GroupSubLevelEnded(false);
        }
        protected override void CrystalPortalCollision(IPhysicBody physObj1, IPhysicBody physObj2)
        {
            if (objective.Type == objectiveType.crystalsAndPortals)
            {
                IGameObject aux = this.findObjectByType("Astronautas2D.GameObjects.Entities.Crystal2D", physObj1, physObj2);
                Crystal2D crystal = (Crystal2D)aux;
                crystal.Disappear(true);
                CrystalsAndPortalsObjective cObjective = (CrystalsAndPortalsObjective)objective;
                cObjective.saveCrystal(crystal);
                int profit = scoreManager.addScore(playerId.team,false);
                this.playerManager.showScoreProfit(playerId.team, profit, crystal.Position2D);
                CrystalsAndPortalsObjective co = (CrystalsAndPortalsObjective)objective;
                co.saveCrystal(crystal);
                SoundManager.Instance.Play(Sounds.crystalIn);
                GroupSubLevelEnded(true);
                foreach (Astronaut2D astro in astronauts)
                {
                    astro.DeactivateCharge();
                }
            }

        }
        #endregion

        #region AbstractLevel Load
        public override void Load(string file)
        {
            this.loaded = true;
            this.currentState = levelState.loading;
            XDocument xmlDoc = XDocument.Load(file);
            XElement xmlScene = xmlDoc.Elements("scene").First();
            XElement xmlPositions = xmlScene.Elements("positions").First();
            XElement xmlObjects = xmlScene.Elements("object_list").First();

            this.LoadAstronauts(xmlObjects);
            this.playerManager.createHUD();
            this.playerManager.createMessageBoard();
            this.LoadAsteroids(xmlObjects);
            this.LoadPortals(xmlObjects);
            this.LoadCrystals(xmlObjects);
            this.LoadRespawnPositions(xmlPositions);
            LoadBackground();
            CreatePhysicsRelations();
            createGameObjectList();
            
        }

 

        protected Vector3 scalePosition(Vector3 Position)
        {
            float x = Position.X * boundries.Width / 1440;
            float y = Position.Y * boundries.Height / 900;

            return new Vector3(x, y, 1);

        }

        protected override void LoadAstronauts(XElement xmlObjects)
        {
            foreach (XElement astronautNode in xmlObjects.Elements("astronaut"))
            {
                float mass = LoadFloat(astronautNode, "mass");
                int id = (int)LoadFloat(astronautNode, "player");
                Vector3 position = LoadXYZ(astronautNode.Elements("position").First());
                position = scalePosition(position);
                Vector3 scale = LoadXYZ(astronautNode.Elements("scale").First());
                this.playerManager.addPlayer(objectFactory, (playerId)id, mass, scale, position, chargeValue);
                Astronaut2D astronaut = playerManager.getAstronaut((playerId)id);
                Bullet2D[] bullets = playerManager.getBullets((playerId)id);
                base.ObjectsList.Add(astronaut);
                this.astronauts.Add(astronaut);
                for (int i = 0; i < bullets.Length; i++)
                {
                    base.ObjectsList.Add(bullets[i]);
                }
                base.Players.Add(astronaut);
            }
        }

        protected override void LoadAsteroids(XElement xmlObjects)
        {
            foreach (XElement asteroidNode in xmlObjects.Elements("asteroid"))
            {
                float mass = LoadFloat(asteroidNode, "mass");
                bool inertial = LoadBool(asteroidNode, "inertial");
                Vector3 position = LoadXYZ(asteroidNode.Elements("position").First());
                position = scalePosition(position);
                Vector3 velocity = LoadXYZ(asteroidNode.Elements("velocity").First());
                Vector3 scale = LoadXYZ(asteroidNode.Elements("scale").First());

                Astronautas2D.GameObjects.Entities.Asteroid2D asteroid = this.objectFactory.createAsteroid(mass, scale, position, velocity, inertial);

                asteroid.isDestroyable = LoadBool(asteroidNode, "destroyable");
                asteroid.isGravitational = LoadBool(asteroidNode, "gravitational");
                asteroid.isSpawnable = LoadBool(asteroidNode, "spawnable");
                if (asteroid.isSpawnable)
                {
                    asteroid.spawnPosition = scalePosition(LoadXYZ(asteroidNode.Elements("spawnposition").First()));
                }
                if (asteroid.isGravitational)
                {
                    asteroid.sunPosition = scalePosition(LoadXYZ(asteroidNode.Elements("sunposition").First()));
                }

                bool useLifeTime = LoadBool(asteroidNode, "lifetime");
                if (useLifeTime)
                {
                    asteroid.LifeTime = LoadFloat(asteroidNode.Elements("lifetime").First(), "value");
                    asteroid.TimetoWait = asteroid.LifeTime;
                    asteroid.InitialWait = LoadFloat(asteroidNode.Elements("lifetime").First(), "wait");
                }
                ObjectsList.Add(asteroid);
            }
        }
        protected override void LoadPortals(XElement xmlObjects)
        {
            foreach (XElement portalNode in xmlObjects.Elements("portal"))
            {
                Vector3 position = LoadXYZ(portalNode.Elements("position").First());
                position = scalePosition(position);
                Vector3 scale = LoadXYZ(portalNode.Elements("scale").First());
                Astronautas2D.GameObjects.Entities.Portal2D portal = objectFactory.createPortal(scale, position, (playerId)playerId.team);
                ObjectsList.Add(portal);
            }
        }
        protected override void LoadCrystals(XElement xmlObjects)
        {
            CrystalsAndPortalsObjective crystalObjective = null;
            if (objective.Type == objectiveType.crystalsAndPortals)
            {
                crystalObjective = (CrystalsAndPortalsObjective)objective;
            }

            foreach (XElement crystalNode in xmlObjects.Elements("crystal"))
            {
                float mass = 100 * LoadFloat(crystalNode, "mass");
                float charge = LoadFloat(crystalNode, "charge");
                bool inertial = LoadBool(crystalNode, "inertial");
                Vector3 position = LoadXYZ(crystalNode.Elements("position").First());
                position = scalePosition(position);
                Vector3 velocity = LoadXYZ(crystalNode.Elements("velocity").First());
                Vector3 scale = LoadXYZ(crystalNode.Elements("scale").First());
                bool isDivisible = LoadBool(crystalNode, "divisible");

                Crystal2D crystal = objectFactory.createCrystal(playerId.team, mass, scale, position, velocity, inertial, charge, true, isDivisible, conditions[(int)levelConditions.showTotalForce], conditions[(int)levelConditions.showIndividualForces]);
                ObjectsList.Add(crystal);
                crystals.Add(crystal);

                if (crystalObjective != null)
                {
                    crystalObjective.addCrystal(crystal);
                }
            }
        }
        #endregion
        #endregion

        #region Level methods
        protected void createGameObjectList()
        {
            gameObjectList = new IGameObject[base.ObjectsList.Count];
            int counter = 0;

            foreach (IPhysicBody g in base.ObjectsList)
            {
                IGameObject aux = (IGameObject)g;
                gameObjectList[counter] = aux;
                counter++;
            }
        }
        protected virtual void CreateCrystals(Crystal2D crystal, bool changeIntensity)
        {
            // Ahora, creamos los 2 nuevos cristales
            crystal.Respawnable = false;
            float mass = crystal.Mass;
            Vector3 scale = new Vector3(crystal.Scale.X / 2, crystal.Scale.Y / 2, 1);
            Vector3 centerPosition = crystal.Position;
            bool isInetial = crystal.IsInertial;
            float charge = crystal.Charge / 8;
            Crystal2D leftCrystal = objectFactory.createCrystal(playerId.team, mass, scale, new Vector3(centerPosition.X - 30, centerPosition.Y, 1), new Vector3(0, 0, 0), isInetial, charge, true, false, conditions[(int)levelConditions.showTotalForce], conditions[(int)levelConditions.showIndividualForces]);
            Crystal2D rightCrystal = objectFactory.createCrystal(playerId.team, mass, scale, new Vector3(centerPosition.X + 30, centerPosition.Y, 1), new Vector3(0, 0, 0), isInetial, charge, true, false, conditions[(int)levelConditions.showTotalForce], conditions[(int)levelConditions.showIndividualForces]);
            this.ObjectsList.Add(leftCrystal);
            this.ObjectsList.Add(rightCrystal);
            // los agregamos a la lista de crystales
            this.crystals.Add(leftCrystal);
            this.crystals.Add(rightCrystal);
        }
        private void SaveInitialLevelObjects()
        {
            foreach (IPhysicBody g in base.ObjectsList)
            {
                InitialLevelObjects.Add(g);
            }
        }
        protected void LoadBackground()
        {
            background = objectFactory.getBackground(tutorial ? backgrounds.tutorial : backgrounds.mission);
        }
        protected virtual void LoadRespawnPositions(XElement xmlObjects)
        {
            foreach (XElement respawnNode in xmlObjects.Elements("respawn"))
            {
                Vector3 position = LoadXYZ(respawnNode);
                position = scalePosition(position);
                respawnPositions.Add(position);
            }
        }
        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch, Writer fontWriter)
        {
            spriteBatch.Draw(this.background, new Vector2(0, 0), new Rectangle(0, 0, 1440, 900), Color.White, 0f, new Vector2(0, 0), new Vector2((float)(boundries.Width) / 1440f, (float)(boundries.Height) / 900f), SpriteEffects.None, 0f);
            //spriteBatch.Draw(this.background, new Vector2(0,0), Color.White);
            //(float)(boundries.Width / 1440), (float)(boundries.Height / 900
            foreach (IPhysicBody g in base.ObjectsList)
            {
                IGameObject aux = (IGameObject)g;

                if (aux.IsVisible)
                {
                    if (!(aux is Astronaut2D) && !(aux is Bullet2D) && !(aux is Crystal2D))
                    {
                        aux.Draw(gameTime, spriteBatch, fontWriter);
                    }
                }
            }
            playerManager.Draw(gameTime, spriteBatch, fontWriter);
            foreach (Crystal2D c in crystals)
            {
                if (c.IsVisible) c.Draw(gameTime, spriteBatch, fontWriter);
            }
            guiManager.Draw(gameTime, spriteBatch, fontWriter);
        }
        protected virtual void UpdateObjectList()
        {
            foreach (IPhysicBody g in base.ObjectsList)
            {
                IGameObject aux = (IGameObject)g;
                if (aux is Crystal2D)
                {
                    Crystal2D c = (Crystal2D)aux;

                    if (c.Randomize)
                    {
                        objectFactory.RandomizeCrystal(c, true);
                    }

                    if (aux.CurrentState == state.idle && !CheckInBoundries(c.Position2D))
                    {
                        if (c.Father != null)
                        {
                            SoundManager.Instance.Play(Sounds.crystalDissapear);
                            c.Father.crashSons(false);
                        }
                        else
                        {
                            SoundManager.Instance.Play(Sounds.crystalDissapear);
                            c.Crash(false);
                        }
                    }
                }
                else if (aux is Astronaut2D)
                {
                    Astronaut2D a = (Astronaut2D)aux;

                    if (aux.CurrentState == state.dead)
                    {
                        a.Reset();
                    }
                }
            }

        }

        protected IGameObject findObjectByType(string type, IPhysicBody physObj1, IPhysicBody physObj2)
        {
            Type searchedType = Type.GetType(type);

            if (searchedType == null)
            {
                return null;
            }

            IGameObject aux1 = (IGameObject)physObj1;
            IGameObject aux2 = (IGameObject)physObj2;

            if (aux1.GetType() == searchedType)
            {
                return aux1;
            }
            else if (aux2.GetType() == searchedType)
            {
                return aux2;
            }
            else
            {
                return null;
            }
        }
        protected bool CheckInBoundries(Vector2 position)
        {
            Point p = new Point((int)position.X, (int)position.Y);
            return boundries.Contains(p);
        }
        protected virtual void CleanObjectsList()
        {
            List<IPhysicBody> toRemoveList = new List<IPhysicBody>();

            foreach (IPhysicBody g in base.ObjectsList)
            {
                IGameObject aux = (IGameObject)g;
                if (aux.CurrentState == state.dead)
                {
                    toRemoveList.Add(g);
                }
            }

            if (toRemoveList.Count > 0)
            {
                foreach (IPhysicBody g in toRemoveList)
                {
                    base.ObjectsList.Remove(g);
                    if (g is Crystal2D)
                    {
                        Crystal2D c = (Crystal2D)g;
                        this.crystals.Remove(c);
                    }
                }
            }

        }
        public virtual void Reset(bool restartTime)
        {
            ObjectsList.Clear();
            totalScore = 0;

            foreach (IPhysicBody g in InitialLevelObjects)
            {
                IGameObject aux = (IGameObject)g;
                aux.Reset();
                ObjectsList.Add(g);

            }

            if (restartTime)
            {
                timeManager.Reset();
                timeManager.Start();
            }

        }
        public virtual void startSubLevel()
        {
            // Mantenemos solo los objetos iniciales
            ObjectsList.Clear();
            foreach (IPhysicBody g in InitialLevelObjects)
            {
                IGameObject aux = (IGameObject)g;
                ObjectsList.Add(g);
            }

        }
        protected bool tryToShatter(Crystal2D crystal)
        {
            if (!crystal.IsDivisible)
            {
                return false;
            }
            shatterList.Add(crystal);
            return true;
        }
        protected virtual void AstronautCrystalCollision(IPhysicBody physObj1, IPhysicBody physObj2)
        {
            IGameObject aux = this.findObjectByType("Astronautas2D.GameObjects.Entities.Crystal2D", physObj1, physObj2);
            Crystal2D crystal = (Crystal2D)aux;
            IGameObject aux2 = this.findObjectByType("Astronautas2D.GameObjects.Entities.Astronaut2D", physObj1, physObj2);
            Astronaut2D astronaut = (Astronaut2D)aux2;
            SoundManager.Instance.Play(Sounds.crystalDeath);
            crystal.Crash(false);
            foreach (Astronaut2D a in astronauts)
            {
                a.DeactivateCharge();
            }
            this.GroupSubLevelEnded(false);
        }
        protected virtual void AstronautAsteroidCollision(IPhysicBody physObj1, IPhysicBody physObj2)
        {
            IGameObject aux = this.findObjectByType("Astronautas2D.GameObjects.Entities.Astronaut2D", physObj1, physObj2);
            Astronaut2D astronaut = (Astronaut2D)aux;
            if (astronaut.CurrentState == state.idle || astronaut.CurrentState == state.moving)
            {
                astronaut.Die();
                this.GroupSubLevelEnded(false);
            }
        }
        
        #endregion 
    }
}
