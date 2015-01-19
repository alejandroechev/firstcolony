using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Astronautas2D.GUI;
using Astronautas2D.GUI.GUIElements;
using Astronautas2D.Utils;
using AstroLib;

namespace Astronautas2D.Factories
{
    public class GUIElementFactory
    {
        private Texture2D[,] guiTextures;
        private Rectangle boundries;
        private Rectangle hudDimensions;
        public Rectangle HudDimensions { get { return hudDimensions; } }
        private bool narrative;

        public GUIElementFactory(Rectangle boundries)
        {
            // Obtenemos los largos de los arreglos
            this.narrative = Configuration.Instance.GetBoolParam("player", "narrative");
            this.boundries = boundries;
            int guiLength = Enum.GetNames(typeof(guiElements)).Length;
            int playerLength = Enum.GetNames(typeof(playerId)).Length;
            // Los inicializamos
            guiTextures = new Texture2D[guiLength, playerLength];
        }

        /// <summary>
        /// Método que carga las texturas de los objetos
        /// </summary>
        /// <param name="content"></param>
        public void LoadContent(ContentManager content)
        {
            guiTextures[(int)guiElements.marker, (int)playerId.circle] = content.Load<Texture2D>("Sprites\\GUI\\HUD\\Markers\\BlueMarker");
            guiTextures[(int)guiElements.marker, (int)playerId.square] = content.Load<Texture2D>("Sprites\\GUI\\HUD\\Markers\\BlueMarker");
            guiTextures[(int)guiElements.marker, (int)playerId.triangle] = content.Load<Texture2D>("Sprites\\GUI\\HUD\\Markers\\BlueMarker");
            guiTextures[(int)guiElements.slider, (int)playerId.circle] = content.Load<Texture2D>("Sprites\\GUI\\HUD\\Sliders\\CircleSlider");
            guiTextures[(int)guiElements.slider, (int)playerId.square] = content.Load<Texture2D>("Sprites\\GUI\\HUD\\Sliders\\SquareSlider");
            guiTextures[(int)guiElements.slider, (int)playerId.triangle] = content.Load<Texture2D>("Sprites\\GUI\\HUD\\Sliders\\TriangleSlider");
            guiTextures[(int)guiElements.codec, (int)playerId.team] = content.Load<Texture2D>("Sprites\\GUI\\Codec\\Skin\\skin1");

            if (narrative)
            {
                guiTextures[(int)guiElements.time, (int)playerId.team] = content.Load<Texture2D>("Sprites\\GUI\\HUD\\Icons\\Time");
                guiTextures[(int)guiElements.score, (int)playerId.team] = content.Load<Texture2D>("Sprites\\GUI\\HUD\\Icons\\Score");
                guiTextures[(int)guiElements.medal, (int)playerId.team] = content.Load<Texture2D>("Sprites\\GUI\\HUD\\Icons\\Trophy");
                guiTextures[(int)guiElements.crystal, (int)playerId.team] = content.Load<Texture2D>("Sprites\\GUI\\HUD\\Icons\\Crystal");
                guiTextures[(int)guiElements.hud, (int)playerId.circle] = content.Load<Texture2D>("Sprites\\GUI\\HUD\\Skin\\hud1");
                guiTextures[(int)guiElements.hud, (int)playerId.team] = content.Load<Texture2D>("Sprites\\GUI\\HUD\\Skin\\hud3");
                guiTextures[(int)guiElements.hud, (int)playerId.square] = content.Load<Texture2D>("Sprites\\GUI\\HUD\\Skin\\hud4");
                guiTextures[(int)guiElements.hud, (int)playerId.triangle] = content.Load<Texture2D>("Sprites\\GUI\\HUD\\Skin\\hud2");
                guiTextures[(int)guiElements.portrait, (int)playerId.team] = content.Load<Texture2D>("Sprites\\GUI\\Codec\\Portraits\\Chief");
                guiTextures[(int)guiElements.objectiveBoard, (int)playerId.team] = content.Load<Texture2D>("Sprites\\GUI\\HUD\\Skin\\skin1");
                hudDimensions = new Rectangle(0, 0, guiTextures[(int)guiElements.hud, (int)playerId.team].Width, guiTextures[(int)guiElements.hud, (int)playerId.team].Height);
            }
            else
            {
                guiTextures[(int)guiElements.time, (int)playerId.team] = content.Load<Texture2D>("Sprites\\GUI\\HUD\\Icons\\Time2");
                guiTextures[(int)guiElements.score, (int)playerId.team] = content.Load<Texture2D>("Sprites\\GUI\\HUD\\Icons\\Score2");
                guiTextures[(int)guiElements.hud, (int)playerId.circle] = content.Load<Texture2D>("Sprites\\GUI\\HUD\\Skin\\hud5");
                guiTextures[(int)guiElements.hud, (int)playerId.square] = content.Load<Texture2D>("Sprites\\GUI\\HUD\\Skin\\hud8");
                guiTextures[(int)guiElements.hud, (int)playerId.team] = content.Load<Texture2D>("Sprites\\GUI\\HUD\\Skin\\hud7");
                guiTextures[(int)guiElements.hud, (int)playerId.triangle] = content.Load<Texture2D>("Sprites\\GUI\\HUD\\Skin\\hud6");
                guiTextures[(int)guiElements.portrait, (int)playerId.team] = content.Load<Texture2D>("Sprites\\GUI\\Codec\\Portraits\\!");
                guiTextures[(int)guiElements.objectiveBoard, (int)playerId.team] = content.Load<Texture2D>("Sprites\\GUI\\HUD\\Skin\\skin3");
                hudDimensions = new Rectangle(0, 0, guiTextures[(int)guiElements.hud, (int)playerId.team].Width, guiTextures[(int)guiElements.hud, (int)playerId.team].Height);
            }

        }

