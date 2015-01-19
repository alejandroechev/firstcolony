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
    
    public class ARCrystal: GameModel.Crystal, GameObject
    {

        /* Variables necesarias para que se muestre
         * en pantalla dado la grafica usada*/
        ContentManager contentManager;

        private TransformNode objectNode;
        public TransformNode ObjectNode { get { return objectNode; } }

        public new bool isAlive { get { return base.isAlive; } set { base.isAlive = value; } }
        public new bool isDivisible { get { return base.isDivisible; } set { base.isDivisible = value; } }

        TransformNode arrowAngleNode;
        TransformNode arrowMagNode;
        TransformNode ModelTrsNode;
        GeometryNode ModelGeoNode;

        private Vector3 scale;

        public float ArrowAngle { get; set; }
        public float ArrowLength { get; set; }

        //estado de division
        bool original;
        Vector3 originalScale;
        float originalCharge;
        int divisions = 0;

        public event Action<ARCrystal> Destroy;

        public float Charge
        {
            get { return base.charge; }
            set
            {

                base.charge = value; setModel(this.contentManager, base.charge, ModelGeoNode);
                originalCharge = value;
            }
        }

        public ARCrystal(ContentManager contentManager, Vector3 scale, float mass, Vector3 position, Vector3 velocity, bool inertial, float charge, bool original)
            :this(contentManager, scale, mass, position, velocity, inertial, charge)
        {
            
            this.original = original;
        }

        public ARCrystal(ContentManager contentManager, Vector3 scale, float mass, Vector3 position, Vector3 velocity, bool inertial, float charge)
            : base(mass, position, velocity, inertial, charge)
        {
            this.contentManager = contentManager;
            this.original = true;
            this.originalCharge = charge;
            this.isAlive = true;
            this.scale = scale;
            this.originalScale = scale;

            objectNode = new TransformNode();
            objectNode.Translation = new Vector3(position.X, position.Y, position.Z);

            //modelo grafico         

            ModelGeoNode = new GeometryNode();
            setModel(contentManager, charge, ModelGeoNode);

            ModelTrsNode = new TransformNode();
            ModelTrsNode.Scale = 0.000018f * scale;
            ModelTrsNode.AddChild(ModelGeoNode);
            ObjectNode.AddChild(ModelTrsNode);

            GeometryNode arrowGeoNode = new GeometryNode();
            arrowGeoNode.Model = contentManager.GetModel("arrow");
            TransformNode arrowTrsNode = new TransformNode();      //rotacion del modelo
            arrowAngleNode = new TransformNode();  //rotacion segun posicion
            arrowMagNode = new TransformNode();  //scale node
            arrowMagNode.Scale = new Vector3();
            arrowTrsNode.Translation = new Vector3(0, 0, 0);
            //arrowTrsNode.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathHelper.ToRadians(180));
            
            //arrowAngleNode.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.ToRadians(90));
            //arrowMagNode.Scale = 0.0f*scale ;            
            arrowAngleNode.AddChild(arrowMagNode);
            arrowTrsNode.AddChild(arrowAngleNode);
            arrowMagNode.AddChild(arrowGeoNode);
            //ObjectNode.AddChild(arrowTrsNode);
            
            
        }

        private void setModel(ContentManager contentManager, float charge, GeometryNode ModelGeoNode)
        {
            if (charge > 0)
                ModelGeoNode.Model = contentManager.GetModel("TiberiumPos");
            else
                ModelGeoNode.Model = contentManager.GetModel("TiberiumNeg");
        }

        public void SetPosition(Vector3 position)
        {
            base.Position = position;
            this.position = position;
            this.initialPosition = new Vector3(position.X, position.Y, position.Z);
            objectNode.Translation = new Vector3(position.X, position.Y, position.Z);
        }
      
        /* Propiedades necesarias de implementar para que funcione 
         * el motor fisico y que dependen del modelo grafico usado¨*/
        
        //Radius: bound del asteroide que implementa un collisionSphere
        public override float Radius { get { return 3 * scale.X; } }

        /* Metodos que se pueden querer extender según el modelo gráfico usado*/        
        public override void Collision(IPhysicBody OtherBody) 
        {
            base.Collision(OtherBody);
        }
        public override void Update(float elapsed_time)
        {
            base.Update(elapsed_time);
            objectNode.Translation = new Vector3(position.X, position.Y, position.Z);
            

        }
        public void DrawArrow(float magnitud,float radians)
        {
            if (magnitud > 0)
            {
                arrowAngleNode.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.ToRadians(-90)+radians);
                arrowMagNode.Scale = 0.15f * scale + magnitud * 0.003f * Vector3.UnitY;
                //arrowMagNode.Translation = new Vector3(12f, 0, 0) + new Vector3(0.003f, 0, 0) * magnitud;
            }
            else 
            {
                arrowMagNode.Scale = new Vector3();
            }
        }
        public ARCrystal Divide(Vector3 dir) 
        {
            //(ContentManager contentManager, Vector3 scale, float mass, Vector3 position, Vector3 velocity, bool inertial, float charge)
            ARCrystal newCrystal = new ARCrystal(contentManager, scale / 1.3f, mass, new Vector3(position.X, position.Y, position.Z)+ 5*dir, new Vector3(velocity.X, velocity.Y, velocity.Z), IsInertial, Charge / 2.0f, false);
            newCrystal.Destroy += this.Destroy;
            //this.mass = this.mass / 2.0f;
            this.scale = this.scale / 1.3f;
            ModelTrsNode.Scale = 0.000018f * scale;
            this.position = new Vector3(position.X, position.Y, position.Z)-5*dir;
            this.charge = this.Charge / 2.0f;
            divisions++;
            return newCrystal;
        }
        private void OnDestroy()
        {
            if (this.Destroy != null)
                Destroy(this);
        }

        public override void Reset()
        {
            if (!original) OnDestroy();
            else
            {
                this.scale = this.originalScale;
                ModelTrsNode.Scale = 0.000018f * scale;
                this.Charge = this.originalCharge;
                
            }
            base.Reset();
        }
    }
}
