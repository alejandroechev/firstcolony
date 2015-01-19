using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GoblinXNA.UI.UI2D;
using GoblinXNA;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using GoblinXNA.Device.Generic;
using Microsoft.Xna.Framework.Input;

namespace ARGame.GameGUI
{
    class GamePlayGUI : GameGUI
    {
        public event Action<float> ChangeCharge;
        public event Action<bool> Activate;
        //public event Action ActivateSecondButton;
        public event Action Shoot;

        
        public G2DPanel FieldPanel { get; private set; }
        public G2DPanel RedFieldPanel { get; private set; }
        public G2DPanel BlueFieldPanel { get; private set; }
        public G2DPanel VioletFieldPanel { get; private set; }
        public G2DPanel PopupMessagePanel { get; private set; }
        public G2DButton OkButton { get; private set; }
        public G2DToggleButton ForceFieldButton { get; private set; }
        //public G2DToggleButton ForceFieldButton2 { get; private set; }
        public G2DToggleButton RayShooterButton { get; private set; }
        public G2DButton PlusButton { get; private set; }
        public G2DButton MinusButton { get; private set; }
        public G2DSlider ChargeSlider { get; private set; }
        public Texture2D PlayerTexture { get; set; }
        public G2DPanel messagePanel { get; set; }
        public G2DPanel toolBarPanel { get; set; }

        protected List<G2DComponent> panels;
        private Action writePopUp;
        private string popupMessage;

        private bool selected = false;
        private bool popup = false;

        public GamePlayGUI() : base() 
        {
            panels = new List<G2DComponent>();            
           
        }

        protected virtual void KeyPressEvent(Keys key, KeyModifier modifier)
        {
            if (key == Keys.Delete)
            {
                if (popup)
                    OnOkPopup(null);
                else
                {
                    ChargeSlider.Value += 20;
                    PopupMessagePanel.Transparency = 0.0f;
                
                }
            }
            else if (key == Keys.Back)
            {
                if (popup)
                    OnOkPopup(null);
                else
                {
                    ChargeSlider.Value -= 20;
                    PopupMessagePanel.Transparency = 0.0f;
                
                }
            }
            else if (key == Keys.Escape || key == Keys.D1)
            {
                if (popup)
                    OnOkPopup(null);
                else
                    OnActivate();
                
            } 
        }
       
