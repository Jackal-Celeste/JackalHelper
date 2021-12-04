using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.JackalHelper
{
	public class JackalModuleSaveData : EverestModuleSaveData
	{
		public HashSet<EntityID> insightCrystals = new HashSet<EntityID>();
	}
}
