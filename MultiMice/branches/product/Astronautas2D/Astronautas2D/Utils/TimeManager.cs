using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Astronautas2D.Utils
{

    public class TimeManager
    {
        private float minutes;
        private float seconds;
        private float miliseconds;
        private const int interval = 1000; // 1000 milisegundos es un seg
        private bool registerTime;
        private float[] flags;

        public TimeManager()
        {
            minutes = 0;
            seconds = 0;
            miliseconds = 0;
            registerTime = false;
            flags = new float[4];
        }

        public void setFlag(playerId id)
        {
            flags[(int)id] = this.getTimeInMiliseconds();
        }


        public float getFlag(playerId id)
        {
            return flags[(int)id];
        }


        public void Update(float elapsedTime)
        {
            if (registerTime)
            {
                // ElapsedTime viene en milisegundos
                miliseconds += elapsedTime;

                if (miliseconds >= interval)
                {
                    miliseconds = 0;
                    seconds++;

                    if (seconds == 60)
                    {
                        minutes++;
                        seconds = 0;
                    }
                }
            }
        }

        public void Stop()
        {
            registerTime = false;
        }

        public void Start()
        {
            registerTime = true;
        }

        public void Reset()
        {
            minutes = 0;
            seconds = 0;
            miliseconds = 0;
            registerTime = false;
        }

        public int getTimeInMiliseconds()
        {
            return (int)(this.minutes * 60 * 1000 + this.seconds * 1000 + miliseconds);
        }

        public string getPlayerTime(playerId id)
        {
            float flag = this.getFlag(id);
            int miliseconds = this.getTimeInMiliseconds() - (int)flag;
            return getTimeInString(miliseconds);

        }

        private string getTimeInString(int miliseconds)
        {
            int auxSeconds = (int)(miliseconds / 1000);
            int minutes = (int)(auxSeconds/60);
            int seconds = (auxSeconds % 60);

            string stime = "";

            if (minutes < 10)
            {
                stime += "0" + minutes.ToString();
            }
            else
            {
                stime += minutes.ToString();
            }
            stime += " : ";
            if (seconds < 10)
            {
                stime += "0" + seconds.ToString();
            }
            else
            {
                stime += seconds.ToString();
            }
            return stime;
        }

        public string getTimeInString()
        {
            string stime = "";

            if (minutes < 10)
            {
                stime += "0" + minutes.ToString();
            }
            else
            {
                stime += minutes.ToString();
            }
            stime += " : ";
            if (seconds < 10)
            {
                stime += "0" + seconds.ToString();
            }
            else
            {
                stime += seconds.ToString();
            }
            return stime;
        }
    }
}
