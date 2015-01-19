using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RawInputSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace GameModel
{
    public class MiceHandler : System.Windows.Forms.Form
    {
        public RawMouseInput inputController;
        public RawMouse[] multiMouse = new RawMouse[3];
        public Texture2D[] cursor = new Texture2D[3];
        public int MouseCount = 0;

        public MiceHandler()
        {
            inputController = new RawMouseInput();
            inputController.RegisterForWM_INPUT(this.Handle);

            if (inputController.Mice.Count > 0)
            {
                foreach (RawMouse mouse in inputController.Mice)
                {
                    if (mouse.Buttons.Length >= 3 && MouseCount < 3)
                    {
                        multiMouse[MouseCount] = mouse;
                        MouseCount++;
                    }
                }
            }
        }

        protected const int WM_INPUT = 0x00FF;
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_INPUT:
                    //read in new mouse values.
                    inputController.UpdateRawMouse(m.LParam);
                    break;
            }
            base.WndProc(ref m);
        }

        
        public void DrawMice(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < MouseCount; i++)
            {
                spriteBatch.Draw(cursor[i], new Vector2((float)multiMouse[i].X - 7.5f, (float)multiMouse[i].Y - 7.5f), Color.White);
                                           
            }
        }

        public Vector2 get2dPosition(int id)
        {
            return new Vector2(multiMouse[id].X, multiMouse[id].Y);
        }

    }
}
