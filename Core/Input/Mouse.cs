using Silk.NET.Input;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace Xirface
{
    public class Mouse
    {
        private IMouse IMouse;

        public HashSet<MouseButton>? current;
        public HashSet<MouseButton>? previous;

        public Vector2 Position;

        public Mouse(IMouse mouse)
        {
            IMouse = mouse;

            current = new();
        }

        public void Update()
        {
            previous = current;

            foreach (MouseButton button in Enum.GetValues<MouseButton>())
            {
                if (IMouse.IsButtonPressed(button)) current!.Add(button);
            }
        }

        public bool Clicked(MouseButton b) => current!.Contains(b) && !previous!.Contains(b);
        public bool Canceled(MouseButton b) => !current!.Contains(b) && !previous!.Contains(b);
        public bool Pressed(MouseButton b) => current!.Contains(b);

        public void OnDown(Action<MouseButton> action) => IMouse.MouseDown += (m,b) => action(b);
        public void OnUp(Action<MouseButton> action) => IMouse.MouseUp += (m,b) => action(b);
        public void OnMove(Action<Vector2> action) => IMouse.MouseMove += (m, pos) => action(pos);
        public void OnScroll(Action<float> action) => IMouse.Scroll += (m, w) => action(w.Y);

    }
}