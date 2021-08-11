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

    [Tracked]
    public class BouncyBooster : Booster
    {
        public bool hasLaunched;

        public Sprite sprite;

        private DynData<Booster> boostData2;

        public BouncyBooster(Vector2 position, float lives) : base(position, true)
        {
            boostData2 = new DynData<Booster>(this);
            base.Remove(boostData2.Get<Sprite>("sprite"));
            sprite = JackalModule.spriteBank.Create("boosterNeo");
            boostData2.Set<Sprite>("sprite", JackalModule.spriteBank.Create("boosterNeo"));
            base.Add(boostData2.Get<Sprite>("sprite"));
            boostData2.Get<BloomPoint>("bloom").Alpha = 0.5f;
            //boostData.Get<ParticleType>("particleType").Color = Color.OrangeRed;
            //boostData.Get<ParticleType>("particleType").Color2 = Color.OrangeRed;

        }
        public BouncyBooster(EntityData data, Vector2 offset)
        : this(data.Position + offset, offset.X)
        {
        }

        public Vector2 floorBounce(Vector2 input)
        {
            return new Vector2(input.X, -input.Y);
        }

        public Vector2 wallBounce(Vector2 input)
        {
            return new Vector2(-input.X, input.Y);
        }
    }
}
