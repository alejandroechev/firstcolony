using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Astronautas2D.Visual_Components;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
namespace Astronautas2D.GUI
{
    public class FloatingText : Text
    {
        private float showTime = 4000f;         // Cuanto tiempo se mostrará
        private const float movement = 0.5f;      // Cuanto se moverá
        private float timeToWait;               // Cuanto queda por mostrarse

        public FloatingText(Vector2 p, Color c, String t)
            : base(p,c,t)
        {
            timeToWait = showTime;
        }

        public void Update(float elapsedTime)
        {
            if (base.isVisible)
            {
                if (timeToWait < 0)
                {
                    base.isVisible = false;
                    timeToWait = 0;
                }
                else
                {
                    base.position = new Vector2(base.position.X, base.position.Y - movement);
                    timeToWait -= elapsedTime;
                }
            }
        }

    }
}
