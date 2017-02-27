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
    class Marker : Sprite
    {
        private bool shouldRender;
        public bool isPlaced;

        public Marker(Texture2D tex) : base(tex)
        {
            shouldRender = false;
            isPlaced = false;
        }

        public void Show()
        {
            shouldRender = true;
            isPlaced = false;
        }

        public void Hide()
        {
            shouldRender = false;
        }

        public void Place()
        {
            isPlaced = true;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (isPlaced || shouldRender)
            {
                base.Draw(spriteBatch);
            }
        }

        public Vector2 toVector2(float blockSize, int xOffset, int yOffset)
        {
            return new Vector2(((position.X - xOffset) / blockSize) * 5, ((position.Y - yOffset) / blockSize) * 5);
        }
    }
}
