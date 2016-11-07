using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SokobanSolver
{
    public struct Position
    {
        public uint[] b;
        public int s;

        public Position(int POSITION_SIZE)
        {
            b = new uint[POSITION_SIZE];
            s = 0;
        }

        public Position(Position pos)
        {
            b = (uint[])pos.b.Clone();
            s = pos.s;
        }

        public static void setPosition(ref Position pos)
        {
            int j = 0, i = 0;
            uint tmp = pos.b[0];
            for(int f = 0; f < Global.levelInfo.numFields; f++)
            {
                int x = Level.xPos(Global.levelInfo.fieldPos[f]);
                int y = Level.yPos(Global.levelInfo.fieldPos[f]);

                Global.level.grid[y][x] = Convert.ToBoolean(tmp % 2) ? Level.putBox(Global.level.grid[y][x]) : Level.removeBox(Global.level.grid[y][x]);
                tmp /= 2;

                if(i++ >= 31)
                {
                    j++;
                    tmp = pos.b[j];
                    i = 0;
                }
            }
            Global.level.px = Level.xPos(pos.s);
            Global.level.py = Level.yPos(pos.s);
        }

        public static void getPosition(ref Position pos)
        {
            pos.s = Level.genPos(Global.level.px, Global.level.py);
            for(int i = 0; i < Global.POSITIONSIZE; i++)
            {
                pos.b[i] = 0;
            }

            int j = 0, k = 0;
            for(int y = 0; y < Global.level.height; y++)
            {
                for(int x = 0; x < Global.level.width; x++)
                {
                    if (Global.level.grid[y][x] == Global.WALL || Global.level.grid[y][x] == Global.DEADFIELD) continue;
                    uint con = Convert.ToUInt32(Level.hasBoxOn(Global.level.grid[y][x]));
                    pos.b[k] = pos.b[k] + (con << j);

                    if(j++ >= 31)
                    {
                        j = 0;
                        k++;
                    }
                }
            }
        }

        public static bool comparePositions(Position pos1, Position pos2)
        {
            if (pos1.s != pos2.s) return false;
            return comparePositionsWithoutSokoban(pos1, pos2);
        }

        public static bool comparePositionsWithoutSokoban(Position pos1, Position pos2)
        {
            for(int i = 0; i < Global.POSITIONSIZE; i++)
            {
                if (pos1.b[i] != pos2.b[i]) return false;
            }

            return true;
        }

        public static bool isSubposition(ref Position pos, ref Position sub)
        {
            for(int i = 0; i < Global.POSITIONSIZE; i++)
            {
                if (sub.b[i] != (sub.b[i] & pos.b[i])) return false;
            }

            return true;
        }

        public static void pushBox(ref Position start, ref Position result, int from, int to, int xsok, int ysok)
        {
            for(int i = 0; i < Global.POSITIONSIZE; i++)
            {
                result.b[i] = start.b[i];
            }

            swapBoxOfPosition(ref result, from);
            swapBoxOfPosition(ref result, to);

            result.s = Level.genPos(xsok, ysok);
        }

        public static void cleanPosition(ref Position pos)
        {
            for(int i = 0; i < Global.POSITIONSIZE; i++)
            {
                pos.b[i] = 0;
            }
        }

        public static void addBoxOfPosition(ref Position pos, int fn)
        {
            pos.b[fn / 32] |= 1u << (fn % 32);
        }

        public static void removeBoxOfPosition(ref Position pos, int fn)
        {
            pos.b[fn / 32] &= (~(1u << (fn % 32)));
        }

        public static void swapBoxOfPosition(ref Position pos, int fn)
        {
            pos.b[fn / 32] ^= 1u << (fn % 32);
        }


    }
}
