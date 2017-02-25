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
    class Triangle : Sprite
    {
        public bool horizontalFlip;
        public bool verticalFlip;

        public Triangle(Texture2D tex) : base(tex)
        {

        }

        public override void Update(GameTimeWrapper gameTime)
        {
            base.Update(gameTime);
            float widthDelta = DrawSize.Width / tex.Width;
            float heightDelta = DrawSize.Height / tex.Height;

            float newScale = Math.Max(widthDelta, heightDelta);
            scale = newScale;

            SpriteEffects newSpriteEffects = SpriteEffects.None;

            if (horizontalFlip && verticalFlip)
            {
                newSpriteEffects = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
            }
            else if (horizontalFlip)
            {
                newSpriteEffects = SpriteEffects.FlipHorizontally;
            }
            else if (verticalFlip)
            {
                newSpriteEffects = SpriteEffects.FlipVertically;
            }

            spriteEffects = newSpriteEffects;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {  
            base.Draw(spriteBatch);
        }
    }
}
