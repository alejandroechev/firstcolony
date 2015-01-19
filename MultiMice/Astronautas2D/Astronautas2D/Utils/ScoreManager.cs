using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Astronautas2D.Utils
{
    public class ScoreManager
    {
        private int teamScore;
        private int[] playersScore;
        private int CircleScore { get { return playersScore[0]; } set { playersScore[0] = value; } }
        private int SquareScore { get { return playersScore[1]; } set { playersScore[1] = value; } }
        private int TriangleScore { get { return playersScore[2]; } set { playersScore[2] = value; } }
        private int crystalScore = 500;
        private int bonusScore = 500;
        private int timeLimit;
        private TimeManager timeManager;
        private int soloFactor = 10;
        private bool narrative;


        public ScoreManager(TimeManager tm, bool narrative)
        {
            // incluimos el timeManagerAsociado
            this.narrative = narrative;
            timeManager = tm;
            teamScore = 0;
            playersScore = new int[3];
            timeLimit = 1 * 60 * 1000; // 2 minutos
        }

        public ScoreManager(TimeManager tm, int timeLimit)
        {
            // incluimos el timeManagerAsociado
            timeManager = tm;
            teamScore = 0;
            playersScore = new int[3];
            this.timeLimit = timeLimit;
        }

        public int addScore(playerId playerId, bool solo)
        {
            if (narrative)
            {
                timeManager.setFlag(playerId);
                addProfit(playerId, 1);
                return 1;
            }

            int time = timeManager.getTimeInMiliseconds();
            int flag = (int)timeManager.getFlag(playerId);
            int bonus = 0;
            
            if(time != flag)
            {
                bonus = (int)(Math.Min(bonusScore, (double)(bonusScore * (timeLimit / (time - flag)))));
            }
            else
            {
                bonus = bonusScore;
            }

            int profit = crystalScore + bonus;
            if (solo) profit /= soloFactor;
            timeManager.setFlag(playerId);
            addProfit(playerId, profit);
            return profit;
        }

        public int addScore(playerId playerId, float currentCharge, float maxCharge, bool solo)
        {
            if (narrative)
            {
                timeManager.setFlag(playerId);
                addProfit(playerId, 1);
                return 1;
            }

            int time = timeManager.getTimeInMiliseconds();
            int flag = (int)timeManager.getFlag(playerId);
            int bonus = 0;

            if (time != flag)
            {
                bonus = (int)(Math.Min(bonusScore, (double)(bonusScore * (timeLimit / (time - flag)))));
            }
            else
            {
                bonus = bonusScore;
            }
            int profit = crystalScore + bonus;
            if (solo) profit /= soloFactor;

            float aux = (float)(Math.Abs(currentCharge)/maxCharge);
            
            if(aux <= 0.2)
            {
                profit = (int)(profit * 0.1);
            }
            else if (aux > 0.2 && aux <= 0.4)
            {
                profit = (int)(profit * 0.15);
            }
            else if(aux > 0.4 && aux <= 0.6)
            {
                profit = (int)(profit * 0.3);
            }
            else if (aux > 0.6 && aux <= 0.8)
            {
                profit = (int)(profit * 1.0);
            }
            else
            {
                profit = (int)(profit * 2.0);
            }
            
            timeManager.setFlag(playerId);
            addProfit(playerId, profit);
            return profit;
        }


        private void addProfit(playerId playerId, int profit)
        {
            switch (playerId)
            {
                case playerId.circle:
                    CircleScore += profit;
                    break;

                case playerId.square:
                    SquareScore += profit;
                    break;

                case playerId.triangle:
                    TriangleScore += profit;
                    break;

                default:
                    teamScore += profit;
                    break;
            }
        }

        public int getScore(playerId id)
        {
            switch (id)
            {
                case playerId.circle:
                    return CircleScore;

                case playerId.square:
                    return SquareScore;

                case playerId.triangle:
                    return TriangleScore;

                default:
                    return teamScore;
            }
        }
    }
}
