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
    public class Codec : GUIElement
    {
        // Lo que se muestra
        private String sender;
        private String message;
        private Icon portraitIcon;
        // Elementos de posicionamiento
        private const float iconPorcentage = 0.35f;
        private const float paddingTop = 0.1f;
        private const float paddingLeft = 0.08f;
        private const float delta = 15f;
        // Posiciones
        private Vector2 senderPosition;
        private Vector2 messagePosition;
        private Vector2 shownPosition;
        private Vector2 hiddenPosition;
        // Variables importantes
        float maxHeight;
        float maxWidth;
        float iconHeight;
        float portraitIconScale;

        public Codec(Vector2 shownPosition, Vector2 hiddenPosition, Texture2D[] textures, bool isVisible, Vector2 scale, float layer)
            : base(hiddenPosition, textures[(int)guiElements.codec], isVisible, scale, layer)
        {
            this.shownPosition = shownPosition;
            this.hiddenPosition = hiddenPosition;

            // Asignamos las alturas de los objetos
            maxHeight = textures[(int)guiElements.codec].Height * scale.Y;
            maxWidth = textures[(int)guiElements.codec].Width * scale.X;
            iconHeight = maxHeight * iconPorcentage;
            portraitIconScale = iconHeight / textures[(int)guiElements.portrait].Height;

            Vector2 corner = this.getUpperLeftCorner();

            // Determinamos la posicion correspondiente
            Vector2 portraitIconPos = new Vector2(textures[(int)guiElements.portrait].Width/2 * portraitIconScale + corner.X + paddingLeft * maxWidth, textures[(int)guiElements.portrait].Height/2 * portraitIconScale+ corner.Y + paddingTop * maxHeight);

            // Se definen los iconos
            portraitIcon = new Icon(textures[(int)guiElements.portrait], portraitIconPos, new Vector2(portraitIconScale, portraitIconScale), layer, false);
            
            // Posiciones de los objetos.
            senderPosition = new Vector2(portraitIconPos.X + textures[(int)guiElements.portrait].Width * 3 / 4 * portraitIconScale, portraitIconPos.Y - textures[(int)guiElements.portrait].Height / 2 * portraitIconScale);
            messagePosition = new Vector2(senderPosition.X, senderPosition.Y + 30);

            // Mensajes de prueba
            sender = "Gran General Gran:";
            message = "blah blah blah blah blah blah blah blah blah blah blah blah.";
        }

        public void Relocate(Vector2 newPosition)
        {
            base.Position = newPosition;
            portraitIcon.Position = this.getIconPosition();
            Vector2[] textPositions = getTextsPositions();
            senderPosition = textPositions[0];
            messagePosition = textPositions[1];
        }

        #region cambioDePosiciones
        private Vector2 getIconPosition()
        {
            Vector2 corner = this.getUpperLeftCorner();
            return new Vector2(portraitIcon.Shape.Width / 2 * portraitIconScale + corner.X + paddingLeft * maxWidth, portraitIcon.Shape.Height / 2 * portraitIconScale + corner.Y + paddingTop * maxHeight);
        }

        private Vector2[] getTextsPositions()
        {
            Vector2[] positions = new Vector2[2];
            positions[0] = new Vector2(portraitIcon.Position.X + portraitIcon.Shape.Width * 3 / 4 * portraitIconScale, portraitIcon.Position.Y - portraitIcon.Shape.Height / 2 * portraitIconScale);
            positions[1] = new Vector2(positions[0].X, positions[0].Y + 30);
            return positions;

        }
        #endregion

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch, Writer fontWriter)
        {
            base.Draw(gameTime, spriteBatch, fontWriter);
            portraitIcon.Draw(gameTime, spriteBatch, fontWriter);
            fontWriter.DrawText(sender, senderPosition, Color.Red);
            fontWriter.DrawText(message, messagePosition, Color.Black); 
        }

    }
}
