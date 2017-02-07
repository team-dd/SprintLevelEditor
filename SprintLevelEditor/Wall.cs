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
    public class Wall
    {
        public Sprite sprite;

        public Wall(GraphicsDeviceManager graphics)
        {
            sprite = new Sprite(graphics);
        }

        public void Update(GameTimeWrapper gameTime)
        {
            sprite.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch, World world, float blockSize)
        {
            Vector2 oldPosition = sprite.position;
            sprite.position.X = world.CurrentCamera.MouseToScreenCoords(sprite.position.ToPoint()).X - (world.CurrentCamera.MouseToScreenCoords(sprite.position.ToPoint()).X % blockSize);
            sprite.position.Y = world.CurrentCamera.MouseToScreenCoords(sprite.position.ToPoint()).Y - (world.CurrentCamera.MouseToScreenCoords(sprite.position.ToPoint()).Y % blockSize);
            sprite.Draw(spriteBatch);
            sprite.position = oldPosition;
        }
    }
}