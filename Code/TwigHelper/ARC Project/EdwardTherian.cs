using System;
using System.Collections;
using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

[Tracked(false)]
public class EdwardTherian : Actor
{

	public static readonly Color TrailColor = Calc.HexToColor("3d0407");

	private Hitbox physicsHitbox;

	private Hitbox breakWallsHitbox;

	private Hitbox attackHitbox;

	private Circle pushRadius;

	private Circle breakWallsRadius;

	private StateMachine State;

	private Vector2 lastSpottedAt;

	private Vector2 lastPathTo;

	private bool spotted;

	private bool canSeePlayer;

	private Collision onCollideH;

	private Collision onCollideV;

	private Random random;

	private Vector2 lastPosition;

	private Shaker shaker;

	private Wiggler scaleWiggler;

	private bool lastPathFound;

	private List<Vector2> path;

	private int pathIndex;

	public VertexLight Light;

	private bool dead;

	private SoundSource aggroSfx;

	private Sprite sprite;

	private int facing = 1;

	private int spriteFacing = 1;

	private string nextSprite;

	private HashSet<string> flipAnimations = new HashSet<string> { "flipMouth", "flipEyes", "skid" };

	public Vector2 Speed;

	private float spottedLosePlayerTimer;

	private float spottedTurnDelay;

	private float attackSpeed;

	private bool attackWindUp;


	[Pooled]
	private class BreakDebris : Entity
	{
		private Image sprite;

		private Vector2 speed;

		private float percent;

		private float duration;

		public BreakDebris()
		{
			Add(sprite = new Image(Calc.Random.Choose(GFX.Game.GetAtlasSubtextures("objects/edward/new/explode/limbs"))));
			sprite.CenterOrigin();
		}

		public BreakDebris Init(Vector2 position, Vector2 direction)
		{
			Depth = -1000;
			Position = position;
			Visible = true;

			direction = Calc.AngleToVector(direction.Angle() + Calc.Random.Range(-0.1f, 0.1f), 1f);
			direction.X += Calc.Random.Range(-0.3f, 0.3f);
			direction.Normalize();
			speed = direction * Calc.Random.Range(140, 180);
			percent = 0f;
			// COLOURSOFNOISE: Calc.Random.Range(2, 3) would only ever return 2
			duration = Calc.Random.Range(2, 4);
			return this;
		}

		public override void Update()
		{
			base.Update();
			if (percent >= 1f)
			{
				RemoveSelf();
				return;
			}
			Position += speed * Engine.DeltaTime;
			speed.X = Calc.Approach(speed.X, 0f, 180f * Engine.DeltaTime);
			speed.Y += 200f * Engine.DeltaTime;
			percent += Engine.DeltaTime / duration;
			sprite.Color = Color.White * (1f - percent);
		}

		public override void Render()
		{
			sprite.DrawOutline(Color.Black);
			base.Render();
		}
	}

	private Vector2 FollowTarget => lastSpottedAt - Vector2.UnitY * 2f;

	public EdwardTherian(Vector2 position)
		: base(position)
	{
		base.Depth = -200;
		lastPosition = position;
		base.Collider = (physicsHitbox = new Hitbox(6f, 6f, -3f, -3f));
		breakWallsHitbox = new Hitbox(6f, 14f, -3f, -7f);
		attackHitbox = new Hitbox(12f, 8f, -6f, -2f);
		pushRadius = new Circle(40f);
		breakWallsRadius = new Circle(16f);
		Add(new PlayerCollider(OnAttackPlayer, attackHitbox));
		Add(shaker = new Shaker(on: false));
		Add(State = new StateMachine());
		State.SetCallbacks(2, SpottedUpdate, SpottedCoroutine, SpottedBegin);
		State.SetCallbacks(3, AttackUpdate, AttackCoroutine, AttackBegin);
		onCollideH = OnCollideH;
		onCollideV = OnCollideV;
		Add(Light = new VertexLight(Color.White, 1f, 32, 64));
		Add(new MirrorReflection());
		path = new List<Vector2>();
		IgnoreJumpThrus = true;
		Add(sprite = TwigModule.spriteBank.Create("birdMonster"));
		sprite.OnLastFrame = delegate (string f)
		{
			if (flipAnimations.Contains(f) && spriteFacing != facing)
			{
				spriteFacing = facing;
				if (nextSprite != null)
				{
					sprite.Play(nextSprite);
					nextSprite = null;
				}
			}
		};
		sprite.OnChange = delegate (string last, string next)
		{
			nextSprite = null;
			sprite.OnLastFrame(last);
		};
		SquishCallback = delegate (CollisionData d)
		{
			if (!dead && !TrySquishWiggle(d, 3, 3))
			{
				Entity entity = new Entity(Position);
				DeathEffect component = new DeathEffect(Color.HotPink, base.Center - Position)
				{
					OnEnd = delegate
					{
						entity.RemoveSelf();
					}
				};
				entity.Add(component);
				entity.Depth = -1000000;
				base.Scene.Add(entity);
				Audio.Play("event:/game/05_mirror_temple/seeker_death", Position);
				RemoveSelf();
				dead = true;
			}
		};
		scaleWiggler = Wiggler.Create(0.8f, 2f);
		Add(scaleWiggler);
		Add(aggroSfx = new SoundSource());
		State.State = 2;
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
		if (entity == null || base.X == entity.X)
		{
			SnapFacing(1f);
		}
		else
		{
			SnapFacing(Math.Sign(entity.X - base.X));
		}
	}

