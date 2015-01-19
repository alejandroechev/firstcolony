using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GoblinXNA.UI.UI2D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace ARGame
{
    public class PlayerMisionSharedOperative : PlayerLevel
    {
        public PlayerMisionSharedOperative(AbstractGame game)
            : base(game) 
        {
        
        }

        

        public override void Draw()
        {
            base.Draw();
            UI2DRenderer.WriteText(new Vector2(10, 0), ">> Lleven el cristal al portal, \n>> " +
            "todos deben activarse para moverlo.", Color.White, game.font);

        }

       
    }
}
