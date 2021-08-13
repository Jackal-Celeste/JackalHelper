using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.JackalHelper.Entities
{
	[Tracked]
	[CustomEntity("JackalHelper/HolySword")]
	public class HolySword : Actor
	{

		public static readonly Color TrailColor = Color.White;

		private const int StIdle = 0;

		private const int StAttack = 3;

		private const int StSkidding = 5;

		private const int size = 12;

		private const float Accel = 600f;

		private const float WallCollideStunThreshold = 100f;

		private const float StunXSpeed = 100f;

		private const float BounceSpeed = 200f;

		private const float SightDistSq = 25600f;

		private const float ExplodeRadius = 40f;

		private Hitbox physicsHitbox;

		private Hitbox attackHitbox;

		private Circle pushRadius;

		private StateMachine State;

		private Vector2 lastSpottedAt;

		private Vector2 lastPathTo;

		private bool spotted;

		private bool canSeePlayer;

		private Random random;

		private Vector2 lastPosition;

		private Wiggler scaleWiggler;

		private bool lastPathFound;

		private List<Vector2> path;

		private int pathIndex;

		public VertexLight Light;

		private Sprite sprite;

		private int facing = 1;

		private int spriteFacing = 1;

		private HashSet<string> flipAnimations = new HashSet<string> { "flipMouth", "flipEyes", "skid" };

		public Vector2 Speed;

		private const float FarDistSq = 12544f;

		private const float IdleAccel = 200f;

		private const float IdleSpeed = 50f;

		private const float PatrolSpeed = 25f;

		private const int PatrolChoices = 3;

		private const float PatrolWaitTime = 0.4f;

		private float patrolWaitTimer;

		private const float SpottedTargetSpeed = 60f;

		private const float SpottedFarSpeed = 90f;

		private const float SpottedMaxYDist = 24f;

		private const float AttackMinXDist = 16f;

		private const float SpottedLosePlayerTime = 0.6f;

		private const float SpottedMinAttackTime = 0.2f;

		private float spottedLosePlayerTimer;

		private float spottedTurnDelay;

		private const float AttackWindUpSpeed = -60f;

		private const float AttackWindUpTime = 0.3f;

		private const float AttackStartSpeed = 180f;

		private const float AttackTargetSpeed = 260f;

		private const float AttackAccel = 300f;

		private const float DirectionDotThreshold = 0.4f;

		private const int AttackTargetUpShift = 2;

		private const float AttackMaxRotateRadians = 0.610865235f;

		private float attackSpeed;

		private bool attackWindUp;

		private const float StunnedAccel = 150f;

		private const float StunTime = 0.8f;

		private const float SkiddingAccel = 200f;

		private const float StrongSkiddingAccel = 400f;

		private const float StrongSkiddingTime = 0.08f;

		private bool strongSkid;

		private Vector2 FollowTarget => lastSpottedAt - Vector2.UnitY * 2f;

		public HolySword(Vector2 position)
			: base(position)
		{
			base.Depth = -12000;
			lastPosition = position;
			base.Collider = (physicsHitbox = new Hitbox(6f, 6f, -3f, -3f));
			attackHitbox = new Hitbox(12f, 8f, -6f, -2f);
			pushRadius = new Circle(40f);
			Add(new PlayerCollider(OnAttackPlayer, attackHitbox));
			Add(State = new StateMachine());
			State.SetCallbacks(0, IdleUpdate, IdleCoroutine);
			State.SetCallbacks(3, AttackUpdate, AttackCoroutine, AttackBegin);
			State.SetCallbacks(5, SkiddingUpdate, SkiddingCoroutine, SkiddingBegin, SkiddingEnd);
			//Add(idleSineX = new SineWave(0.5f, 0f));
			//Add(idleSineY = new SineWave(0.7f, 0f));
			Add(Light = new VertexLight(Color.White, 1f, 32, 64));
			Add(new MirrorReflection());
			path = new List<Vector2>();
			base.IgnoreJumpThrus = true;
			Add(sprite = GFX.SpriteBank.Create("seeker"));

			scaleWiggler = Wiggler.Create(0.8f, 2f);
			Add(scaleWiggler);

		}

		public HolySword(EntityData data, Vector2 offset)
			: this(data.Position + offset)
		{
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			random = new Random(SceneAs<Level>().Session.LevelData.LoadSeed);
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			Player entity = base.Scene.Tracker.GetEntity<Player>();
			State.State = 3;
			sprite.Rotation = (float)Math.Atan2(Position.Y - JackalModule.GetPlayer().Position.Y, Position.X - JackalModule.GetPlayer().Position.X);
			if (entity == null || base.X == entity.X)
			{
				SnapFacing(1f);
			}
			else
			{
				SnapFacing(Math.Sign(entity.X - base.X));
			}
		}


		private void OnAttackPlayer(Player player)
		{
			if (State.State != 4)
			{
				player.Die((player.Center - Position).SafeNormalize());
				return;
			}
		}



		public override void Update()
		{
			Light.Alpha = Calc.Approach(Light.Alpha, 1f, Engine.DeltaTime * 2f);
			sprite.Scale.X = Calc.Approach(sprite.Scale.X, 1f, 2f * Engine.DeltaTime);
			sprite.Scale.Y = Calc.Approach(sprite.Scale.Y, 1f, 2f * Engine.DeltaTime);
			if (State.State == 6)
			{
				canSeePlayer = false;
			}
			else
			{
				Player entity = base.Scene.Tracker.GetEntity<Player>();
				if (entity != null)
				{
					spotted = true;
					lastSpottedAt = entity.Center;
				}

			}
			if (lastPathTo != lastSpottedAt)
			{
				lastPathTo = lastSpottedAt;
				pathIndex = 0;
				lastPathFound = SceneAs<Level>().Pathfinder.Find(ref path, base.Center, FollowTarget);
			}
			base.Update();
			lastPosition = Position;
			MoveH(Speed.X * Engine.DeltaTime);
			MoveV(Speed.Y * Engine.DeltaTime);
			Level level = SceneAs<Level>();
			if (base.Left < level.Bounds.Left && Speed.X < 0f)
			{
				base.Left = level.Bounds.Left;
			}
			else if (base.Right > level.Bounds.Right && Speed.X > 0f)
			{
				base.Right = level.Bounds.Right;
			}
			if (base.Top < level.Bounds.Top + -8 && Speed.Y < 0f)
			{
				base.Top = level.Bounds.Top + -8;
			}
			else if (base.Bottom > level.Bounds.Bottom && Speed.Y > 0f)
			{
				base.Bottom = level.Bounds.Bottom;
			}
			foreach (HolySwordCollider component in base.Scene.Tracker.GetComponents<HolySwordCollider>())
			{
				component.Check(this);
			}
			if (State.State != 3 && Speed.Length() < 12f)
			{
				State.State = 0;
				State.State = 3;
				sprite.Rotation = (float)Math.Atan2(Position.Y - JackalModule.GetPlayer().Position.Y, Position.X - JackalModule.GetPlayer().Position.X);
			}

		}


		private void SnapFacing(float dir)
		{
			if (dir != 0f)
			{
				spriteFacing = (facing = Math.Sign(dir));
			}
		}



		public override void Render()
		{
			Vector2 position = Position;
			Vector2 scale = sprite.Scale;
			sprite.Scale *= 1f - 0.3f * scaleWiggler.Value;
			sprite.Scale.X *= spriteFacing;
			base.Render();
			Position = position;
			sprite.Scale = scale;
			if (JackalModule.GetPlayer() != null)
			{
				Player player = JackalModule.GetPlayer();
				sprite.Rotation = (float)Math.Atan2(Position.Y - player.Position.Y, Position.X - player.Position.X);
			}

		}






		private void CreateTrail()
		{
			Vector2 scale = sprite.Scale;
			sprite.Scale *= 1f - 0.3f * scaleWiggler.Value;
			sprite.Scale.X *= spriteFacing;
			TrailManager.Add(this, TrailColor, 0.5f, frozenUpdate: false, useRawDeltaTime: false);
			sprite.Scale = scale;
		}

		private int IdleUpdate()
		{
			if (canSeePlayer)
			{
				return 2;
			}
			Vector2 vector = Vector2.Zero;
			if (spotted && Vector2.DistanceSquared(base.Center, FollowTarget) > 64f)
			{
				float speedMagnitude = GetSpeedMagnitude(50f);
				vector = ((!lastPathFound) ? (FollowTarget - base.Center).SafeNormalize(speedMagnitude) : GetPathSpeed(speedMagnitude));
			}
			Speed = Calc.Approach(Speed, vector, 200f * Engine.DeltaTime);
			if (spriteFacing == facing)
			{
				sprite.Play("idle");
			}
			return 0;
		}

		private IEnumerator IdleCoroutine()
		{
			if (spotted)
			{
				while (Vector2.DistanceSquared(Center, FollowTarget) > 64f)
				{
					yield return null;
				}
				yield return 0.3f;
				State.State = 1;
			}
		}

		private Vector2 GetPathSpeed(float magnitude)
		{
			if (pathIndex >= path.Count)
			{
				return Vector2.Zero;
			}
			if (Vector2.DistanceSquared(base.Center, path[pathIndex]) < 36f)
			{
				pathIndex++;
				return GetPathSpeed(magnitude);
			}
			return (path[pathIndex] - base.Center).SafeNormalize(magnitude);
		}

		private float GetSpeedMagnitude(float baseMagnitude)
		{
			Player entity = base.Scene.Tracker.GetEntity<Player>();
			if (entity != null)
			{
				if (Vector2.DistanceSquared(base.Center, entity.Center) > 12544f)
				{
					return baseMagnitude * 3f;
				}
				return baseMagnitude * 1.5f;
			}
			return baseMagnitude;
		}



		private bool CanAttack()
		{
			if (Math.Abs(base.Y - lastSpottedAt.Y) > 24f)
			{
				return false;
			}
			if (Math.Abs(base.X - lastSpottedAt.X) < 16f)
			{
				return false;
			}
			Vector2 value = (FollowTarget - base.Center).SafeNormalize();
			if (Vector2.Dot(-Vector2.UnitY, value) > 0.5f || Vector2.Dot(Vector2.UnitY, value) > 0.5f)
			{
				return false;
			}
			if (CollideCheck<Solid>(Position + Vector2.UnitX * Math.Sign(lastSpottedAt.X - base.X) * 24f))
			{
				return false;
			}
			return true;
		}

		private void AttackBegin()
		{
			Audio.Play("event:/game/05_mirror_temple/HolySword_dash", Position);
			attackWindUp = true;
			attackSpeed = -60f;
			Speed = (FollowTarget - base.Center).SafeNormalize(-60f);
		}

		private int AttackUpdate()
		{
			if (!attackWindUp)
			{
				Vector2 vector = (FollowTarget - base.Center).SafeNormalize();
				if (Vector2.Dot(Speed.SafeNormalize(), vector) < 0.4f)
				{
					return 5;
				}
				attackSpeed = Calc.Approach(attackSpeed, 260f, 300f * Engine.DeltaTime);
				Speed = Speed.RotateTowards(vector.Angle(), 0.610865235f * Engine.DeltaTime).SafeNormalize(attackSpeed);
				if (base.Scene.OnInterval(0.04f))
				{
					Vector2 vector2 = (-Speed).SafeNormalize();
				}
				if (base.Scene.OnInterval(0.06f))
				{
					CreateTrail();
				}
			}
			return 3;
		}

		private IEnumerator AttackCoroutine()
		{
			yield return 0.2f;
			attackWindUp = false;
			attackSpeed = 240f;
			Speed = (lastSpottedAt - Vector2.UnitY * 2f - Center).SafeNormalize(240f);
			//SnapFacing(Speed.X);
		}

		private void SkiddingBegin()
		{
			Audio.Play("event:/game/05_mirror_temple/HolySword_dash_turn", Position);
			strongSkid = false;
		}

		private int SkiddingUpdate()
		{
			Speed = Calc.Approach(Speed, Vector2.Zero, (strongSkid ? 400f : 200f) * Engine.DeltaTime);
			if (Speed.LengthSquared() < 400f)
			{
				return 3;
			}
			return 5;
		}

		private IEnumerator SkiddingCoroutine()
		{
			yield return 0.08f;
			strongSkid = true;
		}

		private void SkiddingEnd()
		{
			//spriteFacing = facing;
		}

	}


	[Tracked]
	public class HolySwordCollider : Component
	{
		public Action<HolySword> OnCollide;

		public Collider Collider;

		public HolySwordCollider(Action<HolySword> onCollide, Collider collider = null)
			: base(active: false, visible: false)
		{
			OnCollide = onCollide;
			Collider = null;
		}

		public void Check(HolySword HolySword)
		{
			if (OnCollide != null)
			{
				Collider collider = base.Entity.Collider;
				if (Collider != null)
				{
					base.Entity.Collider = Collider;
				}
				if (HolySword.CollideCheck(base.Entity))
				{
					OnCollide(HolySword);
				}
				base.Entity.Collider = collider;
			}
		}
	}

}
