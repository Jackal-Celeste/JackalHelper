using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.JackalHelper.Triggers
{
	[CustomEntity("JackalHelper/BlizzardRemovalTrigger.cs")]
	public class BlizzardRemovalTrigger : Trigger
	{
		Level level;

		public BlizzardRemovalTrigger(EntityData data, Vector2 offset)
			: base(data, offset)
		{ }


		public void SetColorGrade(string str, float speed)
		{
			level.NextColorGrade(str, speed);
		}

		public void SetColorGrade(string str)
		{
			level.NextColorGrade(str);
		}

		public override void OnEnter(Player player)
		{
			level = Scene as Level;
			level.Wind.X = 0f;
			level.Wind.Y = 0f;
			WindController windController = level.Entities.FindFirst<WindController>();
			level.Remove(windController);
			WindController calm = new WindController(WindController.Patterns.None);
			level.Add(calm);
			SetColorGrade("cryoBase");
		}

	}

}