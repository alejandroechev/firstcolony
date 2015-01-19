using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAnimation.Controllers;

namespace CargaElectricaDemo1
{
    public abstract class AbstractGame : Game
    {
        public SpriteBatch spriteBatch;
        public CamaraAjustable Camara;
        public AnimationController[] animationController;


    }
}
