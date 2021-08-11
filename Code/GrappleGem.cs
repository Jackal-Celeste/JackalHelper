// Celeste.GrappleGem
using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.JackalHelper.Entities
{
	[Tracked]
	[CustomEntity("JackalHelper/GrappleGem")]
	public class GrappleGem : Entity
	{
		public static ParticleType P_GoldShine;


		public const float GhostAlpha = 0.8f;


		private Sprite sprite;

		private Sprite white;

		private ParticleType shineParticle;

		public Wiggler ScaleWiggler;

		private Wiggler moveWiggler;

		private Vector2 moveWiggleDir;

		private BloomPoint bloom;

		private VertexLight light;

		private CustomPoem poem;

		private CustomPoem poem2;

		private float timer;

		private bool autoPulse;

		private float bounceSfxDelay;

		private SoundEmitter sfx;

		private List<InvisibleBarrier> walls;

		private EntityID entityID;

		public bool collected;

		private DynData<Poem> dynData;

		public GrapplingHook hook;

		public GrappleGem(Vector2 position) : base(position)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			if (JackalModule.Session.hasGrapple)
			{
				Visible = false;
				Collidable = false;
				RemoveSelf();
			}
			autoPulse = true;
			walls = new List<InvisibleBarrier>();

			Add(new MirrorReflection());
		}

		public override void Awake(Scene scene)
		{
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_0200: Unknown result type (might be due to invalid IL or missing references)
			//IL_0205: Unknown result type (might be due to invalid IL or missing references)
			//IL_0213: Unknown result type (might be due to invalid IL or missing references)
			//IL_0215: Unknown result type (might be due to invalid IL or missing references)
			//IL_021f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0224: Unknown result type (might be due to invalid IL or missing references)
			//IL_0228: Unknown result type (might be due to invalid IL or missing references)
			//IL_0339: Unknown result type (might be due to invalid IL or missing references)
			base.Awake(scene);
			Level level = base.Scene as Level;
			AreaKey area = level.Session.Area;

			string id = "GrappleGem";
			Add(sprite = JackalModule.spriteBank.Create(id));
			sprite.Play("spin");
			sprite.OnLoop = delegate (string anim)
			{
				//IL_0034: Unknown result type (might be due to invalid IL or missing references)
				//IL_0047: Unknown result type (might be due to invalid IL or missing references)
				//IL_006f: Unknown result type (might be due to invalid IL or missing references)
				if (Visible && anim == "spin" && autoPulse)
				{
					Audio.Play("event:/new_content/game/10_farewell/fakeheart_pulse", Position);

					ScaleWiggler.Start();
					(base.Scene as Level).Displacement.AddBurst(Position, 0.35f, 8f, 48f, 0.25f);
				}
			};
			base.Collider = new Hitbox(16f, 16f, -8f, -8f);
			Add(new PlayerCollider(OnPlayer));
			Add(ScaleWiggler = Wiggler.Create(0.5f, 4f, delegate (float f)
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_001d: Unknown result type (might be due to invalid IL or missing references)
				sprite.Scale = Vector2.One * (1f + f * 0.25f);
			}));
			Add(bloom = new BloomPoint(0.75f, 16f));
			Color val;
			val = Calc.HexToColor("dad8cc");
			shineParticle = P_GoldShine;

			val = Color.Lerp(val, Color.White, 0.5f);
			Add(light = new VertexLight(val, 1f, 32, 64));
			moveWiggler = Wiggler.Create(0.8f, 2f);
			moveWiggler.StartZero = true;
			Add(moveWiggler);
			/*
			if (!IsFake)
			{
				return;
			}
			Player entity = base.Scene.Tracker.GetEntity<Player>();
			if ((entity != null && entity.X > base.X) || (scene as Level).Session.GetFlag("fake_heart"))
			{
				Visible = false;
				Alarm.Set(this, 0.0001f, delegate
				{

					RemoveSelf();
				});
			}*/
		}

		public override void Update()
		{
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_017a: Unknown result type (might be due to invalid IL or missing references)
			//IL_017f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0189: Unknown result type (might be due to invalid IL or missing references)
			bounceSfxDelay -= Engine.DeltaTime;
			timer += Engine.DeltaTime;
			sprite.Position = Vector2.UnitY * (float)Math.Sin(timer * 2f) * 2f + moveWiggleDir * moveWiggler.Value * -8f;
			if (white != null)
			{
				white.Position = sprite.Position;
				white.Scale = sprite.Scale;
				if (white.CurrentAnimationID != sprite.CurrentAnimationID)
				{
					white.Play(sprite.CurrentAnimationID);
				}
				white.SetAnimationFrame(sprite.CurrentAnimationFrame);
			}
			if (collected && (base.Scene.Tracker.GetEntity<Player>()?.Dead ?? true))
			{
				EndCutscene();
			}
			base.Update();
			if (!collected && base.Scene.OnInterval(0.1f))
			{
				//SceneAs<Level>().Particles.Emit(shineParticle, 1, base.Center, Vector2.One * 8f);
			}
		}



		public void OnPlayer(Player player)
		{
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			if (collected || (base.Scene as Level).Frozen)
			{
				return;
			}
			if (player.DashAttacking)
			{
				Collect(player);
				return;
			}
			if (bounceSfxDelay <= 0f)
			{

				Audio.Play("event:/game/general/crystalheart_bounce", Position);

				bounceSfxDelay = 0.1f;
			}
			player.PointBounce(base.Center);
			moveWiggler.Start();
			ScaleWiggler.Start();
			moveWiggleDir = (base.Center - player.Center).SafeNormalize(Vector2.UnitY);
			Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
		}

		private void Collect(Player player)
		{
			base.Scene.Tracker.GetEntity<AngryOshiro>()?.StopControllingTime();
			Coroutine coroutine = new Coroutine(CollectRoutine(player));
			coroutine.UseRawDeltaTime = true;
			Add(coroutine);
			collected = true;

		}

		private IEnumerator CollectRoutine(Player player)
		{

			return orig_CollectRoutine(player);
		}

		private void EndCutscene()
		{
			Level level = base.Scene as Level;
			level.Frozen = false;
			level.CanRetry = true;
			level.FormationBackdrop.Display = false;
			Engine.TimeRate = 1f;
			if (poem != null)
			{
				poem.RemoveSelf();
			}
			if (poem2 != null)
			{
				poem2.RemoveSelf();
			}
			foreach (InvisibleBarrier wall in walls)
			{
				wall.RemoveSelf();
			}
			RemoveSelf();
		}



		private IEnumerator PlayerStepForward()
		{
			yield return 0.1f;
			Player player = Scene.Tracker.GetEntity<Player>();
			if (player?.CollideCheck<Solid>(player.Position + new Vector2(12f, 1f)) ?? false)
			{
				yield return player.DummyWalkToExact((int)player.X + 10);
			}
			yield return 0.2f;
		}





		public GrappleGem(EntityData data, Vector2 offset) : this(data.Position + offset)
		{

			entityID = new EntityID(data.Level.Name, data.ID);
		}

		private IEnumerator orig_CollectRoutine(Player player)
		{
			Level level = Scene as Level;
			AreaKey area = level.Session.Area;
			string poemID = AreaData.Get(level).Mode[(int)area.Mode].PoemID;

			level.CanRetry = false;


			string sfxEvent = "event:/game/general/crystalheart_blue_get";


			sfx = SoundEmitter.Play(sfxEvent, this);
			Add(new LevelEndingHook(delegate
			{
				sfx.Source.Stop();
			}));
			List<InvisibleBarrier> list = walls;
			Rectangle bounds = level.Bounds;
			float num = bounds.Right;
			bounds = level.Bounds;
			list.Add(new InvisibleBarrier(new Vector2(num, bounds.Top), 8f, level.Bounds.Height));
			List<InvisibleBarrier> list2 = walls;
			bounds = level.Bounds;
			float num2 = bounds.Left - 8;
			bounds = level.Bounds;
			list2.Add(new InvisibleBarrier(new Vector2(num2, bounds.Top), 8f, level.Bounds.Height));
			List<InvisibleBarrier> list3 = walls;
			bounds = level.Bounds;
			float num3 = bounds.Left;
			bounds = level.Bounds;
			list3.Add(new InvisibleBarrier(new Vector2(num3, bounds.Top - 8), level.Bounds.Width, 8f));
			foreach (InvisibleBarrier wall in walls)
			{
				Scene.Add(wall);
			}
			Add(white = JackalModule.spriteBank.Create("GrappleGem"));
			Depth = -2000000;
			yield return null;
			Celeste.Freeze(0.2f);
			yield return null;
			Engine.TimeRate = 0.5f;
			player.Depth = -2000000;
			for (int i = 0; i < 10; i++)
			{
				Scene.Add(new AbsorbOrb(Position));
			}
			level.Shake();
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
			level.Flash(Color.White);
			level.FormationBackdrop.Display = true;
			level.FormationBackdrop.Alpha = 1f;
			light.Alpha = (bloom.Alpha = 0f);
			Visible = false;
			for (float t3 = 0f; t3 < 2f; t3 += Engine.RawDeltaTime)
			{
				Engine.TimeRate = Calc.Approach(Engine.TimeRate, 0f, Engine.RawDeltaTime * 0.25f);
				yield return null;
			}
			yield return null;
			if (player.Dead)
			{
				yield return 100f;
			}
			Engine.TimeRate = 1f;
			Tag = Tags.FrozenUpdate;
			level.Frozen = true;


			poem = new CustomPoem("You Got a Grappling Hook!", 3, 0f);
			//dynData = new DynData<Poem>(poem);
			poem.Alpha = 0f;
			poem.Offset -= Vector2.UnitY * 360f;
			poem.Heart.Visible = false;
			poem.ParticleSpeed = 0.25f;
			poem.Color = Calc.HexToColor("00FFFF");
			//dynData.Set<Color>("Color", Calc.HexToColor("FFA500"));
			//dynData.Set<Particle[]>("particles", new Particle[1]);

			poem2 = new CustomPoem("Press Grab to Launch It!", 3, 0f);
			//dynData = new DynData<Poem>(poem2);
			poem2.Alpha = 0f;
			poem2.Offset += Vector2.UnitY * 360f;
			poem2.Heart.Visible = false;
			poem2.ParticleSpeed = 0.25f;
			poem2.Color = Calc.HexToColor("00FFFF");
			//dynData.Set<Color>("Color", Calc.HexToColor("FFA500"));
			//dynData.Set<Particle[]>("particles", new Poem.Particle[1]);



			Scene.Add(poem);
			Scene.Add(poem2);
			//poem.Position.Y -= 40f;
			poem.Heart.Visible = false;
			poem2.Heart.Visible = false;
			for (float t2 = 0f; t2 < 1f; t2 += Engine.RawDeltaTime)
			{
				poem.Alpha = Ease.CubeOut(t2);
				poem2.Alpha = Ease.CubeOut(t2);
				yield return null;
			}
			while (!Input.MenuConfirm.Pressed && !Input.MenuCancel.Pressed)
			{
				yield return null;
			}
			sfx.Source.Param("end", 1f);
			level.FormationBackdrop.Display = false;
			for (float t = 0f; t < 1f; t += Engine.RawDeltaTime * 2f)
			{
				poem.Alpha = Ease.CubeIn(1f - t);
				poem2.Alpha = Ease.CubeIn(1f - t);
				yield return null;
			}
			player.Depth = 0;
			JackalModule.Session.hasGrapple = true;

			EndCutscene();
		}
	}
}

