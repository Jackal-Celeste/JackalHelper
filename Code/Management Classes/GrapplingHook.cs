using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.JackalHelper.Entities
{
	[Tracked]
	public class GrapplingHook : Actor
	{
		public Vector2 Speed;

		public Sprite sprite;

		private bool dead;

		private Level Level;

		private Collision onCollideH;

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
			Position += Vector2.UnitY * 2f;
			base.Depth = 100;
			base.Collider = new Hitbox(8f, 10f, -4f, -12f);
			sprite = JackalModule.spriteBank.Create("grappleHook");
			Add(sprite);
			sprite.Play("idle");
			sprite.Position.Y -= 8f;
			onCollideH = OnCollideH;
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
			sprite.FlipX = JackalModule.GetPlayer().Facing == Facings.Left;
			base.Added(scene);
			Level = SceneAs<Level>();
			frozen = false;

			thrown = true;
			thrownY = Y;
			RemoveTag(Tags.Persistent);

			Vector2 newForce = new Vector2(1.5f, 0f);

			newForce.X *= (JackalModule.GetPlayer().Facing == Facings.Right ? 1f : -1f);
			Speed = newForce * 320f;

		}

		public override void Update()
		{
			if (grappled)
			{
				sprite.Play("spin");
			}
		

			if (JackalModule.GetPlayer() != null)
			{
				if ((!grappled && Speed.X == 0f) || (JackalModule.GetPlayer().Position - Position).Length() > 120f)
				{
					Die();
				}
				else if (grappled)
				{
					grappled = (JackalModule.GetPlayer().StateMachine.State == 0 || JackalModule.GetPlayer().StateMachine.State == 1 || JackalModule.GetPlayer().StateMachine.State == 2);
				}
			}
			if (!grappled)
			{
				base.Update();
				if (thrown && !dead && JackalModule.GetPlayer() != null)
				{
					JackalModule.GetPlayer().Speed.Y *= 0.9f;
				}
				if (dead)
				{
					return;
				}
				base.Depth = 100;
					if (OnGround())
					{
						float target = ((!OnGround(Position + Vector2.UnitX * 3f)) ? 20f : (OnGround(Position - Vector2.UnitX * 3f) ? 0f : (-20f)));
						Speed.X = Calc.Approach(Speed.X, target, 800f * Engine.DeltaTime);
					}
					MoveH(Speed.X * Engine.DeltaTime, onCollideH);
					float x = base.Center.X;
					Rectangle bounds = Level.Bounds;
					if (x > bounds.Right)
					{
						MoveH(32f * Engine.DeltaTime);
						float num3 = base.Left - 8f;
						bounds = Level.Bounds;
						if (num3 > bounds.Right)
						{
							RemoveSelf();
						}
					}
					else if ((x < bounds.Left))
					{
						MoveH(-32f * Engine.DeltaTime);
						float num9 = base.Right + 8f;
						bounds = Level.Bounds;
						if (num9 < bounds.Left)
						{
							RemoveSelf();
						}

					}
					else
					{
						float left = base.Left;
						bounds = Level.Bounds;
						if (left < bounds.Left)
						{
							bounds = Level.Bounds;
							base.Left = bounds.Left;
							Speed.X *= -0.4f;
						}
						else
						{
							float top = base.Top;
							bounds = Level.Bounds;
							if (top < bounds.Top - 4)
							{
								bounds = Level.Bounds;
								base.Top = bounds.Top + 4;
								Speed.Y = 0f;
							}
							else
							{
								float bottom = base.Bottom;
								bounds = Level.Bounds;

									float top2 = base.Top;
									bounds = Level.Bounds;
									if (top2 > bounds.Bottom)
									{
										Die();
									}
								
							}
						}
					}
					float x2 = base.X;
					bounds = Level.Bounds;

					if (x2 < bounds.Left + 10)
					{
						MoveH(32f * Engine.DeltaTime);
					}
			}
			else if (JackalModule.GetPlayer() != null)
			{
				if (grappled && thrown)
				{
					if (canSetPos)
					{
						startDistance = (Position - JackalModule.GetPlayer().Position);
						canSetPos = false;
					}
				}
					JackalModule.GetPlayer().StateMachine.State = 0;
					moveDistance = startDistance;
					moveDistance.Normalize();
					moveDistance *= 360f;
					JackalModule.GetPlayer().Speed = moveDistance;
					JackalModule.GetPlayer().Facing = (JackalModule.GetPlayer().Speed.X < 0 ? Facings.Left : Facings.Right);
				bool colliding = (JackalModule.GetPlayer().CollideCheck<Solid>(JackalModule.GetPlayer().Position + 2 * Vector2.UnitX) || JackalModule.GetPlayer().CollideCheck<Solid>(JackalModule.GetPlayer().Position - 2 * Vector2.UnitX));

					if(Input.Dash.Check || (JackalModule.GetPlayer().Position - Position).Length() < 24f || Speed.X == 0f && !(grappled && thrown) || Input.Jump.Check || colliding)
						{
					if (Input.Jump.Check)
					{
						JackalModule.GetPlayer().Jump();
						JackalModule.GetPlayer().Speed.Y *= 2f;
					}
					grappled = false;
					canSetPos = true;
					thrown = false;
					RemoveSelf();
					}
					//TODO: add kill option if on wall

				
			}
			if (JackalModule.GetLevel() != null)
			{
				if (CollideCheck<LaniStar>())
				{
					Speed.X = 0;
					Speed.Y = 0;
					Speed.X *= -0.4f;
					grappled = true;
				}
			}
		}



		private void OnCollideH(CollisionData data)
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			Speed = Vector2.Zero;
			grappled = true;
		}

		private void OnCollideV(CollisionData data)
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)

		}

		public void Die()
		{
			if (!dead)
			{
				dead = true;
				base.Collider = null;
				Add(new DeathEffect(Calc.HexToColor("777777"), base.Center - Position));
				sprite.Visible = false;
				base.Depth = -1000000;
				AllowPushing = false;

			}
		}



		public override void Render()
		{
			if (JackalModule.GetPlayer() != null && thrown && !dead)
			{
				Draw.Line(Position - 8 * Vector2.UnitY, JackalModule.GetPlayer().Center, Calc.HexToColor("965c22"));
				Draw.Line(Position - 9 * Vector2.UnitY, JackalModule.GetPlayer().Center - Vector2.UnitY, Calc.HexToColor("b67637"));
				Draw.Line(Position - 7 * Vector2.UnitY, JackalModule.GetPlayer().Center + Vector2.UnitY, Calc.HexToColor("b67637"));
			}
			base.Render();
		}
	}
}