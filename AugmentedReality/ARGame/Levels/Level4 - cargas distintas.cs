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
        bool positive, negative;
        public bool pass;
        public float time;
        
        public Level4(AbstractGame game) : base(game) 
        {
            positive = false;
            negative = false;
            
        }

        public override void Init()
        {
            base.Init();
           
        }
        

        public override void Draw()
        {
            base.Draw();
            (gameGUI as GameGUI.GamePlayGUI).RayShooterButton.Visible = false;
            UI2DRenderer.WriteText(new Vector2(10, 0), ">> Mueve el cristal utilizando cargas positivas y negativas.",
                Color.White, game.font);
        }

        public override void Update(float elpasedTime)
        {
            base.Update(elpasedTime);

            if (player.IsActive)
            {
                if (player.Charge > 0) positive = true;
                if (player.Charge < 0) negative = true;
            }
            if (positive && negative)
                pass = true;

            if (pass && time < 3)
                time += elpasedTime;
            else if (pass && time >= 3)
            {
                PassSubLevel();
            }
        }

        
        public override void PassSubLevel()
        {

            base.PassSubLevel();

            positive = false;
            negative = false;
            pass = false;
            time = 0;
            /* le cambiamos la carga a los objetos
             * para que practique con otras.
             */
            foreach (GameObject obj in objects)
            {
                if (obj.MaterialName != null && obj.MaterialName.Equals("crystal"))
                {
                    int aleat0 = rand.Next((int)(xmin / 2), (int)(xmax / 2));//(DateTime.Now.Millisecond % 2 == 0 ? -1 : 1) * (15);
                    int aleat1 = rand.Next((int)(ymin / 2), (int)(ymax / 2));//(DateTime.Now.Millisecond % 2 == 0 ? -1 : 1) * (15);
                    obj.SetPosition(new Vector3(aleat0, aleat1, 0));
                    obj.Charge = obj.Charge * -1;
                }
            }

            if (levelCompleted == 0)
            {
                gameGUI.WriteMessage("\n\n\n\n\nExcelente! Terminaste la prueba"
                                       + "\nEspera al Comandante para que te envie a otra mision.");
            }
            levelCompleted++;
        }
    }
}
