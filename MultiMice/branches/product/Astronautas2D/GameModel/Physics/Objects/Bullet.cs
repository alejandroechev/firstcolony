using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace GameModel
{
    public class Bullet : KinematicBody, IPhysicBody
    {
        protected bool isAlive;
        public bool Alive { get { return isAlive; } }
        public bool ActiveCollisions { get { return isAlive; } }
        public bool ActiveForces { get { return false; } }
        private float radius;
        
        //ICollisionSphere
        public virtual float Radius { get { return radius; } set { radius = value; } }

        private string materialName = "bullet";
        public string MaterialName { get { return materialName; } set { materialName = value; } }

        //IElectricBody
        public float Charge { get { return 0; } set { } }
        public float PotentialCharge { get { return 0; } }
        public float Mass { get { return this.mass; } }

        public Bullet(float mass, Vector3 position, Vector3 velocity)
            : base(mass, position, velocity, true)
        {         
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

        public Vector3 LastPosition { get { return base.lastPosition; } }

        public virtual void Collision(IPhysicBody OtherBody) 
        {
            
        }

        public override void Reset()
        {
            base.Reset();
            isAlive = false;
        }
        
    }
}
