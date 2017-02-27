using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLX;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SprintLevelEditor
{
    class Grid
    {
        private GraphicsDeviceManager graphics;
        private List<Line> verticalLines;
        private List<Line> horizontalLines;
        public bool isShowing;
        private int screenWidth;
        private int screenHeight;
        private float blockSize;
        public Vector2 center;

        public Grid(GraphicsDeviceManager graphics, int width, int height, float blockSize)
        {
            this.graphics = graphics;
            isShowing = true;
            screenWidth = width;
            screenHeight = height;
            this.blockSize = blockSize;

            verticalLines = new List<Line>();
            horizontalLines = new List<Line>();
            float updatedScreenWidth = screenWidth;
            float updatedScreenHeight = screenHeight;

            foreach (int i in Enumerable.Range(1, (int)(updatedScreenWidth * 10 / blockSize)))
            {
                Vector2 start = new Vector2((i * blockSize) - (updatedScreenWidth * 5), updatedScreenHeight * -10);
                Vector2 end = new Vector2((i * blockSize) - (updatedScreenWidth * 5), updatedScreenHeight * 10);
                Line line = new Line(graphics, Line.Type.Point, start, end, 1);
                verticalLines.Add(line);
            }

            foreach (int j in Enumerable.Range(1, (int)(updatedScreenHeight * 10 / blockSize)))
            {
                Vector2 start = new Vector2(updatedScreenWidth * -10, (j * blockSize) - (updatedScreenHeight * 5));
                Vector2 end = new Vector2(updatedScreenWidth * 10, (j * blockSize) - (updatedScreenHeight * 5));
                Line line = new Line(graphics, Line.Type.Point, start, end, 1);
                horizontalLines.Add(line);
            }

            center = new Vector2((updatedScreenWidth * 5 / blockSize), (updatedScreenHeight * 5 / blockSize));
        }

        public void Show()
        {
            isShowing = true;
        }

        public void Hide()
        {
            isShowing = false;
        }

        public void Pan(float xDelta, float yDelta)
        {
            foreach (Line line in verticalLines)
            {
                line.point1.X += xDelta;
                line.point2.X += xDelta;
                line.point1.Y += yDelta;
                line.point2.Y += yDelta;
            }

            foreach (Line line in horizontalLines)
            {
                line.point1.X += xDelta;
                line.point2.X += xDelta;
                line.point1.Y += yDelta;
                line.point2.Y += yDelta;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (isShowing)
            {
                foreach (Line line in verticalLines)
                {
                    line.Draw(spriteBatch);
                }

                foreach (Line line in horizontalLines)
                {
                    line.Draw(spriteBatch);
                }
            }    
        }
    }
}
