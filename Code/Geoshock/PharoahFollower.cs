using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Reflection;

namespace Celeste.Mod.JackalHelper.Code.Geoshock
{
	[Tracked]
	public class PharoahFollower : Entity
	{
		public const string SESSION_FLAG = "has_pharoah_follower";

		private float previousPosition;

		public Follower follower { get; private set; }

		public static PharoahFollower instance { get; set; }

		public PharoahDummy dummy { get; private set; }




		[Command("spawn_follower_pharoah", "spawn pharoah follower")]
		public static void CmdSpawnPharoah()
		{
			Level level = Engine.Scene as Level;
			SpawnPharoahFriendo(level);
		}

		public static void SpawnPharoahFriendo(Level _level)
		{
			Player player = JackalModule.GetPlayer();
			if (_level != null)
			{
				_level.Session.SetFlag("has_pharoah_follower");
				if (player != null)
				{
					PharoahFollower follower = new PharoahFollower(_level, player.Position + new Vector2(player.Facing == Facings.Right ? -12 : 12, -12f));
					_level.Add(follower);
					player.Leader.GainFollower(follower.follower);
				}
			}
		}


		public void Readd(Level lvl, Player obj)
		{
			lvl.Add(this);
			lvl.Add(dummy);
			obj.Leader.GainFollower(follower);
			dummy.Position = obj.Position - new Vector2((obj.Facing == Facings.Left) ? (-32) : 32, 32f);
		}

		public PharoahFollower(Level level, Vector2 position)
			: this(level, new PharoahDummy(position), position)
		{
		}

		public PharoahFollower(Level level, PharoahDummy _dummy, Vector2 position)
			: base(position)
		{
			level.Session.SetFlag("has_pharoah_follower");
			level.Add(dummy = _dummy);
			dummy.Add(this.follower = new Follower());
			this.follower.PersistentFollow = true;
			this.follower.Added(dummy);
			AddTag(Tags.Persistent);
			dummy.AddTag(Tags.Persistent);
			instance = this;
		}

		public override void Update()
		{
			if (JackalModule.GetPlayer() != null && (follower.Leader == null || follower.Leader.Entity != JackalModule.GetPlayer()))
			{
				JackalModule.GetPlayer().Leader.GainFollower(follower);
			}
			if ((previousPosition - dummy.Position.X) * dummy.Sprite.Scale.X > 0f)
			{
				dummy.Sprite.Scale.X *= -1f;
			}
			previousPosition = dummy.Position.X;

			base.Update();
		}
	}


	public class PharoahDummy : Entity
	{
		public Sprite Sprite;

		public SineWave Wave;

		public VertexLight Light;

		public float FloatSpeed = 120f;

		public float FloatAccel = 240f;

		public float Floatness = 2f;

		public Vector2 floatNormal = new Vector2(0f, 1f);

		public VertexLight bp;

		public PharoahDummy(Vector2 position)
			: base(position)
		{
			Depth = 50;
			base.Collider = new Hitbox(6f, 6f, -3f, -7f);
			Sprite = JackalModule.spriteBank.Create("pharoah");
			Sprite.Play("idleFree");
			Sprite.Scale.X = -1f;
			Position.Y -= 4f;
			Add(Sprite);
			Add(Wave = new SineWave(0.25f, 0f));
			Wave.OnUpdate = delegate (float f)
			{
				Sprite.Position = floatNormal * -2f*f * Floatness - Vector2.UnitY * 4f;
			};
			Add(Light = new VertexLight(new Vector2(0f, -8f), Color.Gold, 1f, 20, 60));
			Add(bp = new VertexLight(new Vector2(0f, -8f), Color.Orange, 1f, 10, 70));
			VertexLight bp2;
			Add(bp2 = new VertexLight(new Vector2(0f, -8f), Color.Black, 1f, 30, 50));
			Appear(JackalModule.GetLevel(), false);
		}

		public void Appear(Level level, bool silent = false)
		{
			if (!silent)
			{
				Audio.Play("event:/char/badeline/appear", Position);
				Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
			}
			level.Displacement.AddBurst(base.Center, 0.5f, 24f, 96f, 0.4f);
			ParticleType vanish = new ParticleType(BadelineOldsite.P_Vanish);
			vanish.Color = Color.Black;
			vanish.Color2 = Color.Gold;
			level.Particles.Emit(vanish, 12, base.Center, Vector2.One * 6f);
		}

		public void Vanish()
		{
			Audio.Play("event:/char/badeline/disappear", Position);
			Shockwave();
			ParticleType vanish = new ParticleType(BadelineOldsite.P_Vanish);
			vanish.Color = Color.Black;
			vanish.Color2 = Color.Gold;
			SceneAs<Level>().Particles.Emit(vanish, 12, base.Center, Vector2.One * 6f);
			RemoveSelf();
		}

		private void Shockwave()
		{
			SceneAs<Level>().Displacement.AddBurst(base.Center, 0.5f, 24f, 96f, 0.4f);
		}
		public override void Render()
		{
			Vector2 renderPosition = Sprite.RenderPosition;
			Sprite.RenderPosition = Sprite.RenderPosition.Floor();
			base.Render();
			Sprite.RenderPosition = renderPosition;
		}
	}

}