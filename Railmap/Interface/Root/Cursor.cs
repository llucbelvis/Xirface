using System;
using System.Diagnostics;
using TextCopy;
using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Vulkan;

namespace Xirface
{
    public class Cursor : Root
    {
        public TextBox TextBox;
        public int CursorPosition;

        private float timer;

        private const float interval = 0.50f;

        public Cursor(string id, Vector2 position, Vector2 origin, Vector2 size, float depth, Color fillColor, bool visible)
        : base(id, position, origin, size, depth, fillColor, visible, null)
        {
                Mesh = Mesh<VertexPositionColor>.Square(Size, FillColor);
        }

        public override void Refresh()
        {
            Mesh = Mesh<VertexPositionColor>.Square(Size, FillColor);
        }

        public void Update(float deltaTime, Input input)
        {
            if (TextBox is null) return;

            timer += (float)deltaTime;

            if (timer >= interval) {
                
                Visible = !Visible;
                timer  -= interval;
            }

            if (input.Keyboard.Clicked(Key.Right))
            {
                if (CursorPosition + 1 < TextBox.Content.Length + 1)
                    CursorPosition += 1;
            }

            if (input.Keyboard.Clicked(Key.Left))
            {
                if (CursorPosition - 1 > -1)
                    CursorPosition -= 1;
            }


            if (input.Keyboard.Pressed(Key.ControlLeft) && input.Keyboard.Clicked(Key.V))
            {
                string paste = ClipboardService.GetText()!;

                if (!string.IsNullOrEmpty(paste))
                {
                    TextBox.Content = TextBox.Content.Insert(CursorPosition, paste);
                    CursorPosition += paste.Length;

                    timer = 0;
                    Visible = true;

                    TextBox.Refresh();
                }
            }


            

        }

        public override void Draw(Camera camera, Vector2 world)
        {
            throw new NotImplementedException();
        }

        public void Draw(CommandBuffer cmd, Camera camera)
        {
            if (!Visible) return;

            int cursorOffset = 0;
            for (int i = 0; i < CursorPosition; i++)
            {
                char c = TextBox.Content[i];
                if (TextBox.Font.CharacterGlyphDict.ContainsKey(c))
                    cursorOffset += TextBox.Font.CharacterGlyphDict[c].advanceWidth;
            }

            Vector2 textBox = TextBox.Absolute() - new Vector2(960, 540);

            
            Shader.SetView(Matrix4x4.CreateTranslation(new Vector3(-camera.Position, 0)) * Matrix4x4.CreateScale(camera.Zoom, camera.Zoom, 1));
            Shader.SetWorld(Matrix4x4.CreateTranslation(textBox.X + (cursorOffset * (TextBox.FontSize / TextBox.Font.unitsPerEm)), textBox.Y - (TextBox.FontSize / 8), Depth));
            Shader.SetProjection(Matrix4x4.CreateOrthographic(1920f, 1080f, -1f, 1f));
  
        }

        public void Focus(TextBox textBox)
        {
            TextBox = textBox;
            CursorPosition = textBox.Content.Length;

            Visible = true;

            Mesh = Mesh<VertexPositionColor>.Square(new Vector2(1, textBox.FontSize), textBox.FillColor);
        }

        public void Type(IKeyboard keyboard, char inputChar)
        {
            if (TextBox is null) return;

            if (inputChar == '\b')
            {
                if (TextBox.Content.Length > 0)
                {
                    CursorPosition -= 1;
                    TextBox.Content = TextBox.Content.Remove(CursorPosition, 1);
                }
            }
            else if (inputChar == '\r')
            {
            }
            else
            {
                TextBox.Content = TextBox.Content.Insert(CursorPosition, inputChar.ToString());
                CursorPosition += 1;
            }

            Visible = true;
            timer = 0f;
            TextBox.Refresh();
        }

        public override void Buffer()
        {
            
        }

        public override void AssignShader()
        {
            
        }
    }
}