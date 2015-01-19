using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using GoblinXNA.SceneGraph;
using GoblinXNA.Graphics;
using GoblinXNA.Graphics.Geometry;
using Model = GoblinXNA.Graphics.Model;

using GameModel;

namespace ARGame
{
    public class ARPortal: GameModel.Portal, GameObject
    {

        /* Variables necesarias para que se muestre
         * en pantalla dado la grafica usada*/
        private TransformNode objectNode;
        public TransformNode ObjectNode { get { return objectNode; } }

        private Vector3 scale;

        public ARPortal(Model model, Vector3 scale, Vector3 position)
            : base(position) 
        {
            this.scale = scale;

            //creo el objeto grafico.            
            objectNode = new TransformNode();
            objectNode.Translation = new Vector3(position.X, position.Y, position.Z);

            //modelo grafico      
            objectNode.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(0));
            
            GeometryNode ModelGeoNode = new GeometryNode();
            ModelGeoNode.Model = model;
            objectNode.Scale = 0.058f * scale;
            objectNode.AddChild(ModelGeoNode);            
        }

        public void SetPosition(Vector3 position)
        {
            base.Position = position;
            objectNode.Translation = new Vector3(position.X, position.Y, position.Z);
        }

        /* Propiedades necesarias de implementar para que funcione 
         * el motor fisico y que dependen del modelo grafico usado¨*/
        
        //Radius: bound del asteroide que implementa un collisionSphere
        public override float Radius { get { return 5; } }

        /* Metodos que se pueden querer extender según el modelo gráfico usado*/

        //puedo querer hacer algo más luego de la colision, por ejemplo algun efecto
        public override void Collision(IPhysicBody OtherBody) 
        {
            base.Collision(OtherBody);

        }
        public override void Update(float elapsed_time)
        {
            base.Update(elapsed_time);           
            
        }
        public void Reset()
        {

        }

    }
}
