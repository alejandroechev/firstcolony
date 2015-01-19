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
using GameModel;

namespace Astronautas2D.Utils
{
    public class MiceClickArgs : EventArgs
    {
        public MiceClickArgs(int i, Vector2 pos)
        {
            index = i;
            position = pos;
        }
        // Index del mouse que gatilló el evento en cuestión.
        private int index;
        public int Index
        {
            get { return index; }
        }

        // Posición donde se desarrollo el click
        private Vector2 position;
        public Vector2 Position
        {
            get { return position; }
        }

    }

    public class MiceWheelArgs : EventArgs
    {
        public MiceWheelArgs(int i, int d)
        {
            index = i;
            delta = d;
        }
        // Index del mouse que gatilló el evento en cuestión.
        private int index;
        public int Index
        {
            get { return index; }
        }
        private int delta;
        public int Delta
        {
            get { return delta; }
        }
    }


    public class MiceManager : GameModel.MiceHandler
    {
        int totalPlayers;
        int totalButtons;
        bool[,] buttonsCurrentState;
        bool[,] buttonsPreviousState;
        int[] wheelsCurrentState;
        int[] wheelsPreviousState;
        Rectangle boundries;
        public Rectangle Boundries { get { return boundries;} }

        // Delegados
        public delegate void ClickEventHandler(object sender, MiceClickArgs a);
        public delegate void WheelEventHandler(object sender, MiceWheelArgs a);
        // Eventos
        public event ClickEventHandler LeftClick;
        public event ClickEventHandler RightClick;
        public event ClickEventHandler CenterClick;
        public event WheelEventHandler WheelChange;

        public MiceManager(Rectangle boundries)
            : base()
        {
            totalPlayers = 3;
            totalButtons = 3;
            this.boundries = boundries;
            centerMice();
            buttonsCurrentState = new bool[totalPlayers, totalButtons];
            buttonsPreviousState = new bool[totalPlayers, totalButtons];
            wheelsCurrentState = new int[totalPlayers];
            wheelsPreviousState = new int[totalPlayers];
        }

        public void centerMice()
        {
            Point centerPos = this.boundries.Center;

            for (int i = 0; i < totalPlayers; i++)
            {
                if (base.multiMouse[i] != null)
                {
                    multiMouse[i].X = centerPos.X;
                    multiMouse[i].Y = centerPos.Y;
                }
            }
        }

        private void saveCurrentState(bool[,] stateHolder, int[] wheelHolder)
        {
            // A traves de todos los players
            for (int i = 0; i < multiMouse.Length; i++)
            {
                if (multiMouse[i] != null)
                {
                    // Por todos los botones
                    for (int j = 0; j < multiMouse[i].Buttons.Length && j < 3 ; j++)
                    {
                        stateHolder[i,j] = this.multiMouse[i].Buttons[j];
                    }
                    wheelHolder[i] = this.multiMouse[i].Z;         
                }
            }
        }

        public void loadContent(ContentManager content)
        {
            base.cursor[0] = content.Load<Texture2D>("Sprites\\Cursors\\GreenCircle2");
            base.cursor[1] = content.Load<Texture2D>("Sprites\\Cursors\\GreenTriangle2");
            base.cursor[2] = content.Load<Texture2D>("Sprites\\Cursors\\GreenSquare2");
            this.saveCurrentState(buttonsCurrentState, wheelsCurrentState);
            this.saveCurrentState(buttonsPreviousState, wheelsPreviousState);
        }

