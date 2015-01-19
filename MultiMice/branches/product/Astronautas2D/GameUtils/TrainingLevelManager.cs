using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARGame.Utils
{
    public class TrainingLevelManager : LevelManager
    {
        public event Action<int, bool> SubLevelEnded;

        public TrainingLevelManager(AbstractGame game)
            : base(game) 
        {

        }

        public void EndTraining()
        {
            currentLevel.Close();
        }

        public void GoToLevel(int level)
        {
            currentLevel.Close();
            currentLevelIndex = level;
            currentLevel = Levels[currentLevelIndex];
            currentLevel.Init();
            currentLevel.Pause(true);
        }

        public override void SubLevelEndedHandler(bool endState)
        {
            if (SubLevelEnded != null)
                SubLevelEnded(currentLevelIndex, endState);
            //if (endState)
            //    currentLevel.PassSubLevel();
            //else
            //    currentLevel.ResetSubLevel();
        }
    }
}