        protected override void InitializeComponents(ContentManager contentManager)
        {

            messagePanel = new G2DPanel();//new G2DBoxLayout(G2DBoxLayout.Axis.Y_AXIS));
            messagePanel.Texture = contentManager.GetTexture("barraSuperior");
            messagePanel.ComponentUnit = G2DComponent.Unit.relative;
            //toolBarPanel.Transparency = 1;
            //messagePanel.BackgroundColor = Color.LightBlue;
            messagePanel.Transparency = 0.9f;
            messagePanel.Border = GoblinEnums.BorderFactory.LineBorder;
            messagePanel.BorderColor = Color.CornflowerBlue;
            messagePanel.RelativePosition = new Vector2(0f, 0);
            messagePanel.RelativeSize = new Vector2(0.8f, 0.1f);
            messagePanel.TextFont = textFont;

            PopupMessagePanel = new G2DPanel();//new G2DBoxLayout(G2DBoxLayout.Axis.Y_AXIS));
            PopupMessagePanel.Texture = contentManager.GetTexture("MainScreen");
            PopupMessagePanel.ComponentUnit = G2DComponent.Unit.relative;            
            PopupMessagePanel.Transparency = 0.9f;
            PopupMessagePanel.Border = GoblinEnums.BorderFactory.LineBorder;
            PopupMessagePanel.BorderColor = Color.CornflowerBlue;
            PopupMessagePanel.RelativePosition = new Vector2(0.00f, 0.0f);
            PopupMessagePanel.RelativeSize = new Vector2(1f, 1f);
            PopupMessagePanel.TextFont = textFont;

            
            OkButton = new G2DButton(" ");
            OkButton.ImageTexture = contentManager.GetTexture("check");
            OkButton.HighlightColor = Color.TransparentWhite;
            OkButton.BackgroundColor = Color.TransparentWhite;
            OkButton.BorderColor = Color.TransparentWhite;
            OkButton.ComponentUnit = G2DComponent.Unit.relative;
            OkButton.RelativeSize = new Vector2(0.1f, 0.2f);
            OkButton.RelativePosition = new Vector2(0.8f, 0.7f);
            OkButton.ActionPerformedEvent += OnOkPopup;
            //PopupMessagePanel.AddChild(OkButton);
            

            toolBarPanel = new G2DPanel();//new G2DBoxLayout(G2DBoxLayout.Axis.Y_AXIS));
            toolBarPanel.Texture = contentManager.GetTexture("sliderBack");
            toolBarPanel.ComponentUnit = G2DComponent.Unit.relative;
            //toolBarPanel.Transparency = 1;
            toolBarPanel.BackgroundColor = Color.TransparentWhite;
            toolBarPanel.Border = GoblinEnums.BorderFactory.LineBorder;
            toolBarPanel.BorderColor = Color.CornflowerBlue;
            toolBarPanel.RelativePosition = new Vector2(0.9f, 0);
            toolBarPanel.RelativeSize = new Vector2(0.1f, 0.8f);
            toolBarPanel.TextFont = textFont;
            //toolBarPanel.InsetsComponent = new GoblinXNA.UI.Insets(5, 5, 5, 5);

            ChargeSlider = new G2DSlider(GoblinEnums.Orientation.Vertical, -80, 80, 1);
            ChargeSlider.Orientation = GoblinEnums.Orientation.Vertical;
            ChargeSlider.ComponentUnit = G2DComponent.Unit.relative;
            ChargeSlider.BackgroundColor = Color.TransparentWhite;
            ChargeSlider.TrackBorderColor = Color.TransparentWhite;
            ChargeSlider.KnobBorderColor = Color.TransparentWhite;
            ChargeSlider.KnobLength = 30;
            ChargeSlider.TrackColor = new Color(57, 80, 162);
            ChargeSlider.KnobColor = new Color(88, 183, 221);
            ChargeSlider.PaintTicks = false;
            ChargeSlider.PaintLabels = false;
            //ChargeSlider.MajorTickSpacing = 1;
            //ChargeSlider.Maximum = 5;
            ChargeSlider.Value = 0;
            //ChargeSlider.Maximum = -5;
            ChargeSlider.RelativeSize = new Vector2(0.5f, 0.64f);
            ChargeSlider.RelativePosition = new Vector2(0.28f, 0.18f);
            ChargeSlider.StateChangedEvent += OnChargeChanged;
            toolBarPanel.AddChild(ChargeSlider);

            // This loads the group button
            PlusButton = new G2DButton(" ");
            PlusButton.ImageTexture = contentManager.GetTexture("sliderPlus");            
            PlusButton.HighlightColor = Color.TransparentWhite;
            PlusButton.BackgroundColor = Color.TransparentWhite;
            PlusButton.BorderColor = Color.TransparentWhite;
            PlusButton.ComponentUnit = G2DComponent.Unit.relative;
            PlusButton.RelativeSize = new Vector2(0.9f, 0.125f);
            PlusButton.RelativePosition = new Vector2(0.1f, 0.05f);
            //PlusButton.ActionPerformedEvent += OnPlusCharge;
            toolBarPanel.AddChild(PlusButton);

            // This loads the map button
            MinusButton = new G2DButton(" ");
            MinusButton.ImageTexture = contentManager.GetTexture("sliderMinus");
            MinusButton.HighlightColor = Color.TransparentWhite;
            MinusButton.BackgroundColor = Color.TransparentWhite;
            MinusButton.BorderColor = Color.TransparentWhite;
            MinusButton.ComponentUnit = G2DComponent.Unit.relative;
            MinusButton.RelativeSize = new Vector2(0.9f, 0.125f);
            MinusButton.RelativePosition = new Vector2(0.1f, 0.85f);
            //MinusButton.ActionPerformedEvent += OnMinusCharge;
            toolBarPanel.AddChild(MinusButton);

            // This loads the force field button

            G2DPanel forceFieldPanel = new G2DPanel();
            forceFieldPanel.Bounds = new Rectangle(700, 500, 100, 100);
            forceFieldPanel.Texture = contentManager.GetTexture("chargeBack");
            forceFieldPanel.BackgroundColor = Color.TransparentWhite;
            forceFieldPanel.Border = GoblinEnums.BorderFactory.LineBorder;
            forceFieldPanel.BorderColor = Color.CornflowerBlue;


            ForceFieldButton = new G2DToggleButton(" ");
            ForceFieldButton.IsCircular = true;
            ForceFieldButton.HighlightColor = Color.TransparentWhite;
            ForceFieldButton.ImageTexture = contentManager.GetTexture("activateIcon");
            ForceFieldButton.ToggledTexture = contentManager.GetTexture("activateIconToggle");
            ForceFieldButton.DisabledTexture = contentManager.GetTexture("activateIconDisabled");
            ForceFieldButton.BackgroundColor = Color.TransparentWhite;
            ForceFieldButton.BorderColor = Color.TransparentWhite;
            ForceFieldButton.ComponentUnit = G2DComponent.Unit.relative;
            ForceFieldButton.RelativeSize = new Vector2(0.9f, 0.9f);
            ForceFieldButton.RelativePosition = new Vector2(0.05f, 0.05f);
            forceFieldPanel.AddChild(ForceFieldButton);
            

            G2DPanel rayShooterPanel = new G2DPanel();
            rayShooterPanel.Bounds = new Rectangle(0, 500, 100, 100);
            rayShooterPanel.Texture = contentManager.GetTexture("shooterBack");
            rayShooterPanel.BackgroundColor = Color.TransparentWhite;
            rayShooterPanel.Border = GoblinEnums.BorderFactory.LineBorder;
            rayShooterPanel.BorderColor = Color.CornflowerBlue;


            RayShooterButton = new G2DToggleButton(" ");
            RayShooterButton.IsCircular = true;
            RayShooterButton.HighlightColor = Color.TransparentWhite;
            RayShooterButton.ImageTexture = contentManager.GetTexture("shooterIcon");
            RayShooterButton.ToggledTexture = contentManager.GetTexture("shooterIcon");
            RayShooterButton.DisabledTexture = contentManager.GetTexture("shooterIconDisabled");
            RayShooterButton.BackgroundColor = Color.TransparentWhite;
            RayShooterButton.BorderColor = Color.TransparentWhite;
            RayShooterButton.ComponentUnit = G2DComponent.Unit.relative;
            RayShooterButton.RelativeSize = new Vector2(0.9f, 0.9f);
            RayShooterButton.RelativePosition = new Vector2(0.05f, 0.05f);
            RayShooterButton.ActionPerformedEvent += OnShoot;
            //rayShooterPanel.AddChild(RayShooterButton);


            //this loads the background force field
            FieldPanel = new G2DPanel();
            FieldPanel.ComponentUnit = G2DComponent.Unit.relative;
            FieldPanel.BackgroundColor = Color.LightBlue;
            FieldPanel.Texture = contentManager.GetTexture("storm");
            FieldPanel.Transparency = 0;
            FieldPanel.RelativeSize = new Vector2(1, 1);
            FieldPanel.RelativePosition = new Vector2(0, 0);

            RedFieldPanel = new G2DPanel();
            RedFieldPanel.ComponentUnit = G2DComponent.Unit.relative;
            RedFieldPanel.BackgroundColor = Color.White;
            RedFieldPanel.Texture = contentManager.GetTexture("redfield");
            RedFieldPanel.Transparency = 0;
            RedFieldPanel.RelativeSize = new Vector2(1, 1);
            RedFieldPanel.RelativePosition = new Vector2(0, 0);

            BlueFieldPanel = new G2DPanel();
            BlueFieldPanel.ComponentUnit = G2DComponent.Unit.relative;
            BlueFieldPanel.BackgroundColor = Color.White;
            BlueFieldPanel.Texture = contentManager.GetTexture("bluefield");
            BlueFieldPanel.Transparency = 0;
            BlueFieldPanel.RelativeSize = new Vector2(1, 1);
            BlueFieldPanel.RelativePosition = new Vector2(0, 0);

            VioletFieldPanel = new G2DPanel();
            VioletFieldPanel.ComponentUnit = G2DComponent.Unit.relative;
            VioletFieldPanel.BackgroundColor = Color.LightBlue;
            VioletFieldPanel.Texture = contentManager.GetTexture("violetfield");
            VioletFieldPanel.Transparency = 0;
            VioletFieldPanel.RelativeSize = new Vector2(1, 1);
            VioletFieldPanel.RelativePosition = new Vector2(0, 0);

            //this loads the bulleye panel            
            G2DPanel bulleyePanel = new G2DPanel();
            bulleyePanel.Bounds = new Rectangle((int)(this.width / 2.0) - 17, (int)(this.height / 2.0) - 17, 34, 34);
            bulleyePanel.Texture = contentManager.GetTexture("mira");
            bulleyePanel.BackgroundColor = Color.TransparentWhite;
            bulleyePanel.Border = GoblinEnums.BorderFactory.LineBorder;
            bulleyePanel.BorderColor = Color.CornflowerBlue;


            scene.UIRenderer.Add2DComponent(bulleyePanel);
            scene.UIRenderer.Add2DComponent(RedFieldPanel);
            scene.UIRenderer.Add2DComponent(BlueFieldPanel);
            scene.UIRenderer.Add2DComponent(VioletFieldPanel);
            scene.UIRenderer.Add2DComponent(messagePanel);
            scene.UIRenderer.Add2DComponent(PopupMessagePanel);
            scene.UIRenderer.Add2DComponent(toolBarPanel);
            //scene.UIRenderer.Add2DComponent(forceFieldPanel);
            //scene.UIRenderer.Add2DComponent(forceFieldPanel2);
            //scene.UIRenderer.Add2DComponent(rayShooterPanel);

            panels.Add(bulleyePanel);

            panels.Add(VioletFieldPanel);
            panels.Add(RedFieldPanel);
            panels.Add(BlueFieldPanel);
            panels.Add(messagePanel);
            panels.Add(PopupMessagePanel);
            panels.Add(toolBarPanel);
            //panels.Add(forceFieldPanel);
          //  panels.Add(forceFieldPanel2);
           // panels.Add(rayShooterPanel);

            
            PopupMessagePanel.Transparency = 0.0f;
            writePopUp = new Action(GamePlayGUI_DrawEvent);

            KeyboardInput.Instance.KeyPressEvent += new HandleKeyPress(KeyPressEvent);

        }

