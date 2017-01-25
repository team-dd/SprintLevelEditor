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

        public static SimpleRectangle fromWall(Wall wall)
        {
            Rectangle rectangle = wall.sprite.drawRect;
            return new SimpleRectangle(Math.Max(1, rectangle.X / 10), Math.Max(1, rectangle.Y / 10), Math.Max(1, rectangle.Width / 20), Math.Max(1, rectangle.Height / 20));
        }
    }
}
