using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Astronautas2D.GUI;
using Microsoft.Xna.Framework;
using Astronautas2D.Visual_Components;
using Microsoft.Xna.Framework.Graphics;

namespace Astronautas2D.GUI.GUIElements
{
    public class Slider : GUIElement
    {
        private Marker marker;
        private float symbolHeight;
        private float delta;
        private int levels;

        public Slider(Vector2 position, int levels ,Texture2D sliderTexture,Texture2D markerTexture, bool isVisible, Vector2 scale, float layer)
            : base(position, sliderTexture, isVisible, scale, layer)
        {
            symbolHeight = 50f * scale.Y;
            delta = 5f * scale.Y;
            this.marker = new Marker(position, markerTexture, isVisible, scale, layer + 0.1f);
            this.levels = levels;
        }

        public void moveMarker(int level)
        {
            this.marker.Move(level, delta);
        }

        public override Vector2 Position
        {
            get
            {
                return base.Position;
            }
            set
            {
                base.Position = value;
                marker.BasePosition = value;
            }
        }


        /*
        public void moveMarkerUp()
        {
            markerHeight = Math.Max(markerHeight - delta, -height / 2);
            marker.Position = new Vector2(position.X, position.Y + markerHeight);
        }

        public void moveMarkerDown()
        {
            markerHeight = Math.Min(markerHeight + delta, height / 2);
            marker.Position = new Vector2(position.X, position.Y + markerHeight);
        }
         * */

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch, Writer fontWriter)
        {
            if (this.isVisible)
            {
                this.player.PlayAnimation(idleAnimation);
                base.player.Draw(gameTime, spriteBatch, position, SpriteEffects.None, 0f, scale);
                marker.Draw(gameTime, spriteBatch, fontWriter);
            }
            
        }

    }
}
