using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.JackalHelper.Entities
{

	[CustomEntity("JackalHelper/CryoLava")]
	public class ObsidianBlock : FireBarrier
	{
		private ClimbBlocker climbBlocker;
		public Player player;
		public float radius = JackalModule.Session.CryoRadius;
		public Level level;
		public Vector2 rightVertex;
		private DynData<FireBarrier> solidData;

		public float linSpdY = 0f;
		public float linSpdX = 0f;
		public float sineAmpY = 0f;
		public float sineFreqY = 0f;
		public float sineAmpX = 0f;
		public float sineFreqX = 0f;
		public float cumulativeTime = 0f;
		public float coolDistance = 32f;

		public bool safe = false;





		public ObsidianBlock(Vector2 position, float width, float height, float linSpdY, float linSpdX, float sineAmpY, float sineFreqY, float sineAmpX, float sineFreqX, float coolDistance) : base(position, width, height)
		{
			level = SceneAs<Level>();
			this.linSpdY = linSpdY;
			this.linSpdX = linSpdX;
			this.sineAmpY = sineAmpY;
			this.sineFreqY = sineFreqY;
			this.sineAmpX = sineAmpX;
			this.sineFreqX = sineFreqX;
			this.coolDistance = coolDistance;

		}

		public ObsidianBlock(EntityData data, Vector2 offset) : this(data.Position + offset, data.Width, data.Height, data.Float("linSpeedY", defaultValue: 0f), data.Float("linSpeedX", defaultValue: 0f), data.Float("sineAmplitudeY", defaultValue: 0f), data.Float("sineFrequencyY", defaultValue: 0f), data.Float("sineAmplitudeX", defaultValue: 0f), data.Float("sineFrequencyX", defaultValue: 0f), data.Float("coolDistance", defaultValue: 32f))
		{
		}

		public override void Added(Scene scene)
		{
			sineAmpX *= 0.25f;
			sineAmpY *= 0.25f;
			base.Added(scene);
			Depth = 250;
			Add(climbBlocker = new ClimbBlocker(false));
			level = SceneAs<Level>();
			//solid = level.Tracker.GetEntity<Solid>();
			Vector2 vector2 = Position;
			vector2.X -= 3;
			safe = false;


		}

		public override void Awake(Scene scene)
		{
			level = SceneAs<Level>();
			base.Awake(scene);
			rightVertex = new Vector2(Position.X + Width, Position.Y);
		}

		public override void Update()
		{
			climbBlocker.Blocking = JackalModule.Session.HasCryoDash;
			Motion();

			if (JackalModule.GetLevel() != null)
			{
				solidData = new DynData<FireBarrier>(this);

				solidData.Get<LavaRect>("Lava").CenterColor = colorCheck(Calc.HexToColor("d01c01"), Calc.HexToColor("101010"));
				solidData.Get<LavaRect>("Lava").SurfaceColor = colorCheck(Calc.HexToColor("ff8933"), Color.DarkRed);
				solidData.Get<LavaRect>("Lava").EdgeColor = colorCheck(Calc.HexToColor("f25e29"), Color.Black);

				if (safe)
				{
					solidData.Get<LavaRect>("Lava").UpdateMultiplier = 0f;
					solidData.Get<LavaRect>("Lava").CenterColor = Calc.HexToColor("101010");
					solidData.Get<LavaRect>("Lava").SurfaceColor = Color.DarkRed;
					solidData.Get<LavaRect>("Lava").EdgeColor = Color.Black;
				}
				if (JackalModule.GetPlayer() != null)
				{
					Vector2 min = nearestEdge(JackalModule.GetPlayer(), out float distance);
					if (distance < coolDistance && (JackalModule.Session.HasCryoDash || JackalModule.Session.CryoDashActive || JackalModule.GetPlayer().StateMachine.State == JackalModule.CryoBoostState))
					{
						safe = true;
					}
				}
			}
			base.Update();
		}

		public Color colorCheck(Color orig, Color goal)
		{
			if (JackalModule.GetPlayer() != null)
			{
				Vector2 min = nearestEdge(JackalModule.GetPlayer(), out float p);


				bool inRange = (JackalModule.Session.HasCryoDash || JackalModule.Session.CryoDashActive || JackalModule.GetPlayer().StateMachine.State == JackalModule.CryoBoostState);
				if (p < (4 * coolDistance) && inRange)
				{
					solidData = new DynData<FireBarrier>(this);
					solidData.Get<LavaRect>("Lava").UpdateMultiplier = ((p - coolDistance) / (3 * coolDistance));
					return Color.Lerp(goal, orig, ((p - coolDistance) / (3 * coolDistance)));
				}
			}
			return orig;

		}

		public Vector2 nearestEdge(Player player, out float p)
		{
			Vector2 min = TopLeft;
			float smallest = Vector2.Distance(player.Position, min);
			if (smallest > Vector2.Distance(player.Position, TopCenter))
			{
				min = TopCenter;
				smallest = Vector2.Distance(player.Position, TopCenter);
			}
			if (smallest > Vector2.Distance(player.Position, TopRight))
			{
				min = TopRight;
				smallest = Vector2.Distance(player.Position, TopRight);
			}
			if (smallest > Vector2.Distance(player.Position, CenterRight))
			{
				min = CenterRight;
				smallest = Vector2.Distance(player.Position, CenterRight);
			}
			if (smallest > Vector2.Distance(player.Position, BottomRight))
			{
				min = BottomRight;
				smallest = Vector2.Distance(player.Position, BottomRight);
			}
			if (smallest > Vector2.Distance(player.Position, BottomCenter))
			{
				min = BottomCenter;
				smallest = Vector2.Distance(player.Position, BottomCenter);
			}
			if (smallest > Vector2.Distance(player.Position, BottomLeft))
			{
				min = BottomLeft;
				smallest = Vector2.Distance(player.Position, BottomLeft);
			}
			if (smallest > Vector2.Distance(player.Position, CenterLeft))
			{
				min = CenterLeft;
				smallest = Vector2.Distance(player.Position, CenterLeft);
			}
			p = smallest;
			return min;
		}


		public void Motion()
		{
			float linDeltaX = linSpdX * Engine.DeltaTime;
			float linDeltaY = linSpdY * Engine.DeltaTime;
			float sinDeltaX = sineAmpX * (float)Math.Sin(sineFreqX * cumulativeTime);
			float sinDeltaY = sineAmpY * (float)Math.Sin((sineFreqY * cumulativeTime) + Math.PI / 2);
			float totalDeltaX = linDeltaX + sinDeltaX;
			float totalDeltaY = linDeltaY + sinDeltaY;


			X += totalDeltaX;
			Y += totalDeltaY;
			solidData = new DynData<FireBarrier>(this);
			solidData.Get<Solid>("solid").Speed.X = 0;
			solidData.Get<Solid>("solid").Speed.Y = 0;
			solidData.Get<Solid>("solid").MoveH(totalDeltaX);
			solidData.Get<Solid>("solid").MoveV(totalDeltaY);


			//its not really ligning up, normal motion is fine but the y speed is wack, x motion also happens to drift


			cumulativeTime += Engine.DeltaTime;


		}
	}
}
