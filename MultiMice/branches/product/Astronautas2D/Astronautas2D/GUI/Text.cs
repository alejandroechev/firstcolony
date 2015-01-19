using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Astronautas2D.Visual_Components;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Astronautas2D.GUI
{
    public class Text
    {
        protected Vector2 position;
        protected Color color;
        protected String text;
        protected bool isVisible;
        public bool IsVisible { get { return isVisible; } }

        public Text(Vector2 p, Color c, String t)
        {
            position = p;
            color = c;
            text = t;
            isVisible = true;
        }

        public void Draw(Writer w)
        {
            w.DrawText(text, position, color);
        }
    }
}