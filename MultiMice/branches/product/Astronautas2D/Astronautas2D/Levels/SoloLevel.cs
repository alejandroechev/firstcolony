using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
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
using AstroLib;
using Astronautas2D.Utils;

namespace Astronautas2D.Levels
{
    public class SoloLevel : TeamLevel
    {
        private List<Zone2D> ZoneList;
        private List<IPhysicBody> ObjectsList1, ObjectsList2, ObjectsList3;
        public virtual event Action<bool, int> IndividualSubLevelEnded;

        public SoloLevel(int levelIndex, Objective o, bool narrative, bool tutorial, MiceManager mM, GameObjectFactory of, GUIElementFactory gf, Rectangle boundries, bool[] conditions, float chargeValue, bool allAtOnce)
            : base(levelIndex, o, narrative, tutorial, mM, of, gf, boundries, conditions, chargeValue, allAtOnce)
        {
            ZoneList = new List<Zone2D>();
            ObjectsList1 = new List<IPhysicBody>();
            ObjectsList2 = new List<IPhysicBody>();
            ObjectsList3 = new List<IPhysicBody>();
        }

        public override void Init()
        {
            base.Init();
            if (objective.Type == objectiveType.activateForce || objective.Type == objectiveType.crystals) miceManager.CenterClick += onCenterClick;
        }

