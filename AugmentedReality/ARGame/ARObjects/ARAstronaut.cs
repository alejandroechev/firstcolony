using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using GoblinXNA.SceneGraph;
using GoblinXNA.Graphics;
using GoblinXNA.Graphics.Geometry;
using Model = GoblinXNA.Graphics.Model;

using GameModel;

namespace ARGame
{
    public class ARAstronaut: GameModel.Astronaut, GameObject
    {
        /* Variables necesarias para que se muestre
        * en pantalla dado la grafica usada*/
        private TransformNode objectNode;
        public TransformNode ObjectNode { get { return objectNode; } }
      

        private int id;
        public int Id { get { return id; } set { id = value; } }

        public bool IsActive { get { return isActive; } set { isActive = value; } }
        private float time;

        private bool arrowMode;
        public bool ArrowMode { get { return arrowMode; } set { arrowMode = value; } }

        private ARBullet bullet;
        public ARBullet Bullet { get { return bullet; } }


        public event Action ShootRay;

        public ARAstronaut(int id, float mass, Vector3 position)
            : base(mass, position)
        {
            time = 0;            
            this.id = id;

           
            Vector3 scale = Vector3.One;
            bullet = new ARBullet( scale, 10, Vector3.Zero, Vector3.Zero);

        }

        public void SetPosition(Vector3 position)
        {
            base.Position = position;
        }

        public void SetCharge(float charge)
        {
            base.Charge = charge;
        }

        private void OnShootRay()
        {
            
            if (ShootRay != null)
                ShootRay();
        }

        public void toogleCharge() 
        {
            /*if (isActive) isActive = false;
            else if (arrowMode)
            {
                arrowMode = false;                
                isActive = true;
            }
            else { arrowMode = true; timerCounter = 10; */
            arrowMode=!arrowMode;
            
        }

        public void Activate(bool isActive)
        {
            this.isActive = isActive;
        }

        public void Reset()
        {           
            isActive = false;
        }

    }
}
