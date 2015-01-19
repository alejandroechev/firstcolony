using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using GoblinXNA.UI.UI2D;
using Microsoft.Xna.Framework.Graphics;


namespace ARGame.Levels
{
    /* Usar el rayo laser destruye cristales.
    *  El nivel termina cuando el jugador dispara al cristal.
    */
    public class Level3 : Level
    {
        public Level3(AbstractGame game) : base(game) { }
        
        public override void Draw()
        {
            base.Draw();
            UI2DRenderer.WriteText(new Vector2(10, 10), "Objetivo: Rompe el cristal con tu laser", Color.Blue, game.font);
            if (levelCompleted > 0)
                UI2DRenderer.WriteText(new Vector2(10, 25), "Estado Nivel: Completado " + levelCompleted + " veces", Color.Green, game.font);
            else
                UI2DRenderer.WriteText(new Vector2(10, 25), "Estado Nivel: En Curso", Color.Yellow, game.font);
        }
        protected override void BulletCrystalCollision(GameModel.IPhysicBody physObj1, GameModel.IPhysicBody physObj2)
        {
            base.BulletCrystalCollision(physObj1, physObj2);
            
            PassSubLevel();
            levelCompleted++;
            
        }
    }
}
