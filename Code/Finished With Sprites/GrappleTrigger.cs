using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.JackalHelper.Code.Finished_With_Sprites
{
	[CustomEntity("JackalHelper/GrappleTrigger")]
	public class GrappleTrigger : Trigger
	{
		public bool removeOnLeave = false;
		public GrappleTrigger(EntityData data, Vector2 offset) : base(data, offset)
		{
			removeOnLeave = data.Bool("removeOnLeave", defaultValue: false);
		}
		public override void OnEnter(Player player)
		{
			base.OnEnter(player);
			JackalModule.Session.hasGrapple = true;
		}
		public override void OnLeave(Player player)
		{
			base.OnLeave(player);
			if (removeOnLeave) JackalModule.Session.hasGrapple = false;
		}
	}
}
