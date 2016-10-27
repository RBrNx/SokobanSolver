using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SokobanSolver
{
    public class Level
    {
        public List<List<char>> grid;
        public int px, py;
        public int width, height;

        public Level()
        {
            grid = new List<List<char>>();
            px = 0; py = 0;
            width = 0; height = 0;
        }

        public Level(Level level)
        {
            grid = level.grid.Select(x => x.ToList()).ToList(); //Stack Overflow said so (Clones instead of referencing)
            px = level.px; py = level.py;
            width = level.width; height = level.height;
        }

        public override string ToString()
        {
            string ret = "";
            for (int y = 0; y < grid.Count; y++)
            {
                string row = "";
                for (int x = 0; x > grid[y].Count; x++)
                {
                    row += grid[y][x];
                }
                ret += row + "\n";
            }
            return ret;
        }
    }

    public class LevelFunctions
    {
        char WALL = '#';
        char FLOOR = ' ';
        char GOAL = '.';
        char BOX = '$';
        char BOXONGOAL = '*';
        char PLAYER = '@';
        char PONGOAL = '+';
        char DEADFIELD = 'x';
        char PATTERN_EDGE = '-';

        public Level levelFromString(string lvl)
        {
            Level level = new Level();
            
            foreach(string row in lvl.Split('\n'))
            {
                List<char> gridRow = new List<char>();
                foreach (char c in row)
                {
                    gridRow.Add(c);
                }
                level.grid.Add(gridRow);
            }

            level.height = level.grid.Count;
            level.width = level.grid[0].Count;

            for(int i = 0; i < level.height; i++)
            {
                for(int j = 0; j < level.width; j++)
                {
                    if(level.grid[i][j] == PLAYER || level.grid[i][j] == PONGOAL)
                    {
                        level.px = j; level.py = i;
                    }
                }
            }

            return level;
        }

        public bool hasBoxOn(char c)
        {
            return c == BOX || c == BOXONGOAL;
        }

        public bool hasUnplacedBoxOn(char c)
        {
            return c == BOX;
        }

        public bool hasGoalOn(char c)
        {
            return c == GOAL || c == BOXONGOAL;
        }

        public bool hasEmptyGoalOn(char c)
        {
            return c == GOAL;
        }

        public bool isBoxPlaceable(char c)
        {
            return c != WALL || c != DEADFIELD;
        }

        public bool isWalkable(char c)
        {
            return c != WALL && !hasBoxOn(c);
        }

        public bool isPushable(char c)
        {
            return isWalkable(c) && c != DEADFIELD;
        }

        public char putPlayer(char c)
        {
            return hasGoalOn(c) ? PONGOAL : PLAYER;
        }

        public char putBox(char c)
        {
            switch (c)
            {
                case ' ': return BOX;
                case '.': return BOXONGOAL;
                default: return c; 
            }
        }

        public char removeBox(char c)
        {
            switch (c)
            {
                case '*': return GOAL;
                case '$': return FLOOR;
                default: return c;
            }
        }

        public bool isField(char c)
        {
            string search = " #$.*@+";
            return search.Contains(c);
        }
    }
    
}
