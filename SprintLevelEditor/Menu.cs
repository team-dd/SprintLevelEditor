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
        List<MenuButton> buttons;

        public Menu(List<MenuButton> buttons)
        {
            this.buttons = buttons;
        }

        public void Update(GameTimeWrapper gameTime)
        {
            foreach (MenuButton button in buttons)
            {
                button.Update(gameTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch, World world)
        {
            foreach (MenuButton button in buttons)
            {
                button.Draw(spriteBatch, world);
            }
        }

        public bool isHoveringOverAnyButton()
        {
            return buttons.Any((button) => { return button.isHoveringOver(); });
        }

        public void clickHoveredButton()
        {
            foreach (MenuButton button in buttons)
            {
                if (button.selected)
                {
                    button.Unselect();
                }
            }

            buttons.First((button) => { return button.isHoveringOver(); }).Select();
        }
    }
}
