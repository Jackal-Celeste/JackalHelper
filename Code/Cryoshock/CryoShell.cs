using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.JackalHelper.Entities
{
	public class CryoShell : IceBlock
	{

		private Solid solid;

		public Player player;
		public Level level;
		public Vector2 center;
		public CryoShell(Vector2 position, float width, float height)
	: base(position, width, height)
		{

		}
		public CryoShell(EntityData data, Vector2 offset) : this(data.Position + offset, data.Width, data.Height)
		{
		}

		public override void Added(Scene scene)
		{
			if (Width < 2f)
			{
				center = new Vector2(Position.X, Position.Y + (Height / 2));
			}
			else if (Height < 2f)
			{
				center = new Vector2(Position.X + (Width / 2), Position.Y);
			}

			base.Added(scene);
			scene.Add(solid = new Solid(Position + new Vector2(2f, 3f), base.Width - 4f, base.Height - 5f, false));
			Collidable = (solid.Collidable = (SceneAs<Level>().CoreMode == Session.CoreModes.Cold));
			Depth = 100;
			level = SceneAs<Level>();
		}

		public override void Update()
		{
			bool cryoBoosting = false;

			if (JackalModule.GetLevel() != null)
			{
				foreach (CryoBooster entity in JackalModule.GetLevel().Tracker.GetEntities<CryoBooster>())
				{
					if (entity.FrozenDash)
					{
						cryoBoosting = true;
					}
				}
			}
			if (Width < 2f)
			{
				center = new Vector2(Position.X, Position.Y + (Height / 2));
			}
			else if (Height < 2f)
			{
				center = new Vector2(Position.X + (Width / 2), Position.Y);
			}
			if (JackalModule.GetPlayer() != null)
			{
				player = level.Tracker.GetEntity<Player>();

				Collidable = (JackalModule.GetPlayer() != null) && (JackalModule.Session.HasCryoDash || JackalModule.Session.CryoDashActive || cryoBoosting || player.StateMachine.State == JackalModule.cryoBoostState) && (Vector2.DistanceSquared(player.Position, center) < Math.Pow(JackalModule.Session.CryoRadius, 2.0));
				Visible = (JackalModule.GetPlayer() != null);
			}
			base.Update();
		}
	}
}
