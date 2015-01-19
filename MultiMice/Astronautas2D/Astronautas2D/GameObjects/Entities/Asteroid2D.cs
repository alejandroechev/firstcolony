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
    public class Asteroid2D : GameModel.Asteroid, IGameObject
    {
        private objectId id;
        private state currentState;
        private float rotation;
        private Vector2 scale;
        private Vector3 initialPosition;
        public Vector3 InitialPosition { get { return initialPosition; } }
        private bool isVisible;
        public bool IsVisible { get { return isVisible; } set { isVisible = value; } }
        private float lifeTime;
        public float LifeTime { get { return lifeTime; } set { lifeTime = value; } }
        private float timeToWait;
        public float TimetoWait { get { return timeToWait; } set { timeToWait = value; } }
        private float initialWait;
        public float InitialWait { set { initialWait = value; } }

        // Animaciones
        private Animation idleAnimation;
        private Animation appearingAnimation;
        private AnimationPlayer player;


        public Asteroid2D(float radius, float mass, Vector3 scale, Vector3 position, Vector3 velocity, bool inertial,Texture2D[] textures)
            : base(scale,mass,position,velocity,inertial)
        {
            lifeTime = float.PositiveInfinity;
            timeToWait = 0;
            initialWait = 0;
            this.id = objectId.asteroid;
            this.initialPosition = position;
            this.rotation = 0;
            base.isAlive = true;
            this.isVisible = true;
            this.scale = new Vector2(scale.X, scale.Y);
            base.Radius = radius*this.scale.X;
            this.idleAnimation = new Animation(textures[(int)state.idle], 0.1f, false,1);
            this.appearingAnimation = new Animation(textures[(int)state.appearing], 0.1f, false, 4);
            this.currentState = state.idle;
        }

        #region IGameObjectProperties
        public objectId Id { get { return id; } }
        public Vector2 Position2D { get { return new Vector2(base.Position.X, base.Position.Y); } }
        public float Rotation { get { return rotation; } }
        public Vector2 Scale { get { return scale; } }
        public state CurrentState { get{return currentState;} set{ currentState = value;}}
        public bool Alive { get { return base.isAlive; } set { base.isAlive = value; }}
        private int listIndex;
        public int ListIndex { get { return listIndex; } set { listIndex = value; } }
    
        #endregion

        #region IGameObjectMethods
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Writer fontWriter)
        {
            if(currentState == state.appearing)
            {
                player.PlayAnimation(appearingAnimation);
            }
            else
            {
                player.PlayAnimation(idleAnimation);
            }
            player.Draw(gameTime, spriteBatch, new Vector2(position.X,position.Y), SpriteEffects.None,this.rotation,this.scale);
        }

        public override void Update(float elapsedTime)
        {
            if (initialWait > 0)
            {
                initialWait -= elapsedTime;
            }
            else
            {
                base.Update(elapsedTime);
                this.rotation += 0.03f;

                if (!float.IsPositiveInfinity(lifeTime))
                {
                    this.timeToWait -= elapsedTime;

                    if (this.timeToWait <= 0)
                    {
                        // Deberiamos reposicionarlo
                        //Vector3 delta = Math.Abs(this.elapsedTime) * base.Velocity;
                        //this.Position = base.spawnPosition + delta;
                        //this.elapsedTime = this.lifeTime + elapsedTime;
                        this.SetPosition(base.spawnPosition);
                        this.timeToWait = this.lifeTime;
                    }
                }
            }
        }

        public void SetPosition(Vector3 position)
        {
            base.Position = position;
        }

        public override void Reset()
        {
            this.position = initialPosition;
            this.rotation = 0;
            base.isAlive = true;
            this.isVisible = true;
            this.currentState = state.idle;
        }

        #endregion

    }
}
