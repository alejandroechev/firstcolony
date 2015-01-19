using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Astronautas2D.Visual_Components;


namespace Astronautas2D.GameObjects
{
    // Enums utiles para el juego
    public enum objectId { astronaut, asteroid, crystal, portal, arrow, bullet,forceField, zone};
    public enum state { signal, appearing, idle, moving, up, down, left, right, dying, dead, waiting, symbol};
    public enum backgrounds { tutorial, mission, tutorial1, tutorial2, tutorial3, tutorial4, tutorial5, tutorial6, tutorial7, tutorial8, mission1, mission2, mission3, mission4, mission5, ending };

    public interface IGameObject
    {
        objectId Id { get; }
        Vector2 Position2D { get; }
        float Rotation { get; }
        Vector2 Scale { get; }
        state CurrentState { get; set; }
        bool Alive { get; set; }                      // Indica si está vivo en nuestro juego
        bool IsVisible { get; set; }
        Vector3 InitialPosition { get; }

        void Update(float elapsedTime);
        void Draw(GameTime gameTime, SpriteBatch spriteBatch, Writer fontWriter);
        void SetPosition(Vector3 position);
        void Reset();
        int ListIndex { get; set; }
    }
}
