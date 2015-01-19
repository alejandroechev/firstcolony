using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using GoblinXNA.SceneGraph;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using GoblinXNA.UI.UI2D;
using Microsoft.Xna.Framework;
using GoblinXNA;

namespace ARGame.GameGUI
{
    public abstract class GameGUI
    {
        public event Action DrawEvent;
        protected Scene scene;
        protected SpriteFont textFont;

        protected int width, height;

        public virtual void Init(Scene scene, ContentManager contentManager, SpriteFont textFont)
        {
            this.scene = scene;
            this.textFont = textFont;
            scene.UIRenderer.RemoveAll2DComponents();
            scene.BackgroundColor = Color.Black;
            width = scene.VideoVisibleArea.Width;
            height = scene.VideoVisibleArea.Height;
            InitializeComponents(contentManager);
           
        }

        public virtual void Draw()
        {
            if (DrawEvent != null)
                DrawEvent();
        }

        protected abstract void InitializeComponents(ContentManager contentManager);
        public virtual void WriteMessage(string message){ }
        public virtual void Reset() { }
        public virtual void Pause(bool pause) { }
        
    }
}
