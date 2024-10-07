using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace TwigHelper.ARC_Project
{
	[Tracked]
	[CustomEntity("TwigHelper/ElectricBlock")]
	public class ElectricBlock : Entity
	{
		public static ParticleType P_Deactivate;

		private Solid solid;

		private List<Sprite> eSprites = new List<Sprite>();

		private readonly Vector2 offset = 4 * Vector2.One;


		public ElectricBlock(Vector2 position, int width, int height)
			: base(position)
		{
			base.Tag = Tags.TransitionUpdate;
			base.Collider = new Hitbox(width, height);
			Add(new PlayerCollider(OnPlayer));
			base.Depth = -8500;
			int h = height / 8;
			int w = width / 8;
			Sprite[,] sprites = new Sprite[h,w];
			for(int i = 0; i < h; i++)
            {
				for(int j = 0; j < w; j++)
                {
					sprites[i, j] = TwigModule.spriteBank.Create("electricBlock");
					sprites[i, j].Position += (offset + new Vector2(j * 8, i * 8));
					sprites[i, j].Play("idle" + ((Position.Y/8 + i) % 4 + 1).ToString());
					eSprites.Add(sprites[i, j]);
				}
            }
			foreach(Sprite s in eSprites)
            {
				Add(s);
            }
		}

		public ElectricBlock(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Width, data.Height)
		{
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			scene.Add(solid = new Solid(Position + Vector2.One, base.Width-2, base.Height-2, safe: false));
			Collidable = solid.Collidable = true;
		}

		private void OnPlayer(Player player)
		{
			player.Die((player.Center - base.Center).SafeNormalize());
		}

	}
}