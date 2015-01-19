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
        public bool pass;
        public float time;

        public override void Init()
        {
            base.Init();
        }

        public override void Draw()
        {
           base.Draw();
            UI2DRenderer.WriteText(new Vector2(10, 0), ">> Rompe el cristal con tu laser", Color.White, game.font);
        }
        public override void Update(float elpasedTime)
        {
            base.Update(elpasedTime);
            if (pass && time < 3)
                time += elpasedTime;
            else if (pass && time >= 3)
            {
                PassSubLevel();
            }
        }

        protected override void BulletCrystalCollision(GameModel.IPhysicBody physObj1, GameModel.IPhysicBody physObj2)
        {
            base.BulletCrystalCollision(physObj1, physObj2);

            //ResetSubLevel();
            // Problemas al resetar objetos creados al partir un cristal
            pass = true;
            
        }
        public override void PassSubLevel()
        {
            base.PassSubLevel();
            if (levelCompleted == 0)
            {
                gameGUI.WriteMessage("\n\n\n\n\nBien Hecho!\nHas logrado romper el crystal.\nSigue practicando, mientras esperas instrucciones");      
            }
            levelCompleted++;
            pass = false;
            time = 0;
            foreach (GameObject obj in objects)
            {
                if (obj.MaterialName != null && obj.MaterialName.Equals("crystal"))
                {
                    //tener definidas otras posiciones harcoreadas.
                    int aleat0 = (DateTime.Now.Millisecond % 2 == 0 ? -1 : 1) * (10 + DateTime.Now.Millisecond % 5);
                    int aleat1 = (DateTime.Now.Millisecond % 2 == 0 ? -1 : 1) * (20 + DateTime.Now.Millisecond % 15);
                    obj.SetPosition(obj.Position + new Vector3(aleat0, aleat1, 0));
                    obj.Charge = obj.Charge * -1;
                }
            }

        }
    }
}
