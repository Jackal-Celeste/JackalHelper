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
using MonoMod.Utils;
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using FMOD.Studio;

namespace Celeste.Mod.SusanHelperNew.Obligato
{
	[CustomEntity("SusanHelper/JackalMaidCafe")]
    [Tracked]
	public class JackalMaidCafe : JumpThru
	{
        private Entity frontEntity;

        private Image backSprite;

        private Image frontRightSprite;

        private Image frontLeftSprite;

        private Sprite noise;

        private Sprite neon;

        private Solid frontWall;

        private float insideEase;

        private float cameraEase;

        private bool playerInside;

        private bool inCutscene;

        private Coroutine routine;

        private JackalMaidCafePresentation presentation;

        private float interactStartZoom;

        private EventInstance snapshot;

        private EventInstance usingSfx;

        private SoundSource signSfx;

        private TalkComponent talk;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public JackalMaidCafe(Vector2 position)
            : base(position, 88, safe: true)
        {
            base.Tag = Tags.TransitionUpdate;
            base.Depth = 1000;
            base.Collider = null;
            Add(backSprite = new Image(GFX.Game["objects/wavedashtutorial/building_back"]));
            backSprite.JustifyOrigin(0.5f, 1f);
            Add(noise = new Sprite(GFX.Game, "objects/wavedashtutorial/noise"));
            noise.AddLoop("static", "", 0.05f);
            noise.Play("static");
            noise.CenterOrigin();
            noise.Position = new Vector2(0f, -30f);
            noise.Color = Color.White * 0.5f;
            Add(frontLeftSprite = new Image(GFX.Game["objects/wavedashtutorial/building_front_left"]));
            frontLeftSprite.JustifyOrigin(0.5f, 1f);
            Add(talk = new TalkComponent(new Rectangle(-12, -8, 24, 8), new Vector2(0f, -50f), OnInteract));
            talk.Enabled = false;
            SurfaceSoundIndex = 42;
            Collidable = false;
        }