        public override void Update(float elapsedTime)
        {
            if (currentState == levelState.playing)
            {
                timeManager.Update(elapsedTime);
                playerManager.Update(elapsedTime);
                guiManager.Update(elapsedTime);

                foreach (Crystal2D c in crystals)
                {
                    foreach (Astronaut2D a in astronauts)
                    {
                        if ((playerId)c.ListIndex == a.PlayerId)
                        {
                            IPhysicBody[] aux = new IPhysicBody[1];
                            aux[0] = a;
                            Vector3 force = base.physics.CalculateForce(c, aux);
                            bool active = a.IsGlowing;
                            c.changeOtherForce(a.PlayerId, new Vector2(force.X, force.Y), active);
                        }
                    }
                    foreach(Crystal2D otherCrystal in base.crystals)
                    {
                        if (c.ListIndex == otherCrystal.ListIndex && c != otherCrystal)
                        {
                            IPhysicBody[] aux = new IPhysicBody[1];
                            aux[0] = otherCrystal;
                            Vector3 force = base.physics.CalculateForce(c, aux);
                            c.changeOtherForce(playerId.team, new Vector2(force.X, force.Y), true);
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
                    if (aux.Alive && (aux is Astronaut2D))
                    {
                        Astronaut2D astronaut = (Astronaut2D)aux;
                        foreach (Zone2D zone in ZoneList)
                        {
                            if (zone.checkAstronaut(astronaut) && objective.Type == objectiveType.move)
                            {
                                ZoneObjective zo = (ZoneObjective)objective;
                                // Aqui deberiamos actualizar el objetivo y mover la zona
                                this.objectFactory.freePosition(zone.Position);
                                Vector2 newPos = this.objectFactory.getRandomPosition();
                                int profit = scoreManager.addScore(zone.PlayerIndex, true);
                                timeManager.setFlag(zone.PlayerIndex);
                                this.playerManager.showScoreProfit(zone.PlayerIndex, profit, astronaut.Position2D);

                                IndividualSubLevelEnded(true, (int) zone.PlayerIndex);
                                zone.Move(newPos);
                                zo.catchZone(zone);
                            }
                        }
                    }
                }
                if (shatterList.Count > 0)
                {
                    foreach (Crystal2D c in shatterList)
                    {
                        this.CreateCrystals(c, true);
                    }
                    this.shatterList = new List<Crystal2D>();
                }
                base.physics.Update(elapsedTime, ObjectsList1);
                base.physics.Update(elapsedTime, ObjectsList2);
                base.physics.Update(elapsedTime, ObjectsList3);
                base.UpdateObjectList();

                //Configuration.Instance.
                /*
                if (objective is CrystalObjective)
                {
                    CrystalObjective co = (CrystalObjective)objective;
                    List<Astronaut2D> winners = co.UpdateObjective();

                    if (winners.Count >0)
                    {
                        foreach (Astronaut2D astro in winners)
                        {
                            playerId id = astro.PlayerId;
                            this.IndividualSubLevelEnded(true, (int)id);
                            int profit = scoreManager.addScore(id);
                            timeManager.setFlag(id);
                            this.playerManager.showScoreProfit(profit, astro.Position2D);

                        }
                    }

                }
                 * */
                CleanObjectsList();


            }
            else if (currentState == levelState.appearing)
            {
                currentState = levelState.playing;
            }
        }

        protected override void UpdateObjectList()
        {
            foreach (IPhysicBody g in base.ObjectsList)
            {
                IGameObject aux = (IGameObject)g;
                if (aux is Crystal2D)
                {
                    Crystal2D c = (Crystal2D)aux;

                    if (c.Randomize)
                    {
                        objectFactory.RandomizeCrystal(c, false);
                    }

                    if (aux.CurrentState == state.idle && !base.CheckInBoundries(c.Position2D))
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

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch, Writer fontWriter)
        {
            //spriteBatch.Draw(this.background, new Vector2((float)(base.boundries.Width / 1440), (float)(base.boundries.Height / 900)), Color.White);
            spriteBatch.Draw(this.background, new Vector2(0, 0), new Rectangle(0,0,1440,900), Color.White, 0f, new Vector2(0, 0), new Vector2((float)(boundries.Width) / 1440f, (float)(boundries.Height) / 900f), SpriteEffects.None, 0f);
            foreach(Zone2D z in ZoneList)
            {
                z.Draw(gameTime, spriteBatch);
            }
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

        protected override void CleanObjectsList()
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
                    IGameObject aux = (IGameObject)g;
                    base.ObjectsList.Remove(g);
                    switch (aux.ListIndex)
                    {
                        case 0:
                            this.ObjectsList1.Remove(g);
                            break;
                        case 1:
                            this.ObjectsList2.Remove(g);
                            break;
                        case 2:
                            this.ObjectsList3.Remove(g);
                            break;
                        default:
                            this.ObjectsList1.Remove(g);
                            break;
                    }

                    if (g is Crystal2D)
                    {
                        Crystal2D c = (Crystal2D)g;
                        base.crystals.Remove(c);

                        if (c.Father != null)
                        {
                            c.Father.removeSon(c);
                            if (objective.Type == objectiveType.shootAndPortals)
                            {
                                // En este caso si muere un hijo, deberían morir ambos
                                if (c.Father.SonsSaved == 2 && !c.Father.IsShattered)
                                {
                                    base.ObjectsList.Add(c.Father);
                                    base.crystals.Add(c.Father);
                                    this.loadOnList(c.Father.ListIndex, c.Father);
                                    c.Father.SonsSaved = 0;
                                }
                                else if (c.Father.SonsAlive == 0 && c.Father.IsShattered)
                                {
                                    c.Father.Respawnable = true;
                                    c.Father.IsShattered = false;
                                    c.Father.Disappear(false);
                                    base.crystals.Add(c.Father);
                                    base.ObjectsList.Add(c.Father);
                                    this.loadOnList(c.Father.ListIndex, c.Father);
                                }

                            }
                            else if (objective.Type == objectiveType.shoot)
                            {
                                if (c.Father.SonsAlive == 0 && c.Father.IsShattered)
                                {
                                    c.Father.Respawnable = true;
                                    c.Father.IsShattered = false;
                                    c.Father.Disappear(true);
                                    base.crystals.Add(c.Father);
                                    base.ObjectsList.Add(c.Father);
                                    this.loadOnList(c.Father.ListIndex, c.Father);
                                }
                            }
                        }

                    }
                }
            }

        }

        public override void Load(string file)
        {
            base.loaded = true;
            this.currentState = levelState.loading;
            XDocument xmlDoc = XDocument.Load(file);
            XElement xmlScene = xmlDoc.Elements("scene").First();
            XElement xmlObjects = xmlScene.Elements("object_list").First();
            XElement xmlPositions = xmlScene.Elements("positions").First();

            this.LoadAstronauts(xmlObjects);
            this.playerManager.TeamMode = false;
            this.playerManager.createHUD();
            this.playerManager.createMessageBoard();
            this.LoadAsteroids(xmlObjects);
            this.LoadPortals(xmlObjects);
            this.LoadZones(xmlObjects);
            this.LoadCrystals(xmlObjects);
            this.LoadRespawnPositions(xmlPositions);
            LoadBackground();
            CreatePhysicsRelations();
            createGameObjectList();
        }

        protected override void CreateCrystals(Crystal2D crystal, bool changeIntensity)
        {
            // Obtenemos la direccion:
            Vector2 direction = new Vector2(-crystal.ShatterDirection.Y, crystal.ShatterDirection.X);
            direction.Normalize();

            // Ahora, creamos los 2 nuevos cristales
            crystal.Respawnable = false;
            float mass = crystal.Mass;
            Vector3 scale = new Vector3(crystal.Scale.X * 3 / 4, crystal.Scale.Y * 3 / 4, 1);
            Vector3 centerPosition = crystal.Position;
            bool isInetial = crystal.IsInertial;
            // Aqui asignamos la carga y creamos los portales
            Crystal2D leftCrystal, rightCrystal; 
            if (changeIntensity)
            {
                Random r = new Random();
                int aux = 1 + r.Next(9);

                float bigCharge = crystal.Charge / 3;
                float smallCharge = crystal.Charge / 5;
                float rightCharge, leftCharge;
                ColorIntensity rightIntensity, leftIntensity;

                if (aux > 5)
                {
                    rightCharge = bigCharge;
                    rightIntensity = ColorIntensity.medium;
                    leftCharge = smallCharge;
                    leftIntensity = ColorIntensity.low;
                }
                else
                {
                    rightCharge = smallCharge;
                    rightIntensity = ColorIntensity.low;
                    leftCharge = bigCharge;
                    leftIntensity = ColorIntensity.medium;
                }


                Vector2 leftPosition = new Vector2(centerPosition.X + 30 * direction.X, centerPosition.Y + 30 * direction.Y);
                leftCrystal = objectFactory.createCrystal((playerId)crystal.ListIndex, mass, scale, new Vector3(leftPosition.X, leftPosition.Y, 1), new Vector3(0, 0, 0), isInetial, leftCharge, true, false, conditions[(int)levelConditions.showTotalForce], conditions[(int)levelConditions.showIndividualForces]);
                leftCrystal.Father = crystal;
                leftCrystal.Intensity = leftIntensity;
                crystal.addSon(leftCrystal);
                Vector2 rightPosition = new Vector2(centerPosition.X - 30 * direction.X, centerPosition.Y - 30 * direction.Y);
                rightCrystal = objectFactory.createCrystal((playerId)crystal.ListIndex, mass, scale, new Vector3(rightPosition.X, rightPosition.Y, 1), new Vector3(0, 0, 0), isInetial, rightCharge, true, false, conditions[(int)levelConditions.showTotalForce], conditions[(int)levelConditions.showIndividualForces]);
                rightCrystal.Father = crystal;
                rightCrystal.Intensity = rightIntensity;
                crystal.addSon(rightCrystal);


            }
            else
            {
                float charge = crystal.Charge / 5;
                Vector2 leftPosition = new Vector2(centerPosition.X + 30 * direction.X, centerPosition.Y + 30 * direction.Y);
                leftCrystal = objectFactory.createCrystal((playerId)crystal.ListIndex, mass, scale, new Vector3(leftPosition.X, leftPosition.Y, 1), new Vector3(0, 0, 0), isInetial, charge, true, false, conditions[(int)levelConditions.showTotalForce], conditions[(int)levelConditions.showIndividualForces]);
                leftCrystal.Father = crystal;
                crystal.addSon(leftCrystal);
                Vector2 rightPosition = new Vector2(centerPosition.X - 30 * direction.X, centerPosition.Y - 30 * direction.Y);
                rightCrystal = objectFactory.createCrystal((playerId)crystal.ListIndex, mass, scale, new Vector3(rightPosition.X, rightPosition.Y, 1), new Vector3(0, 0, 0), isInetial, charge, true, false, conditions[(int)levelConditions.showTotalForce], conditions[(int)levelConditions.showIndividualForces]);
                rightCrystal.Father = crystal;
                crystal.addSon(rightCrystal);

            }

            if (objective.Type == objectiveType.shootAndPortals)
            {
                crystal.IsShattered = true;
                crystal.SonsSaved = 0;
                leftCrystal.Respawnable = false;
                rightCrystal.Respawnable = false;
            }
            else if (objective.Type == objectiveType.shoot)
            {
                crystal.IsShattered = true;
                crystal.SonsSaved = 0;
                leftCrystal.startTimer();
                rightCrystal.startTimer();
                leftCrystal.Respawnable = false;
                rightCrystal.Respawnable = false;
            }
            // Los agregamos a la lista de actualización
            this.ObjectsList.Add(leftCrystal);
            this.ObjectsList.Add(rightCrystal);
            // Ahora a las listas de fisica
            this.loadOnList(crystal.ListIndex, leftCrystal);
            this.loadOnList(crystal.ListIndex, rightCrystal);
            // los agregamos a la lista de crystales
            this.crystals.Add(leftCrystal);
            this.crystals.Add(rightCrystal);

        }

        protected override void LoadAstronauts(XElement xmlObjects)
        {
            ActivateForceObjective afo = null;
            if (objective.Type == objectiveType.activateForce)
            {
                afo = (ActivateForceObjective)objective;
            }
            CrystalObjective co = null;
            if (objective.Type == objectiveType.crystals)
            {
                co = (CrystalObjective)objective;
            }

            foreach (XElement astronautNode in xmlObjects.Elements("astronaut"))
            {
                float mass = LoadFloat(astronautNode, "mass");
                int id = (int)LoadFloat(astronautNode, "player");
                Vector3 position = LoadXYZ(astronautNode.Elements("position").First());
                position = scalePosition(position);
                Vector3 scale = LoadXYZ(astronautNode.Elements("scale").First());
                playerManager.addPlayer(objectFactory, (playerId)id, mass, scale, position, chargeValue);
                Astronaut2D astronaut = playerManager.getAstronaut((playerId)id);
                astronaut.ListIndex = id;
                Bullet2D[] bullets = playerManager.getBullets((playerId)id);
                // Se agregan a la lista de dibujo y update
                base.ObjectsList.Add(astronaut);
                this.astronauts.Add(astronaut);
                this.loadOnList(id, astronaut);
                for (int i = 0; i < bullets.Length; i++)
                {
                    bullets[i].ListIndex = id;
                    base.ObjectsList.Add(bullets[i]);
                    this.loadOnList(id, bullets[i]);
                }
                base.Players.Add(astronaut);

                if (afo != null)
                {
                    afo.addAstronaut(astronaut);
                }
                if (co != null)
                {
                    co.addAstronaut(astronaut);
                }
            }
        }

        protected override void LoadAsteroids(XElement xmlObjects)
        {
            foreach (XElement asteroidNode in xmlObjects.Elements("asteroid"))
            {
                float mass = LoadFloat(asteroidNode, "mass");
                int id = (int)LoadFloat(asteroidNode, "player");
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


                asteroid.ListIndex = id;
                ObjectsList.Add(asteroid);

                playerId playerId = (playerId)id;
                if(playerId == playerId.team)
                {
                    this.loadOnList((int)playerId.circle, asteroid);
                    this.loadOnList((int)playerId.square, asteroid);
                    this.loadOnList((int)playerId.triangle, asteroid);
                }
                else
                {
                    this.loadOnList(id, asteroid);
                }
            }
        }

        protected override void LoadPortals(XElement xmlObjects)
        {
            foreach (XElement portalNode in xmlObjects.Elements("portal"))
            {
                int id = (int)LoadFloat(portalNode, "player");
                Vector3 position = LoadXYZ(portalNode.Elements("position").First());
                position = scalePosition(position);
                Vector3 scale = LoadXYZ(portalNode.Elements("scale").First());
                Astronautas2D.GameObjects.Entities.Portal2D portal = objectFactory.createPortal(scale, position, (playerId)id);
                portal.ListIndex = id;
                ObjectsList.Add(portal);
                this.loadOnList(id, portal);
            }
        }

        protected override void LoadCrystals(XElement xmlObjects)
        {
            CrystalsAndPortalsObjective crystalObjective = null;
            if (objective.Type == objectiveType.crystalsAndPortals)
            {
                crystalObjective = (CrystalsAndPortalsObjective)objective;
            }
            ShootAndPortalsObjective bco = null;
            if (objective.Type == objectiveType.shootAndPortals)
            {
                bco = (ShootAndPortalsObjective)objective;
            }
            ActivateForceObjective afo = null;
            if (objective.Type == objectiveType.activateForce)
            {
                afo = (ActivateForceObjective)objective;
            }
            CrystalObjective co = null;
            if(objective.Type == objectiveType.crystals)
            {
                co = (CrystalObjective) objective;
            }

            foreach (XElement crystalNode in xmlObjects.Elements("crystal"))
            {
                float mass = 100 * LoadFloat(crystalNode, "mass");
                int id = (int)LoadFloat(crystalNode, "player");
                float charge = LoadFloat(crystalNode, "charge");
                bool inertial = LoadBool(crystalNode, "inertial");
                Vector3 position = LoadXYZ(crystalNode.Elements("position").First());
                position = scalePosition(position);
                Vector3 velocity = LoadXYZ(crystalNode.Elements("velocity").First());
                Vector3 scale = LoadXYZ(crystalNode.Elements("scale").First());
                bool isDivisible = LoadBool(crystalNode, "divisible");

                Astronautas2D.GameObjects.Entities.Crystal2D crystal = objectFactory.createCrystal((playerId)id, mass, scale, position, velocity, inertial, charge, true, isDivisible, conditions[(int)levelConditions.showTotalForce], conditions[(int)levelConditions.showIndividualForces]);
                ObjectsList.Add(crystal);
                base.crystals.Add(crystal);
                this.loadOnList(id, crystal);

                if (crystalObjective != null)
                {
                    crystalObjective.addCrystal(crystal);
                }
                if (bco != null)
                {
                    bco.addCrystal(crystal);
                }
                if (afo != null)
                {
                    afo.addCrystal(crystal);
                }
                if (co != null)
                {
                    co.addCrystals(crystal);
                }
            }
        }

        public override void Reset(bool restartTime)
        {
            ObjectsList.Clear();
            ObjectsList1.Clear();
            ObjectsList2.Clear();
            ObjectsList3.Clear();
            foreach (IPhysicBody g in InitialLevelObjects)
            {
                IGameObject aux = (IGameObject)g;
                aux.Reset();
                ObjectsList.Add(g);
                this.loadOnList(aux.ListIndex, g);
            }

            if (restartTime)
            {
                timeManager.Reset();
                timeManager.Start();
            }

        }

        #region collisions
        protected override void CrystalPortalCollision(IPhysicBody physObj1, IPhysicBody physObj2)
        {
            if (objective.Type == objectiveType.crystalsAndPortals)
            {
                IGameObject aux = this.findObjectByType("Astronautas2D.GameObjects.Entities.Crystal2D", physObj1, physObj2);
                Crystal2D crystal = (Crystal2D)aux;
                crystal.Disappear(true);
                playerId id = (playerId)crystal.ListIndex;
                // Se completó un subnivel para uno de los jugadores
                this.IndividualSubLevelEnded(true, (int)id);
                int profit = scoreManager.addScore(id, true);
                timeManager.setFlag(id);
                this.playerManager.showScoreProfit(id, profit, crystal.Position2D);
                CrystalsAndPortalsObjective co = (CrystalsAndPortalsObjective)objective;
                SoundManager.Instance.Play(Sounds.crystalIn);
                co.saveCrystal(crystal);

                foreach (Astronaut2D a in astronauts)
                {
                    if (((int)a.PlayerId == crystal.ListIndex) && a.IsGlowing)
                    {
                        a.DeactivateCharge();
                    }
                }

            }
            else if (objective.Type == objectiveType.shootAndPortals)
            {
                IGameObject aux = this.findObjectByType("Astronautas2D.GameObjects.Entities.Crystal2D", physObj1, physObj2);
                Crystal2D crystal = (Crystal2D)aux;
                ShootAndPortalsObjective breakCrystalObjective = (ShootAndPortalsObjective) objective;

                if (breakCrystalObjective.checkFather(crystal))
                {
                    crystal.Die();
                    crystal.Father.SonsSaved++;

                    if (crystal.Father.IsShattered && crystal.Father.SonsSaved == 2)
                    {
                        crystal.Father.Disappear(true);
                        crystal.Father.Respawnable = true;
                        crystal.Father.IsShattered = false;
                        breakCrystalObjective.saveCrystal(crystal);
                        this.IndividualSubLevelEnded(true, crystal.ListIndex);
                        int profit = scoreManager.addScore((playerId)crystal.ListIndex,true);
                        timeManager.setFlag((playerId)crystal.ListIndex);
                        this.playerManager.showScoreProfit((playerId)crystal.ListIndex,profit, crystal.Position2D);
                    }
                }
                else
                {
                    crystal.Disappear(false);
                    crystal.Respawnable = true;
                    crystal.IsShattered = false;
                    this.IndividualSubLevelEnded(false, crystal.ListIndex);
                    int profit = 0;
                    if(!base.narrative)
                    this.playerManager.showScoreProfit((playerId)crystal.ListIndex, profit, crystal.Position2D);
                }
            }

        }

        #endregion

        private void LoadZones(XElement xmlObjects)
        {
            foreach (XElement zoneNode in xmlObjects.Elements("zone"))
            {
                int id = (int)LoadFloat(zoneNode, "player");
                Vector3 scale = LoadXYZ(zoneNode.Elements("scale").First());
                Vector3 position = LoadXYZ(zoneNode.Elements("position").First());
                position = scalePosition(position);
                Zone2D zone = this.objectFactory.createZone(new Vector2(scale.X,scale.Y), new Vector2(position.X, position.Y), id);
                ZoneList.Add(zone);
                if (objective.Type == objectiveType.move)
                {
                    ZoneObjective zo = (ZoneObjective)objective;
                    zo.addZone(zone);
                }
            }
        }
        
        private void loadOnList(int id, IPhysicBody O)
        {
            switch (id)
            {
                case 0:
                    ObjectsList1.Add(O);
                    break;

                case 1:
                    ObjectsList2.Add(O);
                    break;

                case 2:
                    ObjectsList3.Add(O);
                    break;

                default:
                    ObjectsList1.Add(O);
                    break;

            }
        }

        protected override void BulletCrystalCollision(GameModel.IPhysicBody physObj1, GameModel.IPhysicBody physObj2)
        {
            IGameObject aux = this.findObjectByType("Astronautas2D.GameObjects.Entities.Crystal2D", physObj1, physObj2);
            IGameObject aux2 = this.findObjectByType("Astronautas2D.GameObjects.Entities.Bullet2D", physObj1, physObj2);
            Crystal2D crystal = (Crystal2D)aux;
            Bullet2D bullet = (Bullet2D)aux2;
            crystal.ShatterDirection = bullet.ShootDirection;

            if (objective.Type == objectiveType.shoot)
            {
                ShootObjective so = (ShootObjective)objective;
                so.shootCrystal(crystal);
                playerId id = (playerId)crystal.ListIndex;
                IndividualSubLevelEnded(true, (int)id);
                int profit = scoreManager.addScore(id, true);
                timeManager.setFlag(id);
                this.playerManager.showScoreProfit(id,profit, crystal.Position2D);
                this.tryToShatter(crystal);
                crystal.Crash(true);
            }
            else if (objective.Type == objectiveType.shootAndPortals)
            {
                if (!this.tryToShatter(crystal) && crystal.Father != null)
                {
                    crystal.Father.crashSons(false);
                }
                crystal.Crash(false);
            }
            bullet.Explode();
        }

        public void onCenterClick(object sender, MiceClickArgs e)
        {
            if (objective.Type == objectiveType.activateForce)
            {
                ActivateForceObjective afo = (ActivateForceObjective)objective;
                if (afo.getIsActive((playerId)e.Index) && afo.checkCharge((playerId)e.Index))
                {
                    this.IndividualSubLevelEnded(true, e.Index);
                    afo.CompleteObjective((playerId)e.Index);
                    int profit = scoreManager.addScore((playerId)e.Index, afo.getCurrentCharge((playerId)e.Index), afo.getMaxCharge((playerId)e.Index),true);

                    this.playerManager.showScoreProfit((playerId)e.Index, profit, afo.getPosition((playerId)e.Index));
                }
            }
            if (objective.Type == objectiveType.crystals)
            {
                CrystalObjective co = (CrystalObjective)objective;
                if(co.RandomizeCrystal[e.Index])
                {
                    co.RandomizeCrystal[e.Index] = false;
                    co.CrashCrystal((playerId)e.Index, true);
                    SoundManager.Instance.Play(Sounds.crystalDissapear);
                }
                if (co.checkObjective((playerId)e.Index))
                {
                    playerId id = (playerId)e.Index;
                    this.IndividualSubLevelEnded(true, (int)id);
                    int profit = scoreManager.addScore(id, true);
                    timeManager.setFlag(id);
                    this.playerManager.showScoreProfit(id, profit, co.getAstronaut(id).Position2D);
                }
            }
        }

        protected override void AsteroidCrystalCollision(IPhysicBody physObj1, IPhysicBody physObj2)
        {
            IGameObject aux = this.findObjectByType("Astronautas2D.GameObjects.Entities.Crystal2D", physObj1, physObj2);
            Crystal2D crystal = (Crystal2D)aux;
            crystal.Crash(false);
            SoundManager.Instance.Play(Sounds.crystalDeath);
            this.IndividualSubLevelEnded(false, crystal.ListIndex);
        }

        protected override void AstronautAsteroidCollision(IPhysicBody physObj1, IPhysicBody physObj2)
        {
            
            IGameObject aux = this.findObjectByType("Astronautas2D.GameObjects.Entities.Astronaut2D", physObj1, physObj2);
            Astronaut2D astronaut = (Astronaut2D)aux;
            astronaut.Die();
            this.IndividualSubLevelEnded(false, (int)astronaut.PlayerId);
        }

        protected override void AstronautCrystalCollision(IPhysicBody physObj1, IPhysicBody physObj2)
        {
            IGameObject aux = this.findObjectByType("Astronautas2D.GameObjects.Entities.Crystal2D", physObj1, physObj2);
            Crystal2D crystal = (Crystal2D)aux;
            IGameObject aux2 = this.findObjectByType("Astronautas2D.GameObjects.Entities.Astronaut2D", physObj1, physObj2);
            Astronaut2D astronaut = (Astronaut2D)aux2;
            SoundManager.Instance.Play(Sounds.crystalDeath);
            astronaut.DeactivateCharge();
            this.IndividualSubLevelEnded(false, (int)astronaut.PlayerId);

            if (crystal.Father != null)
            {
                if (objective.Type == objectiveType.shootAndPortals)
                    crystal.Father.crashSons(false);
            }
            crystal.Crash(false);
            
            if(objective is CrystalObjective)
            {
                CrystalObjective co = (CrystalObjective)objective;
                co.ResetReactionFlags(astronaut.PlayerId);
            }
        }


        public override void Close()
        {
            base.Close();
            if (objective.Type == objectiveType.activateForce || objective.Type == objectiveType.crystals) miceManager.CenterClick -= onCenterClick;
        }

    }
}
