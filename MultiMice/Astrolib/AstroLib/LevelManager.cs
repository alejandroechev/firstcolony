using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;

namespace AstroLib
{
    public class LevelManager
    {
        protected int currentLevelIndex = 0;
        protected AbstractLevel currentLevel;

        public AbstractLevel IntroLevel;
        public List<AbstractLevel> Levels { get; protected set; }
        public AbstractLevel CurrentLevel { get { return currentLevel; } }

        public bool GameEndedFlag { get { return currentLevelIndex + 1 >= Levels.Count; } }

        public event Action GameEnded;

        public LevelManager() 
        {
        }

        public void Load(string gameFile) 
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
                objectType = levelNode.Attribute("class").Value;
                tObject = System.Type.GetType(objectType);
                ciObject = tObject.GetConstructor(new Type[] { });
                AbstractLevel levelObj = ciObject.Invoke(null) as AbstractLevel;
                levelObj.File = fileName;
                
                levelObj.LevelEnded += LevelEndedHandler;
                levelObj.SubLevelEnded += SubLevelEndedHandler;
                Levels.Add(levelObj);
            }
            currentLevel = Levels.ElementAt(0);
            
        }

        public virtual void Init() 
        {
            currentLevel.Init();
        }
        protected void IntroEndedHandler(bool endState)
        {
            currentLevel = Levels[currentLevelIndex];
            currentLevel.Init();
        }


        public virtual void LevelEndedHandler(bool endState)
        {
            currentLevel.Close();
            if (endState)
            {
                if (!NextLevel())
                    OnGameEnded();
            }
            else
                currentLevel.Init();
        }

        public virtual void SubLevelEndedHandler(bool endState)
        {
        }

        public virtual bool NextLevel()
        {
            currentLevelIndex++;
            if (currentLevelIndex >= Levels.Count)
                return false;
            currentLevel = Levels[currentLevelIndex];
            currentLevel.Init();
            return true;
        }

        public virtual bool GoToLevel(int level)
        {
            if (level >= Levels.Count)
                return false;
            currentLevelIndex = level;
            currentLevel = Levels[currentLevelIndex];
            currentLevel.Init();
            return true;
        }

        protected virtual void OnGameEnded()
        {
            if (GameEnded != null)
                GameEnded();
        }
    }
}
