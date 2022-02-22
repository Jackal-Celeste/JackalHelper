using Celeste.Mod;
using Celeste.Mod.JackalHelper.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.JackalHelper
{
	public class JackalHelperSession : EverestModuleSession
	{

		public bool hasGrapple { get; set; } = false;

		public bool HasCryoDash { get; set; } = false;
		public bool CryoDashActive { get; set; } = false;
		public float CryoRadius { get; set; } = 25f;

		public bool PowerDashActive { get; set; } = false;
		public bool HasPowerDash { get; set; } = false;

		public float lastBird { get; set; } = 0f;
		public float lastAltBird { get; set; } = 0f;

		public Color color { get; set; } = Color.White;

		public bool dashQueue { get; set; } = false;

		public bool grappleStored { get; set; } = false;

		public bool inStaminaZone { get; set; } = false;

		public float regrabTimer { get; set; } = 0.2f;

		public Vector2[] currentHBNodes { get; set; } = null;
	}
}