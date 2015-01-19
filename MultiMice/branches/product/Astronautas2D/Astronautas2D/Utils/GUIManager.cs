using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Astronautas2D.GUI;
using Astronautas2D.Factories;
using Microsoft.Xna.Framework.Graphics;
using Astronautas2D.GUI.GUIElements;
using Astronautas2D.Visual_Components;


namespace Astronautas2D.Utils
{
    public class GUIManager
    {
        private List<GUIElement> GUIElements;               // Elementos gráficos
        private Rectangle boundries;                        // la pantalla completa
        public Rectangle Boundries { get { return boundries; } }
        private GUIElementFactory guiFactory;               // Constructor de objectos graficos
        private Vector2 padding = new Vector2(10, 10);      // Borde c/r al borde
        private float separation = 250f;
        private List<FloatingText> texts;
        private List<Icon> floatingIcons;
        private Vector2 CodecShownPosition;
        private Vector2 CodecHiddenPosition;
        private Vector2 BoardPosition;
        private bool narrative;

        // Tamaño de la pantalla
        private Vector2 aspectRatio;

        // Escalas
        private Vector2 codecScale = new Vector2(2.5f, 0.9f);
        private Vector2 boardScale;
        private Vector2 hudScale = new Vector2(0.8f, 0.8f);

        public GUIManager(Rectangle rectangle, GUIElementFactory factory, bool narrative)
        {
            boundries = rectangle;
            aspectRatio = new Vector2(rectangle.Width / 1440f, rectangle.Height / 900f);
            guiFactory = factory;
            GUIElements = new List<GUIElement>();
            texts = new List<FloatingText>();
            this.narrative = narrative;
            CodecShownPosition = new Vector2(700, 890);
            CodecHiddenPosition = new Vector2(700, 1035);
            BoardPosition = new Vector2(rectangle.Width / 2, rectangle.Height/18);
            floatingIcons = new List<Icon>();

            if (narrative)
                boardScale = new Vector2(3f, 0.3f);
            else
                boardScale = new Vector2(1f, 1f);
            codecScale = new Vector2(2.5f, 0.9f);
            hudScale = new Vector2(0.8f, 0.8f);

            // Multiplicamos por el aspectRatio
            boardScale = new Vector2(boardScale.X * aspectRatio.X, boardScale.Y * aspectRatio.Y);
            codecScale = new Vector2(codecScale.X * aspectRatio.X, codecScale.Y * aspectRatio.Y);
            hudScale = new Vector2(hudScale.X * aspectRatio.X, hudScale.Y * aspectRatio.Y);

        }

        public void changeGroupNumber(int groupNumber)
        {
            foreach (GUIElement element in GUIElements)
            {
                if (element is ObjectiveBoard)
                {
                    ObjectiveBoard aux = (ObjectiveBoard)element;
                    aux.GroupNumber = groupNumber;
                }
            }
        }

        public Slider createSlider(Vector2 position, playerId id, int sliderLevels)
        {
            Slider aux = guiFactory.createSlider(position, id, true, sliderLevels, new Vector2(0.5f, 0.5f), 0.5f);
            GUIElements.Add(aux);
            return aux;
        }

        public Codec createCodec()
        {
            Codec c = guiFactory.createCodec(CodecShownPosition, CodecHiddenPosition, true, codecScale, 0.5f);
            GUIElements.Add(c);
            return c;
        }

        public ObjectiveBoard createObjectiveBoard(int groupNumber, String description)
        {
            ObjectiveBoard c = guiFactory.createObjectiveBoard(groupNumber, description, BoardPosition, true, boardScale, 0.5f);
            GUIElements.Add(c);
            return c;
        }

        public HUD createTeamHUD()
        {
            
            Vector2 hudPos = getHudPosition(playerId.team, hudScale);
            HUD th = guiFactory.createHUD(hudPos, true, hudScale, 0.5f);
            GUIElements.Add(th);
            return th;
        }

        public HUD createPlayerHUD(playerId id)
        {
            Vector2 hudPos = getHudPosition(id, hudScale);
            HUD th = guiFactory.createPlayerHUD(id,hudPos, true, hudScale, 0.5f);
            GUIElements.Add(th);
            return th;
        }

        private Vector2 getHudPosition(playerId id, Vector2 scale)
        {
            Rectangle hud = guiFactory.HudDimensions;

            switch(id)
            {
                case playerId.circle:
                    return new Vector2(0, 0);

                case playerId.triangle:
                    return new Vector2(boundries.Width - (int)hud.Width*scale.X, 0);

                case playerId.square:
                    return new Vector2(0, boundries.Height - (int)hud.Height*scale.Y);
                
                default:
                    return new Vector2(boundries.Width - (int)hud.Width * scale.X, boundries.Height - (int)hud.Height * scale.Y);

            }
        }


