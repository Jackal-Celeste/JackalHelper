using System;
using System.Collections;
using System.Diagnostics;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.JackalHelper.Entities
{
	[Tracked]
	[CustomEntity("Lunaris/LunarisCutscene")]
	public class LunarisCutscene : Entity
	{
		public LunarHeart decoy;
		public float zoom = 1;
		public BadelineBoostDown yeet;
		public Vector2[] positions = new Vector2[1];

		private Vector2 previousDiff;
		public LunarisCutscene(Vector2 position) : base(position)
		{
		}

		public LunarisCutscene(EntityData data, Vector2 offset) : this(data.Position + offset)
		{

		}
		public override void Awake(Scene scene)
		{
			Scene.Add(decoy = new LunarHeart(Position));
			base.Awake(scene);
			positions[0] = Position - (Vector2.UnitY * 200f);
			Scene.Add(yeet = new BadelineBoostDown(positions));
			yeet.Visible = false;
		}

		public override void Update()
		{
			base.Update();
			decoy.Position = Position;


			if (GetPlayer() != null && GetLevel() != null)
			{
				if ((GetPlayer().Position - Position).Length() < 100f)
				{
					float distance = (float)Math.Pow((GetPlayer().Position - Position).Length(), 0.5);
					yeet.Position.Y = Position.Y + 2f;
				}
				else
				{

					zoom = 1f;
					// GetLevel().Camera.Zoom = 1f;
				}
				yeet.Collidable = (GetPlayer().StateMachine.State == 2 && decoy.broken);
				if ((GetPlayer().Position - Position).Length() < 8f && GetPlayer().StateMachine.State == 2)
				{
					decoy.RemoveSelf();
				}
				if (GetPlayer().StateMachine.State == 2)
				{
					if (!yeet.boostin)
					{
						if ((GetPlayer().Position - Position).Length() < 20f)
						{
							Engine.TimeRate = 0.4f;

							//Add(new Coroutine(GetLevel().ZoomTo(yeet.centerLoc, 1.5f, 0.18f)));

							float length = 52f - (GetPlayer().Position - Position).Length();
							//GetLevel().Camera.Zoom = 1 + (0.3f) / (1 + (float)Math.Pow(Math.E, 3.25f - (length / 8f)));

						}
						else if ((GetPlayer().Position - Position).Length() < 36f)
						{
							Engine.TimeRate = 0.6f;
							float length = 52f - (GetPlayer().Position - Position).Length();
							//GetLevel().Camera.Zoom = 1 + (0.3f) / (1 + (float)Math.Pow(Math.E, 3.25f - (length / 8f)));
						}
						else if ((GetPlayer().Position - Position).Length() < 52f)
						{
							Engine.TimeRate = 0.8f;
							//GetLevel().Camera.Zoom = 1.125f;
							float length = 52f - (GetPlayer().Position - Position).Length();
							//GetLevel().Camera.Zoom = 1 + (0.3f) / (1 + (float)Math.Pow(Math.E, 3.25f - (length / 8f)));
						}
						else
						{
							Engine.TimeRate = 1f;
							//GetLevel().Camera.Zoom = 1f;
						}
					}
					else
					{
						Engine.TimeRate = 1f;
						//GetLevel().Camera.Zoom = 1f;
					}
				}
				else
				{
					Engine.TimeRate = 1f;
				}


			}

		}

		public static Level GetLevel()
		{
			try
			{
				return (Engine.Scene as Level);
			}
			catch (NullReferenceException)
			{
				return null;
			}
		}
		public static Player GetPlayer()
		{
			try
			{
				return (Engine.Scene as Level).Tracker.GetEntity<Player>();
			}
			catch (NullReferenceException)
			{
				return null;
			}
		}
	}
}

namespace Celeste.Mod.JackalHelper.Entities
{
	public class BadelineBoostDown : Entity
	{
		private const float MoveSpeed = 320f;

		private readonly Vector2[] nodes;

		private readonly SoundSource relocateSfx;

		private readonly Sprite sprite;

		private readonly Image stretch;

