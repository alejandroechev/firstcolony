using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using GameModel;
using Astronautas2D.GameObjects;
using Astronautas2D.Visual_Components;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Astronautas2D.Utils;

namespace Astronautas2D.GameObjects.Entities
{
    public enum ColorIntensity {high, medium, low};

    public class Crystal2D : GameModel.Crystal, IGameObject
    {
        private objectId id;
        private state currentState;
        private float rotation;
        private Vector2 scale;
        private Vector3 initialPosition;
        public Vector3 InitialPosition { get { return initialPosition; } }
        private Vector3 respawnPosition;
        public Vector3 RespawnPosition { get { return respawnPosition; } set { respawnPosition = value; } }
        // Animations
        private Animation idleAnimation;
        private Animation appearingAnimation;
        private Animation signalAnimation;
        private Animation dyingAnimation;
        private Animation symbolAnimation;
        private AnimationPlayer player;
        private AnimationPlayer symbolPlayer;
        private Color color;
        public Color Color { get { return color; } set { color = value; } }

        // Mis colores
        public Color highRed, mediumRed, lowRed;
        public Color highBlue, mediumBlue, lowBlue;
        private ColorIntensity intensity;
        public ColorIntensity Intensity { 
            get { return intensity; } 
            set 
            { 
                intensity = value;
                this.color = getColor();
            } 
        }

        private bool playSound;
        private bool auxRandomize;
        private bool respawnable;
        public bool Respawnable{ get { return respawnable; } set { respawnable = value; }}
        // La flecha que mostrará la fuerza resultante
        private Arrow2D totalForceArrow;
        private Arrow2D[] otherForcesArrows;
        private Vector2[] otherForces;
        // Colores de las flechas
        private Color narrativeIndividualActiveColor = new Color(255, 119, 0, 200);
        private Color narrativeIndividualInactiveColor = new Color(255, 204, 170, 200);
        private Color narrativeTotalColor = Color.Green;
        private Color narrativeSymbolColor = Color.White;
        private Color noNarrativeIndividualActiveColor = new Color(30, 30, 30, 200);
        private Color noNarrativeIndividualInactiveColor = new Color(150, 150, 150, 200);
        private Color noNarrativeTotalColor = Color.Green;
        private Color noNarrativeSymbolColor = Color.Black;

        // Crystal del cual surgio este crystal
        private Crystal2D father;
        public Crystal2D Father { get { return father; } set { father = value; } }
        // Crystales hijos
        private List<Crystal2D> sons;
        public int SonsAlive { get { return sons.Count; } }
        private int sonsSaved;
        public int SonsSaved { get { return sonsSaved; } set { sonsSaved = value; } }
        protected bool isShattered;
        public bool IsShattered { get { return isShattered; } set { isShattered = value; } }
        private Vector2 shatterDirection;
        public Vector2 ShatterDirection
        {
            set { shatterDirection = value; }
            get { return shatterDirection; }
        }

        // bool que indica si se debe dibujar o no
        private bool isVisible;
        public bool IsVisible { get { return isVisible; } set { isVisible = value; } }

        // Tiempo de vida
        private const float lifeTime = 5000;
        private bool startCounting = false;
        private float timeToDie;

        // Tiempos
        private const float invisibleTime = 2500f;
        private const float signalTime = 2300f;
        private float timeToWait;

        // bool que indica si se debe randomizar
        private bool randomize;
        public bool Randomize { get { return randomize; } set { randomize = value; } }

        private bool showTotalForce;
        private bool showIndividualForces;
        private bool narrative;

        private Vector2 aspectRatio;

