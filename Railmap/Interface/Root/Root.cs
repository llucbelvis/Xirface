using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

using Silk.NET.OpenGL;

namespace Xirface
{
    public abstract class Root : Entity
    {
        public enum Positioning
        {
            Hierarchical,
            Absolute,
            Zero,
        }

        public event EventHandler Clicked;
        public event EventHandler Canceled;
        public event EventHandler Pressed;

        public event EventHandler Entered;
        public event EventHandler Hovering;
        public event EventHandler Exited;

        public string Id { get; set; }
        public bool Visible { get; set; }
        public Positioning positioning { get; set; } = Positioning.Hierarchical;
        public Color FillColor { get; set; }
        public HashSet<Root> Children { get; set; }
        public Root Parent;
        public Root(string id, Vector2 position, Vector2 origin, Vector2 size, float depth, Color fillColor, bool visible, HashSet<Root> children)
        :base(null, position, origin, depth)
        {
            Id = id;
            Position = position;
            Origin = origin;
            Size = size;
            Depth = depth;

            FillColor = fillColor;

            Visible = visible;
            Children = children;
        }

        public virtual void Refresh()
        {
            return;
        }
        
        public abstract void Draw(Camera camera, Vector2 world);

        public abstract void Buffer();

        public abstract void AssignShader();

        public void Invoke(Actions.TriggerType type, Root root)
        {
            switch (type)
            {
                case Actions.TriggerType.Clicked:
                    root?.Clicked?.Invoke(root, EventArgs.Empty);
                    break;
                case Actions.TriggerType.Canceled:
                    root?.Canceled?.Invoke(root, EventArgs.Empty);
                    break;
                case Actions.TriggerType.Pressed:
                    root?.Pressed?.Invoke(root, EventArgs.Empty);
                    break;
                case Actions.TriggerType.Entered:
                    root?.Entered?.Invoke(root, EventArgs.Empty);
                    break;
                case Actions.TriggerType.Hovering:
                    root?.Hovering?.Invoke(root, EventArgs.Empty);
                    break;
                case Actions.TriggerType.Exited:
                    root?.Exited?.Invoke(root, EventArgs.Empty);
                    break;

            }
        }

        public Vector2 Absolute()
        {
            Root Root = this;

            Vector2 absolute = Vector2.Zero;

            while (Root.Parent != null)
            {
                absolute += Root.Position;
                Root = Root.Parent;
            }

            return absolute - this.Origin;
        }


    }
}