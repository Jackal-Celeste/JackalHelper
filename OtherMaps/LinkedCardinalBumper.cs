using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.JackalHelper.Entities
{
	[CustomEntity("JackalHelper/LinkedCardinalBumper")]
	[Tracked]
	public class LinkedCardinalBumper : Entity
	{
		public static ParticleType P_Ambience;

		public static ParticleType P_Launch;

		public static ParticleType P_FireAmbience;

		public static ParticleType P_FireHit;

		private const float RespawnTime = 0.6f;

		private const float MoveCycleTime = 1.81818187f;

		private const float SineCycleFreq = 0.44f;

		private Sprite sprite;

		private VertexLight light;

		private VertexLight light2;

		private BloomPoint bloom;

		private Vector2[] positionNodes;

		private SineWave sine;

		private float respawnTimer;

		private Wiggler hitWiggler;

		private Vector2 hitDir;

		public bool travelling = true;

		public Vector2 startPos;

		public Vector2 goal = Vector2.Zero;

		public int index;

		public bool alwaysBumperBoost;
		public bool wobble;
		public bool dashing = false;

		public Sprite[] outlines;
		public LinkedCardinalBumper(Vector2 position, Vector2[] nodes, bool alwaysBumperBoost, bool wobble, string spriteDirectory) : base(position)
		{
			this.alwaysBumperBoost = alwaysBumperBoost;
			this.wobble = wobble;
			this.startPos = Position;
			positionNodes = new Vector2[nodes.Length];
			outlines = new Sprite[nodes.Length];

			for (int i = 0; i < nodes.Length; i++)
			{
				positionNodes[i] = nodes[i];
				outlines[i] = new Sprite(GFX.Game, "objects/" + spriteDirectory + "/outline");
				outlines[i].Position = nodes[i];
				outlines[i].Visible = false;

			}
			goal = positionNodes[0];
			index = 0;
			base.Collider = new Circle(12f);
			Add(new PlayerCollider(OnPlayer));
			Add(sine = new SineWave(0.44f, 0f).Randomize());
			Add(sprite = JackalModule.spriteBank.Create(spriteDirectory));
			Add(light = new VertexLight(Color.Cyan, 1f, 16, 32));
			Add(light2 = new VertexLight(Color.Indigo, 1f, 16, 32));
			Add(bloom = new BloomPoint(0.5f, 16f));
			Add(outlines);
			if (goal != null && goal != Vector2.Zero)
			{
				UpdatePosition(goal);
			}
		}

		public LinkedCardinalBumper(EntityData data, Vector2 offset) : this(data.Position + offset, data.NodesWithPosition(offset), data.Bool("alwaysBumperBoost", defaultValue: false), data.Bool("wobble", defaultValue: false), data.Attr("spriteDirectory", defaultValue: "bumperCardinal"))
		{

		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			sprite.Visible = true;
		}


		private void UpdatePosition(Vector2 position)
		{
			Vector2 path = position - Position;
			Position += path / 30f;
		}

		public override void Update()
		{
			base.Update();
			//Collidable = Vector2.Distance(Position, positionNodes[index]) < 24;
			if (respawnTimer > 0f)
			{
				respawnTimer -= Engine.DeltaTime;
				if (respawnTimer <= 0f)
				{
					light.Visible = true;
					bloom.Visible = true;
					sprite.Play("on");

					Audio.Play("event:/game/06_reflection/pinballbumper_reset", Position);

				}
			}
			else if (base.Scene.OnInterval(0.05f))
			{
				float num = Calc.Random.NextAngle();
				ParticleType type = (P_Ambience);
				float direction = (num);
				float length = (8);
				if (JackalModule.GetLevel() != null && type != null)
				{
					SceneAs<Level>().Particles.Emit(type, 1, base.Center + Calc.AngleToVector(num, length), Vector2.One * 2f, direction);
				}
			}
			if (goal == positionNodes[positionNodes.Length - 1])
			{
				Vector2[] newpositionNodes = new Vector2[positionNodes.Length];
				for (int i = 0; i < positionNodes.Length; i++)
				{
					newpositionNodes[i] = positionNodes[positionNodes.Length - i - 1];
				}
				index = 0;
				positionNodes = newpositionNodes;
				goal = positionNodes[index];
			}
			if (goal != null && goal != Vector2.Zero)
			{
				UpdatePosition(goal);
			}
		}

		private void OnPlayer(Player player)
		{
			if (respawnTimer <= 0f)
			{
				if ((base.Scene as Level).Session.Area.ID == 9)
				{
					Audio.Play("event:/game/09_core/pinballbumper_hit", Position);
				}
				else
				{
					Audio.Play("event:/game/06_reflection/pinballbumper_hit", Position);
				}
				respawnTimer = 0.7f;
				CardinalLaunch(player, Position, snapUp: false);
				player.StateMachine.State = 0;
				sprite.Play("hit", restart: true);
				light.Visible = false;
				bloom.Visible = false;
				SceneAs<Level>().Displacement.AddBurst(base.Center, 0.3f, 8f, 32f, 0.8f);
			}
		}




		public void CardinalLaunch(Player player, Vector2 from, bool snapUp = true, bool sidesOnly = false)
		{
			DynData<Player> dyn = new DynData<Player>(player);
			dyn.Set<float>("varJumpTimer", 0f);
			dashing = player.StateMachine.State == 2;
			Vector2 speed2 = dashing ? player.Speed : Vector2.Zero;
			Vector2 displacement = new Vector2(player.Position.X - base.Center.X, player.Position.Y - base.Center.Y);
			player.StateMachine.State = 7;
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
			Celeste.Freeze(0.1f);
			Vector2 vector = new Vector2(0, 0);
			//float num = Vector2.Dot(vector, Vector2.UnitY); 
			// num = dot product

			if (Math.Abs(displacement.X) >= Math.Abs(displacement.Y) || (dashing && speed2.X != 0 && speed2.Y == 0))
			{
				if (dashing)
				{
					vector.X = -1f * Math.Sign(speed2.X);
				}
				else
				{
					vector.X = 1f * Math.Sign(displacement.X);
				}
				vector.Y = -0.4f;
				player.AutoJump = true;

			}
			else if (Math.Abs(displacement.Y) > Math.Abs(displacement.X) || (dashing && speed2.Y != 0 && speed2.X == 0))
			{
				vector.X = 0f;

				if (dashing)
				{
					vector.Y = -1f * Math.Sign(speed2.Y);
				}
				else
				{
					vector.Y = 1f * Math.Sign(displacement.Y);
					
				}
				player.AutoJump = false;
				
				
			}
			Vector2 speed = vector * 350f;
			if (!player.Inventory.NoRefills)
			{
				player.RefillDash();
			}
			if (alwaysBumperBoost)
			{
				player.Speed *= 1.4f;
			}
			SlashFx.Burst(base.Center, speed.Angle());
			player.RefillStamina();
			player.Speed = speed;
			travelling = true;

			foreach (LinkedCardinalBumper bumper in JackalModule.GetLevel().Tracker.GetEntities<LinkedCardinalBumper>())
			{
				bumper.updatePos(bumper);
			}
		}
		public void updatePos(LinkedCardinalBumper bumper)
		{
			bumper.travelling = true;
			if (bumper.positionNodes.Length > 1)
			{
				if (Vector2.Distance(bumper.Position, bumper.positionNodes[index]) < 24f || JackalModule.GetLevel().Tracker.GetNearestEntity<LinkedCardinalBumper>(JackalModule.GetPlayer().Position) != this)
				{
					bumper.index += 1;
					bumper.goal = bumper.positionNodes[index];
				}
			}
		}

	}
}

