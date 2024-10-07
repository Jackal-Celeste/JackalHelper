using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace TwigHelper.ARC_Project
{
    [CustomEntity("TwigHelper/FaceLeft")]
    public class _1984Trigger : Trigger
    {
        public _1984Trigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {

        }
        public override void OnStay(Player player)
        {
            if(player.Speed.X < -200f) player.Facing = Facings.Left;
            base.OnStay(player);
            if (player.Speed.X < -200f) player.Facing = Facings.Left;
        }
    }
}
