using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;

namespace Celeste.Mod.JackalHelper
{
	[SettingName("JackalHelper_SettingName1")]
	public class JackalModuleSettings : EverestModuleSettings
	{
		//[SettingSubText("JackalHelper_SettingNameSub1")]
		//[SettingInGame(true)] // Only show this in the in-game menu.
		//public bool ResetOnChapterLoad { get; set; } = false;

		[DefaultButtonBinding(Buttons.LeftTrigger, Keys.D)]
		public ButtonBinding SwapCrystalToggle { get; set; }
	}
}
