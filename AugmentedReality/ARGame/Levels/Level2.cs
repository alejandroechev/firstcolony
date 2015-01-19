using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using GoblinXNA.UI.UI2D;
using Microsoft.Xna.Framework.Graphics;


namespace ARGame.Levels
{
    /* Activar campo eléctrico (TAD).
     * El nivel termina cuando el jugador activa su campo.
     */

    public class Level2 : Level
    {
        public Level2(AbstractGame game) : base(game) { }
        bool inactive = true;
        public override void Draw()
        {
            base.Draw();
            
            UI2DRenderer.WriteText(new Vector2(10, 10), "Objetivo: Activa tu campo electrico", Color.Blue, game.font);
            if (levelCompleted > 0)
                UI2DRenderer.WriteText(new Vector2(10, 25), "Estado Nivel: Completado " + levelCompleted + " veces", Color.Green, game.font);
            else
                UI2DRenderer.WriteText(new Vector2(10, 25), "Estado Nivel: En Curso", Color.Yellow, game.font);
        }
        public override void Update(float elpasedTime)
        {
            base.Update(elpasedTime);
            if (player.IsActive)
            {
                if (inactive)
                {
                    inactive = false;
                    PassSubLevel();
                    levelCompleted++;
                }
            }
            else inactive = true;
        }
    }
}
