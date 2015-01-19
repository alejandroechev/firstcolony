using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameModel;

namespace ArcadeGame
{
    public class ScenePortal: GameModel.Portal, GameObject
    {

        /* Variables necesarias para que se muestre
         * en pantalla dado la grafica usada*/

        private int spriteId;
        public int SpriteId { get { return spriteId; } set { spriteId = value; } }
        
        private Vector2 scale;
        public Vector2 Scale { get { return scale; } set { scale = value; } }
        public float Rotation {get { return 0; }}
        public Vector2 Position2D {get { return new Vector2(base.Position.X, base.Position.Y); }}

        public bool isAlive { get { return true; } set { ;} }

        private float time = 0;
        private int spritePos = 0;

        public ScenePortal(Vector3 position):base(position) 
        {
            
        }
        public void Draw(Texture2D sprite, SpriteBatch spriteBatch, SpriteFont font)
        {
            spriteBatch.Draw(sprite,
                        this.Position2D,
                        new Rectangle(sprite.Width / 4 * spritePos, 0, sprite.Width / 4, sprite.Height),
                        Color.White,
                        this.Rotation,
                        new Vector2(sprite.Width / 8, sprite.Height / 2),
                        this.Scale,
                        SpriteEffects.None, 0);
        }
       
        public override void Update(float elapsedTime)
        {
            base.Update(elapsedTime);
            if ((time += elapsedTime) > 0.03f)
            {
                spritePos++;
                spritePos %= 3;
                time = 0;
            }
        }
        /* Propiedades necesarias de implementar para que funcione 
         * el motor fisico y que dependen del modelo grafico usado¨*/
        
        //Radius: bound del asteroide que implementa un collisionSphere
        public override float Radius { get { return 60 * scale.X - 2; } }

        /* Metodos que se pueden querer extender según el modelo gráfico usado*/

        //puedo querer hacer algo más luego de la colision, por ejemplo algun efecto
        public override void Collision(IPhysicBody OtherBody) 
        {
            base.Collision(OtherBody);

        }

        #region GameObject Members


        public void SetPosition(Vector3 position)
        {
            base.Position = position;
        }

        #endregion
        
    }
}
