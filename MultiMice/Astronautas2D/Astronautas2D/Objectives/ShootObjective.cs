using System;
using Astronautas2D.GameObjects.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Astronautas2D.Objectives
{
    class ShootObjective : Objective
    {
        // Lista de crystales que deben ser trasladados a los portales.
        protected Crystal2D[] targetCrystals;

        // Constructor del objetivo
        public ShootObjective(bool narrative, bool team) :
            base(team, objectiveType.shoot)
        {
            // Se almacena la lista de crystales
            targetCrystals = new Crystal2D[4];

            // Se fija el nombre del objetivo
            if (narrative)
                base.description = "Disparar a los cristales.";
            else
                base.description = "Disparar a las cargas.";

        }
        /// <summary>
        /// Metodo que agrega crystales a la lista de crystales.
        /// </summary>
        /// <param name="c"></param>
        public void addCrystal(Crystal2D c)
        {
            if (targetCrystals[c.ListIndex] == null)
            {
                targetCrystals[c.ListIndex] = c;
                flags[c.ListIndex] = false;
            }
        }

        public void shootCrystal(Crystal2D c)
        {
            flags[c.ListIndex] = true;
        }
    }
}
