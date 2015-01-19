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
using Microsoft.Xna.Framework.Audio;
using AstroLib;

namespace Astronautas2D.GameObjects.Entities
{
    public enum direction { front, up, down, left, right}

    public class Astronaut2D : GameModel.Astronaut, IGameObject
    {
        private objectId objectId;
        private state currentState;
        private state fieldState;
        private float rotation;
        private Vector2 scale;
        private Vector2 drawingScale;
        private Vector2 fieldScale;
        private Vector2 symbolDelta;
        private Vector2 symbolScale;
        private Color symbolColor;
        private Vector3 initialPosition;
        public Vector3 InitialPosition { get { return initialPosition; } }
        private bool isVisible;
        public bool IsVisible { get{return isVisible;} set {isVisible = value;}}
        private Color fieldColor;
        private bool isGlowing;
        public bool IsGlowing { get { return isGlowing; } }

        // id de jugador asociado
        private playerId playerId;
        public playerId PlayerId
        {
            get { return playerId; }
        }

        // Punto donde se movera
        private Vector2 destination;
        private direction movingDirection;
        // Velocidad de movimiento
        private float stepDistance;
        
        // Cambio de carga
        private float deltaCharge = 1f;
        private float maxCharge = 15f;
        public float MaxCharge { get { return maxCharge; } }
        private int chargeLevels;
        public int ChargeLevels{ get { return chargeLevels; }}

        public int ChargeLevel{ get { return (int)(offlineCharge / deltaCharge + maxCharge / deltaCharge);}}

        // Animaciones
        private Animation idleAnimation;
        private Animation appearingAnimation;
        private Animation upAnimation;
        private Animation downAnimation;
        private Animation leftAnimation;
        private Animation rightAnimation;
        private Animation dyingAnimation;
        private Animation symbolAnimation;
        private AnimationPlayer player;
        private AnimationPlayer symbolPlayer;
        private Animation[] fieldAnimations;
        private AnimationPlayer fieldPlayer;
        private bool narrative;

        // Valor para almacenar la carga
        private float offlineCharge;
        public float OfflineCharge { get { return offlineCharge; } set { offlineCharge = value; } }

        // Flag que indica si está vivo o no
        private bool isAlive;

        // Boolean que indica si esta activo el campo de fuerza
        public bool IsActive
        {
            get { return base.isActive; }
        }
        // Mis colores
        private Color myRed;
        private Color myBlue;

        private Vector2 aspectRatio;


        public Astronaut2D(Rectangle body, float radius, playerId id, float mass, Vector3 scale, Vector3 position, Animation[] animations, Texture2D[] fieldTextures, bool narrative, Vector2 aspectRatio)
            : base(mass, position)
        {
            this.movingDirection = direction.front;
            this.aspectRatio = aspectRatio;
            this.narrative = narrative;
            this.isGlowing = false;
            this.playerId = id;
            this.objectId = objectId.astronaut;
            this.initialPosition = position;
            this.rotation = 0;
            base.isActive = false;
            this.isVisible = true;
            this.isAlive = true;
            this.scale = new Vector2(scale.X, scale.Y);

            // Escala de dibujo
            float drawWidth, drawHeight;
            float auxWidth = (float)animations[(int)state.idle].FrameWidth * (float)scale.X;
            float auxHeight = (float)animations[(int)state.idle].FrameHeight * (float)scale.Y;
            drawWidth = auxWidth / (float)body.Width;
            drawHeight = auxHeight / (float)body.Height;
            float factor = (float)Math.Max(drawWidth, drawHeight);
            drawingScale = new Vector2(factor * scale.X, factor * scale.Y); // Escala con la cual se dibujará
            //Debemos calcular la escala del campo
            float fieldFactor = ((float)((float)Math.Max(auxWidth,auxHeight) + 5f));
            fieldFactor /= fieldTextures[(int)state.idle].Height;
            this.fieldScale = new Vector2(fieldFactor, fieldFactor);

            // Tamaño del personaje
            base.Radius = radius * scale.X;
            //Animaciones
            this.idleAnimation = animations[(int)state.idle];
            this.upAnimation = animations[(int)state.up];
            this.downAnimation = animations[(int)state.down];
            this.leftAnimation = animations[(int)state.left];
            this.rightAnimation = animations[(int)state.right];
            this.appearingAnimation = animations[(int)state.appearing];
            this.dyingAnimation = animations[(int)state.dying];
            this.symbolAnimation = animations[(int)state.symbol];
            this.LoadFieldAnimations(fieldTextures);
            // Factores de movimiento
            this.stepDistance = 1.5f;
            this.currentState = state.idle;
            this.fieldState = state.dead;
            this.destination = new Vector2(-1, -1);
            base.Charge = 0.0f;
            this.offlineCharge = 0.0f;
            this.chargeLevels = (int)((maxCharge * 2) / deltaCharge);
            this.fieldColor = Color.White;
            // mis colores
            myBlue = new Color(12, 202, 255);
            myRed = new Color(255, 0, 0);
            // Symbolo
            symbolDelta = new Vector2(0, -34 * aspectRatio.Y);
            symbolScale = new Vector2(0.8f * aspectRatio.X, 0.8f * aspectRatio.Y);
            symbolColor = Color.Black;


        }

