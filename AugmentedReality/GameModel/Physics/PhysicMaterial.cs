using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameModel
{
    public class PhysicMaterial
    {
        private ContactBegin contactBeginCallback;
        private string material1;
        private string material2;
        public string MaterialName1 { get { return material1; } set { material1 = value; } }
        public string MaterialName2 { get { return material2; } set { material2 = value; } }
        public ContactBegin ContactBeginCallback { get { return contactBeginCallback; } set { contactBeginCallback = value; } }
    }
}
