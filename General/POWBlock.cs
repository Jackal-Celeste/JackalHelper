using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

[CustomEntity("JackalHelper/PowBlock")]
[Tracked]
public class PowBlock : Actor
{

	public Holdable Hold;

	public Image sprite;

	private Vector2 respawnPosition;


	private Collision onCollideH;

	private Collision onCollideV;

	public Entity entity;

	public Vector2 Speed;

	private float noGravityTimer;

	private Vector2 prevLiftSpeed;

	private float hardVerticalHitSoundCooldown;

	private Level Level;

	private Vector2 previousPosition;

	private List<Player> playerEntities;

	private List<Spring> springs;

	private bool destroyed;

	private List<CassetteBlock> cassetteBlocks;

	private Player playerEntity;

	private Circle pushRadius;

	private Hitbox hitBox;

	private Vector2 startPos;

	private float maxRespawnTime;

	public Switch switcher;

	private float respawnTime = 0f;

	private float frameCount;

	private bool breakDashBlocks = false;

	private bool destroying;

	private bool dieIfDrop;

	public bool threeHit = false;

	public float range = 64f;

	public int hits;

	private Level level => (Level)base.Scene;

	public PowBlock(EntityData data, Vector2 offset)
		: base(data.Position + offset)
	{
		base.Depth = 100;
		hitBox = new Hitbox(8f, 10f, -4f, -10f);
		base.Collider = hitBox;
		dieIfDrop = data.Bool("dieIfDropped");
		range = data.Float("explodeRange", 64f);
		threeHit = data.Bool("threeHits", false);
		hits = threeHit ? 3 : 1;
		Add(sprite = new Image(GFX.Game[threeHit ? "objects/powBlock/POW3_3" : "objects/powBlock/POW1_1"]));
		sprite.Position += new Vector2(-9f, -16f);


		Add(Hold = new Holdable());
		Hold.PickupCollider = new Hitbox(16f, 16f, -8f, -16f);
		Hold.OnPickup = OnPickup;
		Hold.OnRelease = OnRelease;
		respawnPosition = Position;
		//sprite.Position.Y = sprite.Position.Y - 8f;

		destroying = false;
		onCollideH = OnCollideH;
		onCollideV = OnCollideV;
		Hold.SpeedGetter = () => Speed;
		Add(new VertexLight(base.Collider.Center, Color.White, 1f, 32, 64));
		Add(new MirrorReflection());
	}

	public override void Added(Scene scene)
	{
		base.Added(scene);
		Level = SceneAs<Level>();
		if (level == null)
		{
		}
	}
	/*
	public void CheckAgainstColliders()
	{
		foreach (PowBlockCollider component in base.Scene.Tracker.GetComponents<PowBlockCollider>())
		{
			if (component.Check(this))
			{
				component.OnCollide(this);
			}
		}
	}*/

	private void OnCollideSpring(Spring spring)
	{
		Sprite sprite = spring.Get<Sprite>();
		Spring.Orientations springOrientation = ((sprite.Rotation == (float)Math.PI / 2f) ? Spring.Orientations.WallLeft : ((sprite.Rotation == -(float)Math.PI / 2f) ? Spring.Orientations.WallRight : Spring.Orientations.Floor));
		Audio.Play("event:/game/general/spring", spring.BottomCenter);
		spring.Get<StaticMover>().TriggerPlatform();
		spring.Get<Wiggler>().Start();
		spring.Get<Sprite>().Play("bounce", restart: true);
		HitSpring(springOrientation);
	}

	public override void Awake(Scene scene)
	{
		base.Awake(scene);
		springs = scene.Entities.OfType<Spring>().ToList();
		cassetteBlocks = scene.Tracker.GetEntities<CassetteBlock>().Cast<CassetteBlock>().ToList();
		playerEntity = scene.Tracker.GetEntity<Player>();
		startPos = Position;
	}

	private void OnPickup()
	{
		Speed = Vector2.Zero;
	}

