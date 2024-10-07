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
	public class SnakeBlockTail : Solid
	{
		public Vector2 lastHeadPos;
		private SnakeBlock head;
		private Vector2 speed;
		public bool start = false;
		public bool activated = false;
		public Hitbox hb;
		public Vector2 initDir = Vector2.Zero;
		public bool large = false;
		public Vector2 liftSpd;
		public bool check = false;
		public Directions dupeDir;
		private List<Image> body = new List<Image>();
		public Image image;

		public SnakeBlockTail(Vector2 position, float width, float height, Vector2 speed, SnakeBlock head)
			: base(position, width, height, safe: false)
		{
			this.head = head;
			Depth = -11502;
			liftSpd = head.LiftSpeed;
			initDir = head.directionVector;
			dupeDir = head.direction;
			Add(image = new Image(GFX.Game["objects/snakeBlock/Left/snek_simple1"]));
			if (head.fast) image.Color = Color.OrangeRed;

		}


		public override void Update()
        {
			if (head.fast) image.Color = Color.OrangeRed;
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
			if(head.LiftSpeed != Vector2.Zero&& !check)
            {
				liftSpd = head.LiftSpeed;
				check = true;
            }
			Collidable = true;
            if (HasPlayerRider())
            {
				head.activate();
            }
			if (lastHeadPos != null && lastHeadPos == head.Position)
            {
				Speed = Vector2.Zero;
                if (!head.inactive)
                {
					head.inactive = true;
					Remove(image);
					Add(image = new Image(GFX.Game["objects/snakeBlock/Left/snek_simple2"]));
					if (head.fast) image.Color = Color.OrangeRed;
					foreach (SnakeBlockBody n in head.list){
						n.Remove(n.i);
						n.Add(n.i = new Image(GFX.Game["objects/snakeBlock/Left/snek_simple_stem2"]));
						if (head.fast) n.i.Color = Color.OrangeRed;
					}
				}
            }
            else if (start)
            {
				lastHeadPos = head.Position;

				Speed = liftSpd;
			}
			base.Update();
			if (head.fast) image.Color = Color.OrangeRed;
		}


		public void changeDirection(bool left)
		{
			liftSpd = liftSpd.Rotate((float)(Math.PI / 2 * (left ? 1 : -1)));
			int l = left ? 3 : 1;
			dupeDir = (Directions)(((int)dupeDir + l) % 4);
		}
	}

}
