using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using GameModel;
using Astronautas2D.GameObjects;
using Astronautas2D.Visual_Components;
using Microsoft.Xna.Framework.Graphics;
using Astronautas2D.Utils;

namespace Astronautas2D.GameObjects.Entities
{
    public class Bullet2D : GameModel.Bullet, IGameObject
    {
        private objectId id;
        private state currentState;
        private float rotation;
        private Vector2 scale;
        private Vector3 initialPosition;
        public Vector3 InitialPosition { get { return initialPosition; } }
        // Animations
        private Animation idleAnimation;
        private Animation dyingAnimation;
        private AnimationPlayer player;
        // Direction
        private Vector2 shootDirection;
        public Vector2 ShootDirection { get { return shootDirection; } }
        // Velocidad de movimiento
        private float step = 0.009f;
        // Si esta activa o no
        private float lifetime = 4000f;
        private float timer = 0;
        // Si se dibuja o no
        private bool isVisible;
        public bool IsVisible { get { return isVisible; } set { isVisible = value; } }


        public Bullet2D(float radius, Vector3 position, Texture2D[] textures, Vector2 scale)
            : base(1.0f, position, new Vector3(0f, 0f, 0f))
        {
            this.initialPosition = position;
            this.id = objectId.bullet;
            this.rotation = 0;
            this.scale = scale;
            base.Radius = radius * this.scale.X;
            this.idleAnimation = new Animation(textures[(int)state.idle], 0.1f, true, 1);
            this.dyingAnimation = new Animation(textures[(int)state.dying], 0.1f, false, 4);
            // La bala se construye "apagada"
            base.isAlive = false;
            this.isVisible = false;
            shootDirection = new Vector2(-1, -1);
        }

        public void Shoot(Vector2 target, Vector2 startingPos)
        {
            if (!isAlive && !isVisible)
            {
                shootDirection = target - startingPos;
                shootDirection.Normalize();
                base.Reset();
                this.currentState = state.idle;
                base.position = new Vector3(startingPos.X, startingPos.Y, 1);
                base.ApplyForce(new Vector3(shootDirection.X * step, shootDirection.Y * step, 1));
                base.isAlive = true;
                this.isVisible = true;
                this.timer = 0f;
            }
        }


        #region IGameObjectProperties
        public objectId Id { get { return id; } }
        public Vector2 Position2D { get { return new Vector2(base.Position.X, base.Position.Y); } }
        public float Rotation { get { return rotation; } }
        public Vector2 Scale { get { return scale; } }
        public state CurrentState { get { return currentState; } set { currentState = value; } }
        public bool Alive { get { return base.isAlive; } set { base.isAlive = value; } }
        private int listIndex;
        public int ListIndex { get { return listIndex; } set { listIndex = value; } }
        #endregion


        #region IGameObjectMethods
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Writer fontWriter)
        {
            if (currentState == state.idle)
            {
                this.player.PlayAnimation(idleAnimation);
            }
            else if (currentState == state.dying)
            {
                this.player.PlayAnimation(dyingAnimation);
            }
            player.Draw(gameTime, spriteBatch, new Vector2(position.X, position.Y), SpriteEffects.None, this.rotation, this.Scale, Color.White);
        }

        public override void Update(float elapsed_time)
        {
            if (isAlive)
            {
                base.Update(elapsed_time);

                if (currentState == state.idle)
                {
                    if (timer > lifetime)
                    {
                        this.Explode();
                    }
                    else
                    {
                        rotation += 0.1f;
                        timer += elapsed_time;
                    }
                }
            }
            else
            {
                if (currentState == state.dying && player.Animation == dyingAnimation && player.isFinalFrame())
                {
                    currentState = state.waiting;
                    isVisible = false;
                }
            }

        }

        public void SetPosition(Vector3 position)
        {
            base.Position = position;
        }
        #endregion


        public override void Collision(IPhysicBody OtherBody)
        {
        }

        public void Explode()
        {
            SoundManager.Instance.Play(Sounds.bulletDeath);
            Vector2 aux = this.Position2D;
            base.Reset();
            this.Position = new Vector3(aux.X, aux.Y, 1);
            this.isVisible = true;
            this.isAlive = false;
            currentState = state.dying;
            rotation = 0f;
            timer = 0;
        }

        public override void Reset()
        {
            this.position = initialPosition;
            this.rotation = 0;
            // La bala se construye "apagada"
            base.isAlive = false;
            this.isVisible = false;
            shootDirection = new Vector2(-1, -1);
        }
    }
}