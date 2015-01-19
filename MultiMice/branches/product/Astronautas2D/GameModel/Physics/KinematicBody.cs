using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;


namespace GameModel
{
    public class KinematicBody
    {
        /* Constantes */
        public float mass;       //mass M
        public float IMass;

        /* Variables de estado */
        protected Vector3 lastPosition;
        protected Vector3 position;           // position(t) posicion        
        protected Vector3 momentum;          // momentum(t) momento lineal

        /* Valores derivados (variables auxiliares) */
        protected Vector3 velocity;           // velocity(t) velocidad lineal

        /* Valores calculados */

        Vector3 force;       // F(t) fuerza total  
        Vector3 falseForce;       // F(t) fuerza total no aplicada aún

        public Vector3 Force { get { return falseForce; } set { falseForce = value; } }


        public KinematicBody(float mass, Vector3 position, Vector3 velocity, bool inertial)
        {
            this.mass = mass;
            if (this.mass == 0) this.IMass = float.MaxValue;
            else this.IMass = 1.0f / this.mass;
            this.lastPosition = new Vector3(position.X, position.Y, position.Z);
            this.position = new Vector3(position.X, position.Y, position.Z);
            this.initialPosition = new Vector3(position.X, position.Y, position.Z);           
            this.velocity = new Vector3(velocity.X, velocity.Y, velocity.Z);
            this.initialVelocity = new Vector3(velocity.X, velocity.Y, velocity.Z);

            this.momentum = velocity * this.mass;
            this.force = new Vector3();
            this.IsInertial = inertial;
        }

        public void Reset(Vector3 position, Vector3 velocity)
        {
            this.position = position;
            this.momentum = velocity * this.mass;
            force = new Vector3();
            
        }
        public virtual void Reset()
        {
            this.position = new Vector3(initialPosition.X, initialPosition.Y, initialPosition.Z);
            this.velocity = new Vector3(initialVelocity.X, initialVelocity.Y, initialVelocity.Z);
            this.momentum = velocity * this.mass;
            force = new Vector3();
        }
        public virtual void Update(float elapsed_time)
        {
            /* State(t + ⌂t) = State(t) +  ⌂t*d/dt Sate(t) */
            Vector3 d_x;           // d/dt position(t)
            Vector3 d_P;           // d/dt momentum(t)

           
            /* Calcular variables auxiliares */

            /* velocity(t)= momentum(t)/M */
            this.velocity = this.momentum * this.IMass;

            /* Calcular derivadas */

            /* dx(t)/dt = V(t)*/
            d_x = this.velocity;

            /* dP(t)/dt = F(t)*/
            d_P = this.force;

            this.lastPosition.X = this.position.X;
            this.lastPosition.Y = this.position.Y;
            this.lastPosition.Z = this.position.Z;
            this.position = this.position + elapsed_time * d_x;
            if(IsInertial)
                this.momentum = this.momentum + elapsed_time * d_P;
            else
                this.momentum = elapsed_time * d_P;

            /* no hay concervacion de las fuerzas */            
            this.force = new Vector3();

        }

        public bool IsInertial;

        private Vector3 initialPosition;
        private Vector3 initialVelocity;
       

        public virtual void ApplyForce(Vector3 force)
        {
            this.force.X += force.X;
            this.force.Y += force.Y;
            this.force.Z += force.Z;

            this.falseForce.X += force.X;
            this.falseForce.Y += force.Y;
            this.falseForce.Z += force.Z;

        }

    }

}
