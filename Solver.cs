using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SokobanSolver
{
    public static class Solver
    {

        public static bool Solve(string level, ref string solution)
        { 

            Level.levelFromString(level);

            SolFunc.initArrays();
            Console.WriteLine("Starting Solve");
            Global.solvable = true;
            Global.levelSol.length = 0;
            LevelInfo.preprocessLevel();
            DeadlockTable.calculateStaticDeadlocks();
            //Level.printLevel(Global.level);

            SolvingRoutine.trySolveLevel();
            if (Global.solvable)
            {
                if(!SolFunc.checkSolution(Global.levelSol, Global.level))
                {
                    Global.solvable = false;
                    return false;
                }
                for(int i = 0; i < Global.levelSol.length; i++)
                {
                    solution += Global.levelSol.move[i];
                }
                return true;
            }
            return false;
        }

    }
}
