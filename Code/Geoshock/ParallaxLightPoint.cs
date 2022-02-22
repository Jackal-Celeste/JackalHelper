using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.JackalHelper.Code.Geoshock
{
	[CustomEntity("JackalHelper/ParallaxLightSource")]
	public class ParallaxLightPoint : Backdrop
	{
		public VertexLight light;
		public BloomPoint bloom;
		public PLight p;

		private bool lightsOn = false;
		public ParallaxLightPoint(float x, float y, float scrollX, float scrollY, float speedX, float speedY, float alpha)
		{
			base.Position += new Vector2(x, y);
			base.Scroll = new Vector2(scrollX, scrollY);
			base.Speed = new Vector2(speedX, speedY);
			base.FadeAlphaMultiplier = Math.Min(1f, Math.Max(0f, alpha));

			bloom = new BloomPoint(alpha, 124f);
			light = new VertexLight(Color.White, alpha, 24, 48);

		}

		public override void BeforeRender(Scene scene)
		{
			if (!lightsOn)
			{
				scene.Add(p = new PLight(Position, light, bloom));
				lightsOn = true;
			}
			base.BeforeRender(scene);
		}

		public override void Update(Scene scene)
		{
			if (JackalModule.GetPlayer() != null) p.Position = JackalModule.GetPlayer().Position;
			//p.Position = Position;
		}

		public override void Render(Scene level)
		{
			
		}

	}
}
namespace Celeste.Mod.JackalHelper.Code.Geoshock
{
	public class PLight : Entity
	{
		public VertexLight light;
		public BloomPoint bloom;
		public PLight(Vector2 position, VertexLight l, BloomPoint b) : base(position)
		{
			Add(light = l);
			Add(bloom = b);

		}
	}
}