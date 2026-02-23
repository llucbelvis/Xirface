using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Silk.NET.Input;

namespace Xirface
{
    public class Input
    {
        public Keyboard Keyboard;
        public Mouse Mouse;

        public Input(IMouse mouse, IKeyboard keyboard) 
        {
            Mouse = new(mouse);
            Keyboard = new(keyboard);
        }
        
        public void Update()
        {
            Mouse.Update();
            Keyboard.Update();
        }
    }
}