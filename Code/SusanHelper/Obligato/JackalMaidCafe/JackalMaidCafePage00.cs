using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste;
using SusanHelper.Entities.Paint;
using System.Collections.Generic;
using Celeste.Mod.SusanHelper;
using System.Linq;
using static SusanHelper.Entities.Paint.PaintSource;
using System.Runtime.CompilerServices;
using System.Globalization;
using System.Collections;

namespace Celeste.Mod.SusanHelperNew.Obligato
{

    public class JackalMaidCafePage00 : JackalMaidCafePage
    {
        private Color taskbarColor = Calc.HexToColor("d1b8cf");

        private string time;

        private Vector2 pptIcon, chromaIcon, arcIcon;

        private Vector2 cursor;

        private bool selected, hovered, drawWindow;

        private List<Vector2> iconLocatons = new List<Vector2>();
        private Vector2 nearestIcon;
        private bool terminalProgression;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public JackalMaidCafePage00()
        {
            AutoProgress = true;
            ClearColor = Calc.HexToColor("f1ecf5")* 0f;
            time = DateTime.Now.ToString("h:mm tt", CultureInfo.CreateSpecificCulture("en-US"));
            //pptIcon = new Vector2(160, 320f);
            chromaIcon = new Vector2(160f, 120f);
            //arcIcon = new Vector2(320f, 120f);
            iconLocatons.AddRange(new List<Vector2> { chromaIcon });
            cursor = new Vector2(1000f, 700f);
        }

