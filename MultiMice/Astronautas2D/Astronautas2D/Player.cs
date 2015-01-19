using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using GameModel;
using Astronautas2D.GameObjects;
using Astronautas2D.Visual_Components;
using Microsoft.Xna.Framework.Graphics;
using Astronautas2D.Utils;
using Astronautas2D.GameObjects.Entities;
using Astronautas2D.GUI.GUIElements;
using Astronautas2D.GUI;

namespace Astronautas2D
{
    public enum playerId {circle = 0, triangle = 1, square = 2, team = 3}

    public class Player
    {
        private playerId id;                // Id del jugador
        private Astronaut2D astronaut;      // Astronauta asociado al jugador
        public Astronaut2D Astronaut { get { return astronaut; } }
        private Bullet2D[] bullets;
        public Bullet2D[] Bullets { get { return bullets; } }
        private int currentBullet;
        private Slider slider;
        private bool showCharge;

        // Variables para ocultar el slider
        private float timeCounter;
        public float TimeCounter{ get { return timeCounter;} set { timeCounter = value;} }

        public Player(playerId id, Astronaut2D astronaut, Bullet2D[] bullets, Slider slider, bool showCharge)
        {
            this.id = id;
            this.astronaut = astronaut;
            this.slider = slider;
            this.bullets = bullets;
            this.currentBullet = 0;
            this.timeCounter = 0;
            this.showCharge = showCharge;
        }

        public void onLeftClick(Vector2 position)
        {
            astronaut.onLeftClick(position);
        }

        public void Reset()
        {
            this.astronaut.Reset();
        }

        public void onCenterClick(Vector2 position)
        {
            astronaut.onCenterClick(position);
        }


        public void onRightClick(Vector2 position)
        {
            astronaut.onRightClick(position);
            // Aqui deberiamos manipular ambas balas
            if (bullets[currentBullet].CurrentState == state.idle)
            {
                bullets[currentBullet].Explode();
            }
            currentBullet = (currentBullet + 1) % bullets.Length;
            bullets[currentBullet].Shoot(position, astronaut.Position2D);
        }

        public void onWheelChange(int delta)
        {
            astronaut.onWheelChange(delta);
        }

        public void Update(float elapsedTime)
        {
            timeCounter -= elapsedTime;
            if (astronaut.Alive)
            {
                this.astronaut.Update(elapsedTime);
                this.slider.Position = getSliderPosition();
                this.slider.moveMarker(astronaut.ChargeLevel);
            }
            for (int i = 0; i < bullets.Length; i++)
            {
                if (bullets[i].Alive)
                {
                    bullets[i].Update(elapsedTime);
                }
            }
            if (showCharge)
            {
                if (timeCounter > 0)
                {
                    slider.IsVisible = true;
                }
                else
                {
                    slider.IsVisible = true;
                    // CAMBIO
                    // slider.IsVisible = false;
                }
            }
            else
            {
                slider.IsVisible = false;
            }
        }

        public Vector2 getSliderPosition()
        {
            Vector2 position = new Vector2(astronaut.Position2D.X - (astronaut.Radius+15), astronaut.Position2D.Y);
            return position;
        }

        public void DrawAstronaut(GameTime gameTime, SpriteBatch spriteBatch, Writer fontWriter)
        {
            if(astronaut.IsVisible)
            {
                this.astronaut.Draw(gameTime, spriteBatch, fontWriter);
            }
        }

        public void DrawBullets(GameTime gameTime, SpriteBatch spriteBatch, Writer fontWriter)
        {
            for (int i = 0; i < bullets.Length; i++)
            {
                if (bullets[i].IsVisible)
                {
                    bullets[i].Draw(gameTime, spriteBatch, fontWriter);
                }
            }
        }

    }
}
