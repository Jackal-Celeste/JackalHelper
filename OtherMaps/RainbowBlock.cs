using System.Collections.Generic;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.JackalHelper.Entities
{
	[Tracked]
	[CustomEntity("JackalHelper/RainbowBlock")]

	public class RainbowBlock : Solid
	{
		public float totalTime = 0f;

		private TileGrid tiles;

		private char tileType;

		private RainbowBlock master;

		public List<RainbowBlock> Group;

		public Point GroupBoundsMin;

		public Point GroupBoundsMax;

		public Color color;

		public Rectangle rect;

		public RainbowFilter2 filter;



		public bool HasGroup
		{
			get;
			private set;
		}

		public bool MasterOfGroup
		{
			get;
			private set;
		}

		public RainbowBlock(Vector2 position, float width, float height, char tileType)
			: base(position, width, height, safe: true)
		{
			this.tileType = tileType;
			Add(new LightOcclude());
			//this.Depth = -13000;
			SurfaceSoundIndex = SurfaceIndex.TileToIndex[tileType];
			rect = new Rectangle((int)Position.X, (int)Position.Y, (int)width, (int)height);
			filter = new RainbowFilter2(Position, width, height);
			filter.Visible = true;
			filter.Position = Position;
			
		}

		public RainbowBlock(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Width, data.Height, data.Char("tiletype", '3'))
		{
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			if (HasGroup)
			{
				return;
			}
			MasterOfGroup = true;
			Group = new List<RainbowBlock>();
			GroupBoundsMin = new Point((int)base.X, (int)base.Y);
			GroupBoundsMax = new Point((int)base.Right, (int)base.Bottom);
			AddToGroupAndFindChildren(this);
			_ = base.Scene;
			Rectangle val = new Rectangle(GroupBoundsMin.X / 8, GroupBoundsMin.Y / 8, ((GroupBoundsMax.X - GroupBoundsMin.X) / 8) + 1, (GroupBoundsMax.Y - GroupBoundsMin.Y) / 8 + 1);
			VirtualMap<char> virtualMap = new VirtualMap<char>(val.Width, val.Height, '0');
			foreach (RainbowBlock item in Group)
			{
				int num = (int)(item.X / 8f) - val.X;
				int num2 = (int)(item.Y / 8f) - val.Y;
				int num3 = (int)(item.Width / 8f);
				int num4 = (int)(item.Height / 8f);
				for (int i = num; i < num + num3; i++)
				{
					for (int j = num2; j < num2 + num4; j++)
					{
						virtualMap[i, j] = tileType;
					}
				}
			}
			tiles = GFX.FGAutotiler.GenerateMap(virtualMap, new Autotiler.Behaviour
			{
				EdgesExtend = false,
				EdgesIgnoreOutOfLevel = false,
				PaddingIgnoreOutOfLevel = false
			}).TileGrid;
			tiles.Position = new Vector2((float)GroupBoundsMin.X - base.X, (float)GroupBoundsMin.Y - base.Y);
			Add(tiles);
		}



        private void AddToGroupAndFindChildren(RainbowBlock from)
		{
			if (from.X < (float)GroupBoundsMin.X)
			{
				GroupBoundsMin.X = (int)from.X;
			}
			if (from.Y < (float)GroupBoundsMin.Y)
			{
				GroupBoundsMin.Y = (int)from.Y;
			}
			if (from.Right > (float)GroupBoundsMax.X)
			{
				GroupBoundsMax.X = (int)from.Right;
			}
			if (from.Bottom > (float)GroupBoundsMax.Y)
			{
				GroupBoundsMax.Y = (int)from.Bottom;
			}
			from.HasGroup = true;
			Group.Add(from);
			if (from != this)
			{
				from.master = this;
			}
			foreach (RainbowBlock entity in base.Scene.Tracker.GetEntities<RainbowBlock>())
			{
				if (!entity.HasGroup && entity.tileType == tileType && (base.Scene.CollideCheck(new Rectangle((int)from.X - 1, (int)from.Y, (int)from.Width + 2, (int)from.Height), entity) || base.Scene.CollideCheck(new Rectangle((int)from.X, (int)from.Y - 1, (int)from.Width, (int)from.Height + 2), entity)))
				{
					AddToGroupAndFindChildren(entity);
				}
			}
		}

		
	}
}

namespace Celeste.Mod.JackalHelper.Entities
{
	public class RainbowFilter : Component
	{
		public Rectangle rect;

		public Color color;

		public float totalTime = 0f;
		public RainbowFilter(Vector2 position, float width, float height) : base(true, true)
		{
			rect = new Rectangle((int)position.X, (int)position.Y, (int)width, (int)height);
		}


		public override void Render()
		{

			color = JackalModule.Session.color;
			
			Draw.Rect(rect, color);
			
			base.Render();
		}
	}

	public class RainbowFilter2 : Entity
    {
		public RainbowFilter filter;

		public RainbowFilter2(Vector2 position, float width, float height) : base(position)
        {
			Depth = 8000;
			Add(filter = new RainbowFilter(position, width, height));
        }
    }
}