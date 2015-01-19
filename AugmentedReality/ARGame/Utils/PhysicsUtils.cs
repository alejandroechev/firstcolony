using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using GoblinXNA.Graphics;
using GoblinXNA.SceneGraph;

namespace ARGame
{
    class PhysicsUtils
    {
        public static Material TransparentMaterial()
        {
            Material normal = new Material();
            normal.Diffuse = new Vector4(1, 1, 1, 0);
            normal.Specular = new Vector4(1, 1, 1, 0);
            normal.SpecularPower = 10;

            return normal;
        }
        public static void SetPhysics(GeometryNode PhysicGeoNode)
        {
            PhysicGeoNode.Physics.Interactable = true;
            PhysicGeoNode.Physics.Collidable = true;
            PhysicGeoNode.Material = PhysicsUtils.TransparentMaterial();
            PhysicGeoNode.Physics.Mass = 10;
            PhysicGeoNode.AddToPhysicsEngine = true;
            PhysicGeoNode.Physics.Manipulatable = true;
        }

    }
}
