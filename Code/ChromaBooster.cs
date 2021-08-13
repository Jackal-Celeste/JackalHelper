// Celeste.Booster
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.JackalHelper.Entities
{
	[CustomEntity("JackalHelper/ChromaBooster")]
	[Tracked]
	public class ChromaBooster : Booster
	{

		public float totalTime = 0f;

		public static Scene scene;

		private DynData<Booster> boostData;

		private DynData<Player> playerData;

		public Color color;

		public float range = 72f;

		public bool voreMode;

		public float val;

		private Sprite sprite;

		public float seed = 69f;

		public Color color1;
		public Color color2;
		public Color color3;

		public ChromaBooster(Vector2 position, bool neo)
			: base(position, false)
		{
			scene = Scene;
			Player player = JackalModule.GetPlayer();
			boostData = new DynData<Booster>(this);
			playerData = new DynData<Player>(player);
			boostData.Get<Sprite>("sprite").Visible = false;
			boostData.Set<Sprite>("sprite", neo ? JackalModule.spriteBank.Create("boosterNeo") : JackalModule.spriteBank.Create("boosterBase"));
			base.Add(boostData.Get<Sprite>("sprite"));
			boostData.Get<BloomPoint>("bloom").Alpha = 0.5f;
			boostData.Get<Sprite>("sprite").Color = color;
			ParticleType tempParticle = boostData.Get<ParticleType>("particleType");
			color1 = tempParticle.Color;
			color2 = tempParticle.Color2;
			color3 = boostData.Get<ParticleType>("P_Appear").Color;
			boostData.Set<ParticleType>("particleType", new ParticleType(tempParticle) { Color = JackalModule.Session.color, Color2 = JackalModule.Session.color });


		}


		public ChromaBooster(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Bool("neo", defaultValue: false))
		{
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			boostData = new DynData<Booster>(this);
			scene.Remove(boostData.Get<Entity>("outline"));


		}

		public override void Removed(Scene scene)
		{
			boostData = new DynData<Booster>(this);
			boostData.Get<ParticleType>("particleType").Color = color1;
			boostData.Get<ParticleType>("particleType").Color2 = color2;
			boostData.Get<ParticleType>("P_Appear").Color = color3;
			base.Removed(scene);
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			//timeForColors();


		}



		public override void Update()
		{
			base.Update();
			boostData.Get<Sprite>("sprite").Color = JackalModule.Session.color;
			boostData.Get<ParticleType>("particleType").Color = JackalModule.Session.color;
			boostData.Get<ParticleType>("particleType").Color2 = JackalModule.Session.color;
			boostData.Get<ParticleType>("P_Appear").Color = JackalModule.Session.color;

		}



	}
}
