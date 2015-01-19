using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Astronautas2D.Visual_Components;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Astronautas2D.GUI
{
    public enum guiElements {hud, slider, marker, time, score, codec, portrait, objectiveBoard, medal, crystal};

    public abstract class GUIElement
    {
        protected AnimationPlayer player;
        protected Animation idleAnimation;
        protected Vector2 position;
        public virtual Vector2 Position 
        { 
            get { return position; } 
            set 
            { 
                position = value;
                shape.Location = new Point((int)(value.X - shape.Width / 2), (int)(value.Y - shape.Height / 2));
            }
        } 
        protected bool isVisible;
        public bool IsVisible { get { return isVisible; } set { isVisible = value; }}
        protected float layer;
        public float Layer { get { return layer; } }
        protected Rectangle shape;
        public Rectangle Shape { get { return shape; } set { shape = value; } }
        protected Vector2 scale;
        public Vector2 Scale 
        { 
            get { return scale; } 
            set 
            { 
                // Al cambiar la escala cambia la forma del objeto
                scale = value;
                shape = new Rectangle((int)position.X, (int)position.Y, (int)(idleAnimation.FrameWidth * scale.X), (int)(idleAnimation.FrameHeight * scale.Y));
            } 
        }

        public GUIElement(Vector2 position, Texture2D idleTexture, bool isVisible, Vector2 scale, float layer)
        {
            this.position = position;
            this.idleAnimation = new Animation(idleTexture, 0.1f, false, 1);
            this.isVisible = isVisible;
            this.layer = layer;
            this.scale = scale;
            shape = new Rectangle((int)position.X - (int)(idleAnimation.FrameWidth * scale.X * 1 / 2), (int)position.Y - (int)(idleAnimation.FrameHeight * scale.Y * 1/2), (int)(idleAnimation.FrameWidth * scale.X), (int)(idleAnimation.FrameHeight * scale.Y));
        }

        protected Vector2 getUpperLeftCorner()
        {
            return new Vector2(shape.Left, shape.Top);
        }

        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch, Writer fontWriter)
        {
            player.PlayAnimation(idleAnimation);
            player.Draw(gameTime, spriteBatch, position, SpriteEffects.None, 0f, scale);
        }

        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch, Writer fontWriter, Color c)
        {
            player.PlayAnimation(idleAnimation);
            player.Draw(gameTime, spriteBatch, position, SpriteEffects.None, 0f, scale,c);
        }



        public virtual void Update(float elapsedTime){ }


    }
}
