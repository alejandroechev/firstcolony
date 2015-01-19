using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ArcadeGame
{
    public interface GameObject: GameModel.IPhysicBody
    {
        int SpriteId { get; set; }
        Vector2 Position2D { get; }
        float Rotation {get;}
        Vector2 Scale { get; }
        bool isAlive { get; set; }       

        void Draw(Texture2D sprite, SpriteBatch spriteBatch, SpriteFont font);

        void SetPosition(Vector3 position);
        
    }
}
