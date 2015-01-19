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
    public class ObjectiveBoard : GUIElement
    {
        // Lo que se muestra
        private String title;
        private String description;
        // Elementos de posicionamiento
        private float paddingTop, paddingLeft, groupPaddingLeft, groupPaddingTop;
        // Posiciones
        private Vector2 titlePosition;
        private Vector2 descriptionPosition;
        private Vector2 groupPosition;
        // Variables importantes
        private float maxHeight;
        private float maxWidth;
        private int groupNumber;
        public int GroupNumber { set { groupNumber = value; } }
        private Color textColor;

        public ObjectiveBoard(int groupNumber, String description, Vector2 position, Texture2D[] textures, bool isVisible, Vector2 scale, float layer, bool narrative)
            : base(position, textures[(int)guiElements.objectiveBoard], isVisible, scale, layer)
        {
            this.groupNumber = groupNumber;

            if (narrative)
            {
                textColor = Color.Gold;
                paddingTop = 0.15f;
                paddingLeft = 0.1f;
                groupPaddingLeft = 0.8f;
                groupPaddingTop = 0.15f;
            }
            else
            {
                textColor = Color.Black;
                paddingTop = 0.15f;
                paddingLeft = 0.05f;
                groupPaddingLeft = 0.8f;
                groupPaddingTop = 0.15f;
            }

            // Asignamos las alturas de los objetos
            maxHeight = textures[(int)guiElements.objectiveBoard].Height * scale.Y;
            maxWidth = textures[(int)guiElements.objectiveBoard].Width * scale.X;

            Vector2 corner = this.getUpperLeftCorner();

            // Posiciones de los objetos.
            titlePosition = new Vector2(corner.X + maxWidth * paddingLeft, corner.Y + maxHeight * paddingTop);
            descriptionPosition = new Vector2(titlePosition.X, titlePosition.Y + 30);
            groupPosition = new Vector2(corner.X + maxWidth * groupPaddingLeft, corner.Y + maxHeight * groupPaddingTop);

            // Mensajes de prueba
            title = "Objetivo:";
            this.description = description;
        }

        public void Locate(Vector2 newPosition)
        {
            base.Position = newPosition;
            Vector2[] textPositions = getTextsPositions();
            titlePosition = textPositions[0];
            descriptionPosition = textPositions[1];
        }

        #region cambioDePosiciones

        private Vector2[] getTextsPositions()
        {
            Vector2 corner = this.getUpperLeftCorner();

            // Posiciones de los objetos.
            Vector2[] positions = new Vector2[2];
            positions[0] = new Vector2(corner.X + base.shape.Width * paddingLeft, corner.Y + base.shape.Height * paddingTop);
            positions[1] = new Vector2(titlePosition.X, titlePosition.Y + 30);
            return positions;

        }
        #endregion

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch, Writer fontWriter)
        {
            base.Draw(gameTime, spriteBatch, fontWriter);
            fontWriter.DrawText(title, titlePosition, textColor);
            fontWriter.DrawText(description, descriptionPosition, textColor);
            fontWriter.DrawText("Grupo: " + groupNumber, groupPosition, textColor);
        }
    }
}