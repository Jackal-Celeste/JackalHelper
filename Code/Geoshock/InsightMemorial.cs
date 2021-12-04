using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.JackalHelper.Entities
{
	[Tracked]
	[CustomEntity("JackalHelper/InsightPuzzle")]
	public class InsightMemorial : Entity
	{
		private Image sprite;

		public String unsolvedStr = "Jackal_Geoshock_ExampleUnsolved";

		public String solvedStr = "Jackal_Geoshock_ExampleSolved";

		public InsightMemorialText textCurrent;

		//private Sprite dreamyText;

		private bool wasShowing;

		private SoundSource loopingSfx;

		public bool loaded = false;


		public InsightMemorial(Vector2 position)
			: base(position)
		{
			
			base.Tag = Tags.PauseUpdate;
			Add(sprite = new Image(GFX.Game["scenery/memorial/memorial"]));
			sprite.Origin = new Vector2(sprite.Width / 2f, sprite.Height);
			base.Depth = 100;
			base.Collider = new Hitbox(60f, 80f, -30f, -60f);
			Add(loopingSfx = new SoundSource());
			textCurrent = new InsightMemorialText(this, true, unsolvedStr);
		}

		public InsightMemorial(EntityData data, Vector2 offset)
			: this(data.Position + offset)
		{
		}

		public void checkSolved()
		{
			if (JackalModule.GetLevel() != null && !loaded && textCurrent.alpha <= 0f && JackalModule.SaveData.insightCrystals.Count > 0)
			{
				loaded = true;
				Console.WriteLine(JackalModule.SaveData.insightCrystals.Count);
				textCurrent = new InsightMemorialText(this, false, solvedStr);
				JackalModule.GetLevel().Remove(textCurrent);
				JackalModule.GetLevel().Add(textCurrent);
			}
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			Level level = scene as Level;

			level.Add(textCurrent);
		}

		public override void Update()
		{
			base.Update();
			checkSolved();
			Level level = base.Scene as Level;
			if (level.Paused)
			{
				loopingSfx.Pause();
				return;
			}
			Player entity = base.Scene.Tracker.GetEntity<Player>();
			bool dreaming = level.Session.Dreaming;
			wasShowing = textCurrent.Show;
			textCurrent.Show = entity != null && CollideCheck(entity);
			if (textCurrent.Show && !wasShowing)
			{
				Audio.Play(dreaming ? "event:/ui/game/memorial_dream_text_in" : "event:/ui/game/memorial_text_in");
				if (dreaming)
				{
					loopingSfx.Play("event:/ui/game/memorial_dream_loop");
					loopingSfx.Param("end", 0f);
				}
			}
			else if (!textCurrent.Show && wasShowing)
			{
				Audio.Play(dreaming ? "event:/ui/game/memorial_dream_text_out" : "event:/ui/game/memorial_text_out");
				loopingSfx.Param("end", 1f);
				loopingSfx.Stop();
			}
			loopingSfx.Resume();
		}
	}
}

namespace Celeste.Mod.JackalHelper.Entities
{
	public class InsightMemorialText : Entity
	{
		public bool Show;

		public bool Dreamy;

		public InsightMemorial Memorial;

		private float index;

		private string message;

		public float alpha;

		private float timer;

		private float widestCharacter;

		private int firstLineLength;

		private SoundSource textSfx;

		private bool textSfxPlaying;

		public InsightMemorialText(InsightMemorial memorial, bool dreamy, string dialog)
		{
			AddTag(Tags.HUD);
			AddTag(Tags.PauseUpdate);
			Add(textSfx = new SoundSource());
			Dreamy = dreamy;
			Memorial = memorial;
			message = Dialog.Clean(dialog);
			firstLineLength = CountToNewline(0);
			for (int i = 0; i < message.Length; i++)
			{
				float x = ActiveFont.Measure(message[i]).X;
				if (x > widestCharacter)
				{
					widestCharacter = x;
				}
			}
			widestCharacter *= 0.9f;
		}

		public override void Update()
		{
			base.Update();
			if ((base.Scene as Level).Paused)
			{
				textSfx.Pause();
				return;
			}
			timer += Engine.DeltaTime;
			if (!Show)
			{
				alpha = Calc.Approach(alpha, 0f, Engine.DeltaTime);
				if (alpha <= 0f)
				{
					index = firstLineLength;
				}
			}
			else
			{
				alpha = Calc.Approach(alpha, 1f, Engine.DeltaTime * 2f);
				if (alpha >= 1f)
				{
					index = Calc.Approach(index, message.Length, 32f * Engine.DeltaTime);
				}
			}
			if (Show && alpha >= 1f && index < (float)message.Length)
			{
				if (!textSfxPlaying)
				{
					textSfxPlaying = true;
					textSfx.Play(Dreamy ? "event:/ui/game/memorial_dream_text_loop" : "event:/ui/game/memorial_text_loop");
					textSfx.Param("end", 0f);
				}
			}
			else if (textSfxPlaying)
			{
				textSfxPlaying = false;
				textSfx.Stop();
				textSfx.Param("end", 1f);
			}
			textSfx.Resume();
		}

		private int CountToNewline(int start)
		{
			int i;
			for (i = start; i < message.Length && message[i] != '\n'; i++)
			{
			}
			return i - start;
		}

		public override void Render()
		{
			if ((base.Scene as Level).FrozenOrPaused || (base.Scene as Level).Completed || !(index > 0f) || !(alpha > 0f))
			{
				return;
			}
			Camera camera = SceneAs<Level>().Camera;
			Vector2 vector = new Vector2((Memorial.X - camera.X) * 6f, (Memorial.Y - camera.Y) * 6f - 350f - ActiveFont.LineHeight * 3.3f);
			if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode)
			{
				vector.X = 1920f - vector.X;
			}
			float num = Ease.CubeInOut(alpha);
			int num2 = (int)Math.Min(message.Length, index);
			int num3 = 0;
			float num4 = 64f * (1f - num);
			int num5 = CountToNewline(0);
			for (int i = 0; i < num2; i++)
			{
				char c = message[i];
				if (c == '\n')
				{
					num3 = 0;
					num5 = CountToNewline(i + 1);
					num4 += ActiveFont.LineHeight * 1.1f;
					continue;
				}
				float x = 1f;
				float x2 = (float)(-num5) * widestCharacter / 2f + ((float)num3 + 0.5f) * widestCharacter;
				float num6 = 0f;
				if (Dreamy && c != ' ' && c != '-' && c != '\n')
				{
					c = message[(i + (int)(Math.Sin(timer + (float)i / 8f) * 4.0) + message.Length) % message.Length];
					num6 = (float)Math.Sin(timer * 2f + (float)i / 8f) * 8f;
					x = ((!(Math.Sin(timer * 4f + (float)i / 16f) < 0.0)) ? 1 : (-1));
				}
				ActiveFont.Draw(c, vector + new Vector2(x2, num4 + num6), new Vector2(0.5f, 1f), new Vector2(x, 1f), Color.White * num);
				num3++;
			}
		}
	}
}
