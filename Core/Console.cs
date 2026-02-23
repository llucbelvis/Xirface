using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Xirface
{
    public class Console
    {
        public float FramesPerSeconds;
        public float FrameRenderingTime;

        public string[] Output;

        private Queue<float> recordedFramerate = new();
        public Console(string[] output)
        {
            Output = output; 
        }

        public void Update(float delta, Railmap railmap, Interface @interface, Input input)
        {
            FramesPerSeconds = 1 / delta;
            FrameRenderingTime = 1000 / FramesPerSeconds;

            recordedFramerate.Enqueue(FramesPerSeconds);
            if (recordedFramerate.Count > 100)
                recordedFramerate.Dequeue();

            float displayFPS = recordedFramerate.Average();

            Text Output0 = (@interface.IdentifierDict[Output[0]] as Text);
            Text Output1 = (@interface.IdentifierDict[Output[1]] as Text);
            Text Output2 = (@interface.IdentifierDict[Output[2]] as Text);
            Text Output3 = (@interface.IdentifierDict[Output[3]] as Text);
            Text Output4 = (@interface.IdentifierDict[Output[4]] as Text);

            Output0.Content = $"{displayFPS} FPS";
            Output1.Content = $"{1000 / displayFPS} ms";
            Output2.Content = $"CAMERA - {railmap.Camera.Position} : {railmap.Camera.Zoom}";
            Output3.Content = $"COLLISION - {@interface.Collision}";
            //Output4.Content = $"{input.MouseButtonMap[Input.MouseButton.Left].press}";

            Output0.Refresh();
            Output1.Refresh();
            Output2.Refresh();
            Output3.Refresh();
            Output4.Refresh();
        }
    }
}
