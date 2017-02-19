using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLX;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SprintLevelEditor
{
    class Menu 
    {
        MenuButton rectangle;
        MenuButton triangle;
        MenuButton start;
        MenuButton end;

        public Menu(Texture2D rectangleTexture, Texture2D triangleTexture, Texture2D startTexture, Texture2D endTexture)
        {
            rectangle = new MenuButton(rectangleTexture);
            triangle = new MenuButton(triangleTexture);
            start = new MenuButton(startTexture);
            end = new MenuButton(endTexture);

            rectangle.position = new Vector2(5, 5);
            triangle.position = new Vector2(105, 5);
            start.position = new Vector2(205, 5);
            end.position = new Vector2(305, 5);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            rectangle.Draw(spriteBatch);
            triangle.Draw(spriteBatch);
            start.Draw(spriteBatch);
            end.Draw(spriteBatch);
        }
    }
}