        public override void Reset()
        {
            //deseleccionamos el boton de campo de fuerza
            if (selected)
                OnActivate();

            this.ChargeSlider.Value = 0;
        }
        public override void WriteMessage(string message)
        {
            if (!popup)
            {
                popup = true;

                if (PopupMessagePanel.Children.Count > 1) return;

                foreach (G2DComponent component in panels)
                    component.Transparency = 0.0f;

                PopupMessagePanel.Transparency = 0.9f;
                messagePanel.Transparency = 0.9f;
                ForceFieldButton.Enabled = false;

                G2DTextField text = new G2DTextField(message, 50);
                text.RelativePosition = new Vector2(0.4f, 0.5f);
                text.RelativeSize = new Vector2(0.8f, 0.8f);
                text.DrawBackground = false;
                text.DrawBorder = false;
                PopupMessagePanel.AddChild(text);
            }
        }

        public override void Pause(bool pause)
        {
            //ForceFieldButton.Visible = !pause;
            //RayShooterButton.Visible = !pause;
            PlusButton.Visible = !pause;
            MinusButton.Visible = !pause;
            ChargeSlider.Visible = !pause;
        }

        private void GamePlayGUI_DrawEvent()
        {
            UI2DRenderer.WriteText(new Vector2(0, 60), popupMessage, Color.Blue, this.textFont);
        }