        public override IEnumerator Routine()
        {
            yield return 1f;
            while (!selected)
            {
                yield return FreeMoveCursor(20f * Input.Aim.Value, 2* Engine.DeltaTime);
            }
            /*
            yield return MoveCursor(cursor + new Vector2(0f, -80f), 0.3f);
            yield return 0.2f;
            yield return MoveCursor(pptIcon, 0.8f);
            yield return 0.7f;
            selected = true;
            */
            Audio.Play("event:/new_content/game/10_farewell/ppt_doubleclick");
            yield return 0.1f;
            selected = false;
            yield return 0.1f;
            selected = true;
            yield return 0.08f;
            selected = false;
            yield return 0.5f;
            yield return PressButton();
            terminalProgression = true;
            yield return PressButton();
            yield return 0.5f;
            Presentation.ScaleInPoint = chromaIcon;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private IEnumerator FreeMoveCursor(Vector2 speed, float time)
        {
            for (float t = 0f; t < 1f; t += Engine.DeltaTime / time)
            {
                cursor += speed;
                foreach(Vector2 icon in iconLocatons)
                {
                    if (HighlightIcon(icon, cursor)) { hovered = true; nearestIcon = icon; }
                }
                if (hovered) base.WaitingForInput = true;
                if(hovered && Input.MenuConfirm.Check)
                {
                    selected = true;
                    drawWindow = true;
                    base.WaitingForInput = false;
                }
                yield return null;
            }
        }


        private bool HighlightIcon(Vector2 position, Vector2 cursor)
        {
            bool hovering= cursor.X > position.X - 64f && cursor.Y > position.Y - 64f && cursor.X < position.X + 64f && cursor.Y < position.Y + 80f;
            return hovering;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private IEnumerator MoveCursor(Vector2 to, float time)
        {
            Vector2 from = cursor;
            for (float t = 0f; t < 1f; t += Engine.DeltaTime / time)
            {
                cursor = from + (to - from) * Ease.SineOut(t);
                yield return null;
            }
        }

        public override void Update()
        {
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Render()
        {
            MTexture background = Presentation.Gfx["desktop/LavisBG"];
            background.Draw(Vector2.Zero);

            DrawIcon(chromaIcon, "desktop/chromaCrystal", Dialog.Clean("JACKAL_DESKTOP_TEASER"));
            //DrawIcon(arcIcon, "desktop/RCjoystick", Dialog.Clean("JACKAL_DESKTOP_RECYCLEBIN"));
            //DrawIcon(pptIcon, "desktop/chromaCrystal", Dialog.Clean("JACKAL_DESKTOP_POWERPOINT"));
            DrawTaskbar();
            DrawWindow(new Vector2(480,160));
            Presentation.Gfx["desktop/cursor"].DrawCentered(cursor);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void DrawTaskbar()
        {
            Draw.Rect(0f, (float)base.Height - 80f, base.Width, 80f, taskbarColor);
            Draw.Rect(0f, (float)base.Height - 80f, base.Width, 4f, Color.White * 0.5f);
            MTexture mTexture = Presentation.Gfx["desktop/startberry"];
            float num = 64f;
            float num2 = num / (float)mTexture.Height * 0.7f;
            string text = Dialog.Clean("JACKAL_DESKTOP_STARTBUTTON");
            float num3 = 0.6f;
            float width = (float)mTexture.Width * num2 + ActiveFont.Measure(text).X * num3 + 32f;
            Vector2 vector = new Vector2(8f, (float)base.Height - 80f + 8f);
            Draw.Rect(vector.X, vector.Y, width, num, Color.White * 0.5f);
            mTexture.DrawJustified(vector + new Vector2(8f, num / 2f), new Vector2(0f, 0.5f), Color.White, Vector2.One * num2);
            ActiveFont.Draw(text, vector + new Vector2((float)mTexture.Width * num2 + 16f, num / 2f), new Vector2(0f, 0.5f), Vector2.One * num3, Color.Black * 0.8f);
            ActiveFont.Draw(time, new Vector2((float)base.Width - 24f, (float)base.Height - 40f), new Vector2(1f, 0.5f), Vector2.One * 0.6f, Color.Black * 0.8f);
        }

        public void DrawWindow(Vector2 position)
        {
            if (drawWindow)
            {
                Draw.Rect(position.X,position.Y, 1080, 480, Color.Black * 0.8f);
                Draw.HollowRect(position.X,position.Y, 1080, 480, Color.Black);
                string text1 = Dialog.Clean("JACKAL_APPLICATION_DEMO1");
                string text2 = Dialog.Clean("JACKAL_APPLICATION_DEMO2");
                ActiveFont.Draw(text1, position + 40 * Vector2.One, new Vector2(0f, 0.5f), Vector2.One * 0.6f, Color.WhiteSmoke);
                if(terminalProgression) ActiveFont.Draw(text2, position + 100 * Vector2.One - 60 * Vector2.UnitX, new Vector2(0f, 0.5f), Vector2.One * 0.6f, Color.WhiteSmoke);
            }
        }


        private string PrismSuffix()
        {
            string s = "";
            var collected = SusanModule.SusanSaveData.obligatoPrisms.Where(x => x.Item2).Select(item => item.Item1).ToList();
            if (collected.Contains("Amber"))
            {
                s += "A";
            }
            if (collected.Contains("Sapphire"))
            {
                s += "B";
            }
            if (collected.Contains("Citrine"))
            {
                s += "C";
            }
            if (collected.Contains("Emerald"))
            {
                s += "D";
            }
            if (collected.Contains("Ruby"))
            {
                s += "E";
            }
            if (collected.Contains("Amethyst"))
            {
                s += "F";
            }
            
            return s != "" ? s : "NULL";
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void DrawIcon(Vector2 position, string icon, string text)
        {
            bool flag = cursor.X > position.X - 64f && cursor.Y > position.Y - 64f && cursor.X < position.X + 64f && cursor.Y < position.Y + 80f;
            if (selected && flag)
            {
                Draw.Rect(position.X - 80f, position.Y - 80f, 160f, 200f, Color.White * 0.25f);
            }
            if (flag)
            {
                DrawDottedRect(position.X - 80f, position.Y - 80f, 160f, 200f);
            }
            MTexture mTexture = Presentation.Gfx[icon];
            float scale = 128f / (float)mTexture.Height;
            mTexture.DrawCentered(position, Color.White, scale);
            ActiveFont.Draw(text, position + new Vector2(0f, 80f), new Vector2(0.5f, 0f), Vector2.One * 0.6f, (selected && flag) ? Color.Black : Color.White);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void DrawDottedRect(float x, float y, float w, float h)
        {
            float num = 4f;
            Draw.Rect(x, y, w, num, Color.White);
            Draw.Rect(x + w - num, y, num, h, Color.White);
            Draw.Rect(x, y, num, h, Color.White);
            Draw.Rect(x, y + h - num, w, num, Color.White);
            if (!selected)
            {
                for (float num2 = 4f; num2 < w; num2 += num * 2f)
                {
                    Draw.Rect(x + num2, y, num, num, ClearColor);
                    Draw.Rect(x + w - num2, y + h - num, num, num, ClearColor);
                }
                for (float num3 = 4f; num3 < h; num3 += num * 2f)
                {
                    Draw.Rect(x, y + num3, num, num, ClearColor);
                    Draw.Rect(x + w - num, y + h - num3, num, num, ClearColor);
                }
            }
        }
    }

}