        public Crystal2D(float radius, float mass, Vector3 scale, Vector3 position, Vector3 velocity, 
            bool inertial, float charge, bool isAlive, bool isDivisible, Animation[] animations, Texture2D[,] arrowTextures,bool showTotalForce, bool showIndividualForces, bool narrative, Vector2 aspectRatio)
            : base(scale, mass, position, velocity, inertial, charge, isAlive, isDivisible)
        {
            this.playSound = true;
            this.aspectRatio = aspectRatio;
            this.narrative = narrative;
            this.showTotalForce = showTotalForce;
            this.showIndividualForces = showIndividualForces;
            this.isShattered = false;
            this.sons = new List<Crystal2D>();
            this.sonsSaved = 0;
            this.father = null;
            this.id = objectId.crystal;
            this.initialPosition = position;
            this.rotation = 0;
            this.respawnPosition = position;
            base.isAlive = true;
            this.isVisible = true;
            this.respawnable = true;
            this.scale = new Vector2(scale.X, scale.Y);
            // el radio
            base.Radius = radius * this.Scale.X;
            // Animaciones
            this.idleAnimation = animations[(int)state.idle];
            this.appearingAnimation = animations[(int)state.appearing];
            this.dyingAnimation = animations[(int)state.dying];
            this.signalAnimation = animations[(int)state.signal];
            this.symbolAnimation = animations[(int)state.symbol];
            // Flechas
            this.totalForceArrow = new Arrow2D(playerId.team, this.Position2D, arrowTextures, aspectRatio);
            
            if (narrative) this.totalForceArrow.ArrowColor = narrativeTotalColor;
            else this.totalForceArrow.ArrowColor = noNarrativeTotalColor;
            
            this.otherForcesArrows = new Arrow2D[4];
            this.otherForces = new Vector2[4];
            for (int i = 0; i < 4; i++)
            {
                otherForces[i] = new Vector2();
                otherForcesArrows[i] = new Arrow2D((playerId)i, this.Position2D, arrowTextures, aspectRatio);
                if (narrative) otherForcesArrows[i].SymbolColor = narrativeSymbolColor;
                else otherForcesArrows[i].SymbolColor = noNarrativeSymbolColor;
            }
            this.currentState = state.idle;
            this.timeToWait = 0;

            // Mis colores
            //highBlue = new Color(8,187,241);
            highBlue = new Color(12, 202, 255);
            mediumBlue = new Color(12, 202, 255);
            lowBlue = new Color(2, 68, 165);
            highRed = new Color(255,0,0);
            mediumRed = new Color(255, 0, 0);
            lowRed = new Color(122, 0, 0);
            intensity = ColorIntensity.high;
            this.color = getColor();
        }

        #region IGameObjectProperties
        public objectId Id { get { return id; } }
        public Vector2 Position2D { get { return new Vector2(base.Position.X, base.Position.Y); } }
        public float Rotation { get { return rotation; } }
        public Vector2 Scale { get { return scale; } }
        public state CurrentState { get{return currentState;} set{ currentState = value;}}
        public bool Alive { get { return base.isAlive; } set { base.isAlive = value; } }
        private int listIndex;
        public int ListIndex { get { return listIndex; } set { listIndex = value; } }
        #endregion


