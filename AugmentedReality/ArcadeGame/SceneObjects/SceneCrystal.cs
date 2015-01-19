using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameModel;

namespace ArcadeGame
{
    
    public class SceneCrystal: GameModel.Crystal, GameObject
    {

        /* Variables necesarias para que se muestre
         * en pantalla dado la grafica usada*/

        private int spriteId;
        public int SpriteId { get { return spriteId; } set { spriteId = value; } }  
        
        private Vector2 scale;
        public Vector2 Scale { get { return scale; } set { scale = value; } }
        public float Rotation {get { return 0; }}
        public Vector2 Position2D {get { return new Vector2(base.Position.X, base.Position.Y); }}

        public new bool isAlive { get { return base.isAlive; } set { base.isAlive = value; } }
        public new bool isDivisible { get { return base.isDivisible; } set { base.isDivisible = value; } }

        public event Action next;
        public event Action reset;

        public SceneCrystal(float mass, Vector3 position, Vector3 velocity, bool inertial, float charge)
            : base(mass, position, velocity, inertial, charge)
        { }

        public void Draw(Texture2D sprite, SpriteBatch spriteBatch, SpriteFont font)
        {
            spriteBatch.Draw(sprite,
                        this.Position2D,
                        new Rectangle(0, 0, sprite.Width, sprite.Height),
                        Color.White,
                        this.Rotation,
                        new Vector2(sprite.Width / 2, sprite.Width / 2),
                        this.Scale,
                        SpriteEffects.None, 0);
        }
      
        /* Propiedades necesarias de implementar para que funcione 
         * el motor fisico y que dependen del modelo grafico usado¨*/
        
        //Radius: bound del asteroide que implementa un collisionSphere
        public override float Radius { get { return 16 * scale.X - 2; } }

        #region GameObject Members


        public void SetPosition(Vector3 position)
        {
            base.Position = position;
        }

        #endregion
        
    }
}
