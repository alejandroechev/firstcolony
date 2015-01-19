using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using GoblinXNA.UI.UI2D;
using Microsoft.Xna.Framework.Graphics;


namespace ARGame.Levels
{
    public class Level0 : Level
    {
        public Level0(AbstractGame game) : base(game) { }

        public override void Draw()
        {
            base.Draw();
            UI2DRenderer.WriteText(new Vector2(100,70), "Nivel X", Color.Red, game.font);
        }
    }
}
