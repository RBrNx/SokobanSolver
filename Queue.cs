using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SokobanSolver
{
    public class Queue
    {
        public Queue next;
        public Move e;

        public Queue()
        {
            next = null;
            e = null;
        }

        public static Queue createQueueNode(Move element)
        {
            Queue tmp = Allocator.mallocNode();
            tmp.e = element;
            return tmp;
        }

        public static void appendQueueNode(Queue node, Queue after)
        {
            node.next = after.next;
            after.next = node;
        }

        public static Queue removeQueueNode(Queue before)
        {
            Queue tmp = before.next;
            before.next = before.next.next;
            return tmp;
        }

        public static bool isQueueEmpty(Queue head)
        {
            return head.next == null;
        }
    }

    
}
