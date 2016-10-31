using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SokobanSolver
{
    public static class Solver
    {

        public static bool Solve(string level)
        {
            Level.levelFromString(level);
            int count = 1;

            Console.WriteLine("Starting Solve");
            Global.solvable = true;
            Global.levelSol.length = 0;
            LevelInfo.preprocessLevel();
            DeadlockTable.calculateStaticDeadlocks();
            Level.printLevel(Global.level);

            
            return true;
        }

    }
}