        private bool[,] copyButtonsState(bool[,] buttonState)
        {
            bool[,] result = new bool[3, 3];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    result[i,j] =  buttonState[i,j];
                }
            }
            return result;
        }

        private int[] copyWheelState(int[] wheelState)
        {
            int[] result = new int[3];
            for (int i = 0; i < 3; i++)
            {
                result[i] = wheelState[i]; 
            }
            return result;
        }


        public void Update(float elapsedTime)
        {
            // Revisamos los limites de la pantalla
            CheckBoundries();
            // Revisamos todo el input
            checkAllInput();
            // almacenamos el pasado en el actual
            buttonsPreviousState = copyButtonsState(buttonsCurrentState);
            wheelsPreviousState = copyWheelState(wheelsCurrentState);
            // actualizamos el actual
            this.saveCurrentState(buttonsCurrentState, wheelsCurrentState);
        }

        public void checkLeftClick(int index)
        {
            // Indice correcto y que el boton haya sido apretado y soltado
            if (index < 3 && index >= 0 && !buttonsCurrentState[index, 0] && buttonsPreviousState[index, 0])
            {
                OnLeftClick(new MiceClickArgs(index, new Vector2(multiMouse[index].X,multiMouse[index].Y)));
            }
        }

        public void checkRightClick(int index)
        {
            // Indice correcto y que el boton haya sido apretado y soltado
            if (index < 3 && index >= 0 && !buttonsCurrentState[index, 1] && buttonsPreviousState[index, 1])
            {
                OnRightClick(new MiceClickArgs(index, new Vector2(multiMouse[index].X, multiMouse[index].Y)));
            }
        }

        public void checkCenterClick(int index)
        {
            // Indice correcto y que el boton haya sido apretado y soltado
            if (index < 3 && index >= 0 && !buttonsCurrentState[index, 2] && buttonsPreviousState[index, 2])
            {
                OnCenterClick(new MiceClickArgs(index, new Vector2(multiMouse[index].X, multiMouse[index].Y)));
            }
        }


        private bool checkMouseWheelUp(int index)
        {
            if (index < 3 && index >= 0 && (this.wheelsCurrentState[index] > this.wheelsPreviousState[index]))
            {
                return true;
            }
            return false;
        }

        private bool checkMouseWheelDown(int index)
        {
            if (index < 3 && index >= 0 && (this.wheelsCurrentState[index] < this.wheelsPreviousState[index]))
            {
                return true;
            }
            return false;
        }

        public void CheckBoundries()
        {
            for (int i = 0; i < totalPlayers; i++)
            {
                if (this.multiMouse[i] == null) continue;

                if (this.multiMouse[i].X < 0)
                {
                    this.multiMouse[i].X = 0;
                }
                if (this.multiMouse[i].X > boundries.Width)
                {
                    this.multiMouse[i].X = boundries.Width;
                }
                if (this.multiMouse[i].Y < 0)
                {
                    this.multiMouse[i].Y = 0;
                }
                if (this.multiMouse[i].Y > boundries.Height)
                {
                    this.multiMouse[i].Y = boundries.Height;
                }
            }
        }

        public void checkMouseWheel(int index)
        {
            if (checkMouseWheelUp(index))
            {
                OnWheelChange(new MiceWheelArgs(index,1));
            }
            else if (checkMouseWheelDown(index))
            {
                OnWheelChange(new MiceWheelArgs(index, -1));
            }
        }

        private void checkAllInput()
        {
            for (int i = 0; i < totalPlayers; i++)
            {
                checkRightClick(i);
                checkLeftClick(i);
                checkCenterClick(i);
                checkMouseWheel(i);
            }
        }

        #region Eventos
        protected virtual void OnLeftClick(MiceClickArgs e)
        {
            // Copia del evento para evitar "race-condition"
            ClickEventHandler handler = LeftClick;

            // Si no hay delegados suscritos a este evento, será nulo
            if (handler != null)
            {
                // Levantamos el evento
                handler(this, e);
            }
        }

        protected virtual void OnRightClick(MiceClickArgs e)
        {
            ClickEventHandler handler = RightClick;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnCenterClick(MiceClickArgs e)
        {
            ClickEventHandler handler = CenterClick;
            if (handler != null)
            {
                handler(this, e);
            }
        }


        protected virtual void OnWheelChange(MiceWheelArgs e)
        {
            WheelEventHandler handler = WheelChange;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion
    }
}
