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
    public class Level5 : Level
    {
        
        int diferentePosition;
        Vector3 first = new Vector3();
        Vector3 oldDistance;
        const float epsilon = 20;

        public Level5(AbstractGame game) : base(game) { oldDistance = first;}
        public override void Init()
        {
            base.Init();
            
            
        }

        public override void Draw()
        {
            base.Draw();
            (gameGUI as GameGUI.GamePlayGUI).toolBarPanel.Transparency = 0.5f;
            (gameGUI as GameGUI.GamePlayGUI).ChargeSlider.Enabled = false;
            (gameGUI as GameGUI.GamePlayGUI).PlusButton.Enabled = false;
            (gameGUI as GameGUI.GamePlayGUI).MinusButton.Enabled = false;
            (gameGUI as GameGUI.GamePlayGUI).RayShooterButton.Visible = false;
            UI2DRenderer.WriteText(new Vector2(10, 0), ">> Tu carga no se puede modificar. \n>> Juega con tu posicion para mover el cristal", Color.White, game.font);
        }

        public override void Update(float elpasedTime)
        {
            
            base.Update(elpasedTime);

            if (player.IsActive)
            {
                if (oldDistance == first)
                    oldDistance = player.Position;

                if ((player.Position - oldDistance).LengthSquared() > epsilon)
                {
                    diferentePosition++;
                    
                    oldDistance = player.Position;
                }
            }
            this.player.SetCharge(30);
            (gameGUI as GameGUI.GamePlayGUI).ChargeSlider.Value = (int)this.player.PotentialCharge;
        }
        protected override void CrystallPortalCollision(GameModel.IPhysicBody physObj1, GameModel.IPhysicBody physObj2)
        {

            if (diferentePosition > 0)
            {
                PassSubLevel();

            
            }
            else
            {
                
                gameGUI.WriteMessage("\n\n\n\n\nLlevaste el cristal al portal!"
                                   + "\nIntentalo nuevamente a distintas distancias."); 
                ResetSubLevel();
            }
        }

        public override void PassSubLevel()
        {

            base.PassSubLevel();

            diferentePosition = 0;

            /* le cambiamos la carga a los objetos
             * para que practique con otras.
             */
            foreach (GameObject obj in objects){
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
                gameGUI.WriteMessage("\n\n\n\n\nBien Hecho!\nHas logrado cumplir el objetivo.\nSigue practicando, mientras esperas instrucciones");
            }
            levelCompleted++;
        }
    }
}
