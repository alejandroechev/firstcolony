using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace GameModel
{
    public interface IElectricBody
    {
        float Charge { get; set; }
        float PotentialCharge { get; }  
        float Mass{get;}        
        void ApplyForce(Vector3 force);
        bool ActiveForces { get; }
    }
}