	public void HitSpring(Spring.Orientations springOrientation)
	{
		if (!Hold.IsHeld)
		{
			if (springOrientation == Spring.Orientations.Floor)
			{
				Speed.X *= 0.5f;
				Speed.Y = -160f;
				noGravityTimer = 0.15f;
			}
			else
			{
				Speed.X = 240 * ((springOrientation == Spring.Orientations.WallLeft) ? 1 : (-1));
				Speed.Y = -140f;
				noGravityTimer = 0.15f;
			}
		}
	}

	private void OnRelease(Vector2 force)
	{
		if (force.X != 0f && force.Y == 0f)
		{
			force.Y = -0.4f;
		}
		Speed = force * 200f;
		if (Speed != Vector2.Zero)
		{
			noGravityTimer = 0.1f;
		}
	}

	private void OnCollideH(CollisionData data)
	{
		if (data.Hit is DashSwitch)
		{
			(data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
		}
		Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_side", Position);
		if (Math.Abs(Speed.X) > 100f)
		{
			ImpactParticles(data.Direction, range);
		}
		Speed.X *= -0.4f;
	}

	private void OnCollideV(CollisionData data)
	{
		if (data.Hit is DashSwitch)
		{
			(data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitY * Math.Sign(Speed.Y));
		}
		if (Speed.Y > 0f && frameCount > 15f)
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
		if (Speed.Y > 160f)
		{
			ImpactParticles(data.Direction, range);
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

	private void ImpactParticles(Vector2 dir, float range)
	{
		if (JackalModule.GetLevel() != null && JackalModule.GetPlayer() != null && JackalModule.GetLevel().Tracker.GetEntities<CrystalStaticSpinner>().Count > 0)
		{
			foreach (CrystalStaticSpinner spinner in JackalModule.GetLevel().Tracker.GetEntities<CrystalStaticSpinner>())
			{
				if(Vector2.Distance(Position, spinner.Position) <= range)
                {
					spinner.Destroy(false);
                }
			}
		}
		float direction;
		Vector2 position;
		Vector2 positionRange;
		if (dir.X > 0f)
		{
			direction = (float)Math.PI;
			position = new Vector2(base.Right, base.Y - 4f);
			positionRange = Vector2.UnitY * 6f;
		}
		else if (dir.X < 0f)
		{
			direction = 0f;
			position = new Vector2(base.Left, base.Y - 4f);
			positionRange = Vector2.UnitY * 6f;
		}
		else if (dir.Y > 0f)
		{
			direction = -(float)Math.PI / 2f;
			position = new Vector2(base.X, base.Bottom);
			positionRange = Vector2.UnitX * 6f;
		}
		else
		{
			direction = (float)Math.PI / 2f;
			position = new Vector2(base.X, base.Top);
			positionRange = Vector2.UnitX * 6f;
		}
		Level.Particles.Emit(TheoCrystal.P_Impact, 12, position, positionRange, direction);
		hits--;

		if(hits <= 0)
        {
			noGravityTimer = 999f;
			Speed = Vector2.Zero;
			Remove(sprite);
			Add(new DeathEffect(threeHit ? Color.DarkBlue : Color.Red, base.Center - Position));
			//RemoveSelf();
			Collidable = false;
        }
		else if (threeHit)
		{
			Remove(sprite);

			Add(sprite = new Image(GFX.Game["objects/powBlock/POW3_" + hits.ToString()]));
			sprite.Position += new Vector2(-9f, -16f);
		}

	}

	public void PlayerRelease()
	{
		Player player = Hold.Holder;
		if (player != null && player.Holding != null && player.Holding == Hold && Hold != null)
		{
			Hold.Holder.StateMachine.State = 0;
			Hold.Release(Vector2.Zero);
			Hold.RemoveSelf();
			player.Get<Sprite>().Update();
			Hold.PickupCollider = null;
		}
	}

	private IEnumerator DestroyWithoutItems()
	{
		if (!destroyed)
		{
			base.Collider = new Circle(5f);
			base.Collider = pushRadius;
			level.Displacement.AddBurst(Position, 0.35f, 5f, 30f, 0.15f);
			_ = Hold.Holder;
			base.Collider = hitBox;
			Visible = false;
			Collidable = false;
			Vector2 saveSpeedForDeath = Speed;
			Speed = Vector2.Zero;
			PlayerRelease();
			CrystalDebris.Burst(Position + Vector2.UnitY * -10f, Calc.HexToColor("639bff"), boss: false, 5);
			destroyed = true;
			for (int i = 0; i < 10; i++)
			{
				Audio.Play("event:/game/06_reflection/fall_spike_smash", Position);
			}
			if (dieIfDrop)
			{
				Level.Tracker.GetEntity<Player>()?.Die(Vector2.Zero - saveSpeedForDeath, evenIfInvincible: true);
				yield break;
			}
			yield return 2;
			Add(new Coroutine(Respawn()));
		}
	}

	private IEnumerator Respawn()
	{
		Position = respawnPosition;
		PowBlock PowBlock = this;
		Wiggler component = Wiggler.Create(0.5f, 4f, delegate (float v)
		{
			sprite.Scale = (sprite.Scale = Vector2.One * (1f + v * 0.2f));
		}, start: true);
		PowBlock.Add(component);
		for (int i = 0; i < 5; i++)
		{
			Audio.Play("event:/game/general/assist_nonsolid_out", Position);
		}
		Visible = true;
		Collidable = true;
		Speed = Vector2.Zero;
		yield return 0.5f;
		destroyed = false;
	}

	protected override void OnSquish(CollisionData data)
	{
		if (!TrySquishWiggle(data))
		{
			Add(new Coroutine(DestroyWithoutItems()));
		}
	}

	private IEnumerator MoveEntityUp()
	{
		yield return null;
			Vector2 end = entity.Position + new Vector2(0f, -24f);
			Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeInOut, 1.2f, start: true);
			tween.OnUpdate = delegate (Tween t)
			{
				entity.Position = Vector2.Lerp(entity.Position, end, t.Eased);
			};
			tween.OnComplete = delegate
			{
			};
			Add(tween);
		
	}

	public override void Update()
	{
		base.Update();
		if (destroyed)
		{
			return;
		}
		//CheckAgainstColliders();
		Hold.CheckAgainstColliders();
		frameCount += 1f;
		if (!Hold.IsHeld)
		{
			foreach (Spring spring in springs)
			{
				if (CollideCheck(spring))
				{
					OnCollideSpring(spring);
				}
			}
		}
		hardVerticalHitSoundCooldown -= Engine.DeltaTime;
		base.Depth = 100;
		if (Hold.IsHeld)
		{
			prevLiftSpeed = Vector2.Zero;
			return;
		}
		if (OnGround())
		{
			float target = ((!OnGround(Position + Vector2.UnitX * 3f)) ? 20f : (OnGround(Position - Vector2.UnitX * 3f) ? 0f : (-20f)));
			Speed.X = Calc.Approach(Speed.X, target, 640f * Engine.DeltaTime);
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
			float num = 640f;
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
		if (base.Center.X > (float)Level.Bounds.Right)
		{
			MoveH(32f * Engine.DeltaTime);
			if (base.Right > (float)Level.Bounds.Right)
			{
				base.Right = Level.Bounds.Right;
				Speed.X *= -0.4f;
			}
		}
		else if (base.Left < (float)Level.Bounds.Left)
		{
			base.Left = Level.Bounds.Left;
			Speed.X *= -0.4f;
		}
		else if (base.Top < (float)(Level.Bounds.Top - 4))
		{
			base.Top = Level.Bounds.Top + 4;
			Speed.Y = 0f;
		}
		else if (base.Bottom > (float)Level.Bounds.Bottom + 4f)
		{
			Add(new Coroutine(DestroyWithoutItems()));
			base.Bottom = Level.Bounds.Bottom - 4;
		}
		if (base.X < (float)(Level.Bounds.Left + 10))
		{
			MoveH(32f * Engine.DeltaTime);
		}
		Player entity = base.Scene.Tracker.GetEntity<Player>();
		TempleGate templeGate = CollideFirst<TempleGate>();
		if (templeGate != null && entity != null)
		{
			templeGate.Collidable = false;
			MoveH((float)(Math.Sign(entity.X - base.X) * 32) * Engine.DeltaTime);
			templeGate.Collidable = true;
		}
	}
}