        private void UpdateDirection()
        {
            if (destination.X == -1 && destination.Y == -1)
            {
                //Estamos quietos
                movingDirection = direction.front;
            }
            else
            {
                // Nos movemos
                Vector2 dir = destination - Position2D;
                float m = dir.Y / dir.X;


                if (dir.X > 0)
                {
                    if (m > 1 || float.IsPositiveInfinity(m))
                    {
                        movingDirection = direction.down;
                    }
                    else if (m <= 1 && m >= -1)
                    {
                        movingDirection = direction.right;
                    }
                    else
                    {
                        movingDirection = direction.up;
                    }
                }
                else
                {
                    if (m > 1 || float.IsPositiveInfinity(m))
                    {
                        movingDirection = direction.up;
                    }
                    else if (m <= 1 && m >= -1)
                    {
                        movingDirection = direction.left;
                    }
                    else
                    {
                        movingDirection = direction.down;
                    }
                }

                /*
                double radians = Math.Asin(dir.Y / dir.X);
                double angle = radians * ((double)180 / Math.PI);

                if (angle <= 45 && angle > -45)
                {
                    movingDirection = direction.right;
                }
                else if (angle > 45 && angle <= 135)
                {
                    movingDirection = direction.up;
                }
                else if (angle > 135 && angle <= 225)
                {
                    movingDirection = direction.left;
                }
                else
                {
                    movingDirection = direction.down;
                }
                 */


                /*
                if (dir.Y > 0)
                {
                    movingDirection = direction.down;
                }
                else if (dir.Y < 0)
                {
                    movingDirection = direction.up;
                }

                if (dir.X > 0)
                {
                    movingDirection = direction.right;
                }
                else if (dir.X < 0)
                {
                    movingDirection = direction.left;
                }
                 */

            }

        }


        private void LoadFieldAnimations(Texture2D[] fieldTextures)
        {
            this.fieldAnimations = new Animation[fieldTextures.Length];
            for (int i = 0; i < fieldTextures.Length; i++)
            {
                Texture2D aux = fieldTextures[i];
                if (aux != null)
                {
                    this.fieldAnimations[i] = new Animation(aux, 0.1f, true, (int)(aux.Width / aux.Height));
                }
            }
        }

        #region IGameObjectProperties
        public objectId Id { get { return objectId; } }
        public Vector2 Position2D { get { return new Vector2(base.Position.X, base.Position.Y); } }
        public float Rotation { get { return rotation; } }
        public Vector2 Scale { get { return scale; } }
        public bool Alive { get { return this.isAlive; } set { this.isAlive = value; } }
        public state CurrentState { get{return currentState;} set{ currentState = value;}}
        private int listIndex;
        public int ListIndex { get { return listIndex; } set { listIndex = value; } }
        #endregion

