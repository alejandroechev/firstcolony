using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameModel;

namespace ArcadeGame
{
    public class SceneAstronaut: GameModel.Astronaut, GameObject
    {
        private int spriteId;
        public int SpriteId { get { return spriteId; } set { spriteId = value; } }
        private Vector2 scale;
        public Vector2 Scale { get { return scale; } set { scale = value; } }
        public float Rotation {get { return 0; }}
        public bool isAlive { get { return true; } set { ;} }
        public Vector2 Position2D { get { return new Vector2(base.Position.X, base.Position.Y); } }
        public bool selected;

        private float time;
        public SceneAstronaut(float mass, Vector3 position)
            : base(mass, position)
        {
            time = 0;
            selected = false;
        }
        
        public override void Update(float elapsedTime)
        {
            KeyboardState keyState = Keyboard.GetState();
            if ((time +=elapsedTime) > 0.14f)
            {
                time = 0;
                if (keyState.IsKeyDown(Keys.Space)&&selected)
                    isActive = !isActive;

                if (keyState.IsKeyDown(Keys.Up) && selected)
                    this.AddCharge(1);
                if (keyState.IsKeyDown(Keys.Down) && selected)
                    this.AddCharge(-1);
            }
        }

        public void Draw(Texture2D sprite, SpriteBatch spriteBatch, SpriteFont font)
        {
            spriteBatch.Draw(sprite,
                        this.Position2D,
                        imageRect(sprite),
                        Color.White,
                        this.Rotation,
                        new Vector2(sprite.Width /4, sprite.Height / 2),
                        this.Scale,
                        SpriteEffects.None, 0);

            spriteBatch.DrawString(font, "M: " + this.PotentialCharge, this.Position2D, Color.Red);
        }
        public Rectangle imageRect(Texture2D sprite) 
        {
            if(!selected && !isActive)
                return new Rectangle(0, 0, sprite.Width / 4, sprite.Height);
            else if (selected && !isActive)
                return new Rectangle(2*sprite.Width / 4, 0, sprite.Width / 4, sprite.Height);
            else if (!selected && isActive)
                return new Rectangle(sprite.Width / 4, 0, sprite.Width / 4, sprite.Height);
            else
                return new Rectangle(3 * sprite.Width / 4, 0, sprite.Width / 4, sprite.Height);
                
        }

        #region GameObject Members


        public void SetPosition(Vector3 position)
        {
            base.Position = position;
        }

        #endregion
        
    }
}
