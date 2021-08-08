using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.JackalHelper.Entities
{

	[CustomEntity(
	  "JackalCollab/ChromaSpikesUp = LoadUp",
	  "JackalCollab/ChromaSpikesDown = LoadDown",
	  "JackalCollab/ChromaSpikesLeft = LoadLeft",
	  "JackalCollab/ChromaSpikesRight = LoadRight"
  )]
	[TrackedAs(typeof(Spikes))]
	public class ChromaSpikes : Spikes
	{

		public static Entity LoadUp(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
		{
			return new ChromaSpikes(entityData, offset, Directions.Up);
		}
		public static Entity LoadDown(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
		{
			return new ChromaSpikes(entityData, offset, Directions.Down);
		}
		public static Entity LoadLeft(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
		{
			return new ChromaSpikes(entityData, offset, Directions.Left);
		}
		public static Entity LoadRight(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
		{
			return new ChromaSpikes(entityData, offset, Directions.Right);
		}

		public int size;

		public Sprite image;

		public Color color;

		public float totalTime = 0f;

		public Spikes invis;

		public ChromaSpikes(Vector2 position, int size, Directions direction)
		: base(position, size, direction, "chroma")
		{
			this.size = size;


			invis = new Spikes(this.Position, size, direction, "chroma");
			
		}

		public ChromaSpikes(EntityData data, Vector2 offset, Directions dir)
	: this(data.Position + offset, GetSize(data, dir), dir)
		{

		}

		public override void Update()
		{
			base.Update();
			totalTime += (Engine.DeltaTime * 1.25f);
			Color color = JackalModule.Session.color;
			invis.SetSpikeColor(color);

			Depth = 9000;
		}

        public override void Render()
        {
			invis.SetSpikeColor(JackalModule.Session.color);
			SetSpikeColor(JackalModule.Session.color);
			base.Render();
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
