using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using GameModel;
using Astronautas2D.GameObjects;
using Astronautas2D.Visual_Components;
using Microsoft.Xna.Framework.Graphics;

namespace Astronautas2D.GameObjects.Entities
{
    public class Zone2D
    {
        private AnimationPlayer player;
        private Animation idleAnimation;
        private Vector2 scale;
        private float radius;
        private Vector2 position;
        public Vector2 Position { get { return position; } }
        private Rectangle shape;
        private playerId playerIndex;
        public playerId PlayerIndex { get { return playerIndex; } }
        private Color color;
        public Color Color { get { return color; } set { color = value; } }

        public Zone2D(float radius, Vector2 scale ,playerId id, Vector2 position, Texture2D idleTexture)
        {
            this.color = Color.White;
            this.playerIndex = id;
            this.position = position;
            this.idleAnimation = new Animation(idleTexture, 0.1f, true, 1);
            this.radius = radius * scale.X;
            this.scale = scale;

            // Determinamos el tamaño del cuadrado dentro del circulo
            //float a = (float)(2 / Math.Sqrt(2) * radius);
            shape = new Rectangle((int)(position.X - radius), (int)(position.Y - radius), (int)radius*2, (int)radius*2);

        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            player.PlayAnimation(idleAnimation);
            player.Draw(gameTime, spriteBatch, new Vector2(shape.Center.X, shape.Center.Y), SpriteEffects.None, 0f, this.scale, color);
        }

        public bool checkAstronaut(Astronaut2D astronaut)
        {
            if (astronaut.PlayerId == this.playerIndex)
            {
                Point p = new Point((int)astronaut.Position.X,(int)astronaut.Position.Y);
                if (shape.Contains(p))
                {
                    this.color = Color.Red;
                    return true;
                }
 
            }
            return false;
        }

        public void Move(Vector2 newPosition)
        {
            this.position = newPosition;
            shape.Location = new Point((int)(newPosition.X + shape.Width / 2), (int)(newPosition.Y + shape.Height / 2));
            this.color = Color.White;
        }
    }
}
