using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GoblinXNA.UI.UI2D;
using GoblinXNA;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using ARGame.Utils;

namespace ARGame.GameGUI
{
    public class ServerIndivGUI : GameGUI
    {
        public event Action NextLevel;
        public event Action<int> ResetLevel;
        public event Action PauseGame;
        public event Action ResumeGame;

        protected G2DButton nextLevelButton;
        protected G2DToggleButton pauseButton;
        public G2DToggleButton PauseButton { get { return pauseButton; } }
        protected G2DPanel nextLevelPanel;
        protected Dictionary<G2DButton, int> resetButtonTable;

        public Dictionary<int, GameData> PlayersData { get; protected set; }

        protected ContentManager contentManager;

        public ServerIndivGUI(Dictionary<int, GameData> playersData)
            : base() 
        {
            PlayersData = playersData;
            resetButtonTable = new Dictionary<G2DButton, int>();
        }
       
        protected override void InitializeComponents(ContentManager contentManager)
        {
            this.contentManager = contentManager;

            G2DPanel messagePanel = new G2DPanel();
            messagePanel.Texture = contentManager.GetTexture("barraSuperior");
            messagePanel.ComponentUnit = G2DComponent.Unit.relative;
            messagePanel.Transparency = 0.9f;
            messagePanel.Border = GoblinEnums.BorderFactory.LineBorder;
            messagePanel.BorderColor = Color.CornflowerBlue;
            messagePanel.RelativePosition = new Vector2(0f, 0);
            messagePanel.RelativeSize = new Vector2(0.8f, 0.1f);
            messagePanel.TextFont = textFont;

            G2DPanel gameDataPanel = new G2DPanel();//new G2DBoxLayout(G2DBoxLayout.Axis.Y_AXIS));
            gameDataPanel.Texture = contentManager.GetTexture("MainScreen");
            gameDataPanel.ComponentUnit = G2DComponent.Unit.relative;
            gameDataPanel.Transparency = 0.9f;
            gameDataPanel.Border = GoblinEnums.BorderFactory.LineBorder;
            gameDataPanel.BorderColor = Color.CornflowerBlue;
            gameDataPanel.RelativePosition = new Vector2(0.00f, 0.0f);
            gameDataPanel.RelativeSize = new Vector2(1f, 1f);
            gameDataPanel.TextFont = textFont;

            int i = 0;
            foreach (KeyValuePair<int, GameData> playersDataPair in PlayersData)
            {
                G2DButton button = new G2DButton("Reset " + i);
                button.HighlightColor = Color.TransparentWhite;
                button.BackgroundColor = Color.TransparentWhite;
                button.BorderColor = Color.TransparentWhite;
                //button.Bounds = new Rectangle(300, 400 + i * 50, 20,10);
                button.ComponentUnit = G2DComponent.Unit.relative;
                button.RelativeSize = new Vector2(0.1f, 0.025f);
                button.RelativePosition = new Vector2(0.65f, 0.35f + i*0.05f);
                button.ActionPerformedEvent += OnReset;
                gameDataPanel.AddChild(button);
                resetButtonTable.Add(button, playersDataPair.Key);
                i++;
            }
            
            nextLevelPanel = new G2DPanel();
            nextLevelPanel.Bounds = new Rectangle(700, 500, 100, 100);
            nextLevelPanel.Texture = contentManager.GetTexture("chargeBack");
            nextLevelPanel.BackgroundColor = Color.TransparentWhite;
            nextLevelPanel.Border = GoblinEnums.BorderFactory.LineBorder;
            nextLevelPanel.BorderColor = Color.CornflowerBlue;


            nextLevelButton = new G2DButton(" ");
            nextLevelButton.HighlightColor = Color.TransparentWhite;
            nextLevelButton.ImageTexture = contentManager.GetTexture("answerIcon");
            nextLevelButton.BackgroundColor = Color.TransparentWhite;
            nextLevelButton.BorderColor = Color.TransparentWhite;
            nextLevelButton.ComponentUnit = G2DComponent.Unit.relative;
            nextLevelButton.RelativeSize = new Vector2(0.9f, 0.9f);
            nextLevelButton.RelativePosition = new Vector2(0.05f, 0.05f);
            nextLevelButton.ActionPerformedEvent += OnNextLevel;
            nextLevelPanel.AddChild(nextLevelButton);

            G2DPanel pausePanel = new G2DPanel();
            pausePanel.Bounds = new Rectangle(0, 500, 100, 100);
            pausePanel.Texture = contentManager.GetTexture("shooterBack");
            pausePanel.BackgroundColor = Color.TransparentWhite;
            pausePanel.Border = GoblinEnums.BorderFactory.LineBorder;
            pausePanel.BorderColor = Color.CornflowerBlue;


            pauseButton = new G2DToggleButton(" ");
            pauseButton.IsCircular = true;
            pauseButton.HighlightColor = Color.TransparentWhite;
            pauseButton.ImageTexture = contentManager.GetTexture("pause");
            pauseButton.ToggledTexture = contentManager.GetTexture("play");
            //pauseButton.DisabledTexture = contentManager.GetTexture("activateIconDisabled");
            pauseButton.BorderColor = Color.TransparentWhite;
            pauseButton.ComponentUnit = G2DComponent.Unit.relative;
            pauseButton.RelativeSize = new Vector2(0.9f, 0.9f);
            pauseButton.RelativePosition = new Vector2(0.05f, 0.05f);
            pauseButton.ActionPerformedEvent += OnPause;
            pausePanel.AddChild(pauseButton);


            scene.UIRenderer.Add2DComponent(gameDataPanel);
            scene.UIRenderer.Add2DComponent(messagePanel);
            scene.UIRenderer.Add2DComponent(nextLevelPanel);
            scene.UIRenderer.Add2DComponent(pausePanel);
            
        }

        public override void Draw()
        {
            int i = 0;
            UI2DRenderer.WriteText(new Vector2(100, 200 + (i-1) * 30), "Nivel completado >> Intentos >> Subnivel", Color.White, this.textFont);
            foreach (KeyValuePair<int, GameData> playersDataPair in PlayersData)
            {
                UI2DRenderer.WriteText(new Vector2(100, 200 + i*30), "Jugador " + playersDataPair.Key +" : " + 
                    playersDataPair.Value.CurrentLevelData.ToString(), Color.White, this.textFont);
                i++;
            }
        }



        protected virtual void OnNextLevel(object source)
        {
            if (NextLevel != null)
                NextLevel();
        }

        protected virtual void OnReset(object source)
        {
            G2DButton button = (G2DButton)source;
            if (ResetLevel != null)
                ResetLevel(resetButtonTable[button]);
        }

        protected virtual void OnPause(object source)
        {
            if (pauseButton.Selected)
            {
                if (PauseGame != null)
                    PauseGame();
            }
            else
            {
                if (ResumeGame != null)
                    ResumeGame();
            }
        }

    
    }
}
