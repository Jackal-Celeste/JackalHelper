using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;



namespace Celeste.Mod.JackalHelper.Triggers
{
	[CustomEntity("JackalHelper/BlizzardTrigger.cs")]

	public class BlizzardTrigger : Trigger
	{
		Level level;

		public WindController.Patterns Pattern;

		public bool heartCheck;

		public string flag;

		public bool dormant;

		public BlizzardTrigger(EntityData data, Vector2 offset)
			: base(data, offset)
		{
			Pattern = data.Enum("pattern", WindController.Patterns.None);
			dormant = (data.Bool("heart"));
		}


		public void SetColorGrade(string str)
		{
			level.NextColorGrade(str);
		}

		public override void OnStay(Player player)
		{
			//dormant set to true if data.Bool("heart")
			level = base.Scene as Level;
			heartCheck = level.Session.GetFlag("flagged");
			if (heartCheck)
			{
				dormant = false;
			}
			if (!dormant)
			{
				StartBlizzard(player, level);
			}
		}

		public void StartBlizzard(Player player, Level level)
		{
			WindController windController = base.Scene.Entities.FindFirst<WindController>();
			//SetColorGrade("cryo");
			if (windController == null)
			{
				windController = new WindController(Pattern);
				base.Scene.Add(windController);
			}
			windController.SetPattern(Pattern);
		}

	}

}