		private readonly Wiggler wiggler;

		private Player holding;

		private int nodeIndex;

		private bool travelling;

		public Vector2 centerLoc;

		public bool boostin = false;

		public BadelineBoostDown(Vector2[] nodes)
			: base(nodes[0])
		{
			base.Depth = -1000000;
			this.nodes = nodes;
			base.Collider = new Circle(16f);
			Add(new PlayerCollider(OnPlayer));
			Add(sprite = GFX.SpriteBank.Create("badelineBoost"));
			Add(stretch = new Image(GFX.Game["objects/badelineboost/stretch"]));
			stretch.Visible = false;
			stretch.CenterOrigin();
			Add(wiggler = Wiggler.Create(0.4f, 3f, delegate
			{
				sprite.Scale = Vector2.One * (float)(1.0 + wiggler.Value * 0.400000005960464);
			}));
			Add(relocateSfx = new SoundSource());
		}

		public BadelineBoostDown(EntityData data, Vector2 offset)
			: this(data.NodesWithPosition(offset))
		{
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			if (CollideCheck<FakeWall>())
			{
				base.Depth = -12500;
			}
		}

		private void OnPlayer(Player player)
		{
			Add(new Coroutine(BoostRoutine(player)));
		}

		private IEnumerator BoostRoutine(Player player)
		{
			holding = player;
			travelling = true;
			nodeIndex++;
			sprite.Visible = false;
			sprite.Position = Vector2.Zero;
			Collidable = false;
			bool finalBoost = nodeIndex >= nodes.Length;
			Level level = SceneAs<Level>();
			Stopwatch sw = new Stopwatch();
			sw.Start();
			if (!finalBoost)
			{
				Audio.Play("event:/char/badeline/booster_begin", Position);
			}
			else
			{
				Audio.Play("event:/char/badeline/booster_final", Position);
			}
			if (player.Holding != null)
			{
				player.Drop();
			}
			player.StateMachine.State = 11;
			player.DummyAutoAnimate = false;
			player.DummyGravity = false;
			if (player.Inventory.Dashes > 1)
			{
				player.Dashes = 1;
			}
			else
			{
				player.RefillDash();
			}
			player.RefillStamina();
			player.Speed = Vector2.Zero;
			int side = Math.Sign(player.X - base.X);
			if (side == 0)
			{
				side = -1;
			}
			BadelineDummy badeline = new BadelineDummy(Position);
			base.Scene.Add(badeline);
			if (side == -1)
			{
				player.Facing = Facings.Right;
			}
			else
			{
				player.Facing = Facings.Left;
			}
			badeline.Sprite.Scale.X = side;
			Vector2 playerFrom = player.Position;
			Vector2 playerTo = Position + new Vector2(side * 4, -3f);
			Vector2 badelineFrom = badeline.Position;
			Vector2 badelineTo = Position + new Vector2(-side * 4, 3f);
			for (float p = 0f; p < 1.0; p += Engine.DeltaTime / 0.2f)
			{
				Vector2 target = Vector2.Lerp(playerFrom, playerTo, p);
				if (player.Scene != null)
				{
					player.MoveToX(target.X);
				}
				if (player.Scene != null)
				{
					player.MoveToY(target.Y);
				}
				badeline.Position = Vector2.Lerp(badelineFrom, badelineTo, p);
				yield return null;
			}
			if (finalBoost)
			{
				Vector2 center = new Vector2(Calc.Clamp(player.X - level.Camera.X, 120f, 200f), Calc.Clamp(player.Y - level.Camera.Y, 60f, 120f));
				centerLoc = center;
				//JackalModule.GetLevel().Camera.Zoom = 1 + (0.3f);
				Add(new Coroutine(level.ZoomTo(center, 1.5f, 0.18f)));
				boostin = true;
				Engine.TimeRate = 0.3f;
			}
			else
			{
				Audio.Play("event:/char/badeline/booster_throw", Position);
			}
			badeline.Sprite.Play("boost");
			yield return 0.1f;
			if (!player.Dead)
			{
				player.MoveV(5f);
			}
			yield return 0.1f;
			Add(Alarm.Create(Alarm.AlarmMode.Oneshot, delegate
			{
				if (player.Dashes < player.Inventory.Dashes)
				{
					player.Dashes++;
				}
				base.Scene.Remove(badeline);
				(base.Scene as Level)?.Displacement.AddBurst(badeline.Position, 0.25f, 8f, 32f, 0.5f);
			}, 0.15f, start: true));
			(base.Scene as Level)?.Shake();
			holding = null;
			if (!finalBoost)
			{
				player.BadelineBoostLaunch(base.CenterX);
				Vector2 from = Position;
				Vector2 to = nodes[nodeIndex];
				float time = Vector2.Distance(from, to) / 320f;
				time = Math.Min(3f, time);
				stretch.Visible = true;
				stretch.Rotation = (to - from).Angle();
				Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, time, start: true);
				tween.OnUpdate = delegate (Tween t)
				{
					Position = Vector2.Lerp(from, to, t.Eased);
					stretch.Scale.X = (float)(1.0 + Calc.YoYo(t.Eased) * 2.0);
					stretch.Scale.Y = (float)(1.0 - Calc.YoYo(t.Eased) * 0.75);
					if (!(t.Eased >= 0.899999976158142) && base.Scene.OnInterval(0.03f))
					{
						TrailManager.Add(this, Player.TwoDashesHairColor, 0.5f, frozenUpdate: false, useRawDeltaTime: false);

					}
				};
				tween.OnComplete = delegate
				{
					if (level != null && X >= (double)level.Bounds.Right)
					{
						RemoveSelf();
					}
					else
					{
						travelling = false;
						stretch.Visible = false;
						sprite.Visible = true;
						Collidable = true;
						Audio.Play("event:/char/badeline/booster_reappear", Position);
					}
				};
				Add(tween);
				relocateSfx.Play("event:/char/badeline/booster_relocate");
				Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
				level.DirectionalShake(-Vector2.UnitY);
				level.Displacement.AddBurst(base.Center, 0.4f, 8f, 32f, 0.5f);
			}
			else
			{
				Console.WriteLine("TIME: " + sw.ElapsedMilliseconds);
				Engine.FreezeTimer = 0.1f;
				yield return null;
				Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
				level.Flash(Color.White * 0.5f, drawPlayerOver: true);
				level.DirectionalShake(Vector2.UnitY, 0.6f);
				level.Displacement.AddBurst(base.Center, 0.6f, 8f, 64f, 0.5f);
				level.ResetZoom();
				player.StateMachine.State = 18;
				Engine.TimeRate = 1f;
				Finish();
			}
		}

