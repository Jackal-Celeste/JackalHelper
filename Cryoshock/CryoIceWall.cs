using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

public class CryoIceWall : Entity
{
	public Facings Facing;

	private StaticMover staticMover;

	private ClimbBlocker climbBlocker;

	public bool IceMode;

	private List<Sprite> tiles;

	public CryoIceWall(Vector2 position, float height, bool left)
		: base(position)
	{
		base.Tag = Tags.TransitionUpdate;
		base.Depth = 1999;
		if (left)
		{
			Facing = Facings.Left;
			base.Collider = new Hitbox(2f, height);
		}
		else
		{
			Facing = Facings.Right;
			base.Collider = new Hitbox(2f, height, 6f);
		}
		Add(new CoreModeListener(OnChangeMode));
		Add(staticMover = new StaticMover());
		Add(climbBlocker = new ClimbBlocker(edge: false));
		tiles = BuildSprite(left);
	}

	public CryoIceWall(EntityData data, Vector2 offset)
		: this(data.Position + offset, data.Height, data.Bool("left"))
	{
	}

	private List<Sprite> BuildSprite(bool left)
	{
		List<Sprite> list = new List<Sprite>();
		for (int i = 0; (float)i < base.Height; i += 8)
		{
			string id = ((i == 0) ? "WallBoosterTop" : ((!((float)(i + 16) > base.Height)) ? "WallBoosterMid" : "WallBoosterBottom"));
			Sprite sprite = GFX.SpriteBank.Create(id);
			if (!left)
			{
				sprite.FlipX = true;
				sprite.Position = new Vector2(4f, i);
			}
			else
			{
				sprite.Position = new Vector2(0f, i);
			}
			list.Add(sprite);
			Add(sprite);
		}
		return list;
	}


	private void OnChangeMode(Session.CoreModes mode)
	{
		climbBlocker.Blocking = true;
		tiles.ForEach(delegate (Sprite t)
		{
			t.Play("ice");
		});
	}

	public override void Update()
	{
		if (!(base.Scene as Level).Transitioning)
		{
			base.Update();
		}
	}

}

