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
        public bool isMoving;
        public Texture2D notMovingTex;
        public Texture2D movingTex;
        public Sprite isMovingButton;
        public bool shouldShow;

        public Wall(Texture2D movingTex, Texture2D notMovingTex, GraphicsDeviceManager graphics, bool shouldShow)
        {
            sprite = new Sprite(graphics);
            isMoving = false;
            this.notMovingTex = notMovingTex;
            this.movingTex = movingTex;
            isMovingButton = new Sprite(notMovingTex);
            this.shouldShow = shouldShow;
        }

        public Wall(Texture2D movingTex, Texture2D notMovingTex, GraphicsDeviceManager graphics, Vector2 position, Size size)
        {
            sprite = new Sprite(graphics);
            sprite.position = position;
            sprite.DrawSize = size;
            this.movingTex = movingTex;
            this.notMovingTex = notMovingTex;
            shouldShow = true;
        }

        public void clickIsMovingButton()
        {
            isMoving = !isMoving;

            if (isMoving)
            {
                isMovingButton = new Sprite(movingTex);
            }
            else
            {
                isMovingButton = new Sprite(notMovingTex);
            }

            Vector2 wallPosition = sprite.position;
            Vector2 buttonPosition = new Vector2(wallPosition.X + (sprite.DrawSize.Width / 2), wallPosition.Y + (sprite.DrawSize.Height / 2));
            isMovingButton.position = buttonPosition;
        }

        public void Update(GameTimeWrapper gameTime)
        {
            sprite.Update(gameTime);
            if (shouldShow)
            {
                Vector2 wallPosition = sprite.position;
                Vector2 buttonPosition = new Vector2(wallPosition.X + (sprite.DrawSize.Width / 2), wallPosition.Y + (sprite.DrawSize.Height / 2));
                isMovingButton.position = buttonPosition;
                isMovingButton.Update(gameTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch, World world, float blockSize)
        {
            Vector2 oldPosition = sprite.position;
            sprite.Draw(spriteBatch);
            if (shouldShow)
            {
                isMovingButton.Draw(spriteBatch);
            }
        }
    }
}