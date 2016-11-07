using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SokobanSolver
{
    public struct Solution
    {
        public int length;
        public char[] move;

        public Solution(int size)
        {
            length = 0;
            move = new char[size];
        }
    }

    public class SolFunc
    {
        public static Level oldpos;
        public static int sokx;
        public static int soky;
        public static int box;
        public static int tmpBox;
        public static int toBox;
        public static int toSokX;
        public static int toSokY;

        public static int[] queueSok = new int[4 * Global.LVLSIZE * Global.LVLSIZE];
        public static int[] queueBox = new int[4 * Global.LVLSIZE * Global.LVLSIZE];
        public static int[] len = new int[4 * Global.LVLSIZE * Global.LVLSIZE];
        public static int[] prev = new int[4 * Global.LVLSIZE * Global.LVLSIZE];
        //static char[][] seq = new char[4 * Global.LVLSIZE * Global.LVLSIZE][];
        public static List<Char[]> seq = new List<char[]>(4 * Global.LVLSIZE * Global.LVLSIZE);
        public static int[,,] mov = new int[Global.LVLSIZE , Global.LVLSIZE, 4];

        public static void initArrays()
        {
            for(int i = 0; i < 4 * Global.LVLSIZE * Global.LVLSIZE; i++)
            {
                seq.Add(newArray());
            }
        }

        static char[] newArray()
        {
            return new char[256];
        }

        public static void createSolution(Solution sol, Move lastMove)
        {
            if(lastMove.parent != null)
            {
                createSolution(sol, lastMove.parent);
            }
            else
            {
                Position.setPosition(ref lastMove.pos);
                oldpos = new Level(Global.level);
                return;
            }

            addMovesBetweenTwoPos(sol, lastMove.pos);
        }

        public static bool checkSolution(Solution sol, Level level)
        {
            oldpos = new Level(level);

            for(int i = 0; i < sol.length; i++)
            {
                char m = sol.move[i];
                int dir;
                switch (m)
                {
                    case 'l':
                    case 'L': dir = 0; break;
                    case 'd':
                    case 'D': dir = 1; break;
                    case 'r':
                    case 'R': dir = 2; break;
                    case 'u':
                    case 'U': dir = 3; break;
                    default: return false;
                }

                int x = oldpos.px + Global.movesX[dir];
                int y = oldpos.py + Global.movesY[dir];
                if(m > 'z')
                {
                    if (Level.isWalkable(oldpos.grid[y][x]))
                    {
                        oldpos.px = x;
                        oldpos.py = y;
                    }
                    else
                    {
                        //Can't do move
                        //Console.WriteLine(i + ": Cant do a move '" + m + "'");
                        //Level.printLevel(oldpos);
                    }
                }
                else
                {
                    int xto = oldpos.px + 2 * Global.movesX[dir];
                    int yto = oldpos.py + 2 * Global.movesY[dir];
                    if(Level.hasBoxOn(oldpos.grid[y][x]) && Level.isPushable(oldpos.grid[yto][xto]))
                    {
                        oldpos.grid[y][x] = Level.removeBox(oldpos.grid[y][x]);
                        oldpos.grid[yto][xto] = Level.putBox(oldpos.grid[yto][xto]);
                        oldpos.px = x;
                        oldpos.py = y;
                    }
                }
            }

            for(int y = 0; y < oldpos.height; y++)
            {
                for(int x = 0; x < oldpos.width; x++)
                {
                    if(Level.hasUnplacedBoxOn(oldpos.grid[y][x]) || Level.hasEmptyGoalOn(oldpos.grid[y][x]))
                    {
                        //Level is not finished
                        //Console.WriteLine("Level is not finished!");
                        //Level.printLevel(oldpos);
                        return false;
                    }
                }
            }

            return true;
        }

        public static void addMovesBetweenTwoPos(Solution sol, Position to)
        {
            calculatePush(to);
            tmpBox = box;
            //Console.WriteLine("BOX: " + box);
            int x = Level.xPos(Global.levelInfo.fieldPos[tmpBox]);
            int y = Level.yPos(Global.levelInfo.fieldPos[tmpBox]);
            oldpos.grid[y][x] = Level.removeBox(oldpos.grid[y][x]);

            int first = 0;
            int last = 0;

            for(int yy = 0; yy < Global.level.height; yy++)
            {
                for(int xx = 0; xx < Global.level.width; xx++)
                {
                    for(int i = 0; i < 4; i++)
                    {
                        mov[yy, xx, i] = 0;
                    }
                }
            }

            reachableForSolution();
            x = Level.xPos(Global.levelInfo.fieldPos[box]);
            y = Level.yPos(Global.levelInfo.fieldPos[box]);
            for(int i = 0; i < 4; i++)
            {
                int xto = x + Global.movesX[i + 2];
                int yto = y + Global.movesY[i + 2];
                if(Global.reachable[y + Global.movesY[i], x + Global.movesX[i]] >= 0 && Level.isPushable(oldpos.grid[yto][xto]))
                {
                    len[last] = getTo(x + Global.movesX[i], y + Global.movesY[i], seq[last]);
                    seq[last][len[last]++] = "RULD"[i];
                    queueSok[last] = Level.genPos(x, y);
                    queueBox[last] = Global.levelInfo.fieldNum[yto, xto];
                    prev[last++] = -1;
                    mov[yto, xto, i] = 1;
                }
            }

            while(last > first)
            {
                sokx = Level.xPos(queueSok[first]);
                soky = Level.yPos(queueSok[first]);
                box = queueBox[first];
                x = Level.xPos(Global.levelInfo.fieldPos[box]);
                y = Level.yPos(Global.levelInfo.fieldPos[box]);
                if (box == toBox) break;

                reachableForSolution();

                for(int i = 0; i < 4; i++)
                {
                    int xto = x + Global.movesX[i + 2];
                    int yto = y + Global.movesY[i + 2];
                    if(Global.reachable[y + Global.movesY[i], x + Global.movesX[i]] >= 0 && mov[yto, xto, i] == 0 && Level.isPushable(oldpos.grid[yto][xto]))
                    {
                        len[last] = getTo(x + Global.movesX[i], y + Global.movesY[i], seq[last]);
                        seq[last][len[last]++] = "RULD"[i];
                        queueSok[last] = Level.genPos(x, y);
                        queueBox[last] = Global.levelInfo.fieldNum[yto, xto];
                        prev[last++] = first;
                        mov[yto, xto, i] = 1;
                    }
                }
                first++;
            }

            if(last <= first)
            {
                //Error While Creating Solution
                //Console.WriteLine("Error While Creating Solution");
                //Level.printLevel(oldpos);
                //Console.WriteLine("=>  (" + toSokX + ", " + toSokY + ", " + Level.xPos(Global.levelInfo.fieldPos[toBox]) + ", " + Level.yPos(Global.levelInfo.fieldPos[toBox]) + ")");
            }
            else
            {
                putPushToSol(first, sol);         
            }

            x = Level.xPos(Global.levelInfo.fieldPos[toBox]);
            y = Level.yPos(Global.levelInfo.fieldPos[toBox]);
            oldpos.grid[y][x] = Level.putBox(oldpos.grid[y][x]);
            oldpos.px = sokx;
            oldpos.py = soky;
        }

        public static void calculatePush(Position to)
        {
            int j = 0, i = 0;
            uint tmp = to.b[0];
            for(int f = 0; f < Global.levelInfo.numFields; f++)
            {
                int x = Level.xPos(Global.levelInfo.fieldPos[f]);
                int y = Level.yPos(Global.levelInfo.fieldPos[f]);

                if(Convert.ToUInt32(Level.hasBoxOn(oldpos.grid[y][x])) != tmp % 2)
                {
                    if (Convert.ToBoolean(tmp % 2)) toBox = f;
                    else box = f;
                }
                tmp /= 2;

                if(i++ >= 31)
                {
                    j++;
                    tmp = to.b[j];
                    i = 0;
                }
            }

            toSokX = Level.xPos(to.s);
            toSokY = Level.yPos(to.s);
            sokx = oldpos.px;
            soky = oldpos.py;

        }

        public static void reachableForSolution()
        {
            for(int y = 0; y < oldpos.height; y++)
            {
                for(int x = 0; x < oldpos.width; x++)
                {
                    Global.reachable[y, x] = Level.isWalkable(oldpos.grid[y][x]) ? -1 : -2;
                }
            }
            //printReachable();
            Global.reachable[Level.yPos(Global.levelInfo.fieldPos[box]), Level.xPos(Global.levelInfo.fieldPos[box])] = -2;

            int first = 0;
            Global.searchQueueX[0] = sokx;
            Global.searchQueueY[0] = soky;
            Global.reachable[soky, sokx] = 0;
            int last = 1;

            while(last > first)
            {
                for(int i = 0; i < 4; i++)
                {
                    int x = Global.searchQueueX[first] + Global.movesX[i];
                    int y = Global.searchQueueY[first] + Global.movesY[i];

                    if (Global.reachable[y, x] == -1)
                    {
                        Global.reachable[y, x] = i + 2;
                        Global.searchQueueX[last] = x;
                        Global.searchQueueY[last++] = y;
                    }
                }
                first++;
            }
        }

        public static int getTo(int x, int y, char[] store)
        {
            int l;
            if(Global.reachable[y, x] != 0)
            {
                l = getTo(x + Global.movesX[Global.reachable[y, x]], y + Global.movesY[Global.reachable[y, x]], store);
            }
            else
            {
                return 0;
            }

            store[l] = "ldru"[Global.reachable[y, x] - 2];
            return l + 1;
        }

        public static void putPushToSol(int index, Solution sol)
        {
            if (prev[index] != -1) putPushToSol(prev[index], sol);

            for(int i = 0; i < len[index]; i++)
            {
                sol.move[sol.length + i] = seq[index][i];
            }
            sol.length += len[index];
        }

        public static void writeSolution(Solution sol)
        {
            for(int i = 0; i < sol.length; i++)
            {
                Console.Write(sol.move[i]);
            }
            Console.WriteLine("");
        }

        public static void printReachable()
        {
            for(int y = 0; y < Global.level.height; y++)
            {
                for(int x = 0; x < Global.level.width; x++)
                {
                    Console.Write(" " + Global.reachable[y, x] + " ");
                }
                Console.WriteLine("");
            }
            Console.WriteLine("");
        }
    }
}