        #region IGameObjectMethods
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            player.PlayAnimation(idleAnimation);
            player.Draw(gameTime, spriteBatch, Position2D, SpriteEffects.None,this.rotation,this.scale);
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Writer fontWriter)
        {
            //bool drawSymbol = false;

            if (currentState == state.dying)
            {
                player.PlayAnimation(dyingAnimation);
            }
            else
            {
                //drawSymbol = true;
                symbolPlayer.PlayAnimation(symbolAnimation);
                switch (movingDirection)
                {
                    case direction.front:
                        player.PlayAnimation(idleAnimation);
                        break;

                    case direction.up:
                        player.PlayAnimation(upAnimation);
                        break;

                    case direction.down:
                        player.PlayAnimation(downAnimation);
                        break;
                    case direction.left:
                        player.PlayAnimation(leftAnimation);
                        break;

                    case direction.right:
                        player.PlayAnimation(rightAnimation);
                        break;

                    default:
                        player.PlayAnimation(idleAnimation);
                        break;
                }
            }

            // Si está activo dibujamos ambos
            if (isGlowing)
            {
                fieldPlayer.PlayAnimation(fieldAnimations[(int)fieldState]);
                player.Draw(gameTime, spriteBatch, Position2D, SpriteEffects.None, this.rotation, this.drawingScale, fieldColor);
                //if (drawSymbol && narrative) symbolPlayer.Draw(gameTime, spriteBatch, Position2D + symbolDelta, SpriteEffects.None, 0.0f, symbolScale, symbolColor);
                fieldPlayer.Draw(gameTime, spriteBatch, Position2D, SpriteEffects.None, this.rotation, this.fieldScale, fieldColor);
            }
            else
            {
                // Solo dibujamos al astronauta
                player.Draw(gameTime, spriteBatch, Position2D, SpriteEffects.None, this.rotation, this.drawingScale);
                //if (drawSymbol && narrative) symbolPlayer.Draw(gameTime, spriteBatch, Position2D + symbolDelta, SpriteEffects.None, 0.0f, symbolScale, symbolColor);
                if (fieldState == state.dying)
                {
                    fieldPlayer.PlayAnimation(fieldAnimations[(int)fieldState]);
                    fieldPlayer.Draw(gameTime, spriteBatch, Position2D, SpriteEffects.None, this.rotation, this.fieldScale, fieldColor);
                }
            }
        }


        public void UpdateActive()
        {
            if (!isAlive)
            {
                this.isGlowing = false;
                base.isActive = false;
            }
        }

        public void SetPosition(Vector3 position)
        {
            base.Position = position;
        }
        #endregion

        /// <summary>
        /// Método que mueve al objeto en la dirección del destino
        /// </summary>
        /// <param name="destination"></param>
        public bool Move()
        {
            Vector2 move = this.destination - this.Position2D;
            float distance = move.Length();
            float movementFactor = 0;
            move.Normalize();
            float step = this.stepDistance;
            if (isGlowing)
            {
                step /= 2;
            }

            // Definimos la distancia que nos moveremos
            if (distance == 0)
            {
                destination = new Vector2(-1, -1);
                return false;
            }
            else if (distance < step)
            {
                movementFactor = distance;
            }
            else
            {
                movementFactor = step;
            }

            this.Position = new Vector3(Position.X + movementFactor * move.X, Position.Y + movementFactor * move.Y, Position.Z);
            return true;

        }

        /// <summary>
        /// Método que actualiza el estado del astronauta segun el input del usuario
        /// </summary>
        /// <param name="elapsedTime"></param>
        /// <param name="miceManager"></param>
        public override void Update(float elapsedTime)
        {
            this.UpdateDirection();

            //Actualizamos
            if ((currentState == state.dying) && (player.Animation == dyingAnimation) && player.isFinalFrame())
            {
                currentState = state.dead;
                this.isVisible = false;
            }
            else if (currentState == state.moving)
            {
                if (this.destination.X != -1 && this.destination.Y != -1)
                {
                    if (this.Move())
                    {
                        this.currentState = state.moving;
                    }
                    else
                    {
                        this.currentState = state.idle;
                    }
                }
            }

            // Actualizamos el estado del campo electrico

            if (isGlowing)
            {
                if (fieldState == state.appearing && fieldPlayer.Animation == fieldAnimations[(int)state.appearing] && fieldPlayer.isFinalFrame())
                {
                    fieldState = state.idle;
                }
            }
            else
            {
                if (fieldState == state.dying && fieldPlayer.Animation == fieldAnimations[(int)state.dying] && fieldPlayer.isFinalFrame())
                {
                    fieldState = state.dead;
                }
            }

            this.UpdateColor();

            // Actualizamos si esta vivo o no
            UpdateActive();
        }

