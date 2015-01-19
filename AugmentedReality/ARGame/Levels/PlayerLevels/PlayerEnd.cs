using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARGame
{
    public class PlayerEnd : PlayerLevel
    {
        bool show = true;
        public PlayerEnd(AbstractGame game)
            : base(game) 
        {
        
        }

        public override void Update(float elpasedTime)
        {
            if (show)
            {
                gameGUI.WriteMessage("\n\n\n\n" +
                                     "\n                                   Felicitaciones" +
                                     "\n                            Has completado la Mision!!");
                PassSubLevel();
                (gameGUI as GameGUI.GamePlayGUI).OkButton.Visible = false;
                (gameGUI as GameGUI.GamePlayGUI).OkButton.Enabled = false;
                show = false;
            }
            base.Update(elpasedTime);
        }
    }
}
