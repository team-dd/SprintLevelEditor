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

        public void Draw(SpriteBatch spriteBatch)
        {
            sprite.DrawRect(spriteBatch);
        }
    }
}