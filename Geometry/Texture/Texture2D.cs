using StbImageSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xirface
{
    public class Texture2D
    {
        public int Width;
        public int Height;

        public Texture2D(string path)
        {
            using FileStream stream = File.OpenRead(path);
            ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

         
            Width = image.Width;
            Height = image.Height;

           

        }

        public void Dispose()
        {
           
        }
    }
}
