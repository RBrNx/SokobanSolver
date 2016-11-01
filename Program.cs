using System;

namespace SokobanSolver
{
    class Program
    {
        static void Main(string[] args)
        {
            string level =  "#######\n"+
                            "#     #\n"+
                            "#     #\n"+
                            "#. #  #\n"+
                            "#. $$ #\n"+
                            "#.$$  #\n"+
                            "#.#  @#\n"+
                            "#######";

            bool solved = Solver.Solve(level);
        }
    }
}
