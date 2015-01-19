using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameUtils
{
    public class PlayerLevelManager : LevelManager
    {
        private int playerId;
        public int PlayerId { get { return playerId; } set { playerId = value; } }

        public PlayerLevelManager(AbstractGame game)
            :base(game) 
        {

        }

        public void LevelStateReceived(bool state)
        {           
            if (!state)
                currentLevel.ResetSubLevel();
            else
                currentLevel.PassSubLevel();
        }


        public void GoToLevel(int level)
        {
            currentLevel.Close();
            currentLevelIndex = level;
            currentLevel = Levels[currentLevelIndex];
            currentLevel.PlayerId = playerId;
            currentLevel.Init();            
        }
    }
}