        public JackalMaidCafe(EntityData data, Vector2 position)
            : this(data.Position + position)
        {
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(frontEntity = new Entity(Position));
            frontEntity.Tag = Tags.TransitionUpdate;
            frontEntity.Depth = -10500;
            frontEntity.Add(frontRightSprite = new Image(GFX.Game["objects/wavedashtutorial/building_front_right"]));
            frontRightSprite.JustifyOrigin(0.5f, 1f);
            frontEntity.Add(neon = new Sprite(GFX.Game, "objects/wavedashtutorial/neon_"));
            neon.AddLoop("loop", "", 0.07f);
            neon.Play("loop");
            neon.JustifyOrigin(0.5f, 1f);
            scene.Add(frontWall = new Solid(Position + new Vector2(-41f, -59f), 88f, 38f, safe: true));
            frontWall.SurfaceSoundIndex = 42;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Add(signSfx = new SoundSource(new Vector2(8f, -16f), "event:/new_content/env/local/cafe_sign"));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Update()
        {
            base.Update();
            if (!inCutscene)
            {
                Player entity = base.Scene.Tracker.GetEntity<Player>();
                if (entity != null)
                {
                    frontWall.Collidable = true;
                    bool flag = (entity.X > base.X - 37f && entity.X < base.X + 46f && entity.Y > base.Y - 58f) || frontWall.CollideCheck(entity);
                    if (flag != playerInside)
                    {
                        playerInside = flag;
                        if (playerInside)
                        {
                            signSfx.Stop();
                            snapshot = Audio.CreateSnapshot("snapshot:/game_10_inside_cafe");
                        }
                        else
                        {
                            signSfx.Play("event:/new_content/env/local/cafe_sign");
                            Audio.ReleaseSnapshot(snapshot);
                            snapshot = null;
                        }
                    }
                }
                //SceneAs<Level>().ZoomSnap(new Vector2(160f, 90f), 1f + Ease.QuadInOut(cameraEase) * 0.75f);
            }
            talk.Enabled = playerInside;
            frontWall.Collidable = !playerInside;
            insideEase = Calc.Approach(insideEase, playerInside ? 1f : 0f, Engine.DeltaTime * 4f);
            cameraEase = Calc.Approach(cameraEase, playerInside ? 1f : 0f, Engine.DeltaTime * 2f);
            frontRightSprite.Color = Color.White * (1f - insideEase);
            frontLeftSprite.Color = frontRightSprite.Color;
            neon.Color = frontRightSprite.Color;
            frontRightSprite.Visible = insideEase < 1f;
            frontLeftSprite.Visible = insideEase < 1f;
            neon.Visible = insideEase < 1f;
            if (base.Scene.OnInterval(0.05f))
            {
                noise.Scale = Calc.Random.Choose(new Vector2(1f, 1f), new Vector2(-1f, 1f), new Vector2(1f, -1f), new Vector2(-1f, -1f));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void OnInteract(Player player)
        {
            if (!inCutscene)
            {
                Level level = base.Scene as Level;
                if (usingSfx != null)
                {
                    Audio.SetParameter(usingSfx, "end", 1f);
                    Audio.Stop(usingSfx);
                }
                inCutscene = true;
                interactStartZoom = level.ZoomTarget;
                level.StartCutscene(SkipInteraction, fadeInOnSkip: true, endingChapterAfterCutscene: false, resetZoomOnSkip: false);
                Add(routine = new Coroutine(InteractRoutine(player)));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private IEnumerator InteractRoutine(Player player)
        {
            Level level = Scene as Level;
            player.StateMachine.State = 11;
            player.StateMachine.Locked = true;
            yield return CutsceneEntity.CameraTo(new Vector2(X, Y - 30f) - new Vector2(160f, 90f), 0.25f, Ease.CubeOut);
            yield return level.ZoomTo(new Vector2(160f, 90f), 10f, 1f);
            usingSfx = Audio.Play("event:/state/cafe_computer_active", player.Position);
            Audio.Play("event:/new_content/game/10_farewell/cafe_computer_on", player.Position);
            Audio.Play("event:/new_content/game/10_farewell/cafe_computer_startupsfx", player.Position);
            presentation = new JackalMaidCafePresentation(usingSfx);
            Scene.Add(presentation);
            while (presentation.Viewing)
            {
                yield return null;
            }
            yield return level.ZoomTo(new Vector2(160f, 90f), interactStartZoom, 1f);
            player.StateMachine.Locked = false;
            player.StateMachine.State = 0;
            inCutscene = false;
            (player.Scene as Level).Session.SetFlag("Obligato_alternateColor", true);
            level.EndCutscene();
            Audio.SetAltMusic(null);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void SkipInteraction(Level level)
        {
            Audio.SetAltMusic(null);
            inCutscene = false;
            level.ZoomSnap(new Vector2(160f, 90f), interactStartZoom);
            if (usingSfx != null)
            {
                Audio.SetParameter(usingSfx, "end", 1f);
                usingSfx.release();
            }
            if (presentation != null)
            {
                presentation.RemoveSelf();
            }
            presentation = null;
            if (routine != null)
            {
                routine.RemoveSelf();
            }
            routine = null;
            Player entity = level.Tracker.GetEntity<Player>();
            if (entity != null)
            {
                entity.StateMachine.Locked = false;
                entity.StateMachine.State = 0;
            }
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Dispose();
        }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            Dispose();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void Dispose()
        {
            if (usingSfx != null)
            {
                Audio.SetParameter(usingSfx, "quit", 1f);
                usingSfx.release();
                usingSfx = null;
            }
            Audio.ReleaseSnapshot(snapshot);
            snapshot = null;
        }
    }



    public class JackalMaidCafePresentation : Entity
    {
        public Vector2 ScaleInPoint = new Vector2(1920f, 1080f) / 2f;

        public readonly int ScreenWidth = 1920;

        public readonly int ScreenHeight = 1080;

        private float ease;

        private bool loading;

        private float waitingForInputTime;

        private VirtualRenderTarget screenBuffer;

        private VirtualRenderTarget prevPageBuffer;

        private VirtualRenderTarget currPageBuffer;

        private int pageIndex;

        public List<JackalMaidCafePage> pages = new List<JackalMaidCafePage>();

        private float pageEase;

        private bool pageTurning;

        private bool pageUpdating;

        private bool waitingForPageTurn;

        private VertexPositionColorTexture[] verts = new VertexPositionColorTexture[6];

        private EventInstance usingSfx;

        public bool Viewing { get; private set; }

        public Atlas Gfx { get; private set; }

        public bool ShowInput
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            get
            {
                if (!waitingForPageTurn)
                {
                    if (CurrPage != null)
                    {
                        return CurrPage.WaitingForInput;
                    }
                    return false;
                }
                return true;
            }
        }

        private JackalMaidCafePage PrevPage
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            get
            {
                if (pageIndex <= 0)
                {
                    return null;
                }
                return pages[pageIndex - 1];
            }
        }

        private JackalMaidCafePage CurrPage
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            get
            {
                if (pageIndex >= pages.Count)
                {
                    return null;
                }
                return pages[pageIndex];
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public JackalMaidCafePresentation(EventInstance usingSfx = null)
        {
            base.Tag = Tags.HUD;
            Viewing = true;
            loading = true;
            Add(new Coroutine(Routine()));
            this.usingSfx = usingSfx;
            RunThread.Start(LoadingThread, "Wave Dash Presentation Loading", highPriority: true);

        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void LoadingThread()
        {
            Gfx = Atlas.FromAtlas(Path.Combine("Graphics", "Atlases", "WaveDashing"), Atlas.AtlasDataFormat.Packer);
            loading = false;
        }

        private IEnumerator Routine()
        {
            while (loading)
            {
                yield return null;
            }

            pages.Add(new JackalMaidCafePage00());
            //pages.Add(new JackalMaidCafePage03());
            //pages.Add(new JackalMaidCafePage04());
            /*
            pages.Add(new JackalMaidCafePage00());
            pages.Add(new JackalMaidCafePage01());
            pages.Add(new JackalMaidCafePage02());
            pages.Add(new JackalMaidCafePage03());
            pages.Add(new JackalMaidCafePage04());
            pages.Add(new JackalMaidCafePage05());
            pages.Add(new JackalMaidCafePage06());
            */
            foreach (JackalMaidCafePage page in pages)
            {
                page.Added(this);
            }
            Add(new BeforeRenderHook(BeforeRender));
            while (ease < 1f)
            {
                ease = Calc.Approach(ease, 1f, Engine.DeltaTime * 2f);
                yield return null;
            }
            while (pageIndex < pages.Count)
            {
                pageUpdating = true;
                yield return CurrPage.Routine();
                if (!CurrPage.AutoProgress)
                {
                    waitingForPageTurn = true;
                    while (!Input.MenuConfirm.Pressed)
                    {
                        yield return null;
                    }
                    waitingForPageTurn = false;
                    Audio.Play("event:/new_content/game/10_farewell/ppt_mouseclick");
                }
                pageUpdating = false;
                pageIndex++;
                if (pageIndex < pages.Count)
                {
                    float num = 0.5f;
                    if (CurrPage.Transition == JackalMaidCafePage.Transitions.Rotate3D)
                    {
                        num = 1.5f;
                    }
                    else if (CurrPage.Transition == JackalMaidCafePage.Transitions.Blocky)
                    {
                        num = 1f;
                    }
                    pageTurning = true;
                    pageEase = 0f;
                    Add(new Coroutine(TurnPage(num)));
                    yield return num * 0.8f;
                }
            }
            if (usingSfx != null)
            {
                Audio.SetParameter(usingSfx, "end", 1f);
                usingSfx.release();
            }
            Audio.Play("event:/new_content/game/10_farewell/cafe_computer_off");
            while (ease > 0f)
            {
                ease = Calc.Approach(ease, 0f, Engine.DeltaTime * 2f);
                yield return null;
            }
            Viewing = false;
            RemoveSelf();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private IEnumerator TurnPage(float duration)
        {
            if (CurrPage.Transition != 0 && CurrPage.Transition != JackalMaidCafePage.Transitions.FadeIn)
            {
                if (CurrPage.Transition == JackalMaidCafePage.Transitions.Rotate3D)
                {
                    Audio.Play("event:/new_content/game/10_farewell/ppt_cube_transition");
                }
                else if (CurrPage.Transition == JackalMaidCafePage.Transitions.Blocky)
                {
                    Audio.Play("event:/new_content/game/10_farewell/ppt_dissolve_transition");
                }
                else if (CurrPage.Transition == JackalMaidCafePage.Transitions.Spiral)
                {
                    Audio.Play("event:/new_content/game/10_farewell/ppt_spinning_transition");
                }
            }
            while (pageEase < 1f)
            {
                pageEase += Engine.DeltaTime / duration;
                yield return null;
            }
            pageTurning = false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void BeforeRender()
        {
            if (loading)
            {
                return;
            }
            if (screenBuffer == null || screenBuffer.IsDisposed)
            {
                screenBuffer = VirtualContent.CreateRenderTarget("WaveDash-Buffer", ScreenWidth, ScreenHeight, depth: true);
            }
            if (prevPageBuffer == null || prevPageBuffer.IsDisposed)
            {
                prevPageBuffer = VirtualContent.CreateRenderTarget("WaveDash-Screen1", ScreenWidth, ScreenHeight);
            }
            if (currPageBuffer == null || currPageBuffer.IsDisposed)
            {
                currPageBuffer = VirtualContent.CreateRenderTarget("WaveDash-Screen2", ScreenWidth, ScreenHeight);
            }
            if (pageTurning && PrevPage != null)
            {
                Engine.Graphics.GraphicsDevice.SetRenderTarget(prevPageBuffer);
                Engine.Graphics.GraphicsDevice.Clear(PrevPage.ClearColor);
                Draw.SpriteBatch.Begin();
                PrevPage.Render();
                Draw.SpriteBatch.End();
            }
            if (CurrPage != null)
            {
                Engine.Graphics.GraphicsDevice.SetRenderTarget(currPageBuffer);
                Engine.Graphics.GraphicsDevice.Clear(CurrPage.ClearColor);
                Draw.SpriteBatch.Begin();
                CurrPage.Render();
                Draw.SpriteBatch.End();
            }
            Engine.Graphics.GraphicsDevice.SetRenderTarget(screenBuffer);
            Engine.Graphics.GraphicsDevice.Clear(Color.Black);
            if (pageTurning)
            {
                if (CurrPage.Transition == JackalMaidCafePage.Transitions.ScaleIn)
                {
                    Draw.SpriteBatch.Begin();
                    Draw.SpriteBatch.Draw((RenderTarget2D)prevPageBuffer, Vector2.Zero, Color.White);
                    Vector2 scale = Vector2.One * pageEase;
                    Draw.SpriteBatch.Draw((RenderTarget2D)currPageBuffer, ScaleInPoint, currPageBuffer.Bounds, Color.White, 0f, ScaleInPoint, scale, SpriteEffects.None, 0f);
                    Draw.SpriteBatch.End();
                }
                else if (CurrPage.Transition == JackalMaidCafePage.Transitions.FadeIn)
                {
                    Draw.SpriteBatch.Begin();
                    Draw.SpriteBatch.Draw((RenderTarget2D)prevPageBuffer, Vector2.Zero, Color.White);
                    Draw.SpriteBatch.Draw((RenderTarget2D)currPageBuffer, Vector2.Zero, Color.White * pageEase);
                    Draw.SpriteBatch.End();
                }
                else if (CurrPage.Transition == JackalMaidCafePage.Transitions.Rotate3D)
                {
                    float num = -(float)Math.PI / 2f * pageEase;
                    RenderQuad((RenderTarget2D)prevPageBuffer, pageEase, num);
                    RenderQuad((RenderTarget2D)currPageBuffer, pageEase, (float)Math.PI / 2f + num);
                }
                else if (CurrPage.Transition == JackalMaidCafePage.Transitions.Blocky)
                {
                    Draw.SpriteBatch.Begin();
                    Draw.SpriteBatch.Draw((RenderTarget2D)prevPageBuffer, Vector2.Zero, Color.White);
                    uint seed = 1u;
                    int num2 = ScreenWidth / 60;
                    for (int i = 0; i < ScreenWidth; i += num2)
                    {
                        for (int j = 0; j < ScreenHeight; j += num2)
                        {
                            if (PseudoRandRange(ref seed, 0f, 1f) <= pageEase)
                            {
                                Draw.SpriteBatch.Draw((RenderTarget2D)currPageBuffer, new Rectangle(i, j, num2, num2), new Rectangle(i, j, num2, num2), Color.White);
                            }
                        }
                    }
                    Draw.SpriteBatch.End();
                }
                else if (CurrPage.Transition == JackalMaidCafePage.Transitions.Spiral)
                {
                    Draw.SpriteBatch.Begin();
                    Draw.SpriteBatch.Draw((RenderTarget2D)prevPageBuffer, Vector2.Zero, Color.White);
                    Vector2 scale2 = Vector2.One * pageEase;
                    float rotation = (1f - pageEase) * 12f;
                    Draw.SpriteBatch.Draw((RenderTarget2D)currPageBuffer, global::Celeste.Celeste.TargetCenter, currPageBuffer.Bounds, Color.White, rotation, global::Celeste.Celeste.TargetCenter, scale2, SpriteEffects.None, 0f);
                    Draw.SpriteBatch.End();
                }
            }
            else
            {
                Draw.SpriteBatch.Begin();
                Draw.SpriteBatch.Draw((RenderTarget2D)currPageBuffer, Vector2.Zero, Color.White);
                Draw.SpriteBatch.End();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void RenderQuad(Texture texture, float ease, float rotation)
        {
            float num = (float)screenBuffer.Width / (float)screenBuffer.Height;
            float num2 = num;
            float num3 = 1f;
            Vector3 position = new Vector3(0f - num2, num3, 0f);
            Vector3 position2 = new Vector3(num2, num3, 0f);
            Vector3 position3 = new Vector3(num2, 0f - num3, 0f);
            Vector3 position4 = new Vector3(0f - num2, 0f - num3, 0f);
            verts[0].Position = position;
            verts[0].TextureCoordinate = new Vector2(0f, 0f);
            verts[0].Color = Color.White;
            verts[1].Position = position2;
            verts[1].TextureCoordinate = new Vector2(1f, 0f);
            verts[1].Color = Color.White;
            verts[2].Position = position3;
            verts[2].TextureCoordinate = new Vector2(1f, 1f);
            verts[2].Color = Color.White;
            verts[3].Position = position;
            verts[3].TextureCoordinate = new Vector2(0f, 0f);
            verts[3].Color = Color.White;
            verts[4].Position = position3;
            verts[4].TextureCoordinate = new Vector2(1f, 1f);
            verts[4].Color = Color.White;
            verts[5].Position = position4;
            verts[5].TextureCoordinate = new Vector2(0f, 1f);
            verts[5].Color = Color.White;
            float num4 = 4.15f + Calc.YoYo(ease) * 1.7f;
            Matrix value = Matrix.CreateTranslation(0f, 0f, num) * Matrix.CreateRotationY(rotation) * Matrix.CreateTranslation(0f, 0f, 0f - num4) * Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 4f, num, 1f, 10f);
            Engine.Instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            Engine.Instance.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            Engine.Instance.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            Engine.Instance.GraphicsDevice.Textures[0] = texture;
            GFX.FxTexture.Parameters["World"].SetValue(value);
            foreach (EffectPass pass in GFX.FxTexture.CurrentTechnique.Passes)
            {
                pass.Apply();
                Engine.Instance.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verts, 0, verts.Length / 3);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Update()
        {
            base.Update();
            if (ShowInput)
            {
                waitingForInputTime += Engine.DeltaTime;
            }
            else
            {
                waitingForInputTime = 0f;
            }
            if (!loading && CurrPage != null && pageUpdating)
            {
                CurrPage.Update();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void Render()
        {
            if (!loading && screenBuffer != null && !screenBuffer.IsDisposed)
            {
                float num = (float)ScreenWidth * Ease.CubeOut(Calc.ClampedMap(ease, 0f, 0.5f));
                float num2 = (float)ScreenHeight * Ease.CubeInOut(Calc.ClampedMap(ease, 0.5f, 1f, 0.2f));
                Rectangle rectangle = new Rectangle((int)((1920f - num) / 2f), (int)((1080f - num2) / 2f), (int)num, (int)num2);
                Draw.SpriteBatch.Draw((RenderTarget2D)screenBuffer, rectangle, Color.White);
                if (ShowInput && waitingForInputTime > 0.2f)
                {
                    GFX.Gui["textboxbutton"].DrawCentered(new Vector2(1856f, 1016 + ((base.Scene.TimeActive % 1f < 0.25f) ? 6 : 0)), Color.Black);
                }
                if ((base.Scene as Level).Paused)
                {
                    Draw.Rect(rectangle, Color.Black * 0.7f);
                }
            }
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Dispose();
        }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            Dispose();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void Dispose()
        {
            while (loading)
            {
                Thread.Sleep(1);
            }
            if (screenBuffer != null)
            {
                screenBuffer.Dispose();
            }
            screenBuffer = null;
            if (prevPageBuffer != null)
            {
                prevPageBuffer.Dispose();
            }
            prevPageBuffer = null;
            if (currPageBuffer != null)
            {
                currPageBuffer.Dispose();
            }
            currPageBuffer = null;
            Gfx.Dispose();
            Gfx = null;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static uint PseudoRand(ref uint seed)
        {
            uint num = seed;
            num ^= num << 13;
            num ^= num >> 17;
            return seed = num ^ (num << 5);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static float PseudoRandRange(ref uint seed, float min, float max)
        {
            return min + (float)(PseudoRand(ref seed) % 1000u) / 1000f * (max - min);
        }
    }


    public abstract class JackalMaidCafePage
    {
        public enum Transitions
        {
            ScaleIn,
            FadeIn,
            Rotate3D,
            Blocky,
            Spiral
        }

        public JackalMaidCafePresentation Presentation;

        public Color ClearColor;

        public Transitions Transition;

        public bool AutoProgress;

        public bool WaitingForInput;

        public int Width => Presentation.ScreenWidth;

        public int Height => Presentation.ScreenHeight;

        public abstract IEnumerator Routine();

        public virtual void Added(JackalMaidCafePresentation presentation)
        {
            Presentation = presentation;
        }

        public virtual void Update()
        {
        }

        public virtual void Render()
        {
        }

        protected IEnumerator PressButton()
        {
            WaitingForInput = true;
            while (!Input.MenuConfirm.Pressed)
            {
                yield return null;
            }
            WaitingForInput = false;
            Audio.Play("event:/new_content/game/10_farewell/ppt_mouseclick");
        }
    }


}

