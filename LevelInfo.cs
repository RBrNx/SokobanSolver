using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SokobanSolver
{
    public struct LevelInfo
    {
        public int numFields;
        public int numBoxes;

        public int[,] fieldNum;
        public uint[,] magic;
        public int[] fieldPos;
        public int[,] pushDists;
        public int[] goalDists;
        public int[] tunnel;
        public int[,] tunnelGoals;
        public int[,] tunnelDists;

        public LevelInfo(int LVLSIZE, int MAXFIELDS)
        {
            fieldNum = new int[LVLSIZE, LVLSIZE];
            magic = new uint[LVLSIZE, LVLSIZE];
            fieldPos = new int[MAXFIELDS];
            pushDists = new int[MAXFIELDS, MAXFIELDS];
            goalDists = new int[MAXFIELDS];
            tunnel = new int[MAXFIELDS];
            tunnelGoals = new int[MAXFIELDS, 2];
            tunnelDists = new int[MAXFIELDS, 2];

            numBoxes = 0;
            numFields = 0;
         }

    }

    public static class LevelInfoFunctions
    {
        public static void preprocessLevel(ref Level level)
        {
            addOutsideWalls();
            removeDeadends();
            countBoxesAndGoals();
            calculateMagicNumbers();
            setFieldNumbers();
            calculateDistances();
            setDeadFieldsAndGoalDistances();
            copyToInfo();
            findArticulations();
            calculateTunnels();
        }

        public static uint magicForSokoban(int x, int y)
        {
            uint magic = Global.tempInfo.magic[y, x];
            return (magic << 16) + (magic >> 16);
        }

        public static bool isInLevel(int x, int y)
        {
            return x >= 0 && x < Global.level.width && y >= 0 && y < Global.level.height;
        }

        public static void addOutsideWalls()
        {
            Global.reachable[Global.level.py, Global.level.px] = 1;
            int first = 0; int last = 1;
            Global.searchQueue[0] = Level.genPos(Global.level.px, Global.level.py);
            while (first < last)
            {
                int x = Level.xPos(Global.searchQueue[first]);
                int y = Level.yPos(Global.searchQueue[first++]);

                for (int i = 0; i < 4; i++)
                {
                    int x2 = x + Global.movesX[i];
                    int y2 = y + Global.movesY[i];

                    if (isInLevel(x2, y2) && Global.level.grid[y2][x2] != Global.WALL && Global.reachable[y2, x2] == 0)
                    {
                        Global.reachable[y2, x2] = 1;
                        Global.searchQueue[last++] = x2 * Global.LVLSIZE + y2;
                    }
                }
            }

            for (int y = 0; y < Global.level.height; y++)
            {
                for (int x = 0; x < Global.level.width; x++)
                {
                    if (Global.reachable[y, x] == 0)
                    {
                        if (Level.hasEmptyGoalOn(Global.level.grid[y][x]) || Level.hasUnplacedBoxOn(Global.level.grid[y][x]))
                        {
                            Global.solvable = false;
                        }
                        Global.level.grid[y][x] = Global.WALL;
                    }
                }
            }
        }

        public static void removeDeadends()
        {
            bool hasDeadend;

            do
            {
                hasDeadend = false;

                for (int y = 1; y < Global.level.height - 1; y++)
                {
                    for (int x = 1; x < Global.level.width - 1; x++)
                    {
                        if (Level.hasGoalOn(Global.level.grid[y][x]) || Global.level.grid[y][x] == Global.WALL) continue;

                        int numWalls = 0;
                        for (int i = 0; i < 4; i++)
                        {
                            if (Global.level.grid[y + Global.movesY[i]][x + Global.movesX[i]] == Global.WALL)
                            {
                                numWalls++;
                            }
                        }
                        if (numWalls < 3) continue;

                        if (Level.hasBoxOn(Global.level.grid[y][x])) Global.solvable = false;

                        if (Global.level.px == x && Global.level.py == y)
                        {
                            int dir = 0;
                            for (; dir < 4; dir++)
                            {
                                if (Global.level.grid[y + Global.movesY[dir]][x + Global.movesX[dir]] != Global.WALL) break;
                            }

                            int x2 = x + Global.movesX[dir]; int y2 = y + Global.movesY[dir];
                            int x3 = x + 2 * Global.movesX[dir]; int y3 = y + 2 * Global.movesY[dir];

                            Global.level.px = x2;
                            Global.level.py = y2;
                            Global.levelSol.move[Global.levelSol.length++] = (Level.hasBoxOn(Global.level.grid[y2][x2]) ? "LDRU" : "ldru")[dir];

                            if (Level.hasBoxOn(Global.level.grid[y2][x2]))
                            {
                                if (Level.isPushable(Global.level.grid[y3][x3]))
                                {
                                    Global.level.grid[y2][x2] = Level.removeBox(Global.level.grid[y2][x2]);
                                    Global.level.grid[y3][x3] = Level.putBox(Global.level.grid[y3][x3]);
                                }
                                else
                                {
                                    Global.solvable = false;
                                }
                            }
                            Global.level.grid[y][x] = Global.WALL;
                        }
                    }
                }
            }
            while (hasDeadend);
        }
    }
}
