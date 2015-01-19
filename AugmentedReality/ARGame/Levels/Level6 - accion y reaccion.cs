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
    public class Level6 : Level
    {

        int nPortales;
        ARPortal FirstPortal = null;
        ARCrystal Eliminated = null;
        public Level6(AbstractGame game) : base(game) { }

        public override void Draw()
        {
            base.Draw();
            UI2DRenderer.WriteText(new Vector2(10, 0), ">> Rompe el crystal con el laser \n>> Debe entrar al menos un crystal a cada portal", Color.White, game.font);
        }

        public override void Update(float elpasedTime)
        {
            base.Update(elpasedTime);

            
        }
        protected override void CrystallPortalCollision(GameModel.IPhysicBody physObj1, GameModel.IPhysicBody physObj2)
        {
            if (FirstPortal == null){
                nPortales++;
                FirstPortal = (ARPortal)physObj2;
                this.markerNode.RemoveChild((physObj1 as GameObject).ObjectNode);
                this.objects.Remove(physObj1 as GameObject);
                Eliminated = (ARCrystal)physObj1;
            }
            else if(FirstPortal != physObj2)
            {
                PassSubLevel();
            }
        }

        public override void PassSubLevel()
        {
            this.markerNode.AddChild(Eliminated.ObjectNode);
            this.objects.Add(Eliminated);

            base.PassSubLevel();
            FirstPortal = null;


            foreach (GameObject obj in objects)
            {
                if (obj.MaterialName != null && obj.MaterialName.Equals("crystal"))
                {
                    int aleat0 = (DateTime.Now.Millisecond % 2 == 0 ? -1 : 1) * (10 + DateTime.Now.Millisecond % 5);
                    int aleat1 = (DateTime.Now.Millisecond % 2 == 0 ? -1 : 1) * (20 + DateTime.Now.Millisecond % 15);
                    obj.SetPosition(obj.Position + new Vector3(aleat0, aleat1, 0));
                    obj.Charge = obj.Charge * -1;
                }
            }
            if (levelCompleted == 0)
            {
                gameGUI.WriteMessage("\n\n\n\n\nExcelente novato! Ya controlas tu laser automatico!"
                                           + "\nSigue practicando mientras esperas mas" + "\ninstrucciones del Comandante.");
            }
            levelCompleted++;
        }
    }
}
