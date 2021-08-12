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

		public CryoShell(EntityData data, Vector2 offset) 
			: base(data.Position + offset, data.Width, data.Height)
		{ }

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
			foreach (CryoBooster entity in Scene.Tracker.GetEntities<CryoBooster>())
			{
				if (entity.FrozenDash)
				{
					cryoBoosting = true;
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

			if (JackalModule.TryGetPlayer(out Player player))
			{
				Collidable = (JackalModule.Session.HasCryoDash || JackalModule.Session.CryoDashActive || cryoBoosting || player.StateMachine.State == JackalModule.CryoBoostState) &&
					(Vector2.DistanceSquared(player.Position, center) < Math.Pow(JackalModule.Session.CryoRadius, 2.0));
				Visible = true;
			}
			base.Update();
		}
	}
}
