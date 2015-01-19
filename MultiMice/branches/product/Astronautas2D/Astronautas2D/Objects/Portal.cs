using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace GameModel
{
    public class Portal : IPhysicBody
    {
        //IEletricBody, por si despues se desea hacer electrico
        public float Mass { get{return 0;} }
        public float Charge { get { return 0; } set { } }
        public float PotentialCharge { get { return 0; } }
        public void ApplyForce(Vector3 forces) { }

        private string materialName = "portal";
        public string MaterialName { get { return materialName; } set { materialName = value; } }

        //ICollisionBody
        private float radius;
        public virtual float Radius { get { return radius; } set { radius = value; } }
        public Vector3 Position { get { return position; } set { position = value; } }
        public Vector3 LastPosition { get { return position; } }

        private Vector3 position;
        private Vector3 scale;

        public Portal(Vector3 scale, Vector3 position)            
        {
            this.scale = scale;
            this.position = position;                   
        }

        public virtual void Collision(IPhysicBody OtherBody) { }       
        public virtual void Update(float elapsedTime) { }
    }
}
