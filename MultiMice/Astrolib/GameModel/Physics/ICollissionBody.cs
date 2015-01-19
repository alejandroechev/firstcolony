using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace GameModel
{
    public interface ICollisionBody
    {
        void Collision(IPhysicBody otherBody);
      
    }

    public interface ICollisionSphere : ICollisionBody    {
     
       float Radius {get;}
    }

    /*public interface ICollisionBox : ICollisionBody
    {
        
        Vector3 Box{get;}
    }*/
}