namespace Celeste.Mod.JackalHelper.Entities
{
	public class CustomPoem : Entity
	{
		private struct Particle
		{
			public Vector2 Direction;

			public float Percent;

			public float Duration;

			public void Reset(float percent)
			{
				Direction = Calc.AngleToVector(Calc.Random.NextFloat((float)Math.PI * 2f), 1f);
				Percent = percent;
				Duration = 0.5f + Calc.Random.NextFloat() * 0.5f;
			}
		}

		private const float textScale = 1.5f;

		private static readonly Color[] colors = new Color[4]
		{
		Calc.HexToColor("8cc7fa"),
		Calc.HexToColor("ff668a"),
		Calc.HexToColor("fffc24"),
		Calc.HexToColor("ffffff")
		};

		public float Alpha = 1f;

		public float TextAlpha = 1f;

		public Vector2 Offset;

		public Sprite Heart;

		public float ParticleSpeed = 1f;

		public float Shake = 0f;

		private float timer = 0f;

		private string text;

		private bool disposed;

		private VirtualRenderTarget poem;

		private VirtualRenderTarget smoke;

		private VirtualRenderTarget temp;



		public Color Color
		{
			get;
			set;
		}

		public CustomPoem(string text, int heartIndex, float heartAlpha)
		{
			if (text != null)
			{
				this.text = ActiveFont.FontSize.AutoNewline(text, 1024);
			}
			Color = colors[heartIndex];
			Heart = GFX.GuiSpriteBank.Create("heartgem" + heartIndex);
			Heart.Play("spin");
			Heart.Position = new Vector2(1920f, 1080f) * 0.5f;
			Heart.Color = Color.White * heartAlpha;
			int num = Math.Min(1920, Engine.ViewWidth);
			int num2 = Math.Min(1080, Engine.ViewHeight);
			poem = VirtualContent.CreateRenderTarget("poem-a", num, num2);
			smoke = VirtualContent.CreateRenderTarget("poem-b", num / 2, num2 / 2);
			temp = VirtualContent.CreateRenderTarget("poem-c", num / 2, num2 / 2);
			base.Tag = Tags.HUD | Tags.FrozenUpdate;
			Add(new BeforeRenderHook(BeforeRender));
		}

