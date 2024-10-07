using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;

namespace Celeste.Mod.TwigHelper
{
	[SettingName("TwigHelper_SettingName1")]
	public class TwigModuleSettings : EverestModuleSettings
	{
		[DefaultButtonBinding(Buttons.LeftTrigger, Keys.S)]
		public ButtonBinding LockOnFireKey { get; set; }
	}
}
