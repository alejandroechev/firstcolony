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
    public class Icon: GUIElement
    {
        private bool floatAndDissapear;
        private float showTime = 3000f;         // Cuanto tiempo se mostrará
        private const float movement = 0.5f;      // Cuanto se moverá
        private float timeToWait;               // Cuanto queda por mostrarse

        public Icon(Texture2D texture, Vector2 position, Vector2 scale, float layer, bool floatAndDissapear) :
            base(position, texture, true, scale, layer)
        {
            this.floatAndDissapear = floatAndDissapear;
            timeToWait = showTime;
        }

        public override void Update(float elapsedTime)
        {
            if (floatAndDissapear)
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
}