		public void Dispose()
		{
			if (!disposed)
			{
				poem.Dispose();
				smoke.Dispose();
				temp.Dispose();
				RemoveSelf();
				disposed = true;
			}
		}

		private void DrawPoem(Vector2 offset, Color color)
		{
			MTexture mTexture = GFX.Gui["poemside"];
			float num = ActiveFont.Measure(text).X * 1.5f;
			Vector2 vector = new Vector2(960f, 540f) + offset;
			//mTexture.DrawCentered(vector - Vector2.UnitX * (num / 2f + 64f), color);
			ActiveFont.Draw(text, vector, new Vector2(0.5f, 0.5f), Vector2.One * 1.5f, color);
			//mTexture.DrawCentered(vector + Vector2.UnitX * (num / 2f + 64f), color);
		}

		public override void Update()
		{
			timer += Engine.DeltaTime;
			Heart.Update();
		}

		public void BeforeRender()
		{
			if (!disposed)
			{
				Engine.Graphics.GraphicsDevice.SetRenderTarget(poem);
				Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
				Matrix transformationMatrix = Matrix.CreateScale(poem.Width / 1920f);
				Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, transformationMatrix);
				Heart.Position = Offset + new Vector2(1920f, 1080f) * 0.5f;
				Heart.Scale = Vector2.One * (1f + Shake * 0.1f);
				MTexture mTexture = OVR.Atlas["snow"];
				Heart.Position += new Vector2(Calc.Random.Range(-1f, 1f), Calc.Random.Range(-1f, 1f)) * 16f * Shake;
				Heart.Render();
				if (!string.IsNullOrEmpty(text))
				{
					DrawPoem(Offset + new Vector2(-2f, 0f), Color.Black * TextAlpha);
					DrawPoem(Offset + new Vector2(2f, 0f), Color.Black * TextAlpha);
					DrawPoem(Offset + new Vector2(0f, -2f), Color.Black * TextAlpha);
					DrawPoem(Offset + new Vector2(0f, 2f), Color.Black * TextAlpha);
					DrawPoem(Offset + Vector2.Zero, Color * TextAlpha);
				}
				Draw.SpriteBatch.End();
				Engine.Graphics.GraphicsDevice.SetRenderTarget(smoke);
				Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
				MagicGlow.Render(poem, timer, -1f, Matrix.CreateScale(0.5f));
				GaussianBlur.Blur(smoke, temp, smoke);
			}
		}

		public override void Render()
		{
			if (!disposed && !base.Scene.Paused)
			{
				float num = 1920f / poem.Width;
				Draw.SpriteBatch.Draw(smoke, Vector2.Zero, smoke.Bounds, Color.White * 0.3f * Alpha, 0f, Vector2.Zero, num * 2f, SpriteEffects.None, 0f);
				Draw.SpriteBatch.Draw(poem, Vector2.Zero, poem.Bounds, Color.White * Alpha, 0f, Vector2.Zero, num, SpriteEffects.None, 0f);
			}
		}

		public override void Removed(Scene scene)
		{
			base.Removed(scene);
			Dispose();
		}

		public override void SceneEnd(Scene scene)
		{
			base.SceneEnd(scene);
			Dispose();
		}
	}
}