using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SokobanSolver
{
    public static class SolvingRoutine
    {
        public static Move newMove;
        public static Move mov;

        public static void trySolveLevel()
        {
            intializeRoutine();
            runRoutine();
            cleanRoutine();
        }

        public static void initializeRoutine()
        {
            Hashtable.initializeHash();

        }
    }
}
