
using LGen.LParse;
using LGen.LRender;
using UnityEngine;

namespace LGen.LSimulate
{
    public class GridTile
    {
        public float fertility;
        
        public Agent occupant;

        public int density;
    }

    public class Grid
    {
        GridTile[,] tiles;

        public Grid(int width, int height) { tiles = new GridTile[width, height]; }
        public Grid(int width, int height, Texture2D fertilityMap) : this(width, height)
        {
            int xStep = fertilityMap.width / width;
            int yStep = fertilityMap.height / height;
            
            for(int x = 0; x < width; x++)
            {
                for(int y = 0; y < height; y++)
                {
                    float fert = fertilityMap.GetPixel(x*xStep, y*yStep).r;
                    tiles[x, y] = new GridTile();
                    tiles[x, y].fertility = fert;
                }
            }
        }

        public GridTile this[int x, int y]
        {
           get { return tiles[x, y]; }
           set { tiles[x, y] = value; }
        }
    }
}