using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GoblinXNA.UI.UI2D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace ARGame
{
    public class PlayerMisionBase : PlayerLevel
    {
        public PlayerMisionBase(AbstractGame game)
            : base(game) 
        {
        
        }


        public override void Draw()
        {
            base.Draw();
            UI2DRenderer.WriteText(new Vector2(10, 0), ">> Lleven el cristal al portal.", Color.White, game.font);

        }
       
    }
}
