using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using MonoMod.Utils;
using System.Collections;

namespace Celeste.Mod.JackalCollabHelper.Entities
{
	[CustomEntity(
		"JackalCollabHelper/VentSpringUp = LoadUp",
		"JackalCollabHelper/VentSpringLeft = LoadLeft",
		"JackalCollabHelper/VentSpringRight = LoadRight"
	)]
	[Tracked]
	public class VentSpring : Spring
	{

		public static Entity LoadUp(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new VentSpring(entityData, offset, Orientations.Floor);
		public static Entity LoadLeft(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new VentSpring(entityData, offset, Orientations.WallLeft);
		public static Entity LoadRight(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new VentSpring(entityData, offset, Orientations.WallRight);


		public int charNumber = 0;
		private DynData<Spring> dynData;
		public bool isVent;

		public Sprite sprite;

		public Orientations orientation;

		public VentSpring(Vector2 position, Orientations orientation)
			: base(position, orientation, true)
		{
			this.orientation = orientation;
		}

        public override void Added(Scene scene)
        {
            base.Added(scene);
			charNumber = Calc.Random.Next(0, 12);
			dynData = new DynData<Spring>(this);
			Remove(dynData.Get<Sprite>("sprite"));
			Add(sprite = new Sprite(GFX.Game, "objects/spring/vent" + charNumber + "/"));
			sprite.Add("idle", "", 0f, default(int));
			sprite.Add("bounce", "", 0.07f, "idle", 0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3, 4, 5);
			//sprite.Add("disabled", "white", 0.07f);
			sprite.Play("idle");
			sprite.Origin.X = sprite.Width / 2f;
			sprite.Origin.Y = sprite.Height;
			base.Depth = -9501;
			switch (orientation)
			{
				case Orientations.Floor:
					base.Collider = new Hitbox(16f, 6f, -8f, -6f);
					break;
				case Orientations.WallLeft:
					base.Collider = new Hitbox(6f, 16f, 0f, -8f);
					sprite.Rotation = (float)Math.PI / 2f;
					break;
				default:
					throw new Exception("Orientation not supported!");
				case Orientations.WallRight:
					base.Collider = new Hitbox(6f, 16f, -6f, -8f);
					sprite.Rotation = -(float)Math.PI / 2f;
					break;
			}
			dynData.Set<Sprite>("sprite", sprite);
		}

        public VentSpring(EntityData data, Vector2 offset, Orientations orientation)
			: this(data.Position + offset, orientation)
		{
		}

		public void ChangeSprite()
        {
			int newCharNumber = Calc.Random.Next(0, 12);
			dynData = new DynData<Spring>(this);
			Remove(dynData.Get<Sprite>("sprite"));
			Add(sprite = new Sprite(GFX.Game, "objects/spring/vent" + newCharNumber + "/"));
			sprite.Add("idle", "", 0f, default(int));
			sprite.Add("bounce", "", 0.07f, "idle", 0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3, 4, 5);
			//sprite.Add("disabled", "white", 0.07f);
			sprite.Play("idle");
			sprite.Origin.X = sprite.Width / 2f;
			sprite.Origin.Y = sprite.Height;
			base.Depth = -9501;
			switch (orientation)
			{
				case Orientations.Floor:
					base.Collider = new Hitbox(16f, 6f, -8f, -6f);
					break;
				case Orientations.WallLeft:
					base.Collider = new Hitbox(6f, 16f, 0f, -8f);
					sprite.Rotation = (float)Math.PI / 2f;
					break;
				default:
					throw new Exception("Orientation not supported!");
				case Orientations.WallRight:
					base.Collider = new Hitbox(6f, 16f, -6f, -8f);
					sprite.Rotation = -(float)Math.PI / 2f;
					break;
			}
			dynData.Set<Sprite>("sprite", sprite);
		}


	}
}
