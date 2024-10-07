using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Monocle;

namespace TwigHelper.ARC_Project
{
    public class Edge : Entity
    {
        public Inkrail s;
        public Inkrail e;

        public Edge(Inkrail start, Inkrail end) : base((start.Position + end.Position)/2)
        {
            s = start;
            e = end;
        }
        public override void Render()
        {
            Draw.Line(s.Position, e.Position, Color.Red);
            base.Render();
        }
    }
}
