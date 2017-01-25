﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLX;
using Microsoft.Xna.Framework.Graphics;

namespace SprintLevelEditor
{
    public class Circle : Sprite
    {
        public enum JumpStates
        {
            Ground,
            WallRight,
            WallLeft,
            Air
        }

        public JumpStates jumpState;
        public float storedXVelocity;

        public Circle(Texture2D tex) : base(tex)
        {
            jumpState = JumpStates.Ground;
            storedXVelocity = 0;
        }
    }
}