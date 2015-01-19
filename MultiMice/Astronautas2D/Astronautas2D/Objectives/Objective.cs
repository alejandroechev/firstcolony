using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Astronautas2D.Objectives
{
    public enum objectiveType { move, shoot, shootAndPortals, crystalsAndPortals, activateForce, crystals};

    /// <summary>
    /// Clase que representará el objetivo de un nivel
    /// </summary>
    public abstract class Objective
    {
        // bool que indica si el nivel es en equipo o no
        protected bool team;
        // Conjunto de flags que se deben completar para que el objetivo se cumpla
        protected bool[] flags;
        public bool[] Flags { get { return flags; } }
        // Tipo de objetivo
        protected objectiveType type;
        public objectiveType Type { get { return type; } }
        // Descripción del objetivo
        protected String description;
        public String Description { get { return description; } }
        // Propiedad que determina si el objetivo se ha completado o no
        public bool isComplete
        {
            get 
            {
                if (team)
                {
                    return flags[3];
                }
                else
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (!flags[i])
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
        }

        /// <summary>
        /// Constructor de objetivos
        /// </summary>
        /// <param name="type"></param>
        /// <param name="description"></param>
        /// <param name="flags"></param>
        public Objective(bool team, objectiveType type)
        {
            this.team = team;
            this.type = type;
            this.description = "";
            this.flags = new bool[4];
        }

        /// <summary>
        /// Actualiza el estado de los flags
        /// </summary>
        /// <param name="elapsedTime"></param>
        protected virtual void Update(float elapsedTime)
        {
        }
    }
}
