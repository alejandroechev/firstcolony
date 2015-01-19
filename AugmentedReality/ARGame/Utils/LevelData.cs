using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARGame.Utils
{
    public class LevelData
    {
        public bool IsCompleted { get; set; }
        public int Tries { get; set; }
        public int Iterations { get; set; }

        public LevelData()
        {
            IsCompleted = false;
            Tries = 0;
            Iterations = 0;
        }

        public void ResetData()
        {
            IsCompleted = false;
            Tries = 0;
            Iterations = 0;
        }

        public override string ToString()
        {
            return "Logros: " + Iterations + "  Errores: " + Tries;
        }

        public string ToStringIterations()
        {
            return "Logros: " + Iterations;
        }

    }
}