		public override void Update()
		{

			if (holding != null)
			{
				holding.Speed = Vector2.Zero;
			}
			if (!travelling)
			{
				Player entity = base.Scene.Tracker.GetEntity<Player>();
				if (entity != null)
				{
					float num = Calc.ClampedMap((entity.Center - Position).Length(), 16f, 64f, 12f, 0f);
					sprite.Position = Calc.Approach(sprite.Position, (entity.Center - Position).SafeNormalize() * num, 32f * Engine.DeltaTime);
				}
			}
			base.Update();
		}

		private void Finish()
		{
			SceneAs<Level>().Displacement.AddBurst(base.Center, 0.5f, 24f, 96f, 0.4f);
			SceneAs<Level>().Particles.Emit(BadelineOldsite.P_Vanish, 12, base.Center, Vector2.One * 6f);
			SceneAs<Level>().CameraLockMode = Level.CameraLockModes.None;
			SceneAs<Level>().CameraOffset = new Vector2(0f, -16f);
			RemoveSelf();
		}



	}
	public class LunarHeart : Entity
	{
		private const float RespawnTime = 3f;

		private Sprite sprite;

		private ParticleType shineParticle;

		public Wiggler ScaleWiggler;

		private Wiggler moveWiggler;

		private Vector2 moveWiggleDir;

		private BloomPoint bloom;

		private VertexLight light;

		private HoldableCollider crystalCollider;

		private float timer;

		private float bounceSfxDelay;

		private float respawnTimer;

		private AreaMode color;

		public bool broken = false;

		public LunarHeart(Vector2 position)
			: base(position)
		{
			Add(crystalCollider = new HoldableCollider(OnHoldable));
			Add(new MirrorReflection());
		}

		public LunarHeart(EntityData data, Vector2 offset)
			: this(data.Position + offset)
		{
			color = data.Enum("color", (AreaMode)(-1));
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			AreaMode areaMode = _getCustomColor(Calc.Random.Choose(AreaMode.Normal, AreaMode.BSide, AreaMode.CSide), this);
			Add(sprite = GFX.SpriteBank.Create("heartgem" + (int)areaMode));
			sprite.Play("spin");
			sprite.Color = Color.Black;
			sprite.OnLoop = delegate (string anim)
			{
				if (Visible && anim == "spin")
				{
					Audio.Play("event:/game/general/crystalheart_pulse", Position);
					ScaleWiggler.Start();
					(base.Scene as Level).Displacement.AddBurst(Position, 0.35f, 8f, 48f, 0.25f);
				}
			};
			base.Collider = new Hitbox(16f, 16f, -8f, -8f);
			Add(new PlayerCollider(OnPlayer));
			Add(ScaleWiggler = Wiggler.Create(0.5f, 4f, delegate (float f)
			{
				sprite.Scale = Vector2.One * (1f + f * 0.25f);
			}));
			Add(bloom = new BloomPoint(0.75f, 16f));
			Color value;
			switch (areaMode)
			{
				case AreaMode.Normal:
					value = Color.Black;
					shineParticle = HeartGem.P_BlueShine;
					break;
				case AreaMode.BSide:
					value = Color.Red;
					shineParticle = HeartGem.P_RedShine;
					break;
				default:
					value = Color.Gold;
					shineParticle = HeartGem.P_GoldShine;
					break;
			}
			value = Color.Lerp(value, Color.White, 0.5f);
			Add(light = new VertexLight(value, 1f, 32, 64));
			moveWiggler = Wiggler.Create(0.8f, 2f);
			moveWiggler.StartZero = true;
			Add(moveWiggler);
		}

		public override void Update()
		{
			bounceSfxDelay -= Engine.DeltaTime;
			timer += Engine.DeltaTime;
			sprite.Position = Vector2.UnitY * (float)Math.Sin(timer * 2f) * 2f + moveWiggleDir * moveWiggler.Value * -8f;
			if (respawnTimer > 0f)
			{

				if (respawnTimer <= 0f)
				{
					Collidable = (Visible = true);
					ScaleWiggler.Start();
				}
			}
			base.Update();
			if (Visible && base.Scene.OnInterval(0.1f))
			{
				SceneAs<Level>().Particles.Emit(shineParticle, 1, base.Center, Vector2.One * 8f);
			}
		}

		public void OnHoldable(Holdable h)
		{
			Player entity = base.Scene.Tracker.GetEntity<Player>();
			if (Visible && h.Dangerous(crystalCollider))
			{
				Collect(entity, h.GetSpeed().Angle());
			}
		}

		public void OnPlayer(Player player)
		{
			if (!Visible || (base.Scene as Level).Frozen)
			{
				return;
			}
			if (player.DashAttacking && player.StateMachine.State == 2)
			{
				Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
				Collect(player, player.Speed.Angle());
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

		private void Collect(Player player, float angle)
		{
			if (Collidable)
			{
				Collidable = (Visible = false);
				respawnTimer = 3f;
				Celeste.Freeze(0.05f);
				SceneAs<Level>().Shake();
				SlashFx.Burst(Position, angle);
				player?.RefillDash();
				broken = true;
				//RemoveSelf();
			}
		}

		private static AreaMode _getCustomColor(AreaMode vanillaColor, LunarHeart self)
		{
			if (self.color == (AreaMode)(-1))
			{
				return vanillaColor;
			}
			return self.color;
		}
	}
}