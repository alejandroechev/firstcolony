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
    public class Objeto2D:IGameObject
    {
        public Texture2D sprite;
        public Vector2 position;
        public float rotation;
        public Vector2 center;
        public Vector2 velocity;      
        public float scale;

        public Objeto2D(Texture2D loadedTexture, Vector2 positionO, float scaleO)
        {
            rotation = 0.0f;
            position = positionO;
            sprite = loadedTexture;
            center = new Vector2(sprite.Width / 2, sprite.Height / 2);
            velocity = Vector2.Zero;
            scale = scaleO;
            isAlive=true;
        }

        public override void Draw(AbstractGame Game)
        {
            Draw(Game, Color.White);
        }

        public void Draw(AbstractGame Game, Color c)
        {
            Game.spriteBatch.Draw(this.sprite,
                             this.position,
                             null,
                             c,
                             this.rotation,
                             this.center,
                             this.scale,
                             SpriteEffects.None, 0f);
        }

        public override void Update(GameTime gameTime)
        {
        }

    }
}
