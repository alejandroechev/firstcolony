using System;
using Astronautas2D.GameObjects.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Astronautas2D.Objectives
{
    public enum charge{neutral, positive, negative}

    class ActivateForceObjective : Objective
    {
        private Astronaut2D[] astronauts;
        private Crystal2D[] crystals;
        private charge[] objectiveCharges;

        public ActivateForceObjective(bool narrative, bool team)
            :base(team,objectiveType.activateForce)
        {
            crystals = new Crystal2D[3];
            astronauts = new Astronaut2D[3];
            objectiveCharges = new charge[3];

            for (int i = 0; i < objectiveCharges.Length; i++)
            {
                objectiveCharges[i] = charge.neutral;
            }
                // Se fija el nombre del objetivo
                if (narrative)
                    base.description = "Activar el campo eléctrico con la carga indicada por el cristal.";
                else
                    base.description = "Activar el campo eléctrico con la carga indicada por la carga.";
        }

        public void addAstronaut(Astronaut2D astro)
        {
            astronauts[(int)astro.PlayerId] = astro;
            objectiveCharges[(int)astro.PlayerId] = charge.positive;
        }

        public void addCrystal(Crystal2D crystal)
        {
            crystal.ActiveForces = false;
            crystals[crystal.ListIndex] = crystal;
        }

        public Vector2 getPosition(playerId id)
        {
            return astronauts[(int)id].Position2D;
        }

        public bool getIsActive(playerId id)
        {
            return astronauts[(int)id].IsActive;
        }

        public bool checkCharge(playerId id)
        {
            charge wantedCharge = objectiveCharges[(int)id];
            charge astroCharge;
            if (astronauts[(int)id].OfflineCharge > 0)
            {
                astroCharge = charge.positive;
            }
            else if (astronauts[(int)id].OfflineCharge < 0)
            {
                astroCharge = charge.negative;
            }
            else
            {
                return false;
            }

            return (wantedCharge == astroCharge);
        }

        public float getMaxCharge(playerId id)
        {
            if(astronauts[(int)id] != null)
            {
                return astronauts[(int)id].MaxCharge;
            }
            else
            {
                return 0f;
            }
        }

        public float getCurrentCharge(playerId id)
        {
            if (astronauts[(int)id] != null)
            {
                return astronauts[(int)id].OfflineCharge;
            }
            else
            {
                return 0f;
            }
        }

        public void CompleteObjective(playerId id)
        {
            flags[(int)id] = true;

            if (astronauts[(int)id].OfflineCharge > 0)
            {
                objectiveCharges[(int)id] = charge.negative;
                crystals[(int)id].Color = crystals[(int)id].highRed;
            }
            else
            {
                objectiveCharges[(int)id] = charge.positive;
                crystals[(int)id].Color = crystals[(int)id].highBlue;
            }
        }
    }
}
