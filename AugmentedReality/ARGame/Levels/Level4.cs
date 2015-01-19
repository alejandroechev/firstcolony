using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using GoblinXNA.UI.UI2D;
using Microsoft.Xna.Framework.Graphics;


namespace ARGame.Levels
{
    /* Utilizar distintas cargas para ver como 
     * se ve afectada la fuerza
    */
    public class Level4 : Level
    {
        public Level4(AbstractGame game) : base(game) { }
        int diferenteCharge;
        float oldCharge = float.MaxValue;
        const float epsilon = 20;

        public override void Draw()
        {
            base.Draw();
            UI2DRenderer.WriteText(new Vector2(10, 10), "Objetivo: Prueba distintas cargas para mover el crystal hasta el portal", Color.Blue, game.font);
            if (levelCompleted>0)
                UI2DRenderer.WriteText(new Vector2(10, 25), "Estado Nivel: Completado "+levelCompleted +" veces", Color.Green, game.font);
            else
                UI2DRenderer.WriteText(new Vector2(10, 25), "Estado Nivel: En Curso", Color.Yellow, game.font);
        }

        public override void Update(float elpasedTime)
        {
            base.Update(elpasedTime);

            if (player.IsActive)
            {
                if (oldCharge == float.MaxValue)
                    oldCharge = player.Charge;

                if (Math.Abs(player.Charge - oldCharge) > epsilon)
                {
                    diferenteCharge++;
                    
                    oldCharge = player.Charge;
                }
            }
        }
        protected override void PortalCrystallCollision(GameModel.IPhysicBody physObj1, GameModel.IPhysicBody physObj2)
        {
            
            if (diferenteCharge > 0) PassSubLevel();
            else
                ResetSubLevel();
        }

        public override void PassSubLevel()
        {

            base.PassSubLevel();
            levelCompleted++;
            diferenteCharge = 0;
            ResetObjects();
        }
    }
}
