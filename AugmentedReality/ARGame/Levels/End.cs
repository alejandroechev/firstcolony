using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using GoblinXNA.UI.UI2D;
using Microsoft.Xna.Framework.Graphics;


namespace ARGame.Levels
{
    /* Utilizar distintas distancias para ver como 
     * se ve afectada la fuerza
    */
    public class End : Level
    {
        bool show = true;
        public End(AbstractGame game)
            : base(game) 
        {
        
        }

        public override void Update(float elpasedTime)
        {
            if (show)
            {
                gameGUI.WriteMessage("\n\n\n\n"+
                                     "\n                                   Felicitaciones"+
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
