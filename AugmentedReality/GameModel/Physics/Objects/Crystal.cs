using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace GameModel
{
    public class Crystal : KinematicBody, IPhysicBody
    {
        protected bool isAlive;
        protected bool isDivisible;
        protected float charge;
        private string materialName = "crystal";
        public string MaterialName { get { return materialName; } set { materialName = value; } }

        public Crystal(float mass, Vector3 position, Vector3 velocity, bool inertial, float charge)
            : base(mass, position, velocity, inertial)
        {
            this.charge = charge;
            isAlive = true;            
        }
        //IElectricBody
        public float Charge { get { return charge; } set { charge = value; } }
        public float PotentialCharge { get { return charge; } }    
        public float Mass { get {return base.mass;}}
        public Vector3 Position { get { return base.position; } set { base.position = value; } }
        public Vector3 LastPosition { get { return base.lastPosition; } }

        //ICollisionSphere
        /* Radio para las colisiones
         * debería quedar determinado por la representacion gráfica usada */
        private float radius;
        public virtual float Radius { get { return radius; } set { radius = value; } }
      

        public virtual void Collision(IPhysicBody OtherBody) 
        {
            isAlive = false;
        }
        public override void Reset()
        {
            base.Reset();
            isAlive = true;
        }
    }
}
