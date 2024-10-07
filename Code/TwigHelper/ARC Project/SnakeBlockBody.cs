using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using static TwigHelper.ARC_Project.SnakeBlock;

namespace TwigHelper.ARC_Project
{
	[Tracked]
	public class SnakeBlockBody : Solid
	{
		private SnakeBlock head;
		public bool large = true;
		private List<Image> body = new List<Image>();
		public MTexture mTexture;
		public bool wait = false;
		public Image i;
		public SnakeBlockBody(Vector2 position, int width, int height, SnakeBlock parent)
			: base(position, width, height, safe: false)
		{

			this.head = parent;
			Add(i = new Image(GFX.Game["objects/snakeBlock/Left/snek_simple_stem1"]));
			if (head.fast) i.Color = Color.OrangeRed;
			this.Depth = -11501;
		}





		public override void Added(Scene scene)
		{
			base.Added(scene);
			if (head.fast) i.Color = Color.OrangeRed;
		}

		public override void Render()
		{
			if (head.fast) i.Color = Color.OrangeRed;
			base.Render();
		}
		public override void Update()
		{
			if (!large)
			{
				if (head.directionVector.Y == 0f && Math.Abs(head.Position.X - Position.X) < head.Width)
				{
					if (head.directionVector.X > 0f)
					{
						this.Hitbox.Width = Math.Abs(head.Position.X - Position.X) + 1;
					}
					else
					{
						this.Hitbox.Width = Math.Abs(head.Position.X - Position.X) + 1;
						this.Hitbox.Position.X = head.Width - this.Hitbox.Width;
					}
				}
				else if (head.directionVector.X == 0f && Math.Abs(head.Position.Y - Position.Y) < head.Height)
				{
					if (head.directionVector.Y > 0f)
					{
						this.Hitbox.Height = Math.Abs(head.Position.Y - Position.Y) + 1;

					}
					else
					{
						this.Hitbox.Height = Math.Abs(head.Position.Y - Position.Y) + 1;
						this.Hitbox.Position.Y = head.Height - this.Hitbox.Height;
					}
				}
				else
				{
					large = true;
				}

			}
			base.Update();
		}
	}
}


