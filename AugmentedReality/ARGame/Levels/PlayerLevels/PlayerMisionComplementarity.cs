using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GoblinXNA.UI.UI2D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace ARGame
{
    public class PlayerMisionComplementarity : PlayerLevel
    {
        public PlayerMisionComplementarity(AbstractGame game)
            : base(game) 
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
            UI2DRenderer.WriteText(new Vector2(10, 0), ">> Lleven el cristal al portal, \n>> " +
            "sus cargas no se pueden modificar.", Color.White, game.font);

        }

        public override void Update(float elpasedTime)
        {
            base.Update(elpasedTime);
            int pos = this.PlayerId % 2 == 0 ? 1 : -1;
            this.player.SetCharge(pos * 40);
            (gameGUI as GameGUI.GamePlayGUI).ChargeSlider.Value = (int)this.player.PotentialCharge;
        }
    }
}
