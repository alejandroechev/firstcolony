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
    public class Marker : GUIElement
    {
        private Vector2 basePosition;
        public Vector2 BasePosition
        {
            get
            {
                return basePosition;
            }
            set
            {
                basePosition = value;
            }
        }

        public Marker(Vector2 position, Texture2D idleTexture, bool isVisible, Vector2 scale, float layer)
            : base(position, idleTexture, isVisible, scale, layer)
        {
            basePosition = position;
        }

        public void Move(int level, double delta)
        {
            if (level > 15)
            {
                position.Y = (float)(basePosition.Y - (level-15) * delta);
            }
            else if (level < 15)
            {
                position.Y = (float)(basePosition.Y + (15 - level) * delta);
            }
            else
            {
                position.Y = (float)(basePosition.Y);
            }
            position.X = (float) basePosition.X;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch, Writer fontWriter)
        {
            base.player.PlayAnimation(idleAnimation);
            base.player.Draw(gameTime, spriteBatch, position, SpriteEffects.None, 0f, scale);
        }

    }
}
