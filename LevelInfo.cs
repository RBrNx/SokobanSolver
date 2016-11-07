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

        public static void preprocessLevel()
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

        public static void countBoxesAndGoals()
        {
            int boxes = 0; int goals = 0;

            for (int y = 0; y < Global.level.height; y++)
            {
                for (int x = 0; x < Global.level.width; x++)
                {
                    if (Level.hasBoxOn(Global.level.grid[y][x])) boxes++;
                    if (Level.hasGoalOn(Global.level.grid[y][x])) goals++;
                }
            }

            if (goals != boxes) Global.solvable = false;
            else Global.tempInfo.numBoxes = boxes;
        }

        public static void setFieldNumbers()
        {
            int num = 0;
            for (int y = 0; y < Global.level.height; y++)
            {
                for (int x = 0; x < Global.level.width; x++)
                {
                    if (Global.level.grid[y][x] != Global.WALL)
                    {
                        Global.tempInfo.fieldNum[y, x] = num;
                        Global.tempInfo.fieldPos[num++] = Level.genPos(x, y);
                    }
                }
            }

            Global.tempInfo.numFields = num;
            if (num >= Global.MAXFIELDS)
            {
                //Console.WriteLine("Too Many Fields!");
                Global.solvable = false;
            }
        }

        public static void calculateMagicNumbers()
        {
            for (int y = 0; y < Global.level.height; y++)
            {
                for (int x = 0; x < Global.level.width; x++)
                {
                    Global.tempInfo.magic[y, x] = (uint)Global.random.Next();
                }
            }
        }

        public static void calculateDistances()
        {
            for (int source = 0; source < Global.tempInfo.numFields; source++)
            {
                int a = Level.yPos(Global.tempInfo.fieldPos[source]);
                int b = Level.xPos(Global.tempInfo.fieldPos[source]);
                int first = 0, last = 1;
                Global.searchQueue[0] = Level.genPos(b, a);

                for (int i = 0; i < Global.tempInfo.numFields; i++)
                {
                    Global.tempInfo.pushDists[source, i] = int.MaxValue;
                }
                Global.tempInfo.pushDists[source, source] = 0;

                while (first < last)
                {
                    int x = Level.xPos(Global.searchQueue[first]);
                    int y = Level.yPos(Global.searchQueue[first++]);
                    int dist = Global.tempInfo.pushDists[source, Global.tempInfo.fieldNum[y, x]];

                    for (int i = 0; i < 4; i++)
                    {
                        int xsok = x + Global.movesX[i]; int ysok = y + Global.movesY[i];
                        int xto = x + Global.movesX[i + 2]; int yto = y + Global.movesY[i + 2];
                        int target = Global.tempInfo.fieldNum[yto, xto];

                        if (Global.level.grid[ysok][xsok] != Global.WALL && Global.level.grid[yto][xto] != Global.WALL && Global.tempInfo.pushDists[source, target] == int.MaxValue)
                        {
                            Global.tempInfo.pushDists[source, target] = dist + 1;
                            Global.searchQueue[last++] = Level.genPos(xto, yto);
                        }
                    }
                }
            }
        }

        public static void setDeadFieldsAndGoalDistances()
        {
            for (int source = 0; source < Global.tempInfo.numFields; source++)
            {
                Global.tempInfo.goalDists[source] = int.MaxValue;
                for (int target = 0; target < Global.tempInfo.numFields; target++)
                {
                    int x = Level.xPos(Global.tempInfo.fieldPos[target]);
                    int y = Level.yPos(Global.tempInfo.fieldPos[target]);
                    if (Level.hasGoalOn(Global.level.grid[y][x]))
                    {
                        if (Global.tempInfo.pushDists[source, target] < Global.tempInfo.goalDists[source])
                        {
                            Global.tempInfo.goalDists[source] = Global.tempInfo.pushDists[source, target];
                        }
                    }
                }

                if (Global.tempInfo.goalDists[source] == int.MaxValue)
                {
                    int xx = Level.xPos(Global.tempInfo.fieldPos[source]);
                    int yy = Level.yPos(Global.tempInfo.fieldPos[source]);
                    Global.level.grid[yy][xx] = Global.DEADFIELD;
                }
            }
        }

        public static void findArticulations()
        {
            for (int i = 0; i < Global.MAXFIELDS; i++)
            {
                Global.art[i] = 0;
            }

            for (int i = 0; i < Global.levelInfo.numFields; i++)
            {
                int x = Level.xPos(Global.levelInfo.fieldPos[i]);
                int y = Level.yPos(Global.levelInfo.fieldPos[i]);

                char ch = Global.level.grid[y][x];
                if (Level.hasGoalOn(ch)) continue;

                Global.level.grid[y][x] = Global.WALL;
                Global.art[i] = Convert.ToInt32(LevelComponents() > 1);
                Global.level.grid[y][x] = ch;
            }

            /*Console.WriteLine("Articulations:");
            for (int y = 0; y < Global.level.height; y++)
            {
                for (int x = 0; x < Global.level.width; x++)
                {
                    if (!Level.isBoxPlaceable(Global.level.grid[y][x]) || Global.art[Global.levelInfo.fieldNum[y, x]] == 0)
                        Console.Write(Global.level.grid[y][x]);
                    else
                        Console.Write('A');
                }
                Console.Write("\n");
            }*/
        }

        public static int LevelComponents()
        {
            Global.reachable = new int[Global.LVLSIZE, Global.LVLSIZE];

            int com = 1;
            for (int y = 0; y < Global.level.height; y++)
            {
                for (int x = 0; x < Global.level.width; x++)
                {
                    if (Global.level.grid[y][x] != Global.WALL && Global.reachable[y, x] == 0)
                    {
                        Global.reachable[y, x] = com;

                        int first = 0; int last = 1;
                        Global.searchQueue[0] = Level.genPos(x, y);
                        while (first < last)
                        {
                            int xx = Level.xPos(Global.searchQueue[first]);
                            int yy = Level.yPos(Global.searchQueue[first++]);

                            for (int i = 0; i < 4; i++)
                            {
                                int x2 = xx + Global.movesX[i];
                                int y2 = yy + Global.movesY[i];

                                if (Global.level.grid[y2][x2] != Global.WALL && Global.reachable[y2, x2] == 0)
                                {
                                    Global.reachable[y2, x2] = com;
                                    Global.searchQueue[last++] = Level.genPos(x2, y2);
                                }
                            }
                        }
                        com++;
                    }
                }
            }
            return com - 1;
        }

        public static void calculateTunnels()
        {
            //Console.WriteLine("Creating Tunnels: ");

            for (int i = 0; i < Global.MAXFIELDS; i++)
            {
                Global.levelInfo.tunnel[i] = 0;
            }

            for (int y = 0; y < Global.level.height; y++)
            {
                for (int x = 0; x < Global.level.width; x++)
                {
                    if (Level.isBoxPlaceable(Global.level.grid[y][x]) && !Level.hasGoalOn(Global.level.grid[y][x]))
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            if (Global.level.grid[y + Global.movesY[i]][x + Global.movesX[i]] == Global.WALL &&
                                Global.level.grid[y + Global.movesY[i + 2]][x + Global.movesX[i + 2]] == Global.WALL)
                            {
                                Global.levelInfo.tunnel[Global.levelInfo.fieldNum[y, x]] = i + 1;
                            }
                        }
                    }
                }
            }

            //Setup Horizontal Tunnels
            for (int y = 0; y < Global.level.height; y++)
            {
                int x1, x2, leftend, rightend, leftart, fn, distance;
                for (int x = 0; x < Global.level.width;)
                {
                    x1 = x;
                    while (Level.isBoxPlaceable(Global.level.grid[y][x]) && Global.levelInfo.tunnel[Global.levelInfo.fieldNum[y, x]] == 2)
                    {
                        x++;
                    }
                    x2 = x - 1;
                    if (x2 < x1)
                    {
                        x++;
                        continue;
                    }
                    distance = x2 - x1 + 1;
                    fn = Global.levelInfo.fieldNum[y, x1];

                    //Left End
                    leftart = 0;
                    if (Level.isBoxPlaceable(Global.level.grid[y][x1 - 1]))
                    {
                        if (Convert.ToBoolean(Global.art[Global.levelInfo.fieldNum[y, x1 - 1]]) && Convert.ToBoolean(Global.art[fn]))
                        {
                            leftend = Level.isBoxPlaceable(Global.level.grid[y][x1 - 2]) ? Global.levelInfo.fieldNum[y, x1 - 2] : -1;
                            leftart = 1;
                            distance++;
                        }
                        else
                        {
                            leftend = Global.levelInfo.fieldNum[y, x1 - 1];
                        }
                    }
                    else
                    {
                        //Deadend
                        leftend = -1;
                    }

                    //Right End
                    if (Level.isBoxPlaceable(Global.level.grid[y][x2 + 1]))
                    {
                        if (Convert.ToBoolean(Global.art[Global.levelInfo.fieldNum[y, x2 + 1]]) && Convert.ToBoolean(Global.art[fn]))
                        {
                            rightend = Level.isBoxPlaceable(Global.level.grid[y][x2 + 2]) ? Global.levelInfo.fieldNum[y, x2 + 2] : -1;
                            distance++;
                        }
                        else
                        {
                            rightend = Global.levelInfo.fieldNum[y, x2 + 1];
                        }
                    }
                    else
                    {
                        //Deadend
                        rightend = -1;
                    }

                    if (distance == 1)
                    {
                        Global.levelInfo.tunnel[fn] = 0;
                        continue;
                    }

                    //Console.WriteLine("Adding horizontal tunnel: " + leftend + "<=(" + x1 + "-" + x2 + ", " + y + ")=> " + rightend + "(fn = " + fn + ")");

                    for (int xc = x1; xc <= x2; xc++)
                    {
                        int f = Global.levelInfo.fieldNum[y, xc];
                        Global.levelInfo.tunnelGoals[f, 1] = leftend;
                        Global.levelInfo.tunnelGoals[f, 0] = rightend;

                        Global.levelInfo.tunnelDists[f, 1] = xc - x1 + leftart + 1;
                        Global.levelInfo.tunnelDists[f, 0] = distance + 1 - Global.levelInfo.tunnelDists[f, 1];
                    }
                }
            }

            for (int x = 0; x < Global.level.width; x++)
            {
                int y1, y2, topend, botend, topart, fn, distance;
                for (int y = 0; y < Global.level.height;)
                {
                    y1 = y;
                    while (Level.isBoxPlaceable(Global.level.grid[y][x]) && Global.levelInfo.tunnel[Global.levelInfo.fieldNum[y, x]] == 1)
                    {
                        y++;
                    }
                    y2 = y - 1;
                    if (y2 < y1)
                    {
                        y++;
                        continue;
                    }
                    distance = y2 - y1 + 1;
                    fn = Global.levelInfo.fieldNum[y1, x];

                    //Top End
                    topart = 0;
                    if (Level.isBoxPlaceable(Global.level.grid[y1 - 1][x]))
                    {
                        if (Convert.ToBoolean(Global.art[Global.levelInfo.fieldNum[y1 - 1, x]]) && Convert.ToBoolean(Global.art[fn]))
                        {
                            topend = Level.isBoxPlaceable(Global.level.grid[y1 - 2][x]) ? Global.levelInfo.fieldNum[y1 - 2, x] : -1;
                            topart = 1;
                            distance++;
                        }
                        else
                        {
                            topend = Global.levelInfo.fieldNum[y1 - 1, x];
                        }
                    }
                    else
                    {
                        topend = -1;
                    }

                    //Right End
                    if (Level.isBoxPlaceable(Global.level.grid[y2 + 1][x]))
                    {
                        if (Convert.ToBoolean(Global.art[Global.levelInfo.fieldNum[y2 + 1, x]]) && Convert.ToBoolean(Global.art[fn]))
                        {
                            botend = Level.isBoxPlaceable(Global.level.grid[y2 + 2][x]) ? Global.levelInfo.fieldNum[y2 + 2, x] : -1;
                            distance++;
                        }
                        else
                        {
                            botend = Global.levelInfo.fieldNum[y2 + 1, x];
                        }
                    }
                    else
                    {
                        botend = -1;
                    }

                    if (distance == 1)
                    {
                        Global.levelInfo.tunnel[fn] = 0;
                        continue;
                    }

                    //Console.WriteLine("Adding vertical tunnel: " + topend + "^ (" + x + ", " + y1 + "-" + y2 + ") v " + botend + "(fn = " + fn + ")");

                    for (int yc = y1; yc <= y2; yc++)
                    {
                        int f = Global.levelInfo.fieldNum[yc, x];
                        Global.levelInfo.tunnelGoals[f, 0] = topend;
                        Global.levelInfo.tunnelGoals[f, 1] = botend;

                        Global.levelInfo.tunnelDists[f, 0] = yc - y1 + topart + 1;
                        Global.levelInfo.tunnelDists[f, 1] = distance + 1 - Global.levelInfo.tunnelDists[f, 0];
                    }
                }

            }

            /*for(int y = 0; y < Global.level.height; y++)
            {
                for(int x = 0; x < Global.level.width; x++)
                {
                    if(Global.levelInfo.tunnel[Global.levelInfo.fieldNum[y, x]] == 0)
                    {
                        Console.Write(Global.level.grid[y][x]);
                    }
                    else
                    {
                        Console.Write(Global.levelInfo.tunnel[Global.levelInfo.fieldNum[y, x]] == 1 ? '|' : '-');
                    }
                }
                Console.WriteLine("");
            }
            Console.WriteLine("");*/
        }

        public static void copyToInfo()
        {
            int num = 0;
            for (int i = 0; i < Global.tempInfo.numFields; i++)
            {
                int x = Level.xPos(Global.tempInfo.fieldPos[i]);
                int y = Level.yPos(Global.tempInfo.fieldPos[i]);
                if (Global.level.grid[y][x] == Global.DEADFIELD)
                {
                    continue;
                }

                Global.levelInfo.fieldNum[y, x] = num;
                Global.levelInfo.fieldPos[num++] = Global.tempInfo.fieldPos[i];
            }

            Global.levelInfo.numFields = num;
            Global.levelInfo.numBoxes = Global.tempInfo.numBoxes;
            Global.levelInfo.magic = Global.tempInfo.magic;

            for (int i = 0; i < Global.levelInfo.numFields; i++)
            {
                int x1 = Level.xPos(Global.levelInfo.fieldPos[i]); int y1 = Level.yPos(Global.levelInfo.fieldPos[i]);
                for (int j = 0; j < Global.levelInfo.numFields; j++)
                {
                    int x2 = Level.xPos(Global.levelInfo.fieldPos[j]); int y2 = Level.yPos(Global.levelInfo.fieldPos[j]);
                    Global.levelInfo.pushDists[i, j] = Global.tempInfo.pushDists[Global.tempInfo.fieldNum[y1, x1], Global.tempInfo.fieldNum[y2, x2]];
                }
                Global.levelInfo.goalDists[i] = Global.tempInfo.goalDists[Global.tempInfo.fieldNum[y1, x1]];
            }
        }

    }
}
