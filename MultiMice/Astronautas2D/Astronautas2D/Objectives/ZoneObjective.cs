using System;
using Astronautas2D.GameObjects.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Astronautas2D.Objectives
{
    public class ZoneObjective : Objective
    {
        private Zone2D[] zoneList;

        public ZoneObjective(bool narrative, bool team) :
            base(team, objectiveType.move)
        {
            zoneList = new Zone2D[4];

            if (narrative)
                base.description = "Mover su astronauta a la zona con su símbolo.";
            else
                base.description = "Mover su carga a la zona con su símbolo.";
        }

        public void addZone(Zone2D zone)
        {
            if (zoneList[(int)zone.PlayerIndex] == null)
            {
                zoneList[(int)zone.PlayerIndex] = zone;
                base.flags[(int)zone.PlayerIndex] = false;
            }
        }

        public bool catchZone(Zone2D zone)
        {
            if (zoneList[(int)zone.PlayerIndex] == zone)
            {
                flags[(int)zone.PlayerIndex] = true;
                return base.isComplete;
            }
            else
            {
                return false;
            }
        }

    }
}
