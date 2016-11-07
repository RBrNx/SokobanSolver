using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SokobanSolver
{
    public static class Allocator
    {
        public static Move[] allocMoves = null;
        public static int lastMove = Global.MOVECHUNK;
        public static Move freedMoves = null;

        public static Queue[] allocNodes = null;
        public static int lastNode = Global.QUEUECHUNK;
        public static Queue freedNodes = null;

        public static void initializeAllocator()
        {
            freedMoves.parent = null;
            freedNodes.next = null;
        }

        public static Queue mallocNode()
        {
            if(lastNode < Global.QUEUECHUNK)
            {
                return allocNodes[lastNode++];
            }
            else
            {
                //allocNodes = new Queue[Global.QUEUECHUNK];
                allocNodes = Enumerable.Range(0, Global.QUEUECHUNK).Select(i => new Queue()).ToArray();
                lastNode = 1;
                return allocNodes[0];
            }
        }

        public static Move mallocMove()
        {
            if(lastMove < Global.MOVECHUNK)
            {
                return allocMoves[lastMove++];
            }
            else
            {
                //allocMoves = new Move[Global.MOVECHUNK];
                allocMoves = Enumerable.Range(0, Global.MOVECHUNK).Select(i => new Move()).ToArray();
                lastMove = 1;
                return allocMoves[0];
            }
        }

        public static void freeMove(Move mov)
        {

        }
    }
}