        public Icon creteAchievementIcon(playerId id, Vector2 position)
        {
            if (id == playerId.team)
            {
                return new Icon(guiTextures[(int)guiElements.medal, (int)playerId.team], position, new Vector2(0.5f, 0.5f), 0, true);
            }
            else
            {
                return new Icon(guiTextures[(int)guiElements.medal, (int)playerId.team], position, new Vector2(0.5f, 0.5f), 0, true);
            }
        }

        public Slider createSlider(Vector2 position, playerId id, bool isVisible, int sliderlevels, Vector2 scale, float layer)
        {
            Slider slider = new Slider(position,sliderlevels,guiTextures[(int)guiElements.slider, (int)id],guiTextures[(int)guiElements.marker, (int)id],isVisible,scale,layer);
            return slider;
        }

        public HUD createHUD(Vector2 position, bool isVisible, Vector2 scale, float layer)
        {
            Texture2D[] textures = new Texture2D[Enum.GetNames(typeof(guiElements)).Length];
            textures[(int)guiElements.hud] = guiTextures[(int)guiElements.hud, (int)playerId.team];
            textures[(int)guiElements.time] = guiTextures[(int)guiElements.time, (int)playerId.team];
            textures[(int)guiElements.score] = guiTextures[(int)guiElements.score, (int)playerId.team];
            textures[(int)guiElements.medal] = guiTextures[(int)guiElements.medal, (int)playerId.team];
            textures[(int)guiElements.crystal] = guiTextures[(int)guiElements.crystal, (int)playerId.team];
            Vector2 newPosition = new Vector2(position.X + (textures[(int)guiElements.hud].Width *scale.X) / 2, position.Y + (textures[(int)guiElements.hud].Height * scale.Y) / 2);
            HUD hud = new HUD(playerId.team, newPosition, textures, isVisible, scale, layer, narrative);
            return hud;
        }


        public HUD createPlayerHUD(playerId id, Vector2 position, bool isVisible, Vector2 scale, float layer)
        {
            Texture2D[] textures = new Texture2D[Enum.GetNames(typeof(guiElements)).Length];
            textures[(int)guiElements.hud] = guiTextures[(int)guiElements.hud, (int)id];
            textures[(int)guiElements.time] = guiTextures[(int)guiElements.time, (int)playerId.team];
            textures[(int)guiElements.score] = guiTextures[(int)guiElements.score, (int)playerId.team];
            textures[(int)guiElements.medal] = guiTextures[(int)guiElements.medal, (int)playerId.team];
            textures[(int)guiElements.crystal] = guiTextures[(int)guiElements.crystal, (int)playerId.team];
            Vector2 newPosition = new Vector2(position.X + (textures[(int)guiElements.hud].Width * scale.X) / 2, position.Y + (textures[(int)guiElements.hud].Height * scale.Y) / 2);
            HUD hud = new HUD(id, newPosition, textures, isVisible, scale, layer, narrative);
            return hud;
        }

        public Codec createCodec(Vector2 shownPosition,Vector2 hiddenPosition, bool isVisible, Vector2 scale, float layer)
        {
            Texture2D[] textures = new Texture2D[Enum.GetNames(typeof(guiElements)).Length];
            textures[(int)guiElements.codec] = guiTextures[(int)guiElements.codec, (int)playerId.team];
            textures[(int)guiElements.portrait] = guiTextures[(int)guiElements.portrait, (int)playerId.team];
            Codec codec = new Codec(shownPosition, hiddenPosition, textures, isVisible, scale, layer);
            return codec;
        }

        public ObjectiveBoard createObjectiveBoard(int groupNumber, String description, Vector2 position, bool isVisible, Vector2 scale, float layer)
        {
            Texture2D[] textures = new Texture2D[Enum.GetNames(typeof(guiElements)).Length];
            textures[(int)guiElements.objectiveBoard] = guiTextures[(int)guiElements.objectiveBoard, (int)playerId.team];
            ObjectiveBoard board = new ObjectiveBoard(groupNumber, description, position, textures, isVisible, scale, layer, narrative);
            return board;
        }


    }
}
