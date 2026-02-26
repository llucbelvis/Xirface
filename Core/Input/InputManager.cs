using Silk.NET.Input;

namespace Xirface
{
    public partial class InputManager
    {
        public Keyboard Keyboard;
        public Mouse Mouse;

        public InputManager(IMouse mouse, IKeyboard keyboard) 
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