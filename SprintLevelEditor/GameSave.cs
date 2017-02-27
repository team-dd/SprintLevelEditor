using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace SprintLevelEditor
{
    [JsonObject]
    class GameSave
    {
        [JsonProperty("rectangles")]
        List<SimpleRectangle> rectangles;
        [JsonProperty("start")]
        Vector2 startPoint;
        [JsonProperty("end")]
        Vector2 endPoint;

        public GameSave() { }

        public GameSave(List<Wall> walls, Marker startPoint, Marker endPoint, float blockSize)
        {
            this.rectangles = new List<SimpleRectangle>();

            int xOffset = (int)walls[0].sprite.position.X;
            int yOffset = (int)walls[0].sprite.position.Y;

            foreach (Wall wall in walls)
            {
                if (wall.sprite.position.X < xOffset)
                {
                    xOffset = (int)wall.sprite.position.X;
                }

                if (wall.sprite.position.Y < yOffset)
                {
                    yOffset = (int)wall.sprite.position.Y;
                }
            }

            foreach (Wall wall in walls)
            {
                this.rectangles.Add(SimpleRectangle.fromWall(wall, blockSize, xOffset, yOffset));
            }

            this.startPoint = startPoint.toVector2(blockSize, xOffset, yOffset);
            this.endPoint = endPoint.toVector2(blockSize, xOffset, yOffset);
        }
    }
}
