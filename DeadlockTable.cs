using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SokobanSolver
{
    public static class DeadlockTable
    {
        public static Position newDeadlock = new Position(Global.POSITIONSIZE);

        public static void calculateStaticDeadlocks()
        {
            initializeStaticDeadlocks();
            addStaticDeadlocks();
            //echoDeadPositions();
        }

        public static bool testStaticDeadlocks(Position pos, int fn)
        {
            for(int i = 0; i < Global.staticDeadlocksCount[fn]; i++)
            {
                if (Position.isSubposition(ref pos, ref Global.staticDeadlocks[fn, i]))
                {
                    //Console.WriteLine("STATIC DEADLOCK: fn = " + fn + ", index = " + i + ", position ignored\n");
                    return true;
                }
            }
            return false;
        }

        public static void initializeStaticDeadlocks()
        {
            for(int i = 0; i < Global.MAXSTATICDEADLOCKS; i++)
            {
                Global.staticDeadlocksCount[i] = 0;
            }
        }

        public static void addStaticDeadlocks()
        {
            if(Global.levelInfo.numBoxes >= 2)
            {
                forbiddenPatternWithSymmetries(2, 2, "#$#$");
                //for(int i = 0; i < Global.staticDeadlocksCount.Length; i++) { Console.Write(" " + Global.staticDeadlocksCount[i]); } Console.WriteLine("");
                forbiddenPatternWithSymmetries(3, 3, "?###x$#$?");
                //for (int i = 0; i < Global.staticDeadlocksCount.Length; i++) { Console.Write(" " + Global.staticDeadlocksCount[i]); } Console.WriteLine("");
                forbiddenPatternWithSymmetries(2, 3, "?#$$#?");
                //for (int i = 0; i < Global.staticDeadlocksCount.Length; i++) { Console.Write(" " + Global.staticDeadlocksCount[i]); } Console.WriteLine("");
            }

            if (Global.levelInfo.numBoxes >= 3)
            {
                forbiddenPatternWithSymmetries(2, 2, "#$$$");
                //for (int i = 0; i < Global.staticDeadlocksCount.Length; i++) { Console.Write(" " + Global.staticDeadlocksCount[i]); } Console.WriteLine("");
                forbiddenPatternWithSymmetries(3, 4, "####x$#x$#$?");
                //for (int i = 0; i < Global.staticDeadlocksCount.Length; i++) { Console.Write(" " + Global.staticDeadlocksCount[i]); } Console.WriteLine("");
                forbiddenPatternWithSymmetries(3, 3, "?$#$$?#??");
                //for (int i = 0; i < Global.staticDeadlocksCount.Length; i++) { Console.Write(" " + Global.staticDeadlocksCount[i]); } Console.WriteLine("");
            }

            if (Global.levelInfo.numBoxes >= 4)
            {
                char[] pattern = new char[] { '$', '$', '$', '$' };
                forbiddenPattern(2, 2, pattern);
                //for (int i = 0; i < Global.staticDeadlocksCount.Length; i++) { Console.Write(" " + Global.staticDeadlocksCount[i]); } Console.WriteLine("");
                forbiddenPatternWithSymmetries(3, 4, "??#?$$$$?#??");
                //for (int i = 0; i < Global.staticDeadlocksCount.Length; i++) { Console.Write(" " + Global.staticDeadlocksCount[i]); } Console.WriteLine("");
                forbiddenPatternWithSymmetries(3, 3, "?$$#x$#$?");
                //for (int i = 0; i < Global.staticDeadlocksCount.Length; i++) { Console.Write(" " + Global.staticDeadlocksCount[i]); } Console.WriteLine("");
            }
        }

        public static void forbiddenPatternWithSymmetries(int w, int h, string pattern)
        {
            char[] charPattern = pattern.ToCharArray();
            char[] newPattern = new char[64];
            char[] newPattern2 = new char[64];
            char[] newPattern3 = new char[64];

            forbiddenPattern(w, h, charPattern);
            mirrorV(w, h, charPattern, ref newPattern);
            forbiddenPattern(w, h, newPattern);
            mirrorH(w, h, charPattern, ref newPattern2);
            forbiddenPattern(w, h, newPattern2);
            mirrorH(w, h, newPattern, ref newPattern2);
            forbiddenPattern(w, h, newPattern2);

            transpose(w, h, charPattern, ref newPattern3);
            forbiddenPattern(h, w, newPattern3);
            mirrorV(h, w, newPattern3, ref newPattern);
            forbiddenPattern(h, w, newPattern);
            mirrorH(h, w, newPattern3, ref newPattern2);
            forbiddenPattern(h, w, newPattern2);
            mirrorH(h, w, newPattern, ref newPattern2);
            forbiddenPattern(h, w, newPattern2);
        }

        public static void mirrorV(int w, int h, char[] pattern, ref char[] result)
        {
            for(int y = 0; y < h; y++)
            {
                for(int x = 0; x < w; x++)
                {
                    result[x + y * w] = pattern[w - x - 1 + y * w];
                }
            }
            result[w * h] = '\0';
        }

        public static void mirrorH(int w, int h, char[] pattern, ref char[] result)
        {
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    result[x + y * w] = pattern[x + (h - y - 1) * w];
                }
            }
            result[w * h] = '\0';
        }

        public static void transpose(int w, int h, char[] pattern, ref char[] result)
        {
            for (int y = 0; y < w; y++)
            {
                for (int x = 0; x < h; x++)
                {
                    result[x + y * h] = pattern[y + x * w];
                }
            }
            result[w * h] = '\0';
        }

        public static void forbiddenPattern(int w, int h, char[] pattern)
        {
            for(int y = 0; y <= Global.level.height - h; y++)
            {
                for(int x = 0; x <= Global.level.width - w; x++)
                {
                    int ok = 1; int notGoals = 0;
                    for(int y2 = 0; y2 < h; y2++)
                    {
                        for(int x2 = 0; x2 < w; x2++)
                        {
                            char pat = pattern[x2 + y2 * w];

                            switch (pat)
                            {
                                case '?': break;
                                case '#': if (Global.level.grid[y + y2][x + x2] != Global.WALL)
                                    {
                                        ok = 0;
                                    }
                                    break;
                                case 'x': if (Level.hasGoalOn(Global.level.grid[y + y2][x + x2]))
                                    {
                                        ok = 2;
                                    }
                                    break;
                                case '$': if(!Level.isBoxPlaceable(Global.level.grid[y + y2][x + x2]))
                                    {
                                        ok = 3;
                                    }
                                    else if (!Level.hasGoalOn(Global.level.grid[y + y2][x + x2]))
                                    {
                                        notGoals++;
                                    }
                                    break;
                            }
                        }
                    }

                    if(ok == 1 && notGoals > 0)
                    {
                        Position.cleanPosition(ref newDeadlock);
                        for(int y2 = 0; y2 < h; y2++)
                        {
                            for(int x2 = 0; x2 < w; x2++)
                            {
                                if(pattern[x2 + y2 * w] == Global.BOX)
                                {
                                    Position.addBoxOfPosition(ref newDeadlock, Global.levelInfo.fieldNum[y + y2, x + x2]);
                                }
                            }
                        }

                        //Add this position to all deadlock tables
                        ok = 1;
                        for(int y2 = 0; y2 < h; y2++)
                        {
                            for(int x2 = 0; x2 < w; x2++)
                            {
                                if(pattern[x2 + y2 * w] == Global.BOX)
                                {
                                    int fn = Global.levelInfo.fieldNum[y + y2, x + x2];
                                    for(int i = 0; i < Global.staticDeadlocksCount[fn]; i++)
                                    {
                                        if (Position.isSubposition(ref newDeadlock, ref Global.staticDeadlocks[fn, i]))
                                        {
                                            ok = 0;
                                        }
                                    }
                                }
                            }
                        }

                        if (!Convert.ToBoolean(ok))
                        {
                            continue;
                        }
                        for(int y2 = 0; y2 < h; y2++)
                        {
                            for (int x2 = 0; x2 < w; x2++)
                            {
                                if (pattern[x2 + y2 * w] == Global.BOX)
                                {
                                    int fn = Global.levelInfo.fieldNum[y + y2, x + x2];
                                    Global.staticDeadlocks[fn, Global.staticDeadlocksCount[fn]++] = new Position(newDeadlock);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void echoDeadPositions()
        {
            Position original = new Position(Global.POSITIONSIZE);
            Position.getPosition(ref original);
            Console.WriteLine("Field Numbers: ");
            for(int y = 0; y < Global.level.height; y++)
            {
                for(int x = 0; x < Global.level.width; x++)
                {
                    if(Global.level.grid[y][x] == Global.WALL)
                    {
                        Console.Write(" # ");
                    }
                    else if(Global.level.grid[y][x] == Global.DEADFIELD)
                    {
                        Console.Write(" x ");
                    }
                    else
                    {
                        Console.Write(" " + Global.levelInfo.fieldNum[y, x].ToString("D2"));
                        //Console.Write(" 00");
                    }
                }
                Console.WriteLine("");
            }
            Console.WriteLine("STATIC DEADLOCKS:\n\n");

            for(int i = 0; i < Global.levelInfo.numFields; i++)
            {
                if(Global.staticDeadlocksCount[i] > 0)
                {
                    Console.Write("Forbidden Positions for Field " + i + ": \n");
                }
                for(int j = 0; j < Global.staticDeadlocksCount[i]; j++)
                {
                    Position.setPosition(ref Global.staticDeadlocks[i, j]);
                    Level.printLevel(Global.level);
                }
            }
            Console.WriteLine("------------------\n");
            Position.setPosition(ref original);
        }
    }
}
