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
        public IntPtr e;

        public Queue createQueueNode(IntPtr element)
        {
            Queue tmp = mallocNode();
            tmp.e = element;
            return tmp;
        }

        public void appendQueueNode(ref Queue node, ref Queue after)
        {
            node.next = after.next;
            after.next = node;
        }

        public Queue removeQueueNode(ref Queue before)
        {
            Queue tmp = before.next;
            before.next = before.next.next;
            return tmp;
        }

        public bool isQueueEmpty(Queue head)
        {
            return head.next == null;
        }
    }

    
}
