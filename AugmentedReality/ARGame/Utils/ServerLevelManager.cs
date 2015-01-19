using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARGame.Utils
{
    public class ServerLevelManager : LevelManager
    {
        public event Action<ServerLevelManager, bool> SubLevelEnded;
        public int CurrentLevelIndex { get { return currentLevelIndex; } }

        public ServerLevelManager(AbstractGame game):base(game) 
        {
        }

        public override void SubLevelEndedHandler(bool endState)
        {
            if (SubLevelEnded != null)
                SubLevelEnded(this, endState);
                        
        }

        public override bool NextLevel()
        {
            currentLevelIndex++;
            if (currentLevelIndex >= Levels.Count)
                return false;
            currentLevel = Levels[currentLevelIndex];
            currentLevel.Init();            
            return true;
        }
    }
}
