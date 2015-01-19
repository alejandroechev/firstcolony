using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using GoblinXNA.SceneGraph;
using GoblinXNA.Graphics;
using GoblinXNA.Graphics.Geometry;
using Model = GoblinXNA.Graphics.Model;

namespace ARGame
{
    /** define un objeto que no es 
     * parte del mundo fisico */
    class ARGeneric
    {
        private TransformNode objectNode;
        public TransformNode ObjectNode { get { return objectNode; } }

        public ARGeneric(Model model, Vector3 scale, Vector3 position)
        {
            //creo el objeto grafico.            
            objectNode = new TransformNode();
            objectNode.Translation = new Vector3(position.X, position.Y, position.Z);

            //modelo grafico      
            //objectNode.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(90));

            GeometryNode ModelGeoNode = new GeometryNode();
            ModelGeoNode.Model = model;
            objectNode.Scale = scale;
            objectNode.AddChild(ModelGeoNode); 
        }
    }
}
