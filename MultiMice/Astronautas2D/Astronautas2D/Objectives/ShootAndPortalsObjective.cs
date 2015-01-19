using System;
using Astronautas2D.GameObjects.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Astronautas2D.Objectives
{
    class ShootAndPortalsObjective : Objective
    {
        // Lista de crystales que deben ser trasladados a los portales.
        protected Crystal2D[] parentCrystals;

        // Constructor del objetivo
        public ShootAndPortalsObjective(bool narrative, bool team) :
            base(team, objectiveType.shootAndPortals)
        {
            // Se almacena la lista de crystales
            parentCrystals = new Crystal2D[4];

            // Se fija el nombre del objetivo
            if (narrative)
                base.description = "Disparar a los cristales, ingresar los cristales pequeños en los portales.";
            else
                base.description = "Disparar a las cargas, llevar las cargas pequeñas a las zonas indicadas.";
            
        }
        /// <summary>
        /// Metodo que agrega crystales a la lista de crystales.
        /// </summary>
        /// <param name="c"></param>
        public void addCrystal(Crystal2D c)
        {
            if (parentCrystals[c.ListIndex] == null)
            {
                parentCrystals[c.ListIndex] = c;
                flags[c.ListIndex] = false;
            }
        }


        public bool checkFather(Crystal2D crystal)
        {
            if (parentCrystals[crystal.ListIndex] == crystal.Father)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void saveCrystal(Crystal2D c)
        {
            flags[c.ListIndex] = true;
        }

    }
}