        public void UpdateColor()
        {
            float percCharge = Math.Abs(offlineCharge / maxCharge);
            if (offlineCharge > 0)
            {
                fieldColor = InterpolateColors(myBlue, Color.White,percCharge);
            }
            else if (offlineCharge < 0)
            {
                fieldColor = InterpolateColors(myRed, Color.White, percCharge);
            }
            else
            {
                fieldColor = Color.White;
            }
        }

        private Color InterpolateColors(Color a, Color b, float t)
        {
            float tquad = t * (2 - t);
            Vector3 av = a.ToVector3();
            Vector3 bv = b.ToVector3();
            return new Color(tquad * av + (1 - tquad) * bv);
        }

        public void onLeftClick(Vector2 position)
        {
            if (currentState == state.idle || currentState == state.moving)
            {
                this.destination = position;
                currentState = state.moving;  
            }
        }

        public void UpdateGlow()
        {
            if (!this.isGlowing)
            {
                SoundManager.Instance.Play(Sounds.fieldOn);
                this.isGlowing = true;
                this.fieldState = state.appearing;
            }
            else
            {
                SoundManager.Instance.Play(Sounds.fieldOff);
                this.isGlowing = false;
                this.fieldState = state.dying;
            }
        }


        public void onRightClick(Vector2 position)
        {
            SoundManager.Instance.Play(Sounds.shoot);
        }



        public void onCenterClick(Vector2 position)
        {
            if (!base.isActive)
            {
                this.isGlowing = true;
                base.isActive = true;
                this.fieldState = state.appearing;
                SoundManager.Instance.Play(Sounds.fieldOn);
            }
            else
            {
                this.isGlowing = false;
                base.isActive = false;
                this.fieldState = state.dying;
                SoundManager.Instance.Play(Sounds.fieldOff);
            }
        }



        public void Die()
        {
            SoundManager.Instance.Play(Sounds.astroDeath);
            this.isGlowing = false;
            base.isActive = false;
            currentState = state.dying;
        }


        public void ActivateCharge()
        {
            if (!this.ActiveForces)
            {
                this.isGlowing = true;
                base.isActive = true;
                this.fieldState = state.appearing;
            }
        }

        public void DeactivateCharge()
        {
            if (this.ActiveForces)
            {
                this.isGlowing = false;
                base.isActive = false;
                this.fieldState = state.dying;
            }
        }

        public void onWheelChange(int delta)
        {
            if (delta == 1)
            {
                if (offlineCharge != maxCharge)
                {
                    offlineCharge = (float)Math.Floor((double)offlineCharge + (double)deltaCharge);
                    base.Charge = offlineCharge;
                }
            }
            else
            {
                if (offlineCharge != -maxCharge)
                {
                    offlineCharge = (float)Math.Floor((double)offlineCharge - (double)deltaCharge);
                    base.Charge = offlineCharge;
                }
            }
            
        }

        public Vector2 Destination
        {
            get { return destination; }
            set { destination = value; }
        }

        public override void Reset()
        {
            this.Position = initialPosition;
            this.rotation = 0;
            base.isActive = false;
            this.isGlowing = false;
            this.isVisible = true;
            this.isAlive = true;
            this.currentState = state.idle;
            this.fieldState = state.dead;
            this.destination = new Vector2(-1, -1);
            base.Charge = 0.0f;
            this.offlineCharge = 0.0f;
            this.fieldColor = Color.White;
        }

    }
}
