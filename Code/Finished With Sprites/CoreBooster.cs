using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.JackalHelper.Entities
{
	public class CoreBooster : Booster
	{
		public bool hasLaunched;

		public Sprite sprite;

		public bool canChange;

		private DynData<Booster> boostData;

		public CoreBooster(Vector2 position, bool red, bool canChange) : base(position, red)
		{
			this.canChange = canChange;
			boostData = new DynData<Booster>(this);
			base.Remove(boostData.Get<Sprite>("sprite"));
			boostData.Set<Sprite>("sprite", JackalModule.spriteBank.Create(red ? "coreBoosterHot" : "coreBoosterCold"));
			base.Add(boostData.Get<Sprite>("sprite"));
			boostData.Get<BloomPoint>("bloom").Alpha = 0.5f;
			boostData.Get<ParticleType>("particleType").Color = red ? Color.Maroon : Color.Teal;
			boostData.Get<ParticleType>("particleType").Color2 = red ? Color.Maroon : Color.Teal;
		}





	}
}
