using System;
using Astronautas2D.GameObjects.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Astronautas2D.GameObjects;

namespace Astronautas2D.Objectives
{
    public enum reaction {attract = 0, repel = 1}

    class CrystalObjective : Objective
    {
        private Astronaut2D[] astronauts;
        private Crystal2D[] crystals;
        private bool[,] movementFlags;
        private bool[] randomizeCrystal;
        public bool[] RandomizeCrystal { get { return randomizeCrystal; } }

        public CrystalObjective(bool narrative, bool team)
            : base(team, objectiveType.crystals)
        {
            astronauts = new Astronaut2D[3];
            crystals = new Crystal2D[3];
            movementFlags = new bool[3, 2];
            randomizeCrystal = new bool[3];

            // Se fija el nombre del objetivo
            if (narrative)
                base.description = "Mover el cristal variando la carga.  Deben atraerlo y repelerlo.";
            else
                base.description = "Mover la carga variando la intensidad. Deben atraerla y repelerla.";
        }

        public Astronaut2D getAstronaut(playerId id)
        {
            return astronauts[(int)id];
        }

        public void addAstronaut(Astronaut2D astro)
        {
            astronauts[(int)astro.PlayerId] = astro;
        }

        public void addCrystals(Crystal2D crystal)
        {
            crystals[crystal.ListIndex] = crystal;
        }

        public bool getIsActive(playerId id)
        {
            return astronauts[(int)id].IsActive;
        }

        public void CompleteObjective(playerId id)
        {
            flags[(int)id] = true;
        }

        public bool checkObjective(playerId id)
        {
            int i = (int)id;
            if (astronauts[i].IsActive)
            {
                if (((astronauts[i].OfflineCharge > 0 && crystals[i].Charge < 0) || (astronauts[i].OfflineCharge < 0 && crystals[i].Charge > 0)) && !movementFlags[i, (int)reaction.attract] && crystals[i].CurrentState == state.idle)
                {
                    movementFlags[i, (int)reaction.attract] = true;
                    base.Flags[i] = movementFlags[i, (int)reaction.attract] && movementFlags[i, (int)reaction.repel];
                }
                else if (((astronauts[i].OfflineCharge > 0 && crystals[i].Charge > 0) || (astronauts[i].OfflineCharge < 0 && crystals[i].Charge < 0)) && !movementFlags[i, (int)reaction.repel] && crystals[i].CurrentState == state.idle)
                {
                    movementFlags[i, (int)reaction.repel] = true;
                    base.Flags[i] = movementFlags[i, (int)reaction.attract] && movementFlags[i, (int)reaction.repel];
                }

                if (checkReactionFlags(id))
                {
                    this.ResetReactionFlags((playerId)id);
                    this.randomizeCrystal[i] = true;
                    return true;
                }
            }
            return false;

        }



        public List<Astronaut2D> UpdateObjective()
        {
            List<Astronaut2D> winners = new List<Astronaut2D>();

            for (int i = 0; i < astronauts.Length; i++)
            {
                if (astronauts[i].IsActive)
                {
                    if (((astronauts[i].OfflineCharge > 0 && crystals[i].Charge < 0) || (astronauts[i].OfflineCharge < 0 && crystals[i].Charge > 0)) && !movementFlags[i, (int)reaction.attract] && crystals[i].CurrentState == state.idle)
                    {
                        movementFlags[i, (int)reaction.attract] = true;
                        base.Flags[i] = movementFlags[i, (int)reaction.attract] && movementFlags[i, (int)reaction.repel];
                    }
                    else if (((astronauts[i].OfflineCharge > 0 && crystals[i].Charge > 0) || (astronauts[i].OfflineCharge < 0 && crystals[i].Charge < 0)) && !movementFlags[i, (int)reaction.repel] && crystals[i].CurrentState == state.idle)
                    {
                        movementFlags[i, (int)reaction.repel] = true;
                        base.Flags[i] = movementFlags[i, (int)reaction.attract] && movementFlags[i, (int)reaction.repel];
                    }
                }
                if (checkReactionFlags((playerId)i))
                {
                    winners.Add(astronauts[i]);
                    this.ResetReactionFlags((playerId)i);
                }
            }
            return winners;
        }


        private bool checkReactionFlags(playerId id)
        {
            return base.Flags[(int)id];
        }

        public void ResetReactionFlags(playerId id)
        {
            for (int j = 0; j < 2; j++)
            {
                movementFlags[(int)id,j] = false;
                base.Flags[(int)id] = false;
            }
        }

        public void CrashCrystal(playerId id, bool random)
        {
            crystals[(int)id].Crash(random);
        }

    }
}

