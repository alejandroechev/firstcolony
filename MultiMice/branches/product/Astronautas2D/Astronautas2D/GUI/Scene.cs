using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Astronautas2D.Visual_Components;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Astronautas2D.GUI
{
    public abstract class Scene
    {
        protected List<Texture2D> textures;
        protected List<Text> texts;
        protected Texture2D background;
        // Podriamos poner boundries aqui pero aun no se para que

        public Scene(List<Texture2D>  textures, List<Text> texts, Texture2D background)
        {
            this.textures = textures;
            this.texts = texts;
            this.background = background;
        }

        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch, Writer fontWriter)
        {
            foreach (Texture2D t in textures)
            {
            }
        }

        public abstract void Update(float elapsedTime);

    }
}
