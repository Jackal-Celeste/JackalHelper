using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monocle;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace TwigHelper.ARC_Project
{
    //directed graph of nodes for bosses to jump between
    [Tracked]
    [CustomEntity("TwigHelper/BossNode")]
    public class BossNode : Entity
    {
        private List<BossNode> neighbors = new List<BossNode>();
        public Vector2[] positions;
        public BossNode(Vector2 position, Vector2[] neighbors) : base(position)
        {
            this.positions = neighbors;
        }
        public BossNode(EntityData data, Vector2 offset) : this(data.Position + offset, data.NodesWithPosition(offset))
        {
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            foreach (BossNode n in (scene as Level).Tracker.GetEntities<BossNode>())
            {
                if(n != this)
                {
                    foreach(Vector2 p in positions)
                    {
                        if(n.Position == p)
                        {
                            neighbors.Add(n);
                        }
                    }
                }
            }
        }

        public List<BossNode> GetComponents()
        {
            if (neighbors.Count > 0) return neighbors;
            return new List<BossNode>();
        }

        public static void updateNode(BossNode n)
        {
            n = TwigModule.Session.currentBossNode;
        }
    }
}
