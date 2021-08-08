using System;
using System.Collections;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.JackalHelper.Entities
{
	[Tracked]
	[CustomEntity("JackalHelper/SombraCrystal")]
	public class GrapplingHook : Actor
	{
		public static ParticleType P_Impact;

		public Vector2 Speed;

		public Holdable Hold;

		private Sprite sprite;

		private bool dead;

		private Level Level;

		private Collision onCollideH;

		private Collision onCollideV;

		private Vector2 prevLiftSpeed;

		private Vector2 previousPosition;

		private float swatTimer;

		private bool shattering;

		public bool grappled = false;

		public float thrownY;
		public bool thrown = false;
		public bool frozen = false;

		public Vector2 moveDistance;

		public Vector2 startDistance = Vector2.Zero;
		public bool canSetPos = true;
		public GrapplingHook(Vector2 position)
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
			Add(sprite = GFX.SpriteBank.Create("theo_crystal"));
			sprite.Scale.X = -1f;
			Add(Hold = new Holdable(0.1f));
			Hold.SlowFall = false;
			Hold.SlowRun = false;
			Hold.SpeedGetter = () => Speed;
			Hold.PickupCollider = new Hitbox(0f, 0f);
			Hold.OnPickup = null;
			onCollideH = OnCollideH;
			onCollideV = OnCollideV;
			LiftSpeedGraceTime = 0.1f;
			Add(new VertexLight(base.Collider.Center, Color.White, 1f, 32, 64));
			base.Tag = Tags.TransitionUpdate;
		}

		public GrapplingHook(EntityData e, Vector2 offset)
			: this(e.Position + offset)
		{
		}


		public override void Added(Scene scene)
		{
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			base.Added(scene);
			Level = SceneAs<Level>();
			frozen = false;
			foreach (GrapplingHook entity in Level.Tracker.GetEntities<GrapplingHook>())
			{
				if (entity != this && entity.Hold.IsHeld)
				{
					RemoveSelf();
				}
			}			
				
				thrown = true;
				thrownY = Y;
				RemoveTag(Tags.Persistent);

				Vector2	newForce = new Vector2(1.5f, 0f);
				
				newForce.X *= (JackalModule.GetPlayer().Facing == Facings.Right ? 1f : -1f);
				Speed = newForce * 320f;
			
		}

		public override void Update()
		{
			if(JackalModule.GetPlayer() != null)
            {
				if((JackalModule.GetPlayer().Position - Position).Length() > 120f)
                {
					Die();
                }
            }
			if(grappled && JackalModule.GetPlayer() != null)
            {
				grappled = (JackalModule.GetPlayer().StateMachine.State == 0 || JackalModule.GetPlayer().StateMachine.State == 1 || JackalModule.GetPlayer().StateMachine.State == 2);
            }
			if (!grappled)
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
				base.Depth = 100;
				if (Hold.IsHeld)
				{
					prevLiftSpeed = Vector2.Zero;
				}
				else
				{
					if (OnGround())
					{
						float target = ((!OnGround(Position + Vector2.UnitX * 3f)) ? 20f : (OnGround(Position - Vector2.UnitX * 3f) ? 0f : (-20f)));
						Speed.X = Calc.Approach(Speed.X, target, 800f * Engine.DeltaTime);
						
					}
					
					previousPosition = base.ExactPosition;
					MoveH(Speed.X * Engine.DeltaTime, onCollideH);
					MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
					float x = base.Center.X;
					Rectangle bounds = Level.Bounds;
					if (x > (float)((Rectangle)(bounds)).Right)
					{
						MoveH(32f * Engine.DeltaTime);
						float num3 = base.Left - 8f;
						bounds = Level.Bounds;
						if (num3 > (float)((Rectangle)(bounds)).Right)
						{
							RemoveSelf();
						}
					}
					else if((x < (float)((Rectangle)(bounds)).Left))
						{
						MoveH(-32f * Engine.DeltaTime);
						float num9 = base.Right + 8f;
						bounds = Level.Bounds;
						if (num9 < (float)((Rectangle)(bounds)).Left)
						{
							RemoveSelf();
						}

					}
					else
					{
						float left = base.Left;
						bounds = Level.Bounds;
						if (left < (float)((Rectangle)(bounds)).Left)
						{
							bounds = Level.Bounds;
							base.Left = ((Rectangle)(bounds)).Left;
							Speed.X *= -0.4f;
						}
						else
						{
							float top = base.Top;
							bounds = Level.Bounds;
							if (top < (float)(((Rectangle)(bounds)).Top - 4))
							{
								bounds = Level.Bounds;
								base.Top = ((Rectangle)(bounds)).Top + 4;
								Speed.Y = 0f;
							}
							else
							{
								float bottom = base.Bottom;
								bounds = Level.Bounds;
								if (bottom > (float)((Rectangle)(bounds)).Bottom && SaveData.Instance.Assists.Invincible)
								{
									bounds = Level.Bounds;
									base.Bottom = ((Rectangle)(bounds)).Bottom;
									Speed.Y = -300f;
									Audio.Play("event:/game/general/assist_screenbottom", Position);
								}
								else
								{
									float top2 = base.Top;
									bounds = Level.Bounds;
									if (top2 > (float)((Rectangle)(bounds)).Bottom)
									{
										Die();
									}
								}
							}
						}
					}
					float x2 = base.X;
					bounds = Level.Bounds;

					if (x2 < (float)(((Rectangle)(bounds)).Left + 10))
					{
						MoveH(32f * Engine.DeltaTime);
					}
					Player entity = base.Scene.Tracker.GetEntity<Player>();
				}
				if (!dead)
				{
					Hold.CheckAgainstColliders();
				}


			}
			else if(JackalModule.GetPlayer() != null)
            {
				if(grappled && thrown)
                {
					if (canSetPos)
					{
						startDistance = (Position - JackalModule.GetPlayer().Position);
						canSetPos = false;
					}
				}
                if (grappled && thrown)
                {
					JackalModule.GetPlayer().StateMachine.State = 0;
					moveDistance = startDistance;
					moveDistance.Normalize();
					moveDistance *= 360f;
					JackalModule.GetPlayer().Speed = moveDistance;
					JackalModule.GetPlayer().Facing = (JackalModule.GetPlayer().Speed.X < 0 ? Facings.Left : Facings.Right);
                    if (Input.Jump.Check)
                    {
						JackalModule.GetPlayer().Jump();
						JackalModule.GetPlayer().Speed.Y *= 2f;
						grappled = false;
						canSetPos = true;
						thrown = false;
						RemoveSelf();
                    }
					if((JackalModule.GetPlayer().Position - Position).Length() < 24f)
                    {
						grappled = false;
						canSetPos = true;
						thrown = false;
						RemoveSelf();
					}
                }
            }
			if (JackalModule.GetLevel() != null)
			{
				if (this.CollideCheck<RainbowDecal>())
				{
					Speed.X = 0;
					Speed.Y = 0;

					Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_side", Position);
					Speed.X *= -0.4f;
					grappled = true;
				}
			}
                
				
			

			/*
			if(JackalModule.GetPlayer().Speed != moveDistance && grappled)
            {
				Die();
            }*/
		}

		

		private void OnCollideH(CollisionData data)
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			Speed.X = 0;
			Speed.Y = 0;
			
			Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_side", Position);
			Speed.X *= -0.4f;
			grappled = true;
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
				(data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitY * (float)Math.Sign(Speed.Y));
			}
			if (Speed.Y > 0f)
			{

					Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_ground", Position, "crystal_velocity", 0f);
				
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


		public void Die()
		{
			if (!dead)
			{
				dead = true;
				Add(new DeathEffect(Color.Purple, base.Center - Position));
				sprite.Visible = false;
				base.Depth = -1000000;
				AllowPushing = false;
			
			}
		}



		public override void Render()
		{
			if (JackalModule.GetPlayer() != null && thrown && !dead)
			{
				Draw.Line(Position - 8 * Vector2.UnitY, JackalModule.GetPlayer().Center, Color.SteelBlue);
				Draw.Line(Position - 9 * Vector2.UnitY, JackalModule.GetPlayer().Center - Vector2.UnitY, Color.LightSteelBlue);
				Draw.Line(Position - 7 * Vector2.UnitY, JackalModule.GetPlayer().Center + Vector2.UnitY, Color.LightSteelBlue);
			}
			base.Render();
		}
	}
}