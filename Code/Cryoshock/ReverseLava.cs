using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.JackalHelper.Entities
{
	[CustomEntity("JackalHelper/RevLava")]
	public class ReverseLava : FireBarrier
	{

		public ReverseLava(Vector2 position, float width, float height)
			: base(position, width, height)
		{
			Collidable = true;
		}

		public ReverseLava(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Width, data.Height)
		{
		}//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		 //IL_0007: Unknown result type (might be due to invalid IL or missing references)
		 //IL_0008: Unknown result type (might be due to invalid IL or missing references)


		public override void Added(Scene scene)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			base.Added(scene);
			Collidable = true;
		}


		private void OnPlayer(Player player)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			player.Die((player.Center - base.Center).SafeNormalize());
		}

		public override void Update()
		{
			base.Update();
			Collidable = true;
		}

		public override void Render()
		{
			Collidable = true;
			base.Render();
		}
	}
}
