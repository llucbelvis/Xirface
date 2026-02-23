using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;



namespace Xirface
{
    public class Railmap
    {
        public Camera Camera;
        

        Railmap.State Gamemode;

        public enum State
        {
            None,

            TrackPlace,
            TrackEdit,
            TrackDelete,

            StationPlace,
            StationEdit,
            StationDelete,
        }

        public Railmap()
        {
            this.Camera = new Camera(1920, 1080);
        }

        

        public void Update()
        {

 
            switch (Gamemode)
            {
                case State.TrackPlace:

                    break;
                case State.TrackEdit:

                    break;
                case State.TrackDelete:

                    break;
            }
        }

        public void Draw()
        {
            
        }
    }
}
