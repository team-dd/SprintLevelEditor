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
        public bool selected;
        public bool hovered;
        Texture2D baseTex;
        Texture2D hoverTex;
        Texture2D selectedTex;
        Action selectedFunction;

        public MenuButton(Texture2D tex, Action selectedFunction) : base(tex)
        {
            selected = false;
            hovered = false;
            this.baseTex = tex;
            this.hoverTex = null;
            this.selectedTex = null;
            this.selectedFunction = selectedFunction;
        }

        public MenuButton(Texture2D tex, Texture2D hoverTex, Texture2D selectedTex, Action selectedFunction) : base(tex)
        {
            selected = false;
            baseTex = tex;
            this.hoverTex = hoverTex;
            this.selectedTex = selectedTex;
            this.selectedFunction = selectedFunction;
        }

        public bool isHoveringOver()
        {
            MouseState mouseState = Mouse.GetState();
            return (rectangle.Contains(mouseState.Position.X, mouseState.Position.Y));
        }

        public void Update(GameTime gameTime)
        {
            bool isHovering = isHoveringOver();

            if (isHovering && hoverTex != null && !hovered && !selected)
            {
                tex = hoverTex;
                hovered = true;
            }
            else if (!isHovering && hovered && !selected)
            {
                tex = baseTex;
                hovered = false;
            }

            base.Update(gameTime);
        }

        public void Select()
        {
            if (selectedTex != null)
            {
                this.tex = selectedTex;
            }
            
            selected = true;
            selectedFunction.Invoke();
        }

        public void Unselect()
        {
            this.tex = baseTex;
            selected = false;
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
