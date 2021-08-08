// Celeste.ChromaMover
using System;
using System.Collections;
using System.Collections.Generic;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.JackalHelper.Entities
{
	[Tracked]
	[CustomEntity("JackalHelper/ChromaMover")]
	public class ZipMover : Solid
	{


		public enum Themes
		{
			Normal,
			Moon
		}
		private class ZipMoverPathRenderer : Entity
		{
			public ZipMover ZipMover;

			private MTexture cog;

			private Vector2 from;

			private Vector2 to;

			private Vector2 sparkAdd;

			private float sparkDirFromA;

			private float sparkDirFromB;

			private float sparkDirToA;

			private float sparkDirToB;

			public ZipMoverPathRenderer(ZipMover zipMover)
			{
				base.Depth = 5000;
				ZipMover = zipMover;
				from = ZipMover.start + new Vector2(ZipMover.Width / 2f, ZipMover.Height / 2f);
				to = ZipMover.target + new Vector2(ZipMover.Width / 2f, ZipMover.Height / 2f);
				sparkAdd = (from - to).SafeNormalize(5f).Perpendicular();
				float num = (from - to).Angle();
				sparkDirFromA = num + (float)Math.PI / 8f;
				sparkDirFromB = num - (float)Math.PI / 8f;
				sparkDirToA = num + (float)Math.PI - (float)Math.PI / 8f;
				sparkDirToB = num + (float)Math.PI + (float)Math.PI / 8f;
				cog = GFX.Game["objects/chromaMover/cog"];

			}



			public override void Render()
			{
				DrawCogs(Vector2.UnitY, JackalModule.Session.color);
				DrawCogs(Vector2.Zero, JackalModule.Session.color);
				
				if (ZipMover.drawBlackBorder)
				{
					Draw.Rect(new Rectangle((int)(ZipMover.X + ZipMover.Shake.X - 1f), (int)(ZipMover.Y + ZipMover.Shake.Y - 1f), (int)ZipMover.Width + 2, (int)ZipMover.Height + 2), JackalModule.Session.color);
				}
			}

			private void DrawCogs(Vector2 offset, Color? colorOverride = null)
			{
				Vector2 vector = (to - from).SafeNormalize();
				Vector2 value = vector.Perpendicular() * 3f;
				Vector2 value2 = -vector.Perpendicular() * 4f;
				float rotation = ZipMover.percent * (float)Math.PI * 2f;
				//Draw.Line(from + value + offset, to + value + offset, JackalModule.Session.color);
				//Draw.Line(from + value2 + offset, to + value2 + offset, JackalModule.Session.color);
				cog.DrawCentered(from + offset, JackalModule.Session.color, 1f, rotation / 2);
				cog.DrawCentered(to + offset, JackalModule.Session.color, 1f, rotation / 2);
			}
		}

		private Themes theme;

		private MTexture[,] edges = new MTexture[3, 3];

		private Sprite streetlight;

		private ZipMoverPathRenderer pathRenderer;

		private List<MTexture> innerCogs;

		private MTexture temp = new MTexture();

		private bool drawBlackBorder;

		private Vector2 start;

		private Vector2 target;

		private float percent = 0f;

		private static  Color ropeColor = Calc.HexToColor("000000") * 0f;

		private static  Color ropeLightColor = Calc.HexToColor("ffffff");

		private SoundSource sfx = new SoundSource();

		public ZipMover(Vector2 position, int width, int height, Vector2 target, Themes theme)
			: base(position, width, height, safe: false)
		{
			base.Depth = -9999;
			start = Position;
			this.target = target;
			this.theme = theme;
			Add(new Coroutine(Sequence()));
			Add(new LightOcclude());
			string path;
			string id;
			string key;
			if (theme == Themes.Moon)
			{
				path = "objects/chromaMover/moon/light";
				id = "objects/chromaMover/moon/block";
				key = "objects/chromaMover/moon/innercog";
				drawBlackBorder = false;
			}
			else
			{
				path = "objects/chromaMover/light";
				id = "objects/chromaMover/block";
				key = "objects/chromaMover/innercog";
				drawBlackBorder = true;
			}
			innerCogs = GFX.Game.GetAtlasSubtextures(key);
			Add(streetlight = new Sprite(GFX.Game, path));
			streetlight.Add("frames", "", 1f);
			streetlight.Play("frames");
			streetlight.Active = false;
			streetlight.SetAnimationFrame(1);
			streetlight.Position = new Vector2(base.Width / 2f - streetlight.Width / 2f, 0f);
			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					edges[i, j] = GFX.Game[id].GetSubtexture(i * 8, j * 8, 8, 8);
				}
			}
			SurfaceSoundIndex = 7;
			sfx.Position = new Vector2(base.Width, base.Height) / 2f;
			Add(sfx);
		}

		public ZipMover(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Width, data.Height, data.Nodes[0] + offset, data.Enum("theme", Themes.Normal))
		{
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			scene.Add(pathRenderer = new ZipMoverPathRenderer(this));
		}

		public override void Removed(Scene scene)
		{
			scene.Remove(pathRenderer);
			pathRenderer = null;
			base.Removed(scene);
		}

		public override void Update()
		{
			base.Update();
		}

		public override void Render()
		{
			
			Vector2 position = Position;
			streetlight.Color = JackalModule.Session.color;
			Position += base.Shake;
			Draw.Rect(base.X + 1f, base.Y + 1f, base.Width - 2f, base.Height - 2f, JackalModule.Session.color);
			int num = 1;
			float num2 = 0f;
			int count = innerCogs.Count;
			for (int i = 4; (float)i <= base.Height - 4f; i += 8)
			{
				int num3 = num;
				for (int j = 4; (float)j <= base.Width - 4f; j += 8)
				{
					int index = (int)(mod((num2 + (float)num * percent * (float)Math.PI * 4f) / ((float)Math.PI / 2f), 1f) * (float)count);
					MTexture mTexture = innerCogs[index];
					Rectangle rectangle = new Rectangle(0, 0, mTexture.Width, mTexture.Height);
					Vector2 zero = Vector2.Zero;
					if (j <= 4)
					{
						zero.X = 2f;
						rectangle.X = 2;
						rectangle.Width -= 2;
					}
					else if ((float)j >= base.Width - 4f)
					{
						zero.X = -2f;
						rectangle.Width -= 2;
					}
					if (i <= 4)
					{
						zero.Y = 2f;
						rectangle.Y = 2;
						rectangle.Height -= 2;
					}
					else if ((float)i >= base.Height - 4f)
					{
						zero.Y = -2f;
						rectangle.Height -= 2;
					}
					mTexture = mTexture.GetSubtexture(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, temp);
					mTexture.DrawCentered(Position + new Vector2(j, i) + zero, Color.White * 1f);
					num = -num;
					num2 += (float)Math.PI / 3f;
				}
				if (num3 == num)
				{
					num = -num;
				}
			}
			for (int k = 0; (float)k < base.Width / 8f; k++)
			{
				for (int l = 0; (float)l < base.Height / 8f; l++)
				{
					int num4 = ((k != 0) ? (((float)k != base.Width / 8f - 1f) ? 1 : 2) : 0);
					int num5 = ((l != 0) ? (((float)l != base.Height / 8f - 1f) ? 1 : 2) : 0);
					if (num4 != 1 || num5 != 1)
					{
						edges[num4, num5].Draw(new Vector2(base.X + (float)(k * 8), base.Y + (float)(l * 8)));
					}
				}
			}
			Draw.Rect(Position + 2 * Vector2.One, Width - 4, Height - 4, Color.Black);
			base.Render();
			Position = position;

		}

		private void ScrapeParticlesCheck(Vector2 to)
		{
			if (!base.Scene.OnInterval(0.03f))
			{
				return;
			}
			bool flag = to.Y != base.ExactPosition.Y;
			bool flag2 = to.X != base.ExactPosition.X;
			if (flag && !flag2)
			{
				int num = Math.Sign(to.Y - base.ExactPosition.Y);
				Vector2 value = ((num != 1) ? base.TopLeft : base.BottomLeft);
				int num2 = 4;
				if (num == 1)
				{
					num2 = Math.Min((int)base.Height - 12, 20);
				}
				int num3 = (int)base.Height;
				if (num == -1)
				{
					num3 = Math.Max(16, (int)base.Height - 16);
				}
			}
			else
			{
				if (!flag2 || flag)
				{
					return;
				}
				int num4 = Math.Sign(to.X - base.ExactPosition.X);
				Vector2 value2 = ((num4 != 1) ? base.TopLeft : base.TopRight);
				int num5 = 4;
				if (num4 == 1)
				{
					num5 = Math.Min((int)base.Width - 12, 20);
				}
				int num6 = (int)base.Width;
				if (num4 == -1)
				{
					num6 = Math.Max(16, (int)base.Width - 16);
				}

			}
		}

		private IEnumerator Sequence()
		{
			Vector2 start = Position;
			while (true)
			{
				if (!HasPlayerRider() && !collideCheck())
				{
					yield return null;
					continue;
				}
				sfx.Play("event:/new_content/game/10_farewell/zip_mover");
				Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
				StartShaking(0.1f);
				yield return 0.1f;
				streetlight.SetAnimationFrame(3);
				StopPlayerRunIntoAnimation = false;
				float at = 0f;
				while (at < 1f)
				{
					yield return null;
					at = Calc.Approach(at, 1f, 2f * Engine.DeltaTime);
					percent = Ease.SineIn(at);
					Vector2 to = Vector2.Lerp(start, target, percent);
					ScrapeParticlesCheck(to);
					if (Scene.OnInterval(0.1f))
					{
						//pathRenderer.CreateSparks();
					}
					MoveTo(to);
				}
				StartShaking(0.2f);
				Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
				SceneAs<Level>().Shake();
				StopPlayerRunIntoAnimation = true;
				yield return 0.5f;
				StopPlayerRunIntoAnimation = false;
				streetlight.SetAnimationFrame(2);
				float at2 = 0f;
				while (at2 < 1f)
				{
					yield return null;
					at2 = Calc.Approach(at2, 1f, 0.5f * Engine.DeltaTime);
					percent = 1f - Ease.SineIn(at2);
					Vector2 to2 = Vector2.Lerp(target, start, Ease.SineIn(at2));
					MoveTo(to2);
				}
				StopPlayerRunIntoAnimation = true;
				StartShaking(0.2f);
				streetlight.SetAnimationFrame(1);
				yield return 0.5f;
			}
		}

		public bool collideCheck()
        {
			if(JackalModule.GetPlayer() != null)
            {
				return (this.CollideCheck<Player>(Position - 4*Vector2.UnitX)) || (this.CollideCheck<Player>(Position + new Vector2(0f, (float)Height) - 4*Vector2.UnitX)) || (this.CollideCheck<Player>(Position + 4*Vector2.UnitX)) || (this.CollideCheck<Player>(Position + new Vector2(0, Height) + 4*Vector2.UnitX)) || (this.CollideCheck<Player>(Position + new Vector2(0, Height/2) + 4*Vector2.UnitX)) || (this.CollideCheck<Player>(Position + new Vector2(0, Height/2) - 4*Vector2.UnitX));
			}
            else
            {
				return false;
            }

        }

		private float mod(float x, float m)
		{
			return (x % m + m) % m;
		}
	}


}