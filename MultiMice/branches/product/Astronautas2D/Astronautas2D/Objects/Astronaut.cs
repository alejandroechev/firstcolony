using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace GameModel
{
    public class Astronaut : IPhysicBody
    {
        protected bool isActive;       

        private float charge, mass;
        private Vector3 position;

        //IElectricBody
        public float Charge { get { if (isActive) return charge; else return 0; } set { charge = value; } }       
        public float Mass { get { return mass; } set { mass = value; } }
        public Vector3 Position {get {return position;} set {position = value;}}
        public Vector3 LastPosition { get { return position; } }

        public float PotentialCharge { get { return charge; } } //retorna el valor potencia de la carga
        //(el que tendria al activarse)

        private string materialName;
        public string MaterialName { get { return materialName; } set { materialName = value; } }

        //ICollisionSphere
        /* Radio para las colisiones
         * debería quedar determinado por la representacion gráfica usada */
        private float radius;
        public virtual float Radius { get { return radius; } set { radius = value; } }
    

        public Astronaut(float mass, Vector3 position)            
        {
            this.Mass = mass;
            this.Position = position;
            this.charge = 0;
            isActive = false;            
        }
        /// <summary>
        /// Suma o Resta la carga del astronauta
        /// </summary>
        public void AddCharge(float charge)
        {
            this.charge += charge;
        }

        
        public virtual void Collision(IPhysicBody OtherBody) { }
       
        /// <summary>        
        /// No implementado. 
        /// </summary>
        public virtual void ApplyForce(Vector3 force) { }
        
        /// <summary>        
        /// No implementado. 
        /// </summary>
        public virtual void Update(float elapsedTime) { }
    }
}
