using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;


namespace Xirface
{
    public class Camera
    {
        public Vector2 Position;
        public float Zoom = 1;
        public int Width;
        public int Height;

        
        public Camera(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public void UpdateZoom()
        {
            //float scroll = CurrentState.ScrollWheelValue - PreviousState.ScrollWheelValue;

            //float scrollDelta = scroll / 200f; 
            //Zoom *= 1.0f + (scrollDelta * 0.1f);
            //Zoom = Math.Clamp(Zoom, 0.1f, 100.0f);
        }

        public void Move()
        {
            //if (Up.press)
            {
                Position.Y += 1f * 1;
            }

            //if (Left.press)
            {
                Position.X -= 1f * 1;
            }
                
            //if (Down.press)
            {
                Position.Y -= 1f * 1;
            }
                

            //if (Right.press)
            {
                Position.X += 1f * 1;
            }
                
        }
    }
}
