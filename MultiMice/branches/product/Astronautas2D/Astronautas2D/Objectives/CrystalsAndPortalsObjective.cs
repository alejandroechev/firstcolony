using System;
using Astronautas2D.GameObjects.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Astronautas2D.Objectives
{
    public class CrystalsAndPortalsObjective : Objective
    {
        // Lista de crystales que deben ser trasladados a los portales.
        protected Crystal2D[] crystalList;

        // Constructor del objetivo
        public CrystalsAndPortalsObjective(bool narrative, bool team, bool chargeChange) :
            base(team, objectiveType.crystalsAndPortals)
        {
            // Se almacena la lista de crystales
            crystalList = new Crystal2D[4];

            // Se fija el nombre del objetivo
            if (narrative)
                base.description = "Transportar los cristales a los portales.";
            else
                base.description = "Mover las cargas a las zonas indicadas.";
            if (chargeChange)
                base.description += "";
            else
                base.description += " El valor de la carga es fijo.";
            
            
        }
        /// <summary>
        /// Metodo que agrega crystales a la lista de crystales.
        /// </summary>
        /// <param name="c"></param>
        public void addCrystal(Crystal2D c)
        {
            if (crystalList[c.ListIndex] == null)
            {
                crystalList[c.ListIndex] = c;
                flags[c.ListIndex] = false;
            }
        }


        public bool saveCrystal(Crystal2D crystal)
        {
            if (crystalList[crystal.ListIndex] == crystal)
            {
                flags[crystal.ListIndex] = true;
                return base.isComplete;
            }
            else
            {
                return false;
            }
        }

    }
}
