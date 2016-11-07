using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SokobanSolver
{
    public static class Global
    {
        public static char WALL = '#';
        public static char FLOOR = ' ';
        public static char GOAL = '.';
        public static char BOX = '$';
        public static char BOXONGOAL = '*';
        public static char PLAYER = '@';
        public static char PONGOAL = '+';
        public static char DEADFIELD = 'x';
        public static char PATTERN_EDGE = '-';

        public static int LVLSIZE = 64;
        public static int HASHSIZE = 16769023;
        public static int HASHMAX = HASHSIZE * 9 / 10;
        public static int MOVECHUNK = 65536;
        public static int QUEUECHUNK = 65536;
        public static int MAXFIELDS = 500;
        public static int POSITIONSIZE = 4;
        public static int MAXDISTANCE = 10000;
        public static int MAXSOLUTION = 10000;
        public static int MAXSTATICDEADLOCKS = 100;
        public static int HIBYTES = (1 << 16);

        public static Level level = new Level();
        public static LevelInfo levelInfo = new LevelInfo(LVLSIZE, MAXFIELDS);
        public static LevelInfo tempInfo = new LevelInfo(LVLSIZE, MAXFIELDS);
        public static Solution levelSol = new Solution(MAXSOLUTION);
        public static Random random = new Random();
        public static Move root = new Move();

        public static Position[,] staticDeadlocks = new Position[MAXFIELDS, MAXSTATICDEADLOCKS];
        public static int[] staticDeadlocksCount = new int[MAXSTATICDEADLOCKS];

        public static int[] searchQueue = new int[LVLSIZE * LVLSIZE];
        public static int[] searchQueueX = new int[MAXFIELDS];
        public static int[] searchQueueY = new int[MAXFIELDS];

        public static int[,] reachable = new int[LVLSIZE, LVLSIZE];
        public static int[] boxx = new int[MAXFIELDS];
        public static int[] boxy = new int[MAXFIELDS];
        public static int boxCount;

        public static bool solvable = true;

        public static int[] art = new int[MAXFIELDS];

        public static int[] movesX = new int[] { -1, 0, 1, 0, -1, 0, 1, 0 };
        public static int[] movesY = new int[] { 0, 1, 0, -1, 0, 1, 0, -1 };

        //public static Queue[] moveQueue = new Queue[MAXDISTANCE];
        public static Queue[] moveQueue = Enumerable.Range(0, MAXDISTANCE).Select(i => new Queue()).ToArray();
        public static int currentDistance;
    }
}
