using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using GoblinXNA.SceneGraph;

namespace ARGame
{
    public interface GameObject: GameModel.IPhysicBody
    {
        TransformNode ObjectNode {get;}
        void Reset();
        void SetPosition(Vector3 position);
    }
}
