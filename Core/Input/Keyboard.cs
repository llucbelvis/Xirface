
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Xml.Linq;

using Silk.NET.Input;

namespace Xirface
{
    public class Keyboard
    {
        public IKeyboard IKeyboard;

        public HashSet<Key> current;
        public HashSet<Key> previous;

        public Keyboard(IKeyboard keyboard)
        {
            IKeyboard = keyboard;
        }

        public void Update()
        {
            previous = current;

            foreach (Key key in Enum.GetValues<Key>())
            {
                if (IKeyboard.IsKeyPressed(key)) current.Add(key);
            }
        }

        public bool Clicked(Key k) => current.Contains(k) && !previous.Contains(k);
        public bool Canceled(Key k) => !current.Contains(k) && !previous.Contains(k);
        public bool Pressed(Key k) => current.Contains(k);

        public bool IsDown(Key key) => IKeyboard.IsKeyPressed(key);
        public void OnDown(Action<Key> action) => IKeyboard.KeyDown += (kb, key, sc) => action(key);
        public void OnUp(Action<Key> action) => IKeyboard.KeyUp += (kb, key, sc) => action(key);



    }
}
