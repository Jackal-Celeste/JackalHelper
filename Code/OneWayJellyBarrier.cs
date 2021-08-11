// Celeste.SeekerBarrier
using System;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;


namespace Celeste.Mod.JackalHelper.Entities
{
	[CustomEntity("JackalHelper/OWJellyBarrier")]
	[Tracked]
	public class OneWayJellyBarrier : Solid
	{
		public float Flash = 0f;

		public float alpha;

		public float Solidify = 0f;

		public bool Flashing = false;

		private float solidifyDelay = 0f;

		public JellyBarrierRenderer jellyrend = new JellyBarrierRenderer();

		private List<Vector2> particles = new List<Vector2>();

		private List<OneWayJellyBarrier> barriers = new List<OneWayJellyBarrier>();

		private float[] speeds = new float[3]
		{
		12f,
		20f,
		40f
		};
		private enum Direction
		{
			Up,
			Down,
			Left,
			Right
		}

		public char dir = 'U';

		public List<float> jellySpdX;

		public bool ignoreOnHeld;

		public List<float> jellySpdY;

		public List<Glider> jellies = new List<Glider>();

		public bool jelliesCollected = false;

		public bool trackingSet = false;

		public string direction;

		public Color color;

		public OneWayJellyBarrier(Vector2 position, float width, float height, string direction, bool ignoreOnHeld, string colorName, float alpha)
			: base(position, width, height, safe: false)
		{
			this.alpha = alpha;
			alpha = Math.Min(Math.Abs(alpha), 1f);
			color = Calc.HexToColor(colorName) * alpha;
			this.ignoreOnHeld = ignoreOnHeld;
			this.direction = direction;
			Collidable = false;
			for (int i = 0; i < base.Width * base.Height / 16f; i++)
			{
				particles.Add(new Vector2(Calc.Random.NextFloat(base.Width - 1f), Calc.Random.NextFloat(base.Height - 1f)));
			}
			switch (direction)
			{
				case "up":
					dir = 'U';
					break;
				case "Up":
					dir = 'U';
					break;
				case "down":
					dir = 'D';
					break;
				case "Down":
					dir = 'D';
					break;
				case "left":
					dir = 'L';
					break;
				case "Left":
					dir = 'L';
					break;
				case "right":
					dir = 'R';
					break;
				case "Right":
					dir = 'R';
					break;
				default:
					dir = 'U';
					break;
			}
		}

		public OneWayJellyBarrier(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Width, data.Height, data.Attr("direction", "Up"), data.Bool("ignoreOnHeld", false), data.Attr("color", "000000"), data.Float("alpha", 0.5f))
		{
		}


		public override void Removed(Scene scene)
		{
			base.Removed(scene);
			jellyrend.Untrack(this);
		}

		public override void Update()
		{
			Scene scene = Scene;
			if (JackalModule.GetLevel() != null && !trackingSet)
			{
				scene.Add(jellyrend = new JellyBarrierRenderer());
				jellyrend.Track(this, JackalModule.GetLevel());
				foreach (OneWayJellyBarrier barrier in JackalModule.GetLevel().Tracker.GetEntitiesCopy<OneWayJellyBarrier>())
				{
					barriers.Add(barrier);
				}
				trackingSet = true;
			}
			if (Flashing)
			{
				Flash = Calc.Approach(Flash, 0f, Engine.DeltaTime * 4f);
				if (Flash <= 0f)
				{
					Flashing = false;
				}
			}
			else if (solidifyDelay > 0f)
			{
				solidifyDelay -= Engine.DeltaTime;
			}
			else if (Solidify > 0f)
			{
				Solidify = Calc.Approach(Solidify, 0f, Engine.DeltaTime);
			}
			int num = speeds.Length;
			float height = base.Height;
			int i = 0;
			for (int count = particles.Count; i < count; i++)
			{
				Vector2 value = particles[i] + Vector2.UnitY * speeds[i % num] * Engine.DeltaTime;
				value.Y %= height - 1f;
				particles[i] = value;
			}
			base.Update();
			List<Glider> currentJellies = jellies;
			if (JackalModule.GetLevel() != null)
			{
				if (!jelliesCollected)
				{
					foreach (Glider jelly in JackalModule.GetLevel().Entities.OfType<Glider>())
					{
						jellies.Add(jelly);
						jelliesCollected = true;
					}
				}
			}
		}


		public bool InboundsCheck(OneWayJellyBarrier self, Glider jelly)
		{
			if (jelly.Position.X > self.Position.X && jelly.Position.X < (self.Position.X + self.Width))
			{
				if (jelly.Position.Y > self.Position.Y && jelly.Position.Y < (self.Position.Y + self.Height))
				{
					Console.WriteLine(jelly.Speed.X);
					return true;
				}
			}
			return false;
		}

		public override void Render()
		{
			Color color = Color.White * 0.5f;
			foreach (Vector2 particle in particles)
			{
				Draw.Pixel.Draw(Position + particle, Vector2.Zero, color);
			}
			if (Flashing)
			{
				Draw.Rect(base.Collider, Color.White * Flash * 0.5f);
			}
		}
	}
}
