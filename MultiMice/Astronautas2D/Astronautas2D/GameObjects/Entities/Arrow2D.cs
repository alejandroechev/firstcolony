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
    public enum arrow {head, body, symbol}

    public class Arrow2D
    {
        private AnimationPlayer bodyPlayer;
        private AnimationPlayer headPlayer;
        private AnimationPlayer symbolPlayer;
        private Animation bodyAnimation;
        private Animation headAnimation;
        private Animation symbolAnimation;
        private Vector2 position;
        private Vector2 scale;
        private float rotation;
        private SpriteEffects flip;
        private float minSize;
        private float maxSize;
        private Vector2 direction;
        private Vector2 headPosition;
        private Vector2 symbolPosition;
        private Color arrowColor;
        public Color ArrowColor { get { return arrowColor; } set { arrowColor = value; } }
        private Color symbolColor;
        public Color SymbolColor { get { return symbolColor; } set { symbolColor = value; } }
        private Vector2 aspectRatio;

        public Arrow2D(playerId id, Vector2 position, Texture2D[,] textures, Vector2 aspectRatio)
        {
            this.arrowColor = new Color(16, 237, 26);
            this.SymbolColor = Color.Black;
            this.bodyAnimation = new Animation(textures[(int)id, (int)arrow.body], 0.1f, true, 1);
            this.headAnimation = new Animation(textures[(int)id, (int)arrow.head], 0.1f, true, 1);
            this.symbolAnimation = new Animation(textures[(int)id, (int)arrow.symbol], 0.1f, true, 1);
            this.position = position;
            this.scale = new Vector2(1,1);
            this.aspectRatio = aspectRatio;
            minSize = 0.6f * aspectRatio.X;
            maxSize = minSize + 2.5f * aspectRatio.X;
        }

        public Vector2 getHeadPosition()
        {
            Vector2 head = position + (bodyAnimation.FrameWidth/2*scale.X)* direction;
            return head;
        }

        public Vector2 getSymbolPosition()
        {
            Vector2 symbol = headPosition + (20 * aspectRatio.X) * direction;
            return symbol;
        }

        public void Update(Vector2 newPosition, Vector2 force)
        {   
            // Determinamos la rotación
            rotation = (float)Math.Atan((double)(force.Y / force.X));

            float size = minSize + force.Length();

            if (size > maxSize)
            {
                size = maxSize;
            }
           
            this.scale.X = size;

            // Determinamos la posición
            this.position = newPosition;


            // Determinamos el Giro del sprite
            if (force.X < 0)
            {
                flip = SpriteEffects.FlipHorizontally;
            }
            else
            {
                flip = SpriteEffects.None;
            }
            force.Normalize();
            direction = new Vector2(force.X,force.Y);
            headPosition = this.getHeadPosition();
            symbolPosition = this.getSymbolPosition();
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            bodyPlayer.PlayAnimation(bodyAnimation);
            bodyPlayer.Draw(gameTime, spriteBatch, new Vector2(position.X, position.Y), flip, this.rotation, this.scale, arrowColor);
            headPlayer.PlayAnimation(headAnimation);
            headPlayer.Draw(gameTime, spriteBatch, new Vector2(headPosition.X, headPosition.Y), flip, this.rotation, new Vector2(1, 1), arrowColor);
            symbolPlayer.PlayAnimation(symbolAnimation);
            symbolPlayer.Draw(gameTime, spriteBatch, new Vector2(symbolPosition.X, symbolPosition.Y), flip, 0f, new Vector2(1, 1), symbolColor);

        }
    }
}