        protected virtual void OnOkPopup(object source) 
        {
            foreach (G2DComponent component in panels)
                component.Transparency = 0.9f;

            if(selected)
                SetForceField();
            PopupMessagePanel.Transparency = 0;
            PopupMessagePanel.RemoveChildAt(0);

            popup = false;

        }

        protected virtual void OnActivate()
        {
            selected = !selected;
            SetForceField();
            //FieldPanel.Transparency = selected ? 0.4f : 0;
            if (Activate != null)
                Activate(selected);
        }
       

        protected virtual void OnShoot(object source)
        {
            //FieldPanel.Transparency = ((G2DToggleButton)source).Selected ? 0.35f : 0;
            if (Shoot != null)
                Shoot();
        }
        //protected virtual void OnPlusCharge(object source)
        //{
        //    ChargeSlider.Value = ChargeSlider.Value + 20;
        //}

        //protected virtual void OnMinusCharge(object source)
        //{
        //    ChargeSlider.Value = ChargeSlider.Value - 20;
        //}

        protected virtual void OnChargeChanged(object source)
        {
            SetForceField();
            if (ChangeCharge != null)
                ChangeCharge(ChargeSlider.Value);
        }

        private void SetForceField()
        {
            if (selected)
            {
                //FieldPanel.Transparency = 0.3f;
                if (ChargeSlider.Value > 0)
                {
                    RedFieldPanel.Transparency = 0.4f;
                    BlueFieldPanel.Transparency = 0;
                    VioletFieldPanel.Transparency = 0.0f;
                }
                else if (ChargeSlider.Value < 0)
                {
                    RedFieldPanel.Transparency = 0.0f;
                    BlueFieldPanel.Transparency = 0.4f;
                    VioletFieldPanel.Transparency = 0.0f;
                }
                else
                {
                    RedFieldPanel.Transparency = 0.0f;
                    BlueFieldPanel.Transparency = 0.0f;
                    VioletFieldPanel.Transparency = 0.4f;
                }
            }
            else
            {
                VioletFieldPanel.Transparency = 0;
                RedFieldPanel.Transparency = 0;
                BlueFieldPanel.Transparency = 0;
            }
        }
    }
}
