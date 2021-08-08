using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;

namespace Celeste.Mod.JackalHelper.Entities 
{
	[Tracked]
	[CustomEntity("JackalHelper/DormantDreamBlock")]
	public class DormantDreamBlock : Solid
	{
		private struct DreamParticle
		{
			public Vector2 Position;

			public int Layer;

			public Color Color;

			public float TimeOffset;
		}


		private static readonly Color disabledBackColor = Calc.HexToColor("1f2e2d");


		private static readonly Color disabledLineColor = Calc.HexToColor("6a8480");

		private LightOcclude occlude;

		private MTexture[] particleTextures;

		private DreamParticle[] particles;

		private float whiteFill;

		private float whiteHeight = 1f;

		private Vector2 shake;

		private float animTimer;

		private Shaker shaker;

		private float wobbleFrom = Calc.Random.NextFloat((float)Math.PI * 2f);

		private float wobbleTo = Calc.Random.NextFloat((float)Math.PI * 2f);

		private float wobbleEase;

		private int randomSeed;

		public DormantDreamBlock(Vector2 position, float width, float height, bool below)
			: base(position, width, height, safe: true)
		{
			base.Depth = -11000;
			if (below)
			{
				base.Depth = 5000;
			}
			SurfaceSoundIndex = 11;
			particleTextures = new MTexture[4]
			{
			GFX.Game["objects/dreamblock/particles"].GetSubtexture(14, 0, 7, 7),
			GFX.Game["objects/dreamblock/particles"].GetSubtexture(7, 0, 7, 7),
			GFX.Game["objects/dreamblock/particles"].GetSubtexture(0, 0, 7, 7),
			GFX.Game["objects/dreamblock/particles"].GetSubtexture(7, 0, 7, 7)
			};
		}

		public DormantDreamBlock(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Width, data.Height, data.Bool("below"))
		{
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);

			Add(occlude = new LightOcclude());

			Setup();
		}

		public void Setup()
		{
			particles = new DreamParticle[(int)(base.Width / 8f * (base.Height / 8f) * 0.7f)];
			for (int i = 0; i < particles.Length; i++)
			{
				particles[i].Position = new Vector2(Calc.Random.NextFloat(base.Width), Calc.Random.NextFloat(base.Height));
				particles[i].Layer = Calc.Random.Choose(0, 1, 1, 2, 2, 2);
				particles[i].TimeOffset = Calc.Random.NextFloat();
				particles[i].Color = Color.LightGray * (0.5f + (float)particles[i].Layer / 2f * 0.5f);
			}
		}

		public void OnPlayerExit(Player player)
		{
			Dust.Burst(player.Position, player.Speed.Angle(), 16, null);
			Vector2 vector = Vector2.Zero;
			if (CollideCheck(player, Position + Vector2.UnitX * 4f))
			{
				vector = Vector2.UnitX;
			}
			else if (CollideCheck(player, Position - Vector2.UnitX * 4f))
			{
				vector = -Vector2.UnitX;
			}
			else if (CollideCheck(player, Position + Vector2.UnitY * 4f))
			{
				vector = Vector2.UnitY;
			}
			else if (CollideCheck(player, Position - Vector2.UnitY * 4f))
			{
				vector = -Vector2.UnitY;
			}
			_ = vector != Vector2.Zero;
		}

		public override void Update()
		{
			base.Update();
		}

		public bool BlockedCheck()
		{
			TheoCrystal theoCrystal = CollideFirst<TheoCrystal>();
			if (theoCrystal != null && !TryActorWiggleUp(theoCrystal))
			{
				return true;
			}
			Player player = CollideFirst<Player>();
			if (player != null && !TryActorWiggleUp(player))
			{
				return true;
			}
			return false;
		}

		private bool TryActorWiggleUp(Entity actor)
		{
			bool collidable = Collidable;
			Collidable = true;
			for (int i = 1; i <= 4; i++)
			{
				if (!actor.CollideCheck<Solid>(actor.Position - Vector2.UnitY * i))
				{
					actor.Position -= Vector2.UnitY * i;
					Collidable = collidable;
					return true;
				}
			}
			Collidable = collidable;
			return false;
		}

