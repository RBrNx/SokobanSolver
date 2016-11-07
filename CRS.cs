using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SokobanSolver
{
    public static class CRS
    {
        public static int corralCount;
        public static int[] unfinishedGoal = new int[Global.MAXFIELDS];
        public static int[] isPiCorral = new int[Global.MAXFIELDS];
        public static int[] corralSize = new int[Global.MAXFIELDS];
        public static int[,] reachableStart = new int[Global.LVLSIZE, Global.LVLSIZE];

        public static void initializeCRS()
        {
            for(int y = 0; y < Global.level.height; y++)
            {
                for(int x = 0; x < Global.level.width; x++)
                {
                    reachableStart[y, x] = 0;
                }
            }

            for (int y = 0; y < Global.level.height; y++)
            {
                for (int x = 0; x < Global.level.width; x++)
                {
                    if(Global.level.grid[y][x] == Global.WALL)
                    {
                        reachableStart[y, x] = -1;
                    }
                }
            }
        }

        public static void calculatePosition()
        {
            calculateReachableSqaures();
            findCorrals();
            int prune = piCorralsCheck();
            if (prune != 0)
            {
                prunePositionByPiCorral(prune);
            }
        }

        public static void calculateReachableSqaures()
        {
            for(int y = 0; y < Global.level.height; y++)
            {
                for(int x = 0; x < Global.level.width; x++)
                {
                    Global.reachable[y, x] = reachableStart[y, x];
                }
            }

            Global.reachable[Global.level.py, Global.level.px] = 1;
            Global.boxCount = 0;

            int first = 0; int last = 1;
            Global.searchQueueX[0] = Global.level.px;
            Global.searchQueueY[0] = Global.level.py;

            while(first < last)
            {
                int x = Global.searchQueueX[first];
                int y = Global.searchQueueY[first++];

                for(int i = 0; i < 4; i++)
                {
                    int x2 = x + Global.movesX[i];
                    int y2 = y + Global.movesY[i];

                    if(Global.reachable[y2, x2] == 0)
                    {
                        if (!Level.hasBoxOn(Global.level.grid[y2][x2]))
                        {
                            Global.searchQueueX[last] = x2;
                            Global.searchQueueY[last++] = y2;
                            Global.reachable[y2, x2] = 1;
                        }
                        else
                        {
                            Global.boxx[Global.boxCount] = x2;
                            Global.boxy[Global.boxCount++] = y2;
                        }
                    }
                }
            }
        }

        public static void findCorrals()
        {
            corralCount = 2;
            for(int b = 0; b < Global.boxCount; b++)
            {
                int x = Global.boxx[b];
                int y = Global.boxy[b];

                if (Global.reachable[y, x] == 0)
                {
                    runCorralBFS(x, y, corralCount++);
                }
                else
                {
                    corralSize[Global.reachable[y, x]]++;
                }
            }
        }

        public static void runCorralBFS(int x, int y, int value)
        {
            unfinishedGoal[value] = 0;
            corralSize[value] = 1;

            int first = 0; int last = 1;
            Global.searchQueueX[0] = x;
            Global.searchQueueY[0] = y;
            Global.reachable[y, x] = value;

            while(first < last)
            {
                x = Global.searchQueueX[first];
                y = Global.searchQueueY[first++];

                for(int i = 0; i < 4; i++)
                {
                    int x2 = x + Global.movesX[i];
                    int y2 = y + Global.movesY[i];
                    if(Global.reachable[y2, x2] == 0)
                    {
                        Global.reachable[y2, x2] = value;
                        Global.searchQueueX[last] = x2;
                        Global.searchQueueY[last++] = y2;

                        if(Level.hasUnplacedBoxOn(Global.level.grid[y2][x2]) || Level.hasEmptyGoalOn(Global.level.grid[y2][x2]))
                        {
                            unfinishedGoal[value] = 1;
                        }
                    }
                }
            }
        }

        public static int piCorralsCheck()
        {
            for(int i = 2; i < corralCount; i++)
            {
                isPiCorral[i] = 1;
            }

            for(int b = 0; b < Global.boxCount; b++)
            {
                int x = Global.boxx[b];
                int y = Global.boxy[b];

                for(int i = 0; i < 2; i++)
                {
                    int xa = x + Global.movesX[i]; int ya = y + Global.movesY[i];
                    int xb = x + Global.movesX[i + 2]; int yb = y + Global.movesY[i + 2];

                    if(Global.reachable[ya, xa] == 1 && Global.reachable[yb, xb] == 1)
                    {
                        isPiCorral[Global.reachable[y, x]] = 0;
                    }
                }
            }

            int min = 0;
            corralSize[0] = int.MaxValue;
            for(int i = 2; i < corralCount; i++)
            {
                if(Convert.ToBoolean(isPiCorral[i]) && Convert.ToBoolean(unfinishedGoal[i]) && corralSize[i] < corralSize[min])
                {
                    min = i;
                }
            }

            return min;
        }

        public static void prunePositionByPiCorral(int corralNum)
        {
            //Console.WriteLine("Pruning by PiCorral " + corralNum);
            int last = 0;
            for(int i = 0; i < Global.boxCount; i++)
            {
                int x = Global.boxx[i];
                int y = Global.boxy[i];
                if(Global.reachable[y, x] == corralNum)
                {
                    Global.boxx[last] = Global.boxx[i];
                    Global.boxy[last] = Global.boxy[i];
                }
            }

            Global.boxCount = corralSize[corralNum];
        }

        public static void calculateAreas()
        {
            for(int y = 0; y < Global.level.height; y++)
            {
                for(int x = 0; x < Global.level.width; x++)
                {
                    Global.reachable[y, x] = 0;
                }
            }

            runReachableBFS(Global.level.px, Global.level.py, 1);
            int value = 2;
            for(int y = 0; y < Global.level.height; y++)
            {
                for(int x = 0; x < Global.level.width; x++)
                {
                    if (Global.reachable[y, x] == 0 && Level.isWalkable(Global.level.grid[y][x]))
                    {
                        runReachableBFS(x, y, value++);
                    }
                }
            }
        }

        public static void runReachableBFS(int x, int y, int value)
        {
            int first = 0; int last = 1;
            Global.searchQueueX[0] = x;
            Global.searchQueueY[0] = y;

            while(first < last)
            {
                x = Global.searchQueueX[first];
                y = Global.searchQueueY[first++];

                for(int i = 0; i < 4; i++)
                {
                    int x2 = x + Global.movesX[i];
                    int y2 = y + Global.movesY[i];
                    if(Global.reachable[y2, x2] == 0 && Level.isWalkable(Global.level.grid[y2][x2]))
                    {
                        Global.reachable[y2, x2] = value;
                        Global.searchQueueX[last] = x2;
                        Global.searchQueueY[last++] = y2;
                    }
                }
            }
        }
    }
}
