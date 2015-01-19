using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace GameModel
{
    public class Asteroid : KinematicBody, IPhysicBody
    {
        public bool isAlive;
        public bool isDestroyable;
        public bool isGravitational;
        public bool isSpawnable;
        public Vector3 spawnPosition;
        public Vector3 sunPosition;
        private float radius;
        
        //ICollisionSphere
        public virtual float Radius { get { return radius; } set { radius = value; } }
        
        private string materialName = "asteroid";
        public string MaterialName { get { return materialName; } set { materialName = value; } }

        //IElectricBody
        public float Charge { get { return 0; } set { } }
        public float PotentialCharge { get { return 0; } }
        public float Mass { get { return this.mass; } }

        public Asteroid(float mass, Vector3 position, Vector3 velocity, bool inertial)
            : base(mass, position, velocity, inertial)
        {
            isAlive = true;
            isDestroyable = true;
            isGravitational = false;
            isSpawnable = false;
            spawnPosition = new Vector3();
        }
        public override void Update(float elapsed_time)
        {
            if (isGravitational)
            {
                
                Vector3 Radio = this.position - this.sunPosition;
                Vector3 Tang = Vector3.Cross(Radio, Vector3.UnitZ);
                Tang.Normalize();

                /*calculo un punto en la direccion tangente 
                 * que depende de la magnitud de la velocidad*/
                Tang = velocity.X *elapsed_time* Tang;
                Vector3 newRadio = position + Tang - sunPosition;
                newRadio.Normalize();
                this.position = sunPosition + newRadio * Radio.Length();
            }
            else
             base.Update(elapsed_time); 
        }
        public Vector3 Position
        {
            get
            {
                return base.position;
            }

            set
            {
                position = value;
            }
            
        }

        public Vector3 LastPosition
        {
            get
            {
                return base.lastPosition;
            }

        }

        public virtual void Collision(IPhysicBody OtherBody) 
        {
            if (isDestroyable)
            {
                if (isSpawnable)
                {
                    Reset(spawnPosition, this.velocity);
                }
                else
                {
                    isAlive = false;
                }
            }
        }
        public override void Reset()
        {
            base.Reset();
            isAlive = true;
        }
        
    }
}
