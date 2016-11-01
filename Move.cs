using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SokobanSolver
{
    public class Move
    {
        public uint heuristic;
        public Move parent;
        public Position pos;
        public int magic;

        public Move()
        {
            heuristic = 0;
            parent = null;
            pos = new Position(Global.POSITIONSIZE);
            magic = 0;
        }

        public static bool compareMoves(Move move1, Move move2)
        {
            return move1.magic == move2.magic && Position.comparePositions(move1.pos, move2.pos);
        }
    }
}
