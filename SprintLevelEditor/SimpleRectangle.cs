using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace SprintLevelEditor
{
    public struct SimpleRectangle
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;

        public SimpleRectangle(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public static SimpleRectangle fromWall(Wall wall, float blockSize, int xOffset, int yOffset)
        {
            Rectangle rectangle = new Rectangle((int)wall.sprite.position.X, (int)wall.sprite.position.Y, (int)wall.sprite.DrawSize.Width, (int)wall.sprite.DrawSize.Height);
            return new SimpleRectangle(Math.Max(0, (int) ((rectangle.X - xOffset) / blockSize)), Math.Max(0, (int) ((rectangle.Y - yOffset) / blockSize)), Math.Max(1, (int) (rectangle.Width / blockSize)), Math.Max(1, (int) (rectangle.Height /  blockSize)));
        }
    }
}
