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
    public class GameInitGUI : GameGUI
    {
        public event Action Connect;

        private G2DButton connectButton;
        private G2DPanel connectPanel;
        
        
        public GameInitGUI() : base() 
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

            
            connectPanel = new G2DPanel();
            connectPanel.Bounds = new Rectangle(700, 500, 100, 100);
            connectPanel.Texture = contentManager.GetTexture("chargeBack");
            connectPanel.BackgroundColor = Color.TransparentWhite;
            connectPanel.Border = GoblinEnums.BorderFactory.LineBorder;
            connectPanel.BorderColor = Color.CornflowerBlue;


            connectButton = new G2DButton(" ");
            connectButton.HighlightColor = Color.TransparentWhite;
            connectButton.ImageTexture = contentManager.GetTexture("answerIcon");
            connectButton.BackgroundColor = Color.TransparentWhite;
            connectButton.BorderColor = Color.TransparentWhite;
            connectButton.ComponentUnit = G2DComponent.Unit.relative;
            connectButton.RelativeSize = new Vector2(0.9f, 0.9f);
            connectButton.RelativePosition = new Vector2(0.05f, 0.05f);
            connectButton.ActionPerformedEvent += OnConnect;
            connectPanel.AddChild(connectButton);
            

            scene.UIRenderer.Add2DComponent(messagePanel);
            
        }

        public void EnableConnectPanel()
        {
            scene.UIRenderer.Add2DComponent(connectPanel);
        }

        public void DisableConnectPanel()
        {
            scene.UIRenderer.Remove2DComponent(connectPanel);
        }

        protected virtual void OnConnect(object source)
        {
            if (Connect != null)
                Connect();
        }

    
    }
}
