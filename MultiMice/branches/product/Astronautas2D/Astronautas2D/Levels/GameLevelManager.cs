using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AstroLib;
using Astronautas2D.Factories;
using Astronautas2D.Utils;
using Astronautas2D.Visual_Components;
using Microsoft.Xna.Framework;
using System.Xml.Linq;
using System.Reflection;
using Astronautas2D.Objectives;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace Astronautas2D.Levels
{
    public enum levelConditions { move, shoot, forceActivation, chargeChange, showCharge, showTotalForce, showIndividualForces}

    class GameLevelManager : AstroLib.LevelManager
    {
        private MiceManager miceManager;
        //private int[] subLevelIndex;
        public event Action<bool, int> IndividualSubLevelEnded;
        public event Action<bool> GroupSubLevelEnded;
        private bool narrative;
        private bool allAtOnce;
        private int counter;
        private bool endingReached;
        public bool EndingReached { get { return endingReached; } }
        private Texture2D endingScreen;
        public Texture2D EndingScreen { set { endingScreen = value; } }
        private Rectangle viewRect;

        public int GroupNumber 
        { 
            set 
            {
                foreach (AbstractLevel al in base.Levels)
                {
                    if (al is TeamLevel)
                    {
                        TeamLevel aux = (TeamLevel)al;
                        aux.GroupNumber = value;
                    }
                    else if (al is SoloLevel)
                    {
                        SoloLevel aux = (SoloLevel)al;
                        aux.GroupNumber = value;
                    }
                }
            } 
        }

        public GameLevelManager(MiceManager manager, Rectangle viewRect)
            : base()
        {
            miceManager = manager;
            this.narrative = Configuration.Instance.GetBoolParam("player", "narrative");
            this.allAtOnce = Configuration.Instance.GetBoolParam("player", "allTogether");
            counter = 0;
            endingReached = false;
            this.viewRect = viewRect;
        }

        public void Update(float elapsedTime)
        {
            if(!endingReached) this.currentLevel.Update(elapsedTime);
        }

        public void Load(string gameFile, GameObjectFactory gof, GUIElementFactory gef, Rectangle viewRect)
        {
            Levels = new List<AbstractLevel>();
            XDocument xmlDoc = XDocument.Load(gameFile);

            XElement gameNode = xmlDoc.Elements("game").First();
            XElement levelRoot = gameNode.Elements("levels").First();
            IEnumerable<XElement> levelElements = levelRoot.Elements("level");

            string objectType;
            Type tObject;
            ConstructorInfo ciObject;
            foreach (XElement levelNode in levelElements)
            {
                string fileName = levelNode.Attribute("file").Value;
                string objectiveName = levelNode.Attribute("objective").Value;
                bool move = this.getBool(levelNode.Attribute("move").Value);
                bool shoot = this.getBool(levelNode.Attribute("shoot").Value);
                bool forcefield = this.getBool(levelNode.Attribute("forcefield").Value);
                bool chargeChange = this.getBool(levelNode.Attribute("chargeChange").Value);
                bool showCharge = this.getBool(levelNode.Attribute("chargeShow").Value);
                bool showTotalForce = this.getBool(levelNode.Attribute("showTotalForce").Value);
                bool showIndividualForces = this.getBool(levelNode.Attribute("showIndividualForces").Value);
                bool tutorial = this.getBool(levelNode.Attribute("tutorial").Value);
                float chargeValue;
                if (chargeChange == true)
                {
                    chargeValue = 0f;
                }
                else
                {
                    string value = levelNode.Attribute("chargeValue").Value;
                    chargeValue = (float) Convert.ToDouble(value);
                }

                bool[] conditions = new bool[8];
                conditions[(int)levelConditions.move] = move;
                conditions[(int)levelConditions.shoot] = shoot;
                conditions[(int)levelConditions.forceActivation] = forcefield;
                conditions[(int)levelConditions.chargeChange] = chargeChange;
                conditions[(int)levelConditions.showCharge] = showCharge;
                conditions[(int)levelConditions.showTotalForce] = showTotalForce;
                conditions[(int)levelConditions.showIndividualForces] = showIndividualForces;

                objectType = levelNode.Attribute("class").Value;
                Type objectiveType = Type.GetType("Astronautas2D.Objectives.Objective");

                tObject = System.Type.GetType(objectType);
                ciObject = tObject.GetConstructor(
                    new Type[11] 
                    { 
                        counter.GetType(), objectiveType, narrative.GetType(), tutorial.GetType(),this.miceManager.GetType(), gof.GetType(), gef.GetType(), viewRect.GetType(), conditions.GetType(), chargeValue.GetType(), allAtOnce.GetType()
                    }
                    );
                if (tObject == Type.GetType("Astronautas2D.Levels.SoloLevel"))
                {
                    Objective objective = this.createObjective(objectiveName, narrative, false, chargeChange);
                    SoloLevel levelObj = ciObject.Invoke(new object[] { counter, objective, narrative, tutorial, this.miceManager, gof, gef, viewRect, conditions, chargeValue, false }) as SoloLevel;
                    levelObj.File = fileName;

                    levelObj.LevelEnded += LevelEndedHandler;
                    //levelObj.SubLevelEnds += OnIndividualSubLevelEnded;
                    Levels.Add(levelObj);
                }
                else
                {
                    Objective objective = this.createObjective(objectiveName, narrative, true, chargeChange);
                    TeamLevel levelObj = ciObject.Invoke(new object[] { counter, objective, narrative, tutorial, this.miceManager, gof, gef, viewRect, conditions, chargeValue, allAtOnce }) as TeamLevel;
                    levelObj.File = fileName;

                    levelObj.LevelEnded += LevelEndedHandler;
                    //levelObj.SubLevelEnds += OnGroupSubLevelEnded;
                    Levels.Add(levelObj);
                }
                counter++;
            }
            currentLevel = Levels.First();

        }

        public void OnIndividualSubLevelEnded(bool endState, int id)
        {
            if (IndividualSubLevelEnded != null)
                IndividualSubLevelEnded(endState, id);
            if (endState) SoundManager.Instance.Play(Sounds.victory);
            else SoundManager.Instance.Play(Sounds.defeat);
        }

        public void OnGroupSubLevelEnded(bool endState)
        {
            if (GroupSubLevelEnded != null)
                GroupSubLevelEnded(endState);

            if (endState) SoundManager.Instance.Play(Sounds.victory);
            else SoundManager.Instance.Play(Sounds.defeat);
        }


        public override void Init()
        {
            if(currentLevel is SoloLevel)
            {
                SoloLevel sl = (SoloLevel)currentLevel;
                sl.IndividualSubLevelEnded += OnIndividualSubLevelEnded;
            }
            else if(currentLevel is TeamLevel)
            {
                TeamLevel tl = (TeamLevel)currentLevel;
                tl.GroupSubLevelEnded += OnGroupSubLevelEnded;
            }
            base.Init();
        }

        public override bool GoToLevel(int level)
        {
            if (level >= Levels.Count)
            {
                endingReached = true;
                return false;
            }
            
            if(currentLevel != null)
            {
                if (currentLevel is SoloLevel)
                {
                    SoloLevel sl = (SoloLevel)currentLevel;
                    sl.IndividualSubLevelEnded -= OnIndividualSubLevelEnded;
                }
                else if (currentLevel is TeamLevel)
                {
                    TeamLevel tl = (TeamLevel)currentLevel;
                    tl.GroupSubLevelEnded -= OnGroupSubLevelEnded;
                }
                currentLevel.Close();
            }
            currentLevel = Levels[level];
            if (currentLevel is SoloLevel)
            {
                SoloLevel sl = (SoloLevel)currentLevel;
                sl.IndividualSubLevelEnded += OnIndividualSubLevelEnded;
            }
            else if (currentLevel is TeamLevel)
            {
                TeamLevel tl = (TeamLevel)currentLevel;
                tl.GroupSubLevelEnded += OnGroupSubLevelEnded;
            }
            currentLevel.Close();
            TeamLevel gl = (TeamLevel)currentLevel;
            gl.Load(currentLevel.File);
            gl.Init();
            return true;
            
        }

        public override bool NextLevel()
        {
            currentLevelIndex++;
            if (currentLevelIndex >= Levels.Count)
            {
                currentLevelIndex--;
                endingReached = true;
                return false;
            }
            else
            {
                if (currentLevel is SoloLevel)
                {
                    SoloLevel sl = (SoloLevel)currentLevel;
                    sl.IndividualSubLevelEnded -= OnIndividualSubLevelEnded;
                }
                else if (currentLevel is TeamLevel)
                {
                    TeamLevel tl = (TeamLevel)currentLevel;
                    tl.GroupSubLevelEnded -= OnGroupSubLevelEnded;
                }
                currentLevel.Close();
                currentLevel = Levels[currentLevelIndex];
                if (currentLevel is SoloLevel)
                {
                    SoloLevel sl = (SoloLevel)currentLevel;
                    sl.IndividualSubLevelEnded += OnIndividualSubLevelEnded;
                }
                else if (currentLevel is TeamLevel)
                {
                    TeamLevel tl = (TeamLevel)currentLevel;
                    tl.GroupSubLevelEnded += OnGroupSubLevelEnded;
                }
                TeamLevel gl = (TeamLevel)currentLevel;
                gl.Load(currentLevel.File);
                gl.Init();
                return true;
            }
        }

        public void ResetLevel(bool resetTimer)
        {
            TeamLevel gl = (TeamLevel)currentLevel;
            gl.Reset(resetTimer);
        }

        protected Objective createObjective(String name, bool narrative, bool team, bool chargeChange)
        {
            switch (name)
            {
                case "crystalsAndPortals":
                    return new CrystalsAndPortalsObjective(narrative, team, chargeChange);
                case "move":
                    return new ZoneObjective(narrative, team);
                case "crystals":
                    return new CrystalObjective(narrative, team);
                case "shootAndPortals":
                    return new ShootAndPortalsObjective(narrative, team);
                case "shoot":
                    return new ShootObjective(narrative, team);
                case "activateCharge":
                    return new ActivateForceObjective(narrative, team);
                default:
                    return null;
            }
        }

        private bool getBool(string s)
        {
            if (s.CompareTo("true") == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Writer fontWriter)
        {
            if (!endingReached)
            {
                TeamLevel tl = (TeamLevel)currentLevel;
                tl.Draw(gameTime, spriteBatch, fontWriter);
                miceManager.DrawMice(spriteBatch);
            }
            else
            {
                spriteBatch.Draw(this.endingScreen, new Vector2(0, 0), new Rectangle(0, 0, 1440, 900), Color.White, 0f, new Vector2(0, 0), new Vector2((float)(viewRect.Width) / 1440f, (float)(viewRect.Height) / 900f), SpriteEffects.None, 0f);
            }
        }
    }
}
