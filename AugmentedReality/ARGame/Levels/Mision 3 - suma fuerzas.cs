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
    public class Mision3 : Mision1
    {

        public Mision3(AbstractGame game) : base(game) 
        {
        
        }

        public override void Init()
        {
            base.Init();
            (gameGUI as GameGUI.GamePlayGUI).toolBarPanel.Transparency = 0.5f;
            (gameGUI as GameGUI.GamePlayGUI).ChargeSlider.Enabled = false;
            (gameGUI as GameGUI.GamePlayGUI).PlusButton.Enabled = false;
            (gameGUI as GameGUI.GamePlayGUI).MinusButton.Enabled = false;
            
        }

        public override void Draw()
        {
            base.Draw();
            
        }

        public override void Update(float elpasedTime)
        {
            base.Update(elpasedTime);
            int pos = this.player.Id%2==0?1:-1;
            this.player.SetCharge(pos * 40);
            (gameGUI as GameGUI.GamePlayGUI).ChargeSlider.Value = (int)this.player.PotentialCharge;
        }
       
    }
}