        #region IGameObjectMethods
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Writer fontWriter)
        {
            bool drawSymbol = false;

            if (currentState == state.idle)
            {
                this.player.PlayAnimation(idleAnimation);
                this.symbolPlayer.PlayAnimation(symbolAnimation);
                if (showIndividualForces)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        otherForcesArrows[i].Draw(gameTime, spriteBatch);
                    }
                }
                if (showTotalForce)
                {
                    totalForceArrow.Draw(gameTime, spriteBatch);
                }
                drawSymbol = true;
            }
            else if (currentState == state.dying)
            {
                this.player.PlayAnimation(dyingAnimation);
                color = Color.White;
            }
            else if (currentState == state.signal)
            {
                this.player.PlayAnimation(signalAnimation);
                color = Color.Orange;
            }
            else if (currentState == state.appearing)
            {
                this.player.PlayAnimation(appearingAnimation);
                color = Color.White;
            }
            player.Draw(gameTime, spriteBatch, new Vector2(position.X, position.Y), SpriteEffects.None, this.rotation, this.Scale, color);
            if (drawSymbol) symbolPlayer.Draw(gameTime, spriteBatch, new Vector2(position.X, position.Y), SpriteEffects.None, 0f, new Vector2(1, 1), Color.White);
            this.Force = new Vector3();
            this.clearOtherForces();
        }


        public override void Update(float elapsed_time)
        {
            base.Update(elapsed_time);

            if (startCounting)
            {
                if (timeToDie <= 0)
                {
                    SoundManager.Instance.Play(Sounds.crystalDissapear);
                    this.Crash(false); 
                    this.startCounting = false;
                }
                else
                {
                    timeToDie -= elapsed_time;
                }
            }

            if(currentState == state.dying && player.isFinalFrame())
            {
                if (this.respawnable)
                {
                    Disappear(auxRandomize);
                }
                else
                {
                    currentState = state.dead;
                }
                isVisible = false;
            }
            else if (currentState == state.waiting)
            {
                if (timeToWait > 0)
                {
                    timeToWait -= elapsed_time;
                }
                else
                {
                    timeToWait = signalTime;
                    currentState = state.signal;
                    this.position = this.respawnPosition;
                    this.isVisible = true;
                }
            }
            else if (currentState == state.signal)
            {
                if (timeToWait > 0)
                {
                    timeToWait -= elapsed_time;
                    if (player.FrameIndex == 0 && playSound)
                    {
                        SoundManager.Instance.Play(Sounds.crystalAppearing);
                        playSound = false;
                    }
                    if (player.FrameIndex != 0)
                    {
                        playSound = true;
                    }
                }
                else
                {
                    timeToWait = 0;
                    SoundManager.Instance.Play(Sounds.crystalRespawn);
                    currentState = state.appearing;
                }
            }
            else if(currentState == state.appearing && player.Animation == appearingAnimation && player.isFinalFrame())
            {
                // Aqui deberiamos reaparecer
                Respawn();

            }
            Vector2 force = new Vector2(base.Force.X, base.Force.Y);
            totalForceArrow.Update(this.Position2D, force);
            for (int i = 0; i < otherForces.Length; i++)
            {
                otherForcesArrows[i].Update(this.Position2D, otherForces[i]);
            }
        }

        public void clearOtherForces()
        {
            for (int i = 0; i < 4; i++)
            {
                otherForces[i] = new Vector2();
            }
        }

        public void SetPosition(Vector3 position)
        {
            base.Position = position;
        }
        #endregion

        private Color getColor()
        {
            if (base.Charge > 0)
            {
                if (intensity == ColorIntensity.high)
                {
                    return highBlue;
                }
                else if (intensity == ColorIntensity.medium)
                {
                    return mediumBlue;
                }
                else
                {
                    return lowBlue;
                }
            }
            else if(base.Charge < 0)
            {
                if (intensity == ColorIntensity.high)
                {
                    return highRed;
                }
                else if (intensity == ColorIntensity.medium)
                {
                    return mediumRed;
                }
                else
                {
                    return lowRed;
                }
            }
            else
            {
                // La idea es que este caso NUNCA ocurra
                return Color.White;
            }
        }

        public void UpdateColor()
        {
            this.color = getColor();
        }

        public void startTimer()
        {
            timeToDie = lifeTime;
            startCounting = true;
        }


        public void Respawn()
        {
            this.ActiveCollisions = true;
            this.ActiveForces = true;
            this.isVisible = true;
            this.color = getColor();
            this.currentState = state.idle;
            this.position = this.respawnPosition;
        }

        public override void Reset()
        {
            this.position = initialPosition;
            this.ActiveCollisions = true;
            this.ActiveForces = true;
            this.isVisible = true;
            this.color = getColor();
            this.currentState = state.idle;
        }

        public override void Collision(IPhysicBody OtherBody)
        {
        }

        public void Crash(bool random)
        {
            this.currentState = state.dying;
            this.auxRandomize = random;
            this.player.PlayAnimation(dyingAnimation);
            this.ActiveCollisions = false;
            this.ActiveForces = false;
        }

        public void Disappear(bool random)
        {
            this.currentState = state.waiting;
            this.randomize = random;
            this.timeToWait = invisibleTime;
            this.ActiveCollisions = false;
            base.ActiveForces = false;
            this.isVisible = false;
        }

        public void Die()
        {
            this.currentState = state.dead;
            this.ActiveCollisions = false;
            base.ActiveForces = false;
            this.isVisible = false;
            this.startCounting = false;
            this.timeToDie = 0;
        }

        public void addSon(Crystal2D son)
        {
            if (son != this)
            {
                foreach (Crystal2D c in sons)
                {
                    if (son == c)
                    {
                        return;
                    }
                }
                sons.Add(son);
            }
        }

        public void removeSon(Crystal2D son)
        {
            this.sons.Remove(son);
        }

        public void crashSons(bool random)
        {
            foreach (Crystal2D c in sons)
            {
                c.Crash(randomize);
            }
        }

        // Flechas y fuerzas

        public void changeOtherForce(playerId id, Vector2 force, bool active)
        {
            otherForces[(int)id] = force;

            if (active)
            {
                if (narrative)
                {
                    otherForcesArrows[(int)id].ArrowColor = narrativeIndividualActiveColor;
                }
                else
                {
                    otherForcesArrows[(int)id].ArrowColor = noNarrativeIndividualActiveColor;
                }
            }
            else
            {
                if (narrative) 
                { 
                    otherForcesArrows[(int)id].ArrowColor = narrativeIndividualInactiveColor; 
                }
                else 
                { 
                    otherForcesArrows[(int)id].ArrowColor = noNarrativeIndividualInactiveColor; 
                }
            }
        }


        /*
        public override void ApplyForce(Vector3 force)
        {
            base.ApplyForce(force);
            Vector2 auxForce = this.otherForces[3];
            Vector2 sumForce = new Vector2(auxForce.X + force.X, auxForce.Y + force.Y);
            this.otherForces[3] = sumForce;

        }

        public void ApplyForce(Vector3 force, playerId id)
        {
            base.ApplyForce(force);
            Vector2 auxForce = this.otherForces[(int)id];
            Vector2 sumForce = new Vector2(auxForce.X + force.X, auxForce.Y + force.Y);
            this.otherForces[(int)id] = sumForce;
        }
         */
    }
}
