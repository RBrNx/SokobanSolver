using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SokobanSolver
{
    public struct Level
    {
        public List<List<char>> grid;
        public int px, py;
        public int width, height;

        public Level(Level lvl)
        {
            grid = lvl.grid.Select(x => x.ToList()).ToList();
            px = lvl.px; py = lvl.py;
            width = lvl.width; height = lvl.height;
        }

        public static void levelFromString(string lvl)
        {
            Global.level.grid = new List<List<char>>();

            foreach (string row in lvl.Split('\n'))
            {
                List<char> gridRow = new List<char>();
                foreach (char c in row)
                {
                    gridRow.Add(c);
                }
                Global.level.grid.Add(gridRow);
            }

            Global.level.height = Global.level.grid.Count;
            Global.level.width = Global.level.grid[0].Count;

            for (int i = 0; i < Global.level.height; i++)
            {
                for (int j = 0; j < Global.level.width; j++)
                {
                    if (Global.level.grid[i][j] == Global.PLAYER || Global.level.grid[i][j] == Global.PONGOAL)
                    {
                        Global.level.px = j; Global.level.py = i;
                    }
                }
            }
        }

        public static void cleanLevel()
        {
            for (int i = 0; i < Global.level.height; i++)
            {
                for (int j = 0; j < Global.level.width; j++)
                {
                    Global.level.grid[i][j] = 'O';
                }
            }
        }

        public static void printLevel(Level level)
        {
            for(int y = 0; y < level.height; y++)
            {
                for(int x = 0; x < level.width; x++)
                {
                    Console.Write((y == level.py && x == level.px) ? putPlayer(level.grid[y][x]) : level.grid[y][x]);
                }
                Console.WriteLine("");
            }
            Console.WriteLine("\n");
        }

        public static int genPos(int x, int y) { return Global.LVLSIZE * x + y; }

        public static int xPos(int pos) { return pos / Global.LVLSIZE; }

        public static int yPos(int pos) { return pos % Global.LVLSIZE; }

        public static bool hasBoxOn(char c)
        {
            return c == Global.BOX || c == Global.BOXONGOAL;
        }

        public static bool hasUnplacedBoxOn(char c)
        {
            return c == Global.BOX;
        }

        public static bool hasGoalOn(char c)
        {
            return c == Global.GOAL || c == Global.BOXONGOAL;
        }

        public static bool hasEmptyGoalOn(char c)
        {
            return c == Global.GOAL;
        }

        public static bool isBoxPlaceable(char c)
        {
            return c != Global.WALL && c != Global.DEADFIELD;
        }

        public static bool isWalkable(char c)
        {
            return c != Global.WALL && !hasBoxOn(c);
        }

        public static bool isPushable(char c)
        {
            return isWalkable(c) && c != Global.DEADFIELD;
        }

        public static char putPlayer(char c)
        {
            return hasGoalOn(c) ? Global.PONGOAL : Global.PLAYER;
        }

        public static char putBox(char c)
        {
            switch (c)
            {
                case ' ': return Global.BOX;
                case '.': return Global.BOXONGOAL;
                default: return c;
            }
        }

        public static char removeBox(char c)
        {
            switch (c)
            {
                case '*': return Global.GOAL;
                case '$': return Global.FLOOR;
                default: return c;
            }
        }

        public static bool isField(char c)
        {
            string search = " #$.*@+";
            return search.Contains(c);
        }
    }    
}
