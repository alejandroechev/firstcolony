using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Astronautas2D.Visual_Components;
using Microsoft.Xna.Framework.Graphics;
using Astronautas2D.GUI;
using Astronautas2D.Utils;

namespace Astronautas2D.GUI.GUIElements
{
    class StatsBar : GUIElement
    {
        private String stageTitle = "ETAPA";
        private Vector2 stageTitlePos = new Vector2(50, 50);
        private String stage;
        private Vector2 stagePos = new Vector2(70, 70);
        private String timeTitle = "TIEMPO";
        private Vector2 timeTitlePos = new Vector2(1400, 50);
        private String time;
        private Vector2 timePos = new Vector2(1420, 70);
 
        public StatsBar(int stageIndex, Vector2 position, Texture2D texture, bool visible, Vector2 scale, float layer) :
            base(position, texture, visible, scale, layer)
        {
            this.stage = stageIndex.ToString();
            this.time = "00:00";
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch, Writer fontWriter)
        {
            base.Draw(gameTime, spriteBatch, fontWriter);
            Color fontColor = Color.Black;
            fontWriter.DrawText(stageTitle, stageTitlePos, fontColor);
            fontWriter.DrawText(stage, stagePos, fontColor);
            fontWriter.DrawText(timeTitle, timeTitlePos, fontColor);
            fontWriter.DrawText(time, timePos, fontColor);
        }

        public void UpdateTime(string Time)
        {
            time = Time;
        }
    }
}
