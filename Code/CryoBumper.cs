using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.JackalHelper.Entities
{
    [CustomEntity("JackalHelper/CryoBumper")]
    [Tracked]
    public class CryoBumper : Bumper
    {

        public CryoBumper(Vector2 position, Vector2? node) : base(position, node)
        {
            Add(new PlayerCollider(OnPlayerCryoBoost));
            

        }

        public CryoBumper(EntityData data, Vector2 offset) : this(data.Position + offset, data.FirstNodeNullable(offset)) { }

        private void OnPlayerCryoBoost(Player player)
        {
            JackalModule.Session.HasCryoDash = true;
        }

        public override void Update()
        {
            base.Update();
            new DynData<Bumper>(this).Set<bool>("fireMode", false);
        }

    }
}