		public override void Render()
		{
			Camera camera = SceneAs<Level>().Camera;
			if (base.Right < camera.Left || base.Left > camera.Right || base.Bottom < camera.Top || base.Top > camera.Bottom)
			{
				return;
			}
			Draw.Rect(shake.X + base.X, shake.Y + base.Y, base.Width, base.Height, disabledBackColor);
			Vector2 position = SceneAs<Level>().Camera.Position;
			for (int i = 0; i < particles.Length; i++)
			{
				int layer = particles[i].Layer;
				Vector2 position2 = particles[i].Position;
				position2 += position * (0.3f + 0.25f * (float)layer);
				position2 = PutInside(position2);
				Color color = particles[i].Color;
				MTexture mTexture;
				switch (layer)
				{
					case 0:
						{
							int num2 = (int)((particles[i].TimeOffset * 4f + animTimer) % 4f);
							mTexture = particleTextures[3 - num2];
							break;
						}
					case 1:
						{
							int num = (int)((particles[i].TimeOffset * 2f + animTimer) % 2f);
							mTexture = particleTextures[1 + num];
							break;
						}
					default:
						mTexture = particleTextures[2];
						break;
				}
				if (position2.X >= base.X + 2f && position2.Y >= base.Y + 2f && position2.X < base.Right - 2f && position2.Y < base.Bottom - 2f)
				{
					mTexture.DrawCentered(position2 + shake, color);
				}
			}
			if (whiteFill > 0f)
			{
				Draw.Rect(base.X + shake.X, base.Y + shake.Y, base.Width, base.Height * whiteHeight, Color.White * whiteFill);
			}
			WobbleLine(shake + new Vector2(base.X, base.Y), shake + new Vector2(base.X + base.Width, base.Y), 0f);
			WobbleLine(shake + new Vector2(base.X + base.Width, base.Y), shake + new Vector2(base.X + base.Width, base.Y + base.Height), 0.7f);
			WobbleLine(shake + new Vector2(base.X + base.Width, base.Y + base.Height), shake + new Vector2(base.X, base.Y + base.Height), 1.5f);
			WobbleLine(shake + new Vector2(base.X, base.Y + base.Height), shake + new Vector2(base.X, base.Y), 2.5f);
			Draw.Rect(shake + new Vector2(base.X, base.Y), 2f, 2f, disabledLineColor);
			Draw.Rect(shake + new Vector2(base.X + base.Width - 2f, base.Y), 2f, 2f, disabledLineColor);
			Draw.Rect(shake + new Vector2(base.X, base.Y + base.Height - 2f), 2f, 2f, disabledLineColor);
			Draw.Rect(shake + new Vector2(base.X + base.Width - 2f, base.Y + base.Height - 2f), 2f, 2f, disabledLineColor);
		}

		private Vector2 PutInside(Vector2 pos)
		{
			while (pos.X < base.X)
			{
				pos.X += base.Width;
			}
			while (pos.X > base.X + base.Width)
			{
				pos.X -= base.Width;
			}
			while (pos.Y < base.Y)
			{
				pos.Y += base.Height;
			}
			while (pos.Y > base.Y + base.Height)
			{
				pos.Y -= base.Height;
			}
			return pos;
		}

		private void WobbleLine(Vector2 from, Vector2 to, float offset)
		{
			float num = (to - from).Length();
			Vector2 vector = Vector2.Normalize(to - from);
			Vector2 vector2 = new Vector2(vector.Y, 0f - vector.X);
			Color color = (disabledLineColor);
			Color color2 = (disabledBackColor);
			if (whiteFill > 0f)
			{
				color = Color.Lerp(color, Color.White, whiteFill);
				color2 = Color.Lerp(color2, Color.White, whiteFill);
			}
			float num2 = 0f;
			int num3 = 16;
			for (int i = 2; (float)i < num - 2f; i += num3)
			{
				float num4 = Lerp(LineAmplitude(wobbleFrom + offset, i), LineAmplitude(wobbleTo + offset, i), wobbleEase);
				if ((float)(i + num3) >= num)
				{
					num4 = 0f;
				}
				float num5 = Math.Min(num3, num - 2f - (float)i);
				Vector2 vector3 = from + vector * i + vector2 * num2;
				Vector2 vector4 = from + vector * ((float)i + num5) + vector2 * num4;
				Draw.Line(vector3 - vector2, vector4 - vector2, color2);
				Draw.Line(vector3 - vector2 * 2f, vector4 - vector2 * 2f, color2);
				Draw.Line(vector3, vector4, color);
				num2 = num4;
			}
		}

		private float LineAmplitude(float seed, float index)
		{
			return (float)(Math.Sin((double)(seed + index / 16f) + Math.Sin(seed * 2f + index / 32f) * 6.2831854820251465) + 1.0) * 1.5f;
		}

		private float Lerp(float a, float b, float percent)
		{
			return a + (b - a) * percent;
		}


	}


	
}