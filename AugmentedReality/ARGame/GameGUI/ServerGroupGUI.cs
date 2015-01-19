using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GoblinXNA.UI.UI2D;
using GoblinXNA;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using ARGame.Utils;

namespace ARGame.GameGUI
{
    public class ServerGroupGUI : ServerIndivGUI
    {
        
        public ServerGroupGUI(Dictionary<int, GameData> playersData)
            : base(playersData) 
        {
            
        }
       
        public override void Draw()
        {
            int i = 0;
            UI2DRenderer.WriteText(new Vector2(100, 200 + (i-1) * 30), ">> Nivel completado >> Intentos >> Subnivel", Color.White, this.textFont);
            foreach (KeyValuePair<int, GameData> playersDataPair in PlayersData)
            {
                UI2DRenderer.WriteText(new Vector2(100, 200 + i*30), "Grupo " + playersDataPair.Key +" : " + 
                    playersDataPair.Value.CurrentLevelData.ToString(), Color.White, this.textFont);
                i++;
            }
        }

   
    }
}
