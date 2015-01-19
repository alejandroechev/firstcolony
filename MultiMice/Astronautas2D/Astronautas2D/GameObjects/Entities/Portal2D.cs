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
    public class Portal2D : GameModel.Portal, IGameObject
    {
        private objectId id;
        private state currentState;
        private float rotation;
        private Vector2 scale;
        private Vector3 initialPosition;
        public Vector3 InitialPosition { get { return initialPosition; } }

        private Animation idleAnimation;
        private Animation appearingAnimation;
        private Animation symbolAnimation;
        private AnimationPlayer player;
        private AnimationPlayer symbolPlayer;
        private Vector2 symbolScale = new Vector2(1.5f, 1.5f);

        private bool isVisible;
        public bool IsVisible { get { return isVisible; } set { isVisible = value; } }
        private bool narrative;
        


        public Portal2D(float radius, Vector3 scale, Vector3 position, Texture2D[] textures, bool narrative)
            : base(scale,position)
        {
            this.id = objectId.portal;
            this.initialPosition = position;
            this.rotation = 0;
            this.scale = new Vector2(scale.X,scale.Y);
            idleAnimation = new Animation(textures[(int)state.idle], 0.1f, true, 1);
            appearingAnimation = new Animation(textures[(int)state.appearing], 0.1f, false, 4);
            symbolAnimation = new Animation(textures[(int)state.symbol], 0.1f, false, 1);
            base.Radius = radius * Scale.X;
            this.isVisible = true;
            this.currentState = state.idle;
            this.narrative = narrative;
        }

        #region IGameObjectProperties
        public objectId Id { get { return id; } }
        public Vector2 Position2D { get { return new Vector2(base.Position.X, base.Position.Y); } }
        public float Rotation { get { return rotation; } }
        public Vector2 Scale { get { return scale; } }
        public state CurrentState { get{return currentState;} set{ currentState = value;}}
        public bool Alive { get { return true; } set { } }
        private int listIndex;
        public int ListIndex { get { return listIndex; } set { listIndex = value; } }
        #endregion

        #region IGameObjectMethods
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Writer fontWriter)
        {
            player.PlayAnimation(idleAnimation);
            player.Draw(gameTime, spriteBatch, new Vector2(base.Position.X, base.Position.Y), SpriteEffects.None, this.rotation,this.Scale);
            if (narrative)
            {
                symbolPlayer.PlayAnimation(symbolAnimation);
                symbolPlayer.Draw(gameTime, spriteBatch, new Vector2(base.Position.X, base.Position.Y), SpriteEffects.None, 0.0f, symbolScale, Color.White);
            }
        }
        public override void Update(float elapsedTime)
        {
        }

        public void SetPosition(Vector3 position)
        {
            base.Position = position;
        }

        public override void Reset()
        {
            this.Position = initialPosition;
            this.rotation = 0;
            this.scale = new Vector2(scale.X, scale.Y);
            this.isVisible = true;
            this.currentState = state.idle;
        }

        #endregion
    }
}
