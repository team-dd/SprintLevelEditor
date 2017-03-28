using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SprintLevelEditor
{
    public struct SimpleRectangle
    {
        const int SCALE = 5;
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public bool IsMoving;

        public SimpleRectangle(int x, int y, int width, int height, bool isMoving)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            IsMoving = isMoving;
        }

        public static SimpleRectangle fromWall(Wall wall, float blockSize, int xOffset, int yOffset)
        {
            Rectangle rectangle = new Rectangle((int)wall.sprite.position.X, (int)wall.sprite.position.Y, (int)wall.sprite.DrawSize.Width, (int)wall.sprite.DrawSize.Height);
            return new SimpleRectangle((int) (Math.Max(0, (rectangle.X - xOffset) / blockSize) * SCALE), (int) (Math.Max(0, (rectangle.Y - yOffset) / blockSize) * SCALE), (int) (Math.Max(1, rectangle.Width / blockSize) * SCALE), (int) (Math.Max(1, rectangle.Height / blockSize) * SCALE), wall.isMoving);
        }

        public Wall toWall(Texture2D movingTex, Texture2D notMovingTex, GraphicsDeviceManager graphics, float blockSize)
        {
            Vector2 position = new Vector2(((X / SCALE) * blockSize), ((Y / SCALE) * blockSize));
            Vector2 size = new GLX.Size((Width / SCALE) * blockSize, (Height / SCALE) * blockSize);
            Wall wall = new Wall(movingTex, notMovingTex, graphics, position, size);
            if (IsMoving)
            {
                wall.clickIsMovingButton();
            }
            return wall;
        }
    }
}
