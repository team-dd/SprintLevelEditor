using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLX;
using Microsoft.Xna.Framework.Graphics;

namespace SprintLevelEditor
{
    class MenuButton : Sprite
    {
        private bool selected;

        public MenuButton(Texture2D tex) : base(tex)
        {
            selected = false;
        }
    }
}
