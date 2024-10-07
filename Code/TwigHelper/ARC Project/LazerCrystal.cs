using System;
using System.Collections;
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
	[CustomEntity(
		"TwigHelper/CrystalLazerUp = LoadUp",
		"TwigHelper/CrystalLazerLeft = LoadLeft",
		"TwigHelper/CrystalLazerRight = LoadRight",
		"TwigHelper/CrystalLazerDown = LoadDown"
	)]

	public class LazerCrystal : Entity
    {
		public static Entity LoadUp(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new LazerCrystal(entityData, offset, Directions.Up);
		public static Entity LoadLeft(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new LazerCrystal(entityData, offset, Directions.Left);
		public static Entity LoadRight(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new LazerCrystal(entityData, offset, Directions.Right);
		public static Entity LoadDown(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new LazerCrystal(entityData, offset, Directions.Down);



		public enum Directions
		{
			Up,
			Down,
			Left,
			Right
		}

		public Directions Direction;

		private PlayerCollider pc;

		private Vector2 imageOffset;

		private string flag;

		private int length;

		private Image crystal;

		private List<Image> lazers = new List<Image>();

		private Hitbox lazerHitbox;

		private bool lazerActive = false;

		public LazerCrystal(Vector2 position, Directions direction, float delay, float duration, int length, string flag)
			: base(position)
		{
			if (direction == Directions.Up || direction == Directions.Down) Position.X -= 8;
			else Position.Y -= 8;
			base.Depth = -1;
			this.flag = flag;
			this.length = length;
			Direction = direction;
			string str = Direction.ToString().ToLower();
			int mod = length % 16;
			if(mod != 0) length += (mod < 8 ? -mod : 16 - mod);
			Vector2 unitDir = Vector2.Zero;
			base.Depth = -100;
			Vector2 bP = Vector2.Zero;
			switch (direction)
			{
				case Directions.Up:
					base.Collider = new Hitbox(16f, 9f, 0f, -9f);
					lazerHitbox = new Hitbox(16f, length + 16, 0f, -length-16);
					Add(new LedgeBlocker());
					unitDir = -Vector2.UnitY;
					bP = Position-(16*Vector2.UnitY);
					break;
				case Directions.Down:
					base.Collider = new Hitbox(16f, 9f);
					lazerHitbox = new Hitbox(16f, length+16, 0f, 0f);
					unitDir = Vector2.UnitY;
					bP = Position;
					break;
				case Directions.Left:
					base.Collider = new Hitbox(9f, 16f, -9f);
					lazerHitbox = new Hitbox(length+16, 16f, -length-16, 0f);
					Add(new LedgeBlocker());
					unitDir = -Vector2.UnitX;
					bP = Position - (16 * Vector2.UnitX);
					break;
				case Directions.Right:
					base.Collider = new Hitbox(9f, 16f);
					lazerHitbox = new Hitbox(length+16, 16f, 0f, 0f);
					Add(new LedgeBlocker());
					unitDir = Vector2.UnitX;
					bP = Position;
					break;
			}
			unitDir *= 16;
			for (int j = 1; j <= 1+(length / 16); j++)
			{
				Image s;
				Add(s = new Image(GFX.Game["danger/CrystalLaser/laser_" + str + "_00"]));
				s.RenderPosition = bP + ((j-1) * unitDir);
				lazers.Add(s);
			}
			Add(crystal = new Image(GFX.Game["danger/CrystalLaser/crystal_" + str + "_00"]));
			crystal.RenderPosition = bP;
			Add(new PlayerCollider(OnCollideL, lazerHitbox));
			Add(pc = new PlayerCollider(OnCollide));
			Add(new StaticMover
			{
				OnShake = OnShake,
				SolidChecker = IsRiding,
				JumpThruChecker = IsRiding,
				OnEnable = OnEnable,
				OnDisable = OnDisable
			});
			Add(new Coroutine(lazerCycle(delay, duration), true));
		}

		public LazerCrystal(EntityData data, Vector2 offset, Directions dir)
			: this(data.Position + offset, (Directions)data.Int("orientation"), data.Float("delay"), data.Float("duration"), data.Int("length"), data.Attr("flag"))
		{
		}

        public override void Update()
        {
            base.Update();
			if(TwigModule.GetLevel() != null)
            {
                if (TwigModule.GetLevel().Session.GetFlag(flag))
                {
					lazerActive = false;
					foreach (Image s in lazers)
					{
						s.Visible = false;
					}
					crystal.Color = Color.Gray;
				}
            }
        }

        private IEnumerator lazerCycle(float delay, float duration)
        {
			while (TwigModule.GetLevel() != null)
			{
				if (!TwigModule.GetLevel().Session.GetFlag(flag)){
					if (delay > 0)
					{
						foreach (Image s in lazers)
						{
							s.Visible = false;
						}
						lazerActive = false;
						yield return delay;
					}
					if (duration > 0)
					{
						foreach (Image s in lazers)
						{
							s.Visible = true;
						}
						lazerActive = true;
						yield return duration;
					}
				}
				yield return null;
			}

        }


		private void OnEnable()
		{
			Active = (Visible = (Collidable = true));
		}

		private void OnDisable()
		{
			Active = (Visible = (Collidable = false));
		}

		private void OnShake(Vector2 amount)
		{
			imageOffset += amount;
		}

		public override void Render()
		{
			Vector2 position = Position;
			Position += imageOffset;
			base.Render();
			Position = position;
		}


		private void OnCollide(Player player)
		{
			switch (Direction)
			{
				case Directions.Up:
					if (player.Speed.Y >= 0f && player.Bottom <= base.Bottom)
					{
						player.Die(new Vector2(0f, -1f));
					}
					break;
				case Directions.Down:
					if (player.Speed.Y <= 0f)
					{
						player.Die(new Vector2(0f, 1f));
					}
					break;
				case Directions.Left:
					if (player.Speed.X >= 0f)
					{
						player.Die(new Vector2(-1f, 0f));
					}
					break;
				case Directions.Right:
					if (player.Speed.X <= 0f)
					{
						player.Die(new Vector2(1f, 0f));
					}
					break;
			}
		}

		private void OnCollideL(Player player)
		{
			if (lazerActive)
			{
				if(Direction == Directions.Up || Direction == Directions.Down) {
					if (player.Speed.X >= 0f)
					{
						player.Die(new Vector2(-1f, 0f));
					}
					else
					{
						player.Die(new Vector2(1f, 0f));
					}
				}
                else
                {
					if (player.Speed.Y >= 0f)
					{
						player.Die(new Vector2(0f, -1f));
					}
                    else
                    {
						player.Die(new Vector2(0f, 1f));
					}
				}
			}
		}



		private bool IsRiding(Solid solid)
		{
			switch (Direction)
			{
				case Directions.Up:
					return CollideCheckOutside(solid, Position + Vector2.UnitY);
				case Directions.Down:
					return CollideCheckOutside(solid, Position - Vector2.UnitY);
				case Directions.Left:
					return CollideCheckOutside(solid, Position + Vector2.UnitX);
				case Directions.Right:
					return CollideCheckOutside(solid, Position - Vector2.UnitX);
				default:
					return false;

			}
		}

		private bool IsRiding(JumpThru jumpThru)
		{
			if (Direction != 0)
			{
				return false;
			}
			return CollideCheck(jumpThru, Position + Vector2.UnitY);
		}
	}
}
