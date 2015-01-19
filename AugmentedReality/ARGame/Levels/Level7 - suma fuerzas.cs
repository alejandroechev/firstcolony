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
    public class Level7 : Level
    {

       
        public Level7(AbstractGame game) : base(game) { }

        public override void Draw()
        {
            base.Draw();
            UI2DRenderer.WriteText(new Vector2(10, 0), ">> Lleven el cristal al portal \n>> Todos deben participar", Color.White, game.font);
            
            

        }

        public override void Update(float elpasedTime)
        {
            base.Update(elpasedTime);

            
        }
        protected override void CrystallPortalCollision(GameModel.IPhysicBody physObj1, GameModel.IPhysicBody physObj2)
        {
            PassSubLevel();
        }

        protected override void AsteroidCrystalCollision(GameModel.IPhysicBody physObj1, GameModel.IPhysicBody physObj2)
        {
            base.AsteroidCrystalCollision(physObj1, physObj2);
            gameGUI.WriteMessage("\n\n\n\n\nREINICIANDO ENTRENAMIENTO.\n Cuidado con los asteroides, no deben chocar con ellos");

        }
        public override void PassSubLevel()
        {
            base.PassSubLevel();

            if (levelCompleted == 0)
            {
                gameGUI.WriteMessage("\n\n\n\n\nMuy bien! pasaron el ultimo nivel de entrenamiento." +
                    "\n Sigan practicando mientras el comandante les asigna una mision");
            }
            levelCompleted++;

            //ResetObjects();
            
            foreach (GameObject obj in objects){
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
