﻿using System;
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
        public static int posnum = 0;

        public static void trySolveLevel()
        {
            initializeRoutine();
            runRoutine();
            cleanRoutine();
        }

        public static void initializeRoutine()
        {
            Hashtable.initializeHash();
            CRS.initializeCRS();
            Position.getPosition(ref Global.root.pos);

            Global.root.magic = 0;
            Global.root.heuristic = 0;

            for(int y = 0; y < Global.level.height; y++)
            {
                for(int x = 0; x < Global.level.width; x++)
                {
                    if (Level.hasBoxOn(Global.level.grid[y][x]))
                    {
                        Global.root.magic = Global.root.magic ^ (int)Global.levelInfo.magic[y, x];
                        Global.root.heuristic += (uint)Global.levelInfo.goalDists[Global.levelInfo.fieldNum[y, x]];
                    }
                }
            }

            Global.root.magic = Global.root.magic ^ (int)LevelInfo.magicForSokoban(Global.level.px, Global.level.py);
            Global.root.parent = null;

            for(int i = 0; i < Global.MAXDISTANCE; i++)
            {
                Global.moveQueue[i].next = null;
            }
            Global.currentDistance = (int)Global.root.heuristic;
            Global.moveQueue[Global.currentDistance].next = Queue.createQueueNode(Global.root);
            Hashtable.addToHashtable(Global.root);
        }

        public static void cleanRoutine()
        {
            Hashtable.cleanHash();
        }

        public static void runRoutine()
        {
            newMove = Allocator.mallocMove();
            for( ; Global.currentDistance < Global.MAXDISTANCE; Global.currentDistance++)
            {
                while (!Queue.isQueueEmpty(Global.moveQueue[Global.currentDistance]))
                {
                    if(Hashtable.count > Global.HASHMAX)
                    {
                        Global.solvable = false;
                        //Console.WriteLine("Hash Too Full");
                        return;
                    }

                    Queue node = Queue.removeQueueNode(Global.moveQueue[Global.currentDistance]);
                    mov = node.e;

                    Position.setPosition(ref mov.pos);
                    posnum++;
                    /*if (posnum % 50000 == 0)
                    {
                        Level.printLevel(Global.level);
                    }*/

                    CRS.calculatePosition();

                    //echoAreas();
                    //Level.printLevel(Global.level);
                    //Console.WriteLine("Current Heuristic = " + Global.currentDistance);

                    //Console.WriteLine("mov.heuristic = " + mov.heuristic + " HIBYTES = " + Global.HIBYTES);
                    //Console.WriteLine("Heuristic %: " + mov.heuristic % Global.HIBYTES + " Heuristic /: " + mov.heuristic / Global.HIBYTES);
                    if(mov.heuristic % Global.HIBYTES == mov.heuristic / Global.HIBYTES)
                    {
                        //Level.printLevel(Global.level);
                        //Console.WriteLine("Level Solved with " + mov.heuristic / Global.HIBYTES + " pushes!");

                        SolFunc.createSolution(Global.levelSol, mov);
                        SolFunc.writeSolution(Global.levelSol);

                        return;
                    }

                    for(int b = 0; b < Global.boxCount; b++)
                    {
                        int x = Global.boxx[b];
                        int y = Global.boxy[b];
                        int from = Global.levelInfo.fieldNum[y, x];

                        if(Global.levelInfo.tunnel[from] == 0)
                        {
                            for(int i = 0; i < 4; i++)
                            {
                                int xsok = x + Global.movesX[i]; int ysok = y + Global.movesY[i];
                                int xto = x + Global.movesX[i + 2]; int yto = y + Global.movesY[i + 2];
                                if(Global.reachable[ysok, xsok] == 1 && Level.isPushable(Global.level.grid[yto][xto]))
                                {
                                    int to = Global.levelInfo.fieldNum[yto, xto];
                                    addMove(x, y, from, xto, yto, to, 1);
                                }
                            }
                        }
                        else
                        {

                            //Console.WriteLine("Using Tunnel Macro...");
                            for(int i = 0; i < 2; i++)
                            {
                                int type = Global.levelInfo.tunnel[from] = 1;
                                int xsok = x + Global.movesX[i * 2 + type]; int ysok = y + Global.movesY[i * 2 + type];
                                int to = Global.levelInfo.tunnelGoals[from, i];

                                if(to != -1)
                                {
                                    int xto = Level.xPos(Global.levelInfo.fieldPos[to]); int yto = Level.yPos(Global.levelInfo.fieldPos[to]);
                                    if(Global.reachable[ysok, xsok] == 1 && Level.isPushable(Global.level.grid[yto][xto]))
                                    {
                                        int pd = Global.levelInfo.tunnelDists[from, i];
                                        //Console.WriteLine("Tunnel: (" + xsok + ", " + ysok + ") =" + pd + "=> (" + xto + ", " + yto + ")");
                                        addMove(x, y, from, xto, yto, to, pd);
                                    }
                                }
                            }
                        }
                    }
                    //Console.WriteLine("--------------------------------------------------------------\n");
                }
            }
        }

        public static void addMove(int x, int y, int from, int xto, int yto, int to, int pd)
        {
            Position.pushBox(ref mov.pos, ref newMove.pos, from, to, x, y);
            newMove.magic = mov.magic ^ (int)Global.levelInfo.magic[y, x] ^ (int)Global.levelInfo.magic[yto, xto] ^ (int)LevelInfo.magicForSokoban(Global.level.px, Global.level.py)
                                        ^ (int)LevelInfo.magicForSokoban(x, y);

            if(!DeadlockTable.testStaticDeadlocks(newMove.pos, to) && Hashtable.addToHashtable(newMove))
            {
                //echoMove(mov, newMove);
                newMove.parent = mov;
                newMove.heuristic = mov.heuristic + (uint)pd - (uint)Global.levelInfo.goalDists[from] + (uint)Global.levelInfo.goalDists[to] + (uint)(Global.HIBYTES * pd);
                Queue.appendQueueNode(Queue.createQueueNode(newMove), Global.moveQueue[newMove.heuristic % Global.HIBYTES]);
                newMove = Allocator.mallocMove();
            }

        }

        public static void echoMove(Move cur, Move next)
        {
            int pos = 0;
            int pos2 = 0;

            for (int y = 0; y < Global.level.height; y++)
            {
                for(int x = 0; x < Global.level.width; x++)
                {
                    if(Global.level.grid[y][x] == Global.WALL)
                    {
                        Console.Write('#');
                    }
                    else if(Global.level.grid[y][x] == Global.DEADFIELD)
                    {
                        Console.Write('x');
                    }
                    else
                    {
                        Console.Write(Convert.ToBoolean(((cur.pos.b[pos / 32] >> (pos++ % 32)) % 2)) ? Level.putBox(Global.level.grid[y][x]) : (cur.pos.s == Level.genPos(x, y) ? Level.putPlayer(Global.level.grid[y][x]) : Global.level.grid[y][x]));
                    }
                }
                Console.Write(y == Global.level.height / 2 ? "    -->    " : "           ");

                for(int x = 0; x < Global.level.width; x++)
                {
                    if(Global.level.grid[y][x] == Global.WALL)
                    {
                        Console.Write('#');
                    }
                    else if(Global.level.grid[y][x] == Global.DEADFIELD)
                    {
                        Console.Write('x');
                    }
                    else
                    {
                        Console.Write(Convert.ToBoolean(((next.pos.b[pos2 / 32] >> (pos2++ % 32)) % 2)) ? Level.putBox(Global.level.grid[y][x]) : (next.pos.s == Level.genPos(x, y) ? Level.putPlayer(Global.level.grid[y][x]) : Global.level.grid[y][x]));
                    }
                }
                Console.WriteLine("");
            }
            Console.WriteLine("");
        }

        public static void echoReachable()
        {
            Console.WriteLine("Reachable Squares: ");
            for(int y = 0; y < Global.level.height; y++)
            {
                for (int x = 0; x < Global.level.width; x++)
                {
                    if (Global.reachable[y, x] != -1)
                    {
                        Console.Write(Global.reachable[y, x]);
                    }
                    else
                    {
                        Console.Write("#");
                    }
                }
                Console.WriteLine("");
            }
            Console.WriteLine("");
        }

        public static void echoAreas()
        {
            Console.WriteLine("Areas:");
            for(int y = 0; y < Global.level.height; y++)
            {
                for(int x = 0; x < Global.level.width; x++)
                {
                    if(Global.level.grid[y][x] == Global.WALL)
                    {
                        Console.Write('#');
                    }
                    else
                    {
                        Console.Write(Global.reachable[y, x]);
                    }
                }
                Console.WriteLine("");
            }
            Console.WriteLine("");
        }
    }
}
