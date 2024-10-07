using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace TwigHelper.ARC_Project
{
	[Tracked]
	[CustomEntity("TwigHelper/NodedBadelineBoss")]
    public class  NodedBadelineBoss : Entity
{

	private const float MoveSpeed = 600f;

	private const float AvoidRadius = 12f;

	public Sprite Sprite;


	private Vector2 avoidPos;

	public bool Moving;

	public bool Sitting;

	private int facing;

	private Level level;

	private Circle circle;

		private Hitbox rectangle;
		private ColliderList list;
		private Hitbox swipeAnim;

		public BossNode node;
		public BossNode goTo;

	private int patternIndex;

	private Coroutine attackCoroutine;

	private bool playerHasMoved;

	private SineWave floatSine;


	private VertexLight light;

	private Wiggler scaleWiggler;

		private bool nodesSet;
		public bool leaping = false;
		public bool swiping = false;


	public  NodedBadelineBoss(Vector2 position)
		: base(position)
	{
		this.patternIndex = 10;
		Add(light = new VertexLight(Color.White, 1f, 32, 64));
			circle = new Circle(14f, 0f,4f);
			swipeAnim = new Hitbox(48f, 40f, -28f, -16f);
			rectangle = new Hitbox(44f, 40f, -24f, -16f);
			list = new ColliderList(circle, rectangle, swipeAnim);
		base.Collider = list;
		attackCoroutine = new Coroutine(removeOnComplete: false);
			Add(new PlayerCollider(OnPlayer));
			Add(attackCoroutine);
		Add(floatSine = new SineWave(0.6f, 0f));
		Add(scaleWiggler = Wiggler.Create(0.6f, 3f));
	}

	public  NodedBadelineBoss(EntityData e, Vector2 offset) : this(e.Position + offset)
	{
	}

		public void SetNodes(Vector2 position)
        {
			node = TwigModule.GetLevel()?.Tracker.GetNearestEntity<BossNode>(position);
			
				goTo = node.GetComponents().Count > 0 ? node.GetComponents()[0] : node;
			
		}

		public void OnPlayer(Player player)
		{
			if(Moving) player.Die((player.Center - Position).SafeNormalize());
			base.Collider = new ColliderList(rectangle, circle);
            if (base.Collider.Collide(player))
            {
				player.Die((player.Center - Position).SafeNormalize());
			}
            else
            {
				Sprite.Play("bossSwipe");
				Add(new Coroutine(Wait(player)));
				if(kill) player.Die((player.Center - Position).SafeNormalize());
			}
			base.Collider = list;
		}


		public override void Added(Scene scene)
	{
		base.Added(scene);
		level = SceneAs<Level>();

			CreateBossSprite();
		

		light.Position = Sprite.Position + new Vector2(0f, -10f);
	}


	private void CreateBossSprite()
	{
		Add(Sprite = TwigModule.spriteBank.Create("cryobot_boss"));
			Sprite.Play("idleBoss");
		facing = -1;
	}


	public bool CheckForPlayer(Player p)
    {
			
			float leniency = 16f;
			if (node == goTo) return false;
			if (goTo.X > node.X && goTo.X > p.X && p.X > node.X)
			{
				//player in between left node and right aim
				//node, p, goto
				float targetY = Math.Max(goTo.Y, node.Y) - Math.Abs(goTo.Y - node.Y) * ((p.X - node.X) / (goTo.X - node.X));
				return (p.Y < targetY + leniency && p.Y > targetY - leniency);
			}
			else if(goTo.X < node.X && p.X < node.X && p.X > goTo.X)
            {
				//goTo, p, node
				float targetY = Math.Max(goTo.Y, node.Y) - Math.Abs(goTo.Y - node.Y) * ((p.X - goTo.X) / (node.X - goTo.X));
				return (p.Y < targetY + leniency && p.Y > targetY - leniency);
			}
			else if(goTo.X == node.X && node.Y > p.Y && p.Y > goTo.Y)
            {
				//node, p, goTo up
				return p.X > node.X - leniency && p.X < node.X + leniency;
			}
			else if (goTo.X == node.X && node.Y < p.Y && p.Y < goTo.Y)
			{
				//node, p, goTo down
				return p.X > node.X - leniency && p.X < node.X + leniency;
			}
			return false;

			

    }
		public void UpdateNodes(BossNode n, Player p)
        {
			Vector2[] proxies = new Vector2[n.GetComponents().Count];
			int closestIndex = 0;
			for(int i = 0; i < proxies.Length; i++)
            {
				Vector2 offset = n.GetComponents()[i].Position - n.Position;
				offset.Normalize();
				offset *= 10;
				proxies[i] = n.Position + offset;
            }
			for(int i = 1; i < proxies.Length; i++)
            {

				float a0 = Vector2.Distance(proxies[closestIndex], p.Position);
				float a1 =Vector2.Distance(proxies[i], p.Position);
				if (a1 < a0) closestIndex = i;
            }
			goTo = n.GetComponents()[closestIndex];
		}

		public bool kill = false;

		public IEnumerator Wait(Player p)
        {

			yield return 0.3f;
            if (base.Collider.Collide(p))
            {
				kill = true;
			}
        }

	public override void Update()
	{
			if(Sprite.Scale.X < 0)
            {
				//facing right
				rectangle = new Hitbox(44f, 40f, -20f, -16f);
				swipeAnim = new Hitbox(48f, 40f, -20f, -16f);

			}
            else
            {
				rectangle = new Hitbox(44f, 40f, -24f, -16f);
				swipeAnim = new Hitbox(48f, 40f, -28f, -16f);
			}
			Player p = TwigModule.GetPlayer();
			base.Update();
			if(TwigModule.GetLevel() != null && !nodesSet)
            {
				SetNodes(Position);
				nodesSet = true;
			}
			if (p != null && nodesSet && node.GetComponents().Count > 1 && !leaping)
            {
				UpdateNodes(node, p);
            }
			if(p != null && node != null && goTo != null)
            {
                if (CheckForPlayer(p) && !leaping)
                {
					SeesPlayer(p);
                }
            }
			Sprite sprite = Sprite;
		if (!Sitting)
		{
			Player entity = base.Scene.Tracker.GetEntity<Player>();
			if (!Moving && entity != null)
			{
				if (facing == -1 && entity.X > base.X + 20f)
				{
					facing = 1;
					scaleWiggler.Start();
				}
				else if (facing == 1 && entity.X < base.X - 20f)
				{
					facing = -1;
					scaleWiggler.Start();
				}
			}
			if (!playerHasMoved && entity != null && entity.Speed != Vector2.Zero)
			{
				playerHasMoved = true;
				if (patternIndex != 0)
				{
					StartAttacking();
				}
			}
			if(sprite != null)
			{
				sprite.Position = Calc.Approach(sprite.Position, Vector2.Zero, 12f * Engine.DeltaTime);
			}
			float radius = circle.Radius;
			circle.Radius = 6f;
			CollideFirst<DashBlock>()?.Break(base.Center, -Vector2.UnitY, true, true);
			circle.Radius = radius;
			if (!level.IsInBounds(Position, 24f))
			{
				Active = (Visible = (Collidable = false));
				return;
			}
			Vector2 target;
			if (!Moving && entity != null)
			{
				float val = (base.Center - entity.Center).Length();
				val = Calc.ClampedMap(val, 32f, 88f, 12f, 0f);
				target = ((!(val <= 0f)) ? (base.Center - entity.Center).SafeNormalize(val) : Vector2.Zero);
			}
			else
			{
				target = Vector2.Zero;
			}
			avoidPos = Calc.Approach(avoidPos, target, 40f * Engine.DeltaTime);
		}
		if(Sprite != null) light.Position = sprite.Position + new Vector2(0f, -10f);
	}

	public override void Render()
	{
		if (Sprite != null)
		{
			Sprite.Scale.X = -facing;
			Sprite.Scale.Y = 1f;
			Sprite.Scale *= 1f + scaleWiggler.Value * 0.2f;
		}

			base.Render();
			if (node != null && goTo != null)
			{
				Draw.Circle(node.Position, 4f, Color.Red, 20);
				Draw.Circle(goTo.Position, 4f, Color.Blue, 20);
			}
		
	}

	public void SeesPlayer(Player player)
	{
			leaping = true;
		if (Sprite == null)
		{
			CreateBossSprite();
		}
		Audio.Play("event:/char/badeline/boss_hug", Position);
		Collidable = false;
		avoidPos = Vector2.Zero;
		attackCoroutine.Active = false;
		Moving = true;
			base.Collider = circle;
			Add(new Coroutine(MoveSequence(player)));
	}


		private IEnumerator MoveSequence(Player player)
		{
			Sprite.Play("bossLunge");
			level.Shake();
			yield return 0.2f;
			Vector2 from = node.Position;
			Vector2 to = goTo.Position;
			float duration = Vector2.Distance(from, to) / 400f;
			float dir = (to - from).Angle();
			Tween tween4 = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, duration, start: true);
			tween4.OnUpdate = delegate (Tween t)
			{
				Collidable = true;
				Position = Vector2.Lerp(from, to, t.Eased);
				if (t.Eased >= 0.1f && t.Eased <= 0.9f && Scene.OnInterval(0.02f))
				{
					TrailManager.Add(this, Player.NormalHairColor, 0.5f, frozenUpdate: false, useRawDeltaTime: false);
					level.Particles.Emit(Player.P_DashB, 2, Center, Vector2.One * 3f, dir);
				}
			};
			tween4.OnComplete = delegate
			{
				Moving = false;
				Collidable = true;
				Player entity = Scene.Tracker.GetEntity<Player>();
				if (entity != null)
				{
					facing = Math.Sign(entity.X - X);
					if (facing == 0)
					{
						facing = -1;
					}
				}
				StartAttacking();
				floatSine.Reset();
				leaping = false;
				SetNodes(Position);
				base.Collider = list;
			};
			Add(tween4);
		}



		private void StartAttacking()
	{
		attackCoroutine.Replace(Attack10Sequence());
	}



	private IEnumerator Attack10Sequence()
	{
		yield break;
	}

}

}