	public override bool IsRiding(JumpThru jumpThru)
	{
		return false;
	}

	public override bool IsRiding(Solid solid)
	{
		return false;
	}

	private void OnAttackPlayer(Player player)
	{

			player.Die((player.Center - Position).SafeNormalize());
			return;
	}





	public void HitSpring()
	{
		Speed.Y = -150f;
	}

	private bool CanSeePlayer(Player player)
	{
		return true;
	}

	public override void Update()
	{
		Light.Alpha = Calc.Approach(Light.Alpha, 1f, Engine.DeltaTime * 2f);
		foreach (Entity entity2 in base.Scene.Tracker.GetEntities<SeekerBarrier>())
		{
			entity2.Collidable = true;
		}
		sprite.Scale.X = Calc.Approach(sprite.Scale.X, 1f, 2f * Engine.DeltaTime);
		sprite.Scale.Y = Calc.Approach(sprite.Scale.Y, 1f, 2f * Engine.DeltaTime);

			Player entity = base.Scene.Tracker.GetEntity<Player>();
			canSeePlayer = entity != null;
			if (canSeePlayer)
			{
				spotted = true;
				lastSpottedAt = entity.Center;
			}
		
		if (lastPathTo != lastSpottedAt)
		{
			lastPathTo = lastSpottedAt;
			pathIndex = 0;
			lastPathFound = SceneAs<Level>().Pathfinder.Find(ref path, base.Center, FollowTarget);
		}
		base.Update();
		lastPosition = Position;
		MoveH(Speed.X * Engine.DeltaTime, onCollideH);
		MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
		Level level = SceneAs<Level>();
		if (base.Left < (float)level.Bounds.Left && Speed.X < 0f)
		{
			base.Left = level.Bounds.Left;
			onCollideH(CollisionData.Empty);
		}
		else if (base.Right > (float)level.Bounds.Right && Speed.X > 0f)
		{
			base.Right = level.Bounds.Right;
			onCollideH(CollisionData.Empty);
		}
		if (base.Top < (float)(level.Bounds.Top + -8) && Speed.Y < 0f)
		{
			base.Top = level.Bounds.Top + -8;
			onCollideV(CollisionData.Empty);
		}
		else if (base.Bottom > (float)level.Bounds.Bottom && Speed.Y > 0f)
		{
			base.Bottom = level.Bounds.Bottom;
			onCollideV(CollisionData.Empty);
		}
		foreach (SeekerCollider component in base.Scene.Tracker.GetComponents<SeekerCollider>())
		{
			//component.Check(this);
		}
		foreach (Entity entity3 in base.Scene.Tracker.GetEntities<SeekerBarrier>())
		{
			entity3.Collidable = false;
		}
	}

