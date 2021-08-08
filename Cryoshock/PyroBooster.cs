using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.JackalHelper.Entities
{
    [CustomEntity("JackalHelper/PyroBoosterObject")]
    [Tracked]
    public class PyroBooster : Booster
    {
        public bool hasLaunched;

        public Sprite sprite;

        private DynData<Booster> boostData;

        public PyroBooster(Vector2 position) : base(position, true)
        {
            boostData = new DynData<Booster>(this);
            base.Remove(boostData.Get<Sprite>("sprite"));
            sprite = JackalModule.spriteBank.Create("boosterLava");
            boostData.Set<Sprite>("sprite", JackalModule.spriteBank.Create("boosterLava"));
            base.Add(boostData.Get<Sprite>("sprite"));
            boostData.Get<BloomPoint>("bloom").Alpha = 0.5f;
            boostData.Get<ParticleType>("particleType").Color = Color.OrangeRed;
            boostData.Get<ParticleType>("particleType").Color2 = Color.OrangeRed;
            base.Ch9HubBooster = true;

        }
        public PyroBooster(EntityData data, Vector2 offset)
        : this(data.Position + offset)
        {
        }


    }
}
