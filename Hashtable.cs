using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SokobanSolver
{
    static class Hashtable
    {
        public static Move[] data = new Move[Global.HASHSIZE];
        public static int count;

        public static void initializeHash()
        {
            count = 0;
            data = new Move[Global.HASHSIZE];
        }

        /*public static cleanHash()
        {
            for(int i = 0; i < Global.HASHSIZE; i++)
            {
                if (data[i] != 0)
                {

                }
            }
        }*/

        public static int addToHashtable(Move move)
        {
            uint pos = (uint)move.magic;
            while(data[pos % Global.HASHSIZE] != null)
            {
                if(Move.compareMoves(move, data[pos % Global.HASHSIZE]))
                {
                    return 0;
                }
                else
                {
                    pos++;
                }
            }
            data[pos % Global.HASHSIZE] = move;
            count++;
            return 1;
        }
    }

    
}
