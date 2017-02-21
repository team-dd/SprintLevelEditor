using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLX;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SprintLevelEditor
{
    class MenuButton : Sprite
    {
        private bool selected;
        private bool hovered;
        Texture2D baseTex;
        Texture2D hoverTex;
        Texture2D selectedTex;

        public MenuButton(Texture2D tex) : base(tex)
        {
            selected = false;
            this.baseTex = tex;
            this.hoverTex = null;
            this.selectedTex = null;
        }

        public MenuButton(Texture2D tex, Texture2D hoverTex, Texture2D selectedTex) : base(tex)
        {
            selected = false;
            baseTex = tex;
            this.hoverTex = hoverTex;
            this.selectedTex = selectedTex;
        }

        private bool isHoveringOver()
        {
            MouseState mouseState = Mouse.GetState();
            return (rectangle.Contains(mouseState.Position.X, mouseState.Position.Y));
        }

        public void Update(GameTime gameTime)
        {
            bool isHovering = isHoveringOver();

            if (isHovering && hoverTex != null && this.tex == baseTex)
            {
                tex = hoverTex;
            }
            else if (!isHovering && this.tex != baseTex)
            {
                tex = baseTex;
            }

            base.Update(gameTime);
        }

        public void Select()
        {
            this.tex = selectedTex;
        }

        public void Unselect()
        {
            this.tex = baseTex;
        }

        public void Draw(SpriteBatch spriteBatch, World world)
        {
            Vector2 oldPosition = position;
            position.X = world.CurrentCamera.MouseToScreenCoords(position.ToPoint()).X;
            position.Y = world.CurrentCamera.MouseToScreenCoords(position.ToPoint()).Y;
            base.Draw(spriteBatch);
            position = oldPosition;
        }
    }
}
