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
        public Vector2 Position;

        public SimpleRectangle(int x, int y, int width, int height, Vector2 position)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Position = position;
        }

        public static SimpleRectangle fromWall(Wall wall)
        {
            Rectangle rectangle = wall.sprite.drawRect;
            return new SimpleRectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, rectangle.Location.ToVector2());
        }
    }
}