        public bool Hide(GUIElement element)
        {
            GUIElement aux = element;

            if (aux != null)
            {
                aux.IsVisible = false;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Show(GUIElement element)
        {
            GUIElement aux = element;

            if (aux != null)
            {
                aux.IsVisible = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void showScore(playerId id, Vector2 position, string profit)
        {
            
            if (narrative)
            {
                Icon achievement = guiFactory.creteAchievementIcon(id, position);
                floatingIcons.Add(achievement);
            }
            else
            {
                FloatingText text;
                text = new FloatingText(position, new Color(50,176,183), "+" + profit);
                texts.Add(text);
            }
            
        }

        public void Update(float elapsedTime)
        {
            foreach (FloatingText t in texts)
            {
                t.Update(elapsedTime);
            }
            foreach (Icon i in floatingIcons)
            {
                i.Update(elapsedTime);
            }
            UpdateFloatLists();
        }

        public void UpdateFloatLists()
        {
            List<FloatingText> delete = new List<FloatingText>();

            foreach (FloatingText t in texts)
            {
                if (!t.IsVisible)
                {
                    delete.Add(t);
                }
            }
            foreach (FloatingText t in delete)
            {
                texts.Remove(t);
            }

            List<Icon> deleteIcon = new List<Icon>();
            foreach (Icon i in floatingIcons)
            {
                if (!i.IsVisible)
                {
                    deleteIcon.Add(i);
                }
            }
            foreach (Icon i in deleteIcon)
            {
                floatingIcons.Remove(i);
            }
        }

      
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Writer fontWriter)
        {
            foreach (GUIElement e in GUIElements)
            {
                if (e.IsVisible)
                {
                    e.Draw(gameTime, spriteBatch, fontWriter);
                }
            }

            if (texts.Count > 0)
            {
                foreach (FloatingText t in texts)
                {
                    t.Draw(fontWriter);
                }
            }

            if (floatingIcons.Count > 0)
            {
                foreach (Icon i in floatingIcons)
                {
                    i.Draw(gameTime, spriteBatch, fontWriter);
                }
            }
        }


        #region positions
        private Vector2 getUpperLeftCorner(Texture2D t, Vector2 scale)
        {
            float Width = t.Width * scale.X;
            float Height = t.Height * scale.Y;
            Vector2 aux = new Vector2(Width / 2 + padding.X, Height / 2 + padding.Y);
            return aux;
        }

        private Vector2 getUpperRightCorner(Texture2D t, Vector2 scale)
        {
            float Width = t.Width * scale.X;
            float Height = t.Height * scale.Y;
            Vector2 aux = new Vector2(boundries.Width - (padding.X + Width / 2), Height / 2 + padding.Y);
            return aux;
        }

        private Vector2 getLowerLeftCorner(Texture2D t, Vector2 scale)
        {
            float Width = t.Width * scale.X;
            float Height = t.Height * scale.Y;
            Vector2 aux = new Vector2(Width / 2 + padding.X, boundries.Height - (padding.Y + Height / 2));
            return aux;
        }

        private Vector2 getLowerRightCorner(Texture2D t, Vector2 scale)
        {
            float Width = t.Width * scale.X;
            float Height = t.Height * scale.Y;
            Vector2 aux = new Vector2(boundries.Width - (padding.X + Width / 2), boundries.Height - (padding.Y + Height / 2));
            return aux;
        }

        private Vector2 getPosition1(Texture2D t, Vector2 scale)
        {
            float Width = t.Width * scale.X;
            float Height = t.Height * scale.Y;
            Vector2 aux = new Vector2(Width / 2 + padding.X, boundries.Height - (padding.Y + Height / 2));
            return aux;
        }

        private Vector2 getPosition2(Texture2D t, Vector2 scale)
        {
            float Width = t.Width * scale.X;
            float Height = t.Height * scale.Y;
            Vector2 aux = new Vector2(Width / 2 + padding.X + separation + Width, boundries.Height - (padding.Y + Height / 2));
            return aux;
        }

        private Vector2 getPosition3(Texture2D t, Vector2 scale)
        {
            float Width = t.Width * scale.X;
            float Height = t.Height * scale.Y;
            Vector2 aux = new Vector2(Width / 2 + padding.X + 2 * separation + 2 * Width, boundries.Height - (padding.Y + Height / 2));
            return aux;
        }


        private Vector2 getPosition4(Texture2D t, Vector2 scale)
        {
            float Width = t.Width * scale.X;
            float Height = t.Height * scale.Y;
            Vector2 aux = new Vector2(Width / 2 + padding.X + 3 * separation + 3 * Width, boundries.Height - (padding.Y + Height / 2));
            return aux;
        }
        #endregion

    }
}
