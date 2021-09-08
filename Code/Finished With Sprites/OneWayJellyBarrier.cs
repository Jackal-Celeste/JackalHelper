using System;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.JackalHelper.Entities
{
	[CustomEntity("JackalHelper/OWJellyBarrier")]
	[Tracked]
	public class OneWayJellyBarrier : Solid
	{
		private static readonly float[] particleSpeeds = new float[3]
		{
			12f,
			20f,
			40f
		};

		public float Flash = 0f;

		public float Solidify = 0f;

		private float solidifyDelay = 0f;

		private JellyBarrierRenderer renderer = new JellyBarrierRenderer();

		private List<Vector2> particles = new List<Vector2>();

		public enum Directions
		{
			Up,
			Down,
			Left,
			Right
		}

		public Directions Direction;

		public bool ignoreOnHeld;

		public float alpha;
		public Color color;

		private bool trackingSet = false;


		public OneWayJellyBarrier(Vector2 position, float width, float height, Directions direction, bool ignoreOnHeld, string colorName, float alpha)
			: base(position, width, height, safe: false)
		{
			this.alpha = alpha;
			color = Calc.HexToColor(colorName) * Math.Min(Math.Abs(alpha), 1f);
			this.ignoreOnHeld = ignoreOnHeld;
			Direction = direction;

			Collidable = false;

			for (int i = 0; i < Width * Height / 16f; i++)
			{
				particles.Add(new Vector2(Calc.Random.NextFloat(Width - 1f), Calc.Random.NextFloat(Height - 1f)));
			}
		}

		public OneWayJellyBarrier(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Width, data.Height, data.Enum("direction", Directions.Up), data.Bool("ignoreOnHeld", false), data.Attr("color", "000000"), data.Float("alpha", 0.5f))
		{
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);

			if (!trackingSet)
			{
				// COLOURSOFNOISE: This should ideally be done with a hook but eh whatever
				scene.Add(renderer = new JellyBarrierRenderer());
				foreach (OneWayJellyBarrier barrier in Scene.Tracker.GetEntitiesCopy<OneWayJellyBarrier>())
				{
					barrier.renderer = renderer;
					renderer.Track(barrier, SceneAs<Level>());
					barrier.trackingSet = true;
				}
			}
		}

		public override void Removed(Scene scene)
		{
			base.Removed(scene);
			renderer.Untrack(this);
		}

		public override void Update()
		{
			if (Flash > 0)
			{
				Flash = Calc.Approach(Flash, 0f, Engine.DeltaTime * 4f);
			}
			else if (solidifyDelay > 0f)
			{
				solidifyDelay -= Engine.DeltaTime;
			}
			else if (Solidify > 0f)
			{
				Solidify = Calc.Approach(Solidify, 0f, Engine.DeltaTime);
			}

			float height = Height;
			for (int i = 0; i < particles.Count; i++)
			{
				// COLOURSOFNOISE: Make particles move in Direction
				Vector2 value = particles[i] + Vector2.UnitY * particleSpeeds[i % particleSpeeds.Length] * Engine.DeltaTime;
				value.Y %= height - 1f;
				particles[i] = value;
			}
			base.Update();
		}

		public bool InboundsCheck(Glider jelly)
		{
			return CollideCheck(jelly);
		}

		public bool IsAgainst(Vector2 dir)
		{
			return Direction switch
			{
				Directions.Up => dir.Y > 0,
				Directions.Down => dir.Y < 0,
				Directions.Left => dir.X > 0,
				Directions.Right => dir.X < 0,
				_ => false,
			};
		}

		public override void Render()
		{
			Color color = Color.White * 0.5f;
			foreach (Vector2 particle in particles)
			{
				Draw.Pixel.Draw(Position + particle, Vector2.Zero, color);
			}
			if (Flash > 0)
			{
				Draw.Rect(Collider, Color.White * Flash * 0.5f);
			}
		}
	}
}
