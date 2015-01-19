using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace GameModel
{
    public interface IPhysicBody : IElectricBody, ICollisionSphere
    {
        Vector3 Position { get;}
        Vector3 LastPosition { get; }        
        string MaterialName { get; set; }
        void Update(float elapsedTime);
        bool ActiveCollisions { get; }
    }
}
