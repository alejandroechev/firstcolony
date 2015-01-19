using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace CargaElectricaDemo1
{
    
    // Using this to handle 3d & 2d objects simply as objects
    public abstract class IGameObject
    {
        public bool isAlive;

        public abstract void Update(GameTime gameTime);

        public abstract void Draw(AbstractGame Game);
    }
}