	private void TurnFacing(float dir, string gotoSprite = null)
	{
		if (dir != 0f)
		{
			facing = Math.Sign(dir);
		}
		if (spriteFacing != facing)
		{
			if (State.State == 3 || State.State == 2)
			{
				sprite.Play("flipMouth");
			}
			else
			{
				sprite.Play("flipEyes");
			}
			nextSprite = gotoSprite;
		}
		else if (gotoSprite != null)
		{
			sprite.Play(gotoSprite);
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
		Position += shaker.Value;
		Vector2 scale = sprite.Scale;
		sprite.Scale *= 1f - 0.3f * scaleWiggler.Value;
		sprite.Scale.X *= spriteFacing;
		base.Render();
		Position = position;
		sprite.Scale = scale;
	}

	public override void DebugRender(Camera camera)
	{
		Collider collider = base.Collider;
		base.Collider = attackHitbox;
		attackHitbox.Render(camera, Color.Red);
		base.Collider = collider;
	}

	private void SlammedIntoWall(CollisionData data)
	{
		Vector2 spd = Speed;
		float direction;
		float x;
		if (data.Direction.X > 0f)
		{
			direction = (float)Math.PI;
			x = base.Right;
		}
		else
		{
			direction = 0f;
			x = base.Left;
		}
		SceneAs<Level>().Particles.Emit(new ParticleType(), 12, new Vector2(x, base.Y), Vector2.UnitY * 4f, direction);
		if (data.Hit is DashSwitch)
		{
			(data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
		}
		base.Collider = breakWallsHitbox;
		foreach (TempleCrackedBlock entity in base.Scene.Tracker.GetEntities<TempleCrackedBlock>())
		{
			if (CollideCheck(entity, Position + Vector2.UnitX * Math.Sign(Speed.X)))
			{
				entity.Break(base.Center);
			}
		}
		base.Collider = physicsHitbox;
		SceneAs<Level>().DirectionalShake(Vector2.UnitX * Math.Sign(Speed.X));
		Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
		Speed.X = (float)Math.Sign(Speed.X) * -100f;
		Speed.Y *= 0.4f;
		sprite.Scale.X = 0.6f;
		sprite.Scale.Y = 1.4f;
		shaker.ShakeFor(0.5f, removeOnFinish: false);
		scaleWiggler.Start();
		if (data.Hit is SeekerBarrier)
		{
			(data.Hit as SeekerBarrier).OnReflectSeeker();
			Audio.Play("event:/game/05_mirror_temple/seeker_hit_lightwall", Position);
		}
		else
		{
			Audio.Play("event:/game/05_mirror_temple/seeker_hit_normal", Position);
		}
		Entity e = new Entity(Position);
		Scene.Add(e);
		DeathEffect d = new DeathEffect(Color.DarkRed, base.Center - Position);
		for (int x1 = -24; x1 < 24; x1 += 8)
		{
			for (int y = -24; y < 24; y += 8)
			{
				TwigModule.GetLevel()?.Add(Engine.Pooler.Create<BreakDebris>().Init(new Vector2(Position.X + x, Position.Y + y), -spd));
				//player.Die(Vector2.Zero);
			}
		}
		e.Add(d);
		RemoveSelf();
		e.RemoveSelf();
	}

	private void OnCollideH(CollisionData data)
	{
		if (State.State == 3 && data.Hit != null)
		{
			int num = Math.Sign(Speed.X);
			if ((!CollideCheck<Solid>(Position + new Vector2(num, 4f)) && !MoveVExact(4)) || (!CollideCheck<Solid>(Position + new Vector2(num, -4f)) && !MoveVExact(-4)))
			{
				return;
			}
		}
		SlammedIntoWall(data);
	}

	private void OnCollideV(CollisionData data)
	{
		if (State.State == 3)
		{
			Speed.Y *= -0.6f;
		}
		else
		{
			Speed.Y *= -0.2f;
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

	private void SpottedBegin()
	{
		aggroSfx.Play("event:/game/05_mirror_temple/seeker_aggro");
		Player entity = TwigModule.GetPlayer();
		if (entity != null)
		{
			TurnFacing(entity.X - base.X, "spot");
		}
		spottedLosePlayerTimer = 0.6f;
		spottedTurnDelay = 1f;
	}

	private int SpottedUpdate()
	{

			spottedLosePlayerTimer = 0.6f;
		
		float speedMagnitude = GetSpeedMagnitude(60f);
		Vector2 vector = ((!lastPathFound) ? (FollowTarget - base.Center).SafeNormalize(speedMagnitude) : GetPathSpeed(speedMagnitude));
		if (Vector2.DistanceSquared(base.Center, FollowTarget) < 2500f && base.Y < FollowTarget.Y)
		{
			float num = vector.Angle();
			if (base.Y < FollowTarget.Y - 2f)
			{
				num = Calc.AngleLerp(num, (float)Math.PI / 2f, 0.5f);
			}
			else if (base.Y > FollowTarget.Y + 2f)
			{
				num = Calc.AngleLerp(num, -(float)Math.PI / 2f, 0.5f);
			}
			vector = Calc.AngleToVector(num, 60f);
			Vector2 vector2 = Vector2.UnitX * Math.Sign(base.X - lastSpottedAt.X) * 48f;
			if (Math.Abs(base.X - lastSpottedAt.X) < 36f && !CollideCheck<Solid>(Position + vector2) && !CollideCheck<Solid>(lastSpottedAt + vector2))
			{
				vector.X = Math.Sign(base.X - lastSpottedAt.X) * 60;
			}
		}
		Speed = Calc.Approach(Speed, vector, 600f * Engine.DeltaTime);
		spottedTurnDelay -= Engine.DeltaTime;
		if (spottedTurnDelay <= 0f)
		{
			TurnFacing(Speed.X, "spotted");
		}
		return 3;
	}

	private IEnumerator SpottedCoroutine()
	{
		yield return null;

		State.State = 3;
	}

	private void AttackBegin()
	{
		Audio.Play("event:/game/05_mirror_temple/seeker_dash", Position);
		attackWindUp = false;
		attackSpeed = 60f;
	}

	private int AttackUpdate()
	{
		if (!attackWindUp)
		{
			Vector2 vector = (FollowTarget - base.Center).SafeNormalize();
			attackSpeed = Calc.Approach(attackSpeed, 720f, 400f * Engine.DeltaTime);
			Speed = Speed.RotateTowards(vector.Angle(), 1.5f * Engine.DeltaTime).SafeNormalize(attackSpeed);
			if (base.Scene.OnInterval(0.04f))
			{
				Vector2 vector2 = (-Speed).SafeNormalize();
				SceneAs<Level>().Particles.Emit(new ParticleType(), 2, Position + vector2 * 4f, Vector2.One * 4f, vector2.Angle());
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
		TurnFacing(lastSpottedAt.X - X, "windUp");
		yield return 0f;
		attackWindUp = false;
		attackSpeed = 320f;
		Speed = (lastSpottedAt - Vector2.UnitY * 2f - Center).SafeNormalize(180f);
		SnapFacing(Speed.X);
	}

}
