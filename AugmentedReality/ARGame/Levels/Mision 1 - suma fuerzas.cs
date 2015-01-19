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
    public class Mision1 : Level
    {

        float subleveltime;
        float maxtime;
        public Mision1(AbstractGame game) : base(game) 
        {
            this.ArrowMode = false;
            
        }
        public override void Init()
        {
            base.Init();
            (gameGUI as GameGUI.GamePlayGUI).RayShooterButton.Enabled = false;
        }
        public override void Draw()
        {
            base.Draw();
            UI2DRenderer.WriteText(new Vector2(10, 0), ">> Lleven el cristal al portal \n>> Todos deben participar", Color.White, game.font);

        }

        public override void Update(float elpasedTime)
        {
            base.Update(elpasedTime);
            subleveltime += elpasedTime;
            if (subleveltime > maxtime)
                ArrowMode = false;
            
        }

        protected override void CrystallPortalCollision(GameModel.IPhysicBody physObj1, GameModel.IPhysicBody physObj2)
        {
            PassSubLevel();
        }

        protected override void AsteroidCrystalCollision(GameModel.IPhysicBody physObj1, GameModel.IPhysicBody physObj2)
        {
            base.AsteroidCrystalCollision(physObj1, physObj2);
            this.ArrowMode = true;
            subleveltime = 0;
            maxtime += 30;
            gameGUI.WriteMessage("\n\n\n\n\nREINICIANDO MISION.\n Cuidado con los asteroides, no deben chocar con ellos");

        }
        public override void PassSubLevel()
        {
            base.PassSubLevel();

            if (levelCompleted == 0)
            {
                gameGUI.WriteMessage("\n\n\n\n\nMuy bien! pasaron la mision.\n Recogan mas cristales mientras el comandante les asigna\n una nueva mision");
            }
            levelCompleted++;
            ResetObjects();
            
            foreach (GameObject obj in objects){
                if (obj.MaterialName != null && obj.MaterialName.Equals("crystal"))
                {
                    if (respawnPositions.Count == 0)
                    {
                        int aleat0 = (DateTime.Now.Millisecond % 2 == 0 ? -1 : 1) * (10 + DateTime.Now.Millisecond % 5);
                        int aleat1 = (DateTime.Now.Millisecond % 2 == 0 ? -1 : 1) * (20 + DateTime.Now.Millisecond % 15);
                        obj.SetPosition(obj.Position + new Vector3(aleat0, aleat1, 0));
                    }
                    else
                    {
                        obj.SetPosition(respawnPositions[currentRespawnPosition]);
                        currentRespawnPosition++;
                        if (currentRespawnPosition >= respawnPositions.Count)
                            currentRespawnPosition = 0;
                    }
                }
            }
        }
    }
}
