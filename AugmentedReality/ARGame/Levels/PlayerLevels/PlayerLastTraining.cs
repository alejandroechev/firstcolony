using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARGame
{
    public class PlayerLastTraining : PlayerLevel
    {
        public PlayerLastTraining(AbstractGame game)
            : base(game) 
        {
        
        }

        public override void Update(float elpasedTime)
        {
            UpdateMarkerFound();
            UpdatePlayerPosition();
            player.ArrowMode = true;
            UpdateArrow();
            playtime += elpasedTime;
        }

        public override void PassSubLevel()
        {
            if (levelCompleted == 0)
            {
                gameGUI.WriteMessage("\n\n\n\n\nMuy bien! pasaron el ultimo nivel de entrenamiento." +
                        "\n Sigan practicando mientras el comandante les asigna una mision");
            }
            this.gameGUI.Reset();
            levelCompleted++;            
        }

        public override void ResetSubLevel()
        {
            gameGUI.WriteMessage("\n\n\n\n\nREINICIANDO ENTRENAMIENTO.\n Cuidado con los asteroides, no deben chocar con ellos");
            this.gameGUI.Reset();
            foreach (GameObject obj in ObjectsList)
                obj.Reset();
        }
    }
}
