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

            string solution = "";

            bool solved = Solver.Solve(level, ref solution);
            if (solved) Console.WriteLine(solution);
            Console.ReadLine();
        }
    }
}
