// Celeste.Booster
using System;
using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.JackalCollabHelper.Entities
{
	[CustomEntity("JackalCollabHelper/FlagBooster")]
	[Tracked]
	public class FlagBooster : Booster
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

		public FlagBooster(Vector2 position, bool neo)
			: base(position, true)
		{
			scene = this.Scene;
			Player player = TwigModule.GetPlayer();
			boostData = new DynData<Booster>(this);
			playerData = new DynData<Player>(player);
			boostData.Get<Sprite>("sprite").Visible = false;
			boostData.Set<Sprite>("sprite", neo ? TwigModule.spriteBank.Create("boosterNeo") : TwigModule.spriteBank.Create("boosterBase"));
			base.Add(boostData.Get<Sprite>("sprite"));
			boostData.Get<BloomPoint>("bloom").Alpha = 0.5f;
			Color color = rainbowCycle();
			boostData.Get<Sprite>("sprite").Color = color;
			color1 = boostData.Get<ParticleType>("particleType").Color;
			color2 = boostData.Get<ParticleType>("particleType").Color2;
			color3 = boostData.Get<ParticleType>("P_RedAppear").Color;
			boostData.Get<ParticleType>("particleType").Color = color;
			boostData.Get<ParticleType>("particleType").Color2 = color;
			boostData.Get<ParticleType>("P_RedAppear").Color = color;

		}


		public FlagBooster(EntityData data, Vector2 offset)
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
			boostData.Get<ParticleType>("P_RedAppear").Color = color3;
			base.Removed(scene);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
			//timeForColors();


		}

		public void timeForColors()
        {
			boostData.Get<Sprite>("sprite").Color = rainbowCycle();
			boostData.Get<ParticleType>("particleType").Color = rainbowCycle();
			boostData.Get<ParticleType>("particleType").Color2 = rainbowCycle();
			boostData.Get<ParticleType>("P_RedAppear").Color = rainbowCycle();
		}


        public override void Update()
		{
			base.Update();
			Player player = TwigModule.GetPlayer();
			totalTime += (Engine.DeltaTime * 1.25f);
			Color color = rainbowCycle();
			boostData.Get<Sprite>("sprite").Color = color;
			boostData.Get<ParticleType>("particleType").Color = color;
			boostData.Get<ParticleType>("particleType").Color2 = color;
			boostData.Get<ParticleType>("P_RedAppear").Color = color;

		}


		public Color rainbowCycle()
		{
			seed = (float)Calc.Random.NextDouble() * 6;
			float time = (float)(totalTime % 6);
			int timeInt = (int)time;
			switch (timeInt)
			{
				case 1:
					color = Color.Lerp(Color.Red, Color.Orange, time % 1f);
					break;
				case 2:
					color = Color.Lerp(Color.Orange, Color.Yellow, time % 1f);
					break;
				case 3:
					color = Color.Lerp(Color.Yellow, Color.Green, time % 1f);
					break;
				case 4:
					color = Color.Lerp(Color.Green, Color.Blue, time % 1f);
					break;
				case 5:
					color = Color.Lerp(Color.Blue, Color.Purple, time % 1f);
					break;
				default:
					color = Color.Lerp(Color.Purple, Color.Red, time % 1f);
					break;
			}
			return color * 0.8f;
		}


		public Color voreCycle()
        {
			Color color = Color.White;
			float time = (float)(totalTime % 6);
			int timeInt = (int)time;

			switch (timeInt)
			{
				case 1:
					color = Color.Lerp(Color.Cyan, Color.DarkBlue, time % 1f);
					break;
				case 2:
					color = Color.Lerp(Color.DarkBlue, Color.Indigo, time % 1f);
					break;
				case 3:
					color = Color.Lerp(Color.Indigo, Color.Cyan, time % 1f);
					break;
				case 4:
					color = Color.Lerp(Color.Cyan, Color.DarkBlue, time % 1f);
					break;
				case 5:
					color = Color.Lerp(Color.DarkBlue, Color.Indigo, time % 1f);
					break;
				default:
					color = Color.Lerp(Color.Indigo, Color.Cyan, time % 1f);
					break;

			}

			return color;
        }


		public Color spaceCycle(Color hue)
        {
			Player player = TwigModule.GetPlayer();
			if (player != null)
			{
				float distance = Vector2.DistanceSquared(player.Position, this.Position) / (float)Math.Pow(range, 2);
				if (distance > 1)
				{
					distance = 1;
				}
                if (TwigModule.Session.spaceBoosterFlight)
                {
					distance = 0;
                }
				distance = (float)Math.Pow(distance, 3);
				Color shade = Color.Lerp(hue, Color.LightSlateGray, distance);
				return shade;
			}
			return Color.LightSlateGray;
        }
		
	}
}