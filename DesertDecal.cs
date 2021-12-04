using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste;
using Celeste.Mod.Entities;

namespace Celeste.Mod.JackalHelper.Entities
{
	[Tracked]
	[CustomEntity("JackalHelper/DesertDecal")]
	public class DesertDecal : Entity
	{
		private char[] letters;
		private Sprite[] sprites;
		public DesertDecal(Vector2 position, String name) : base(position)
		{
			letters = name.ToLower().Trim().ToCharArray();
			sprites = new Sprite[name.Length];
			Vector2 offset = Vector2.Zero;
			int getvalue;
			string basePath = "JackalHelper/desertDecal/CryoBytes/";
			for (int i = 0; i < letters.Length; i++)
			{
				getvalue = (int)letters[i] - 97;
				String extraZero = getvalue < 10 ? "0" : "";
				if (i == 0)
				{
					sprites[i] = new Sprite(GFX.Game, basePath + "start/cryoBytes_end" + extraZero + getvalue);
				}
				else if (i == letters.Length - 1)
				{
					sprites[i] = new Sprite(GFX.Game, basePath + "end/cryoBytes_end" + extraZero + getvalue);
				}
				else
				{
					sprites[i] = new Sprite(GFX.Game, basePath + "mid/cryoBytes_mid" + extraZero + getvalue);
				}
				sprites[i].Position = this.Position + offset;
				offset.X += 8f;
				Console.WriteLine(sprites[i].Position.X);
				sprites[i].Visible = true;
			}
			Add(sprites);

		}

		public override void Added(Scene scene)
		{
		}

		public DesertDecal(EntityData data, Vector2 offset) : this(data.Position + offset, data.Attr("name"))
		{

		}

	}
}
