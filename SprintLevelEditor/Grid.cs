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
        private int blockSize;

        public Grid(GraphicsDeviceManager graphics, int width, int height, int blockSize)
        {
            this.graphics = graphics;
            isShowing = true;
            screenWidth = width;
            screenHeight = height;
            this.blockSize = blockSize;

            resetGrid(1);    
        }

        public void resetGrid(float zoom)
        {
            verticalLines = new List<Line>();
            horizontalLines = new List<Line>();
            float updatedScreenWidth = screenWidth * zoom;
            float updatedscreenHeight = screenHeight * zoom;

            foreach (int i in Enumerable.Range(1, (int) (screenWidth / blockSize)))
            {
                Vector2 start = new Vector2(i * blockSize, 0);
                Vector2 end = new Vector2(i * blockSize, screenHeight);
                Line line = new Line(graphics, Line.Type.Point, start, end, 1);
                verticalLines.Add(line);
            }

            foreach (int j in Enumerable.Range(1, (int) (screenHeight / blockSize)))
            {
                Vector2 start = new Vector2(0, j * blockSize);
                Vector2 end = new Vector2(screenWidth, j * blockSize);
                Line line = new Line(graphics, Line.Type.Point, start, end, 1);
                horizontalLines.Add(line);
            }
        }

        public void Show()
        {
            isShowing = true;
        }

        public void Hide()
        {
            isShowing = false;
        }

        public void Pan(int xDelta, int yDelta)
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

        public void Zoom(float zoom)
        {
            resetGrid(zoom);
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
