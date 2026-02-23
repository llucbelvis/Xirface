using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using Silk.NET.Input;
using Silk.NET.OpenGL;



namespace Xirface
{
    public class Interface
    {
        public Camera Camera;

        public Dictionary<Key, Action> KeyActionMap = new();
        public Dictionary<MouseButton, Action> MouseActionMap = new();
        public Actions Actions;

        public HashSet<Root> Roots = new();
        public HashSet<Frame> InterfaceLayer = new();
        public Dictionary<string, Root> IdentifierDict = new();

        public Cursor Cursor;

        public Root previousRoot;

        public Vector2 channel00;
        public Vector2 channel01;
        public float channel02;
        public string Collision;

        public Interface(GL gl, Input input, AssetManager assetManager)
        {
            Camera = new Camera(1920,1080);
            
            
            Cursor = new Cursor("Cursor", Vector2.Zero, Vector2.Zero, Vector2.Zero, 0, new Color(0,0,0,255), false);

            //window.TextInput += Cursor.Type;

            LoadPartitions(assetManager);
            MapRoots(Roots);
            Actions = new Actions(this);
        }

        public void UpdateActions()
        {

        }

        public void Update(float deltaTime, GL gl, Input input)
        {
            Cursor.Update(deltaTime, input);

            Root currentRoot = Physics.Check(Roots, input.Mouse.Position);
            
            if (currentRoot != previousRoot)
            {
                currentRoot?.Invoke(Actions.TriggerType.Entered, currentRoot);

        
                previousRoot?.Invoke(Actions.TriggerType.Exited, currentRoot!);
            }

            currentRoot?.Invoke(Actions.TriggerType.Hovering, currentRoot);

            Collision = currentRoot !=  null ? currentRoot.GetType().ToString() :  "-";
            if (input.Mouse.Clicked(MouseButton.Left))
            {

                currentRoot?.Invoke(Actions.TriggerType.Clicked, currentRoot);


            }

            if (input.Mouse.Pressed(MouseButton.Left))
            {

                currentRoot?.Invoke(Actions.TriggerType.Pressed, currentRoot);
            }

            if (input.Mouse.Canceled(MouseButton.Left))
            {

                currentRoot?.Invoke(Actions.TriggerType.Canceled, currentRoot);
            }
      
            Actions.Zoom(this);

            previousRoot = currentRoot!;
        }

        private void LoadPartitions(AssetManager assetManager)
        {
            Roots.Add(new Partition(assetManager, this, "C:\\Users\\llucb\\source\\repos\\Xirface\\Railmap\\Interface\\Partitions\\rightbar.hjson").Root);
            Roots.Add(new Partition(assetManager, this, "C:\\Users\\llucb\\source\\repos\\Xirface\\Railmap\\Interface\\Partitions\\console.hjson").Root);
        }

        public void Clean() 
        {
           

            foreach (Root Root in IdentifierDict.Values)
            {
               
                Root.Buffer();
            }

 
            Cursor.Buffer();
        }

        public void Draw()
        {
            Clean();

            foreach (Root currentRoot in Roots)
            {
                currentRoot.Draw(Camera, Vector2.Zero);
            }

            //Cursor.Draw(Camera);
        }

        public void MapRoots(HashSet<Root> Roots)
        {
            foreach (Root currentRoot in Roots)
            {
                MapRoot(currentRoot);

                IdentifierDict.Add(currentRoot.Id, currentRoot);
            }

        }
        public void MapRoot (Root Root)
        {
            foreach (Root Child in Root.Children)
            {
                Child.Parent = Root;

                MapRoot(Child);

                IdentifierDict.Add(Child.Id, Child);
            }
        }

       
    }
}
