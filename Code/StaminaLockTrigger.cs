using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.JackalHelper.Code
{
	[CustomEntity("JackalHelper/StaminaLockTrigger")]
	public class StaminaLockTrigger : Trigger
	{
		public StaminaLockTrigger(EntityData data, Vector2 offset)
			: base(data, offset)
		{
			JackalModule.Session.inStaminaZone = false;
		}

		public override void OnEnter(Player player)
		{
			JackalModule.Session.inStaminaZone = true;
			base.OnEnter(player);
		}

		public override void OnLeave(Player player)
		{
			JackalModule.Session.inStaminaZone = false;
			base.OnLeave(player);
		}
	}
}
