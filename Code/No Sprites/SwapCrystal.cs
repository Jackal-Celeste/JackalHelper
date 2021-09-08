using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.JackalHelper.Entities
{
	[Tracked]
	[CustomEntity("JackalHelper/DummyCrystal")]
	public class DummyCrystal : Actor
	{
		public static ParticleType P_Impact;

		public Vector2 Speed;

		public bool OnPedestal;

		public Holdable Hold;

		public Sprite crest;

		public Sprite main;

		private bool dead;

		private Level Level;

		private Collision onCollideH;

		private Collision onCollideV;

		private float noGravityTimer;

		private Vector2 prevLiftSpeed;

		private Vector2 previousPosition;

		private HoldableCollider hitSeeker;

		private float swatTimer;

		private bool shattering;

		private float hardVerticalHitSoundCooldown = 0f;

		private BirdTutorialGui tutorialGui;

		private float tutorialTimer = 0f;

		public float thrownY;
		public bool thrown = false;
		public bool frozen = false;
		public float dashes = 0f;

		public DummyCrystal(Vector2 position)
			: base(position)
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
			previousPosition = position;
			base.Depth = 100;
			base.Collider = new Hitbox(8f, 10f, -4f, -10f);
			Add(crest = JackalModule.spriteBank.Create("dummy_crest"));
			crest.Scale.X = -1f;
			Add(main = JackalModule.spriteBank.Create("dummy_main"));
			main.Scale.X = -1f;
			Add(Hold = new Holdable(0.1f));
			Hold.PickupCollider = new Hitbox(16f, 22f, -8f, -16f);
			Hold.SlowFall = false;
			Hold.SlowRun = false;
			Hold.OnPickup = OnPickup;
			Hold.OnRelease = OnRelease;
			Hold.DangerousCheck = Dangerous;
			Hold.OnSwat = Swat;
			Hold.OnHitSpring = HitSpring;
			Hold.SpeedGetter = () => Speed;
			onCollideH = OnCollideH;
			onCollideV = OnCollideV;
			LiftSpeedGraceTime = 0.1f;
			Add(new VertexLight(base.Collider.Center, Color.White, 1f, 32, 64));
			base.Tag = Tags.TransitionUpdate;
			Add(new MirrorReflection());
			crest.Color = Color.Red * 0.5f;
			dashes = 1f;
		}

		public DummyCrystal(EntityData e, Vector2 offset)
			: this(e.Position + offset)
		{
		}//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		 //IL_0007: Unknown result type (might be due to invalid IL or missing references)
		 //IL_0008: Unknown result type (might be due to invalid IL or missing references)


		public override void Added(Scene scene)
		{
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			base.Added(scene);
			Level = SceneAs<Level>();
			frozen = false;
			foreach (DummyCrystal entity in Level.Tracker.GetEntities<DummyCrystal>())
			{
				if (entity != this && entity.Hold.IsHeld)
				{
					RemoveSelf();
				}
			}

		}

		public override void Update()
		{
			base.Update();
			if (shattering || dead)
			{
				return;
			}
			if (swatTimer > 0f)
			{
				swatTimer -= Engine.DeltaTime;
			}
			hardVerticalHitSoundCooldown -= Engine.DeltaTime;
			if (OnPedestal)
			{
				base.Depth = 8999;
				return;
			}
			base.Depth = 100;
			if (Hold.IsHeld)
			{
				prevLiftSpeed = Vector2.Zero;
			}
			else
			{
				if (OnGround())
				{
					dashes = Math.Max(dashes, 1f);
					if (dashes == 1f)
					{
						crest.Color = Color.Red * 0.5f;
					}
					/*
					crest.Color = Color.Red * 0.5f;
					dashes = 1f;*/
					float target = ((!OnGround(Position + Vector2.UnitX * 3f)) ? 20f : (OnGround(Position - Vector2.UnitX * 3f) ? 0f : (-20f)));
					Speed.X = Calc.Approach(Speed.X, target, 800f * Engine.DeltaTime);
					Vector2 liftSpeed = base.LiftSpeed;
					if (liftSpeed == Vector2.Zero && prevLiftSpeed != Vector2.Zero)
					{
						Speed = prevLiftSpeed;
						prevLiftSpeed = Vector2.Zero;
						Speed.Y = Math.Min(Speed.Y * 0.6f, 0f);
						if (Speed.X != 0f && Speed.Y == 0f)
						{
							Speed.Y = -60f;
						}
						if (Speed.Y < 0f)
						{
							noGravityTimer = 0.15f;
						}
					}
					else
					{
						prevLiftSpeed = liftSpeed;
						if (liftSpeed.Y < 0f && Speed.Y < 0f)
						{
							Speed.Y = 0f;
						}
					}
				}
				else if (Hold.ShouldHaveGravity)
				{
					float num = 800f;
					if (Math.Abs(Speed.Y) <= 30f)
					{
						num *= 0.5f;
					}
					float num2 = 350f;
					if (Speed.Y < 0f)
					{
						num2 *= 0.5f;
					}
					Speed.X = Calc.Approach(Speed.X, 0f, num2 * Engine.DeltaTime);
					if (noGravityTimer > 0f)
					{
						noGravityTimer -= Engine.DeltaTime;
					}
					else
					{
						Speed.Y = Calc.Approach(Speed.Y, 200f, num * Engine.DeltaTime);
					}
				}
				previousPosition = base.ExactPosition;
				MoveH(Speed.X * Engine.DeltaTime, onCollideH);
				MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
				if (base.Center.X > Level.Bounds.Right)
				{
					MoveH(32f * Engine.DeltaTime);
					if (base.Left - 8f > Level.Bounds.Right)
					{
						RemoveSelf();
					}
				}
				else if (base.Left < Level.Bounds.Left)
				{
					base.Left = Level.Bounds.Left;
					Speed.X *= -0.4f;
				}
				else if (base.Top < Level.Bounds.Top - 4)
				{
					base.Top = Level.Bounds.Top + 4;
					Speed.Y = 0f;
				}
				else if (base.Bottom > Level.Bounds.Bottom && SaveData.Instance.Assists.Invincible)
				{
					base.Bottom = Level.Bounds.Bottom;
					Speed.Y = -300f;
					Audio.Play("event:/game/general/assist_screenbottom", Position);
				}
				else if (base.Top > Level.Bounds.Bottom)
				{
					Die();
				}
				if (base.X < Level.Bounds.Left + 10)
				{
					MoveH(32f * Engine.DeltaTime);
				}
				Player entity = base.Scene.Tracker.GetEntity<Player>();

			}
			if (!dead)
			{
				Hold.CheckAgainstColliders();
			}
			if (hitSeeker != null && swatTimer <= 0f && !hitSeeker.Check(Hold))
			{
				hitSeeker = null;
			}

			if (JackalModule.GetPlayer() != null)
			{
				if (Input.Grab.Pressed && (JackalModule.GetPlayer().Position - Position).Length() > 32f)
				{
					Collidable = false;
					Die(JackalModule.GetPlayer().Position, out Vector2 p);
					JackalModule.GetPlayer().Position = p;

				}
				else
				{
					Collidable = true;
				}
			}

		}


		public void ExplodeLaunch(Vector2 from)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			if (!Hold.IsHeld)
			{
				Speed = (base.Center - from).SafeNormalize(120f);
				SlashFx.Burst(base.Center, Speed.Angle());
			}
		}

		public void Swat(HoldableCollider hc, int dir)
		{
			if (Hold.IsHeld && hitSeeker == null)
			{
				swatTimer = 0.1f;
				hitSeeker = hc;
				Hold.Holder.Swat(dir);
			}
		}

		public bool Dangerous(HoldableCollider holdableCollider)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			return !Hold.IsHeld && Speed != Vector2.Zero && hitSeeker != holdableCollider;
		}





		public bool HitSpring(Spring spring)
		{
			if (!Hold.IsHeld)
			{
				if (spring.Orientation == Spring.Orientations.Floor && Speed.Y >= 0f)
				{
					Speed.X *= 0.5f;
					Speed.Y = -160f;
					noGravityTimer = 0.15f;
					return true;
				}
				if (spring.Orientation == Spring.Orientations.WallLeft && Speed.X <= 0f)
				{
					MoveTowardsY(spring.CenterY + 5f, 4f);
					Speed.X = 220f;
					Speed.Y = -80f;
					noGravityTimer = 0.1f;
					return true;
				}
				if (spring.Orientation == Spring.Orientations.WallRight && Speed.X >= 0f)
				{
					MoveTowardsY(spring.CenterY + 5f, 4f);
					Speed.X = -220f;
					Speed.Y = -80f;
					noGravityTimer = 0.1f;
					return true;
				}
			}
			return false;
		}

		private void OnCollideH(CollisionData data)
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			if (data.Hit is DashSwitch)
			{
				(data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
			}
			Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_side", Position);
			Speed.X *= -0.4f;
		}

		private void OnCollideV(CollisionData data)
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			if (data.Hit is DashSwitch)
			{
				(data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitY * Math.Sign(Speed.Y));
			}
			if (Speed.Y > 0f)
			{
				if (hardVerticalHitSoundCooldown <= 0f)
				{
					Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_ground", Position, "crystal_velocity", Calc.ClampedMap(Speed.Y, 0f, 200f));
					hardVerticalHitSoundCooldown = 0.5f;
				}
				else
				{
					Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_ground", Position, "crystal_velocity", 0f);
				}
			}

			if (Speed.Y > 140f && !(data.Hit is SwapBlock) && !(data.Hit is DashSwitch))
			{
				Speed.Y *= -0.6f;
			}
			else
			{
				Speed.Y = 0f;
			}
		}



		public override bool IsRiding(Solid solid)
		{
			return Speed.Y == 0f && base.IsRiding(solid);
		}

		protected override void OnSquish(CollisionData data)
		{
			if (!TrySquishWiggle(data) && !SaveData.Instance.Assists.Invincible)
			{
				Die();
			}
		}

		private void OnPickup()
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			thrown = false;
			frozen = false;
			Speed = Vector2.Zero;
			AddTag(Tags.Persistent);
		}

		private void OnRelease(Vector2 force)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			thrown = true;
			thrownY = Y;
			RemoveTag(Tags.Persistent);
			if (force.X != 0f && force.Y == 0f)
			{
				force.Y = -0.4f;
			}
			Speed = force * 250f;
			if (Speed != Vector2.Zero)
			{
				noGravityTimer = 0.1f;
			}
		}

		public void Die()
		{
			if (!dead)
			{
				dead = true;
				Add(new DeathEffect(Color.Purple, base.Center - Position));
				crest.Visible = false;
				base.Depth = -1000000;
				AllowPushing = false;
			}
		}

		public override void Render()
		{
			if (JackalModule.GetPlayer() != null && thrown && !dead)
			{
				//Draw.Line(Position - 8*Vector2.UnitY, JackalModule.GetPlayer().Center, Color.Violet * 0.8f);
			}
			base.Render();
		}

		public void Die(Vector2 goal, out Vector2 pos)
		{
			Collidable = false;

			pos = Position;
			Position = goal;
			dashes = crest.Color == Color.Blue ? 0f : (crest.Color == Color.Red * 0.5f ? 1f : 2f);
			int temp = JackalModule.GetPlayer().Dashes;
			JackalModule.GetPlayer().Dashes = (int)dashes;
			dashes = temp;
			crest.Color = dashes == 0f ? Color.Blue : (dashes == 1f ? Color.Red * 0.5f : Color.HotPink);

			//Die();
		}
	}
}