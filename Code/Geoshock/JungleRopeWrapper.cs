using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.JungleHelper;
using MonoMod.RuntimeDetour;
using System.Reflection;


namespace Celeste.Mod.JackalHelper.Code.Geoshock
{
	[Tracked]
	[CustomEntity("JackalHelper/JungleRopeWrapper")]
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

		public bool sameDirBoost;

		public string staminaBehavior;

		public float initialStam = -1f;

		public bool stamStored = false;
		public JungleRopeWrapper(Vector2 position, float b, float c, string stam, bool boost) : base(position)
		{
			curvature = c;
			time = b;
			staminaBehavior = stam.ToLower();
			sameDirBoost = boost;
			if (staminaBehavior == "conserve") initialStam++;
		}
		public JungleRopeWrapper(EntityData data, Vector2 offset) : this(data.Position + offset, data.Float("decayTime", defaultValue: 0.5f), data.Float("decayCurvature", defaultValue: 0.7f), data.Attr("staminaBehavior", defaultValue: "conserve"), data.Bool("sameDirectionJumpBoost", defaultValue: true))
		{

		}

		public override void Update()
		{
			base.Update();
			if (stamStored && JackalModule.GetPlayer() != null) stamStored = Input.GrabCheck && JackalModule.GetPlayer().StateMachine.State == 1;
		}

	}
}
