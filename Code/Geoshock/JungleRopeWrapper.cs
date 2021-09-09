using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.JackalHelper.Code.Geoshock
{
	[Tracked]
	public class JungleRopeWrapper : Entity
	{
		#region CornerBoostVars

		/** must be <=1 and > 0, rate at which stored speed decays*/
		public float curvature;

		/** time for all speed to decay */
		public float time;

		/** generates stored speed */
		private double bonusSpeed(float spdX, float time)
		{
			return spdX * Math.Pow(this.time, -2.0) * (this.time + time) * (this.time - time) * Math.Pow(curvature, time);
		}

		#endregion CornerBoostVars

		public bool staminaLock;

		public bool sameDirBoost;

		public JungleRopeWrapper(Vector2 position, float b, float c, bool stam, bool boost) : base(position)
		{
			curvature = c;
			time = b;
			staminaLock = stam;
			sameDirBoost = boost;
		}

	}
}
