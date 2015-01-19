using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using GoblinXNA.SceneGraph;
using GoblinXNA.Graphics;
using GoblinXNA.Graphics.Geometry;
using GoblinXNA.Physics;
using Model = GoblinXNA.Graphics.Model;

using GameModel;

namespace ARGame
{
    public class ARBullet: GameModel.Bullet, GameObject
    {

        /* Variables necesarias para que se muestre
         * en pantalla dado la grafica usada*/
        private TransformNode objectNode;
        public TransformNode ObjectNode { get { return objectNode; } }
        private Vector3 scale;
        public event Action<ARBullet> Destroy;

        public Vector3 Direction { get { return new Vector3(base.velocity.X, base.velocity.Y,0); } }

        public ARBullet(Vector3 scale, float mass, Vector3 position, Vector3 velocity)
            : base(mass, position, velocity)
        {
            isAlive = true;
            this.scale = scale;
            /* Primero creamos el nodo que contiene 
             * al modelo grafico y fisico */

            objectNode = new TransformNode();
            objectNode.Translation = new Vector3(position.X, position.Y, position.Z);

            /* Creamos el modelo gráfico */
            GeometryNode ModelGeoNode = new GeometryNode();
           // objectNode.Scale = 0.008f * scale;
            ModelGeoNode.Model = new Sphere(1f, 20, 20);

            Material ballMaterial = new Material();
            ballMaterial.Diffuse = Color.Blue.ToVector4();
            ballMaterial.Specular = Color.LightBlue.ToVector4();
            ballMaterial.SpecularPower = 30;

            ModelGeoNode.Material = ballMaterial;
            objectNode.AddChild(ModelGeoNode);
        }

        public void SetPosition(Vector3 position)
        {
            
        }


       
        /* Propiedades necesarias de implementar para que funcione 
         * el motor fisico y que dependen del modelo grafico usado¨*/
        
        //Radius: bound del asteroide que implementa un collisionSphere
        public override float Radius { get { return 1f; } }

        /* Metodos que se pueden querer extender según el modelo gráfico usado*/

        //puedo querer hacer algo más luego de la colision, por ejemplo algun efecto
        public override void Collision(IPhysicBody OtherBody) 
        {
            base.Collision(OtherBody);

        }

        public override void Update(float elapsed_time)
        {
            base.Update(elapsed_time);
            objectNode.Translation = new Vector3(position.X, position.Y, position.Z);
            if (position.Z <= 0) { OnDestroy(); }
          
        }

        private void OnDestroy()
        {
            if (this.Destroy != null)
                Destroy(this);
        }

        public override void Reset()
        {
            OnDestroy();
        }
    }
}
