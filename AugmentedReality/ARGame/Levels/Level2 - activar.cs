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
        
        public override void Init()
        {
            base.Init();
            
        }
        
        public override void Draw()
        {
            base.Draw();
            (gameGUI as GameGUI.GamePlayGUI).RayShooterButton.Visible = false;
            //(gameGUI as GameGUI.GamePlayGUI).PlusButton.Visible = false;
            //(gameGUI as GameGUI.GamePlayGUI).MinusButton.Visible = false;
            //(gameGUI as GameGUI.GamePlayGUI).ChargeSlider.Visible = false;   
            UI2DRenderer.WriteText(new Vector2(10, 0), ">> Activa tu campo electrico.", Color.White, game.font);
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

                    player.IsActive = true;
                    (gameGUI as GameGUI.GamePlayGUI).ForceFieldButton.DoClick();

                    if (levelCompleted == 0)
                    {
                        gameGUI.WriteMessage("\n\n\n\n\nFelicidades! Aprendiste a usar el campo electrico!"
                                           + "\nSigue practicando mientras esperas nuevas" + "\ninstrucciones del Comandante.");
                    }    
                    levelCompleted++;
                }
            }
            else inactive = true;
        }
    }
}
