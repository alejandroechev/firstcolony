using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Astronautas2D.GameObjects;
using Astronautas2D.GameObjects.Entities;

namespace Astronautas2D.Visual_Components
{
    public enum fontType {statusBar, codec, floatingText}

    public class Writer
    {
        private SpriteFont font;
        private SpriteBatch sprite;

        public Writer(SpriteBatch sp)
        {
            sprite = sp;
        }

        public void loadContent(SpriteFont _font)
        {
            font = _font; 
        }

        public void DrawText(String text, Vector2 position, Color color)
        {
            sprite.DrawString(font, text, position, color);
        }
    }
}
