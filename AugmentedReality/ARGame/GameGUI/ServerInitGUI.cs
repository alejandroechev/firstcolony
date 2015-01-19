using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GoblinXNA.UI.UI2D;
using GoblinXNA;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace ARGame.GameGUI
{
    public class ServerInitGUI : GameGUI
    {
        public event Action Start;

        private G2DButton startButton;
        private G2DPanel startPanel;


        public ServerInitGUI()
            : base() 
        {
        }
       
        protected override void InitializeComponents(ContentManager contentManager)
        {

            G2DPanel messagePanel = new G2DPanel();
            messagePanel.Texture = contentManager.GetTexture("barraSuperior");
            messagePanel.ComponentUnit = G2DComponent.Unit.relative;
            messagePanel.Transparency = 0.9f;
            messagePanel.Border = GoblinEnums.BorderFactory.LineBorder;
            messagePanel.BorderColor = Color.CornflowerBlue;
            messagePanel.RelativePosition = new Vector2(0f, 0);
            messagePanel.RelativeSize = new Vector2(0.8f, 0.1f);
            messagePanel.TextFont = textFont;

            
            startPanel = new G2DPanel();
            startPanel.Bounds = new Rectangle(700, 500, 100, 100);
            startPanel.Texture = contentManager.GetTexture("chargeBack");
            startPanel.BackgroundColor = Color.TransparentWhite;
            startPanel.Border = GoblinEnums.BorderFactory.LineBorder;
            startPanel.BorderColor = Color.CornflowerBlue;


            startButton = new G2DButton(" ");
            startButton.HighlightColor = Color.TransparentWhite;
            startButton.ImageTexture = contentManager.GetTexture("answerIcon");
            startButton.BackgroundColor = Color.TransparentWhite;
            startButton.BorderColor = Color.TransparentWhite;
            startButton.ComponentUnit = G2DComponent.Unit.relative;
            startButton.RelativeSize = new Vector2(0.9f, 0.9f);
            startButton.RelativePosition = new Vector2(0.05f, 0.05f);
            startButton.ActionPerformedEvent += OnStart;
            startPanel.AddChild(startButton);
            

            scene.UIRenderer.Add2DComponent(messagePanel);
            scene.UIRenderer.Add2DComponent(startPanel);
        }

        protected virtual void OnStart(object source)
        {
            if (Start != null)
                Start();
        }

    
    }
}
