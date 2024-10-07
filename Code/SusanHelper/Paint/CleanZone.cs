using Celeste;
using Celeste.Mod.Entities;
using Celeste.Mod.SusanHelper;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using static SusanHelper.Entities.Paint.PaintSource;
using static SusanHelper.Entities.Paint.PaintBall;
using SusanHelper.Entities.Paint;

namespace Celeste.Mod.SusanHelperNew.Paint
{
	[CustomEntity("SusanHelper/CleanZone")]
	public class CleanZone : Entity
	{
		private bool keepRainbow;
		public CleanZone(EntityData data, Vector2 offset) : base(data.Position + offset)
		{
			Collider = new Hitbox(data.Width, data.Height);
			keepRainbow = data.Bool("keepRainbow", defaultValue: false);
		}
        public override void Added(Scene scene)
        {
            base.Added(scene);
			foreach(PaintLiquid l in (scene as Level).Tracker.GetEntities<PaintLiquid>())
			{
				if (l.CollideCheck(this) && (!keepRainbow || l.evil)) l.RemoveSelf();
			}
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
			Draw.Rect(Collider, Color.LightCoral * 0.5f);
            Draw.HollowRect(Collider, Color.Coral);
        }
    }
}

