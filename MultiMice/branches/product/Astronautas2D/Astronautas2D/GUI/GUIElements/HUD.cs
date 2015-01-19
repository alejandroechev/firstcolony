using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Astronautas2D.Visual_Components;
using Microsoft.Xna.Framework.Graphics;
using Astronautas2D.GUI;
using Astronautas2D.Utils;

namespace Astronautas2D.GUI.GUIElements
{
    public class HUD : GUIElement
    {
        private String firstString;
        private String secondString;
        private Vector2 firstStringPosition;
        private Vector2 firstXPosition;
        private Vector2 secondStringPosition;
        private Icon firstIcon;
        private Icon secondIcon;
        private const float iconPorcentage = 0.3f;
        private const float paddingTop = 0.5f;
        private const float paddingCenter = 0.1f;
        private const float paddingLeft = 0.35f;
        private const float delta = 0f;
        private const float delta2 = 40f;
        private playerId playerId;
        public playerId PlayerId { get { return playerId; } }
        private bool narrative;
        private Color firstIconColor;


        public HUD(playerId id, Vector2 position, Texture2D[] textures, bool isVisible, Vector2 scale, float layer, bool narrative)
            : base(position, textures[(int)guiElements.hud],isVisible, scale, layer)
        {
            firstIconColor = Color.White;
            //Guardamos los atributos
            this.narrative = narrative;
            this.playerId = id;
            // Asignamos las alturas de los objetos
            float maxHeight = textures[(int)guiElements.hud].Height * scale.Y;
            float maxWidth = textures[(int)guiElements.hud].Width * scale.X;
            float iconHeight = maxHeight * iconPorcentage;

            Texture2D firstIconTexture, secondIconTexture;

            if (narrative)
            {
                firstIconTexture = textures[(int)guiElements.crystal];
                firstIconColor = Color.White;
                //if (id == playerId.team)
                //{
                //    firstIconTexture = textures[(int)guiElements.crystal];
                //    firstIconColor = Color.White;
                //}
                //else
                //{
                //    firstIconTexture = textures[(int)guiElements.medal];
                //}
                secondIconTexture = textures[(int)guiElements.time];
            }
            else
            {
                firstIconTexture = textures[(int)guiElements.score];
                secondIconTexture = textures[(int)guiElements.time];
            }

            float firstIconScale = iconHeight / firstIconTexture.Height;
            float secondIconScale = iconHeight / secondIconTexture.Height;
            Vector2 corner = this.getUpperLeftCorner();

            // Determinamos la posicion correspondiente
            Vector2 FirstIconPos = new Vector2(corner.X + paddingLeft * maxWidth, corner.Y + paddingTop * maxHeight);
            firstXPosition = new Vector2(corner.X + paddingLeft * maxWidth + firstIconTexture.Width * secondIconScale + delta, corner.Y + paddingTop * maxHeight - iconHeight / 3);
            firstStringPosition = new Vector2(firstXPosition.X + delta2, firstXPosition.Y);
            Vector2 SeconIconPos = new Vector2(corner.X + paddingLeft * maxWidth, corner.Y + iconHeight + (paddingTop + paddingCenter) * maxHeight);
            secondStringPosition = new Vector2(corner.X + paddingLeft * maxWidth + secondIconTexture.Width * firstIconScale + delta, corner.Y + iconHeight + (paddingTop + paddingCenter) * maxHeight - iconHeight / 3);

            // Se definen los iconos
            firstIcon = new Icon(firstIconTexture, FirstIconPos, new Vector2(firstIconScale, firstIconScale), layer, false);
            secondIcon = new Icon(secondIconTexture, SeconIconPos, new Vector2(secondIconScale, secondIconScale), layer, false);

            if (narrative)
            {
                // Será el contador de medallas o crystales
                firstString = "0";
            }
            else
            {
                // Nos importa el puntaje
                firstString = "0";
                secondString = "00:00";
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch, Writer fontWriter)
        {
            base.Draw(gameTime, spriteBatch, fontWriter); 
            firstIcon.Draw(gameTime, spriteBatch, fontWriter,firstIconColor);

            if (narrative)
            {
                fontWriter.DrawText("x", firstXPosition, Color.White);
                fontWriter.DrawText(firstString, firstStringPosition, Color.White);
            }
            else
            {
                fontWriter.DrawText("x", firstXPosition, Color.Black);
                fontWriter.DrawText(firstString, firstStringPosition, Color.Black);
            }
            
        }

        public void UpdateTime(string Time)
        {
            secondString = Time;
        }

        public void UpdateScore(string Score)
        {
            firstString = Score;
        }
    }
}
