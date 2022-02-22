using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using Celeste.Mod.JackalHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.JackalHelper.Code.Geoshock
{
	[Tracked]
	[CustomEntity("JackalHelper/ParallaxLight")]
	public class ParrallaxLight : Entity
	{
		private readonly float alpha;

		private BloomPoint bloom;

		private readonly Color color;

		private VertexLight light;

		private readonly float radius;

		public ParrallaxLight(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			alpha = data.Float("alpha", 1f);
			radius = data.Float("radius", 48f);
			color = Calc.HexToColor(data.Attr("color", "ffffff"));
			Add(bloom = new BloomPoint(alpha, radius));
			Add(light = new VertexLight(color, alpha, data.Int("startFade", 24), data.Int("endFade", 48)));
		}

		public override void Update()
		{
			base.Update();
			Level l = JackalModule.GetLevel();
			if(l != null)
			{
				
			}
		}
	}
}
