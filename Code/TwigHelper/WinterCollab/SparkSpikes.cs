using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.JackalCollabHelper.Entities
{

	[CustomEntity(
	  "JackalCollab/SparkSpikesUp = LoadUp",
	  "JackalCollab/SparkSpikesDown = LoadDown",
	  "JackalCollab/SparkSpikesLeft = LoadLeft",
	  "JackalCollab/SparkSpikesRight = LoadRight"
  )]
	[TrackedAs(typeof(Spikes))]
	public class SparkSpikes : Spikes
    {

		public static Entity LoadUp(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
		{
			return new SparkSpikes(entityData, offset, Directions.Up);
		}
		public static Entity LoadDown(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
		{
			return new SparkSpikes(entityData, offset, Directions.Down);
		}
		public static Entity LoadLeft(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
		{
			return new SparkSpikes(entityData, offset, Directions.Left);
		}
		public static Entity LoadRight(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
		{
			return new SparkSpikes(entityData, offset, Directions.Right);
		}

		public int size;

		public Sprite image;

		public PlayerCollider pc;

		public Spikes invis;

        public SparkSpikes(Vector2 position, int size, Directions direction)
        : base(position, size, direction, "default")
        {
			this.size = size;
			

		    invis = new Spikes(this.Position, size, direction, "default");
			invis.Visible = false;
		}

        public SparkSpikes(EntityData data, Vector2 offset, Directions dir)
    : this(data.Position + offset, GetSize(data, dir), dir)
        {

        }

        public override void Added(Scene scene)
        {
			base.Added(scene);
			SetSpikeColor(Color.LavenderBlush * 0.3f);
			for (int j = 0; j < size / 8; j++)
			{
				switch (Direction)
				{
					case Directions.Up:
						image = TwigModule.spriteBank.Create("sparkspikesup");
						image.JustifyOrigin(0.5f, 1f);
						image.Position = Vector2.UnitX * ((float)j + 0.5f) * 8f + Vector2.UnitY;
						Add(image);
						break;
					case Directions.Down:
						image = TwigModule.spriteBank.Create("sparkspikesdown");
						image.JustifyOrigin(0.5f, 0f);
						image.Position = Vector2.UnitX * ((float)j + 0.5f) * 8f - Vector2.UnitY;
						Add(image);
						break;
					case Directions.Right:
						image = TwigModule.spriteBank.Create("sparkspikesright");
						image.JustifyOrigin(0f, 0.5f);
						image.Position = Vector2.UnitY * ((float)j + 0.5f) * 8f - Vector2.UnitX;
						Add(image);
						break;
					case Directions.Left:
						image = TwigModule.spriteBank.Create("sparkspikesleft");
						image.JustifyOrigin(1f, 0.5f);
						image.Position = Vector2.UnitY * ((float)j + 0.5f) * 8f + Vector2.UnitX;
						Add(image);
						break;
				}
				image.Play("idle", false, true);
			}
		}
        public override void Update()
        {
            base.Update();
			invis.SetSpikeColor(Color.LavenderBlush * 0.3f);
			Depth = 9000;
		}

        public void OnCollide(Player player)
		{
		
			player.Die(player.Speed);
		}

		public static int GetSize(EntityData data, Directions dir)
        {
            switch (dir)
            {
                default:
                    return data.Height;
                case Directions.Up:
                case Directions.Down:
                    return data.Width;
            }
        }

    }
}
