using System;
using System.Collections.Generic;
using Monocle;
using Celeste.Mod.Entities;
using Celeste.Mod;
using MonoMod.Utils;
using System.Collections;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace Celeste.Mod.JackalCollabHelper.Entities
{
    [CustomEntity("JackalCollabHelper/DarkMatter")]
    [Tracked]
    public class DarkMatter : Solid
    {
        public static readonly Vector2 playerOffset = new Vector2(-16f, -24f);
        internal float Solidify = 0f;
        private static DynData<Player> playerData;
        internal bool Invisible;
        public float starFlyTimer = 2f;
        public Color color;
        public Player player = TwigModule.GetPlayer();
        private List<Vector2> particles = new List<Vector2>();
        public bool isFrozen = false;
        private float[] speeds = new float[3] { 12f, 20f, 40f };
        public float starFlySpeedLerp;
        public BloomPoint starFlyBloom;
        public static Vector2 spdVec;
        public static SoundSource starFlyLoopSfx;
        public Vector2 frozenSpd;
        public static Vector2 lastPos = Vector2.Zero;
        public List<DarkMatter> barriers = new List<DarkMatter>();
        public static Sprite warpSprite;
        public bool spriteCreated = false;
        public bool trackingSet = false;
        public static int dashCountCurrent;
        public static Vector2 spdVector;
        public Vector2[] positions = new Vector2[4];
        public int index = 0;
        public float totalTime;
        public static Vector2 initialSpd;
        public static Vector2 finalSpd;
        public Color mainColor;
        public bool untracked = false;
        public Rectangle rectangle;
        public bool zapLeft = false;
        public bool zapRight = false;
        public bool zapTop = false;
        public bool zapBottom = false;

        public static ScreenshakeAmount screenShakeBeta = ScreenshakeAmount.Off;
        public static bool screenShakeNonbeta = false;

        private static FieldInfo settingsDisableScreenShake = typeof(Settings).GetField("DisableScreenShake"); //Checks for if the boolean DisableScreenShake exists in the code, as well as allows us to cleanly mess with the values. In Beta this should be null.
        private static MemberInfo[] settingsScreenShake = typeof(Settings).GetMember("ScreenShake"); //This is the magic of Reflection at work.
        /* NonBeta - MemberInfo settingsScreenShake is a *property* of type ScreenshakeAmount,
         * Beta - MemberInfo settingsScreenShake is a *field* of type ScreenshakeAmount
         * We *know* it is either one of these, and neatly, we can cast MemberInfo as a FieldInfo
         * if we know it's a field, in the case of the Beta
         */

        public bool needToCall = true;


        //Controller Variables
        public DarkMatterController controller;

        public static float staticAccelerationModifier;
        public static float staticFinalSpeedModifier;
        public static bool staticHasController = false;

        public float darkMatterAcceleration;
        private object screenShakeNonBeta;
        public static bool staticFlipX;
        public static bool staticFlipY;
        public static DarkMatterController staticController;

        public DarkMatter(EntityData data, Vector2 offset, bool zapTop, bool zapBottom, bool zapLeft, bool zapRight)
            : base(data.Position + offset, data.Width, data.Height, safe: false)
        {

            Add(warpSprite = TwigModule.spriteBank.Create("darkMatter"));
        }
        public DarkMatter(EntityData data, Vector2 offset) : this(data, offset, data.Bool("zapTop", defaultValue: false), data.Bool("zapBottom", defaultValue: false), data.Bool("zapLeft", defaultValue: false), data.Bool("zapRight", defaultValue: false))
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Depth = -100;
            Collidable = false;
            // rectangle = new Rectangle((int)TwigModule.GetLevel().Camera.Position.X, (int)TwigModule.GetLevel().Camera.Position.Y, TwigModule.GetLevel().Camera., TwigModule.GetLevel().Camera.Position.X)
            if (scene.Tracker.GetEntities<DarkMatterRenderer>().Count < 1)
            {
                scene.Add(new DarkMatterRenderer());
            }
        }

        public override void Removed(Scene scene)
        {
            scene.Tracker.GetEntity<DarkMatterRenderer>().Untrack(this);

            base.Removed(scene);
        }

        public Color indigoCycle()
        {
            float time = (float)(2 * totalTime % 12);
            int timeInt = (int)time;
            if (TwigModule.GetLevel() != null)
            {
                switch (timeInt)
                {
                    case 1:
                        color = Color.Lerp(Calc.HexToColor(controller.darkMatterBaseColors[3]), Calc.HexToColor(controller.darkMatterBaseColors[4]), time % 1f);
                        break;
                    case 2:
                        color = Color.Lerp(Calc.HexToColor(controller.darkMatterBaseColors[4]), Calc.HexToColor(controller.darkMatterBaseColors[5]), time % 1f);
                        break;
                    case 3:
                        color = Color.Lerp(Calc.HexToColor(controller.darkMatterBaseColors[5]), Calc.HexToColor(controller.darkMatterBaseColors[0]), time % 1f);
                        break;
                    case 4:
                        color = Color.Lerp(Calc.HexToColor(controller.darkMatterBaseColors[0]), Calc.HexToColor(controller.darkMatterBaseColors[1]), time % 1f);
                        break;
                    case 5:
                        color = Color.Lerp(Calc.HexToColor(controller.darkMatterBaseColors[1]), Calc.HexToColor(controller.darkMatterBaseColors[2]), time % 1f);
                        break;
                    case 6:
                        color = Color.Lerp(Calc.HexToColor(controller.darkMatterBaseColors[2]), Calc.HexToColor(controller.darkMatterBaseColors[3]), time % 1f);
                        break;
                    case 7:
                        color = Color.Lerp(Calc.HexToColor(controller.darkMatterBaseColors[3]), Calc.HexToColor(controller.darkMatterBaseColors[4]), time % 1f);
                        break;
                    case 8:
                        color = Color.Lerp(Calc.HexToColor(controller.darkMatterBaseColors[4]), Calc.HexToColor(controller.darkMatterBaseColors[5]), time % 1f);
                        break;
                    case 9:
                        color = Color.Lerp(Calc.HexToColor(controller.darkMatterBaseColors[5]), Calc.HexToColor(controller.darkMatterBaseColors[0]), time % 1f);
                        break;
                    case 10:
                        color = Color.Lerp(Calc.HexToColor(controller.darkMatterBaseColors[0]), Calc.HexToColor(controller.darkMatterBaseColors[1]), time % 1f);
                        break;
                    case 11:
                        color = Color.Lerp(Calc.HexToColor(controller.darkMatterBaseColors[1]), Calc.HexToColor(controller.darkMatterBaseColors[2]), time % 1f);
                        break;
                    default:
                        color = Color.Lerp(Calc.HexToColor(controller.darkMatterBaseColors[2]), Calc.HexToColor(controller.darkMatterBaseColors[3]), time % 1f);
                        break;
                }
            }
            return color;
        }


        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (!this.CollideCheck<DarkMatter>(CenterRight + Vector2.UnitX))
            {
                zapRight = true;
            }
            if (!this.CollideCheck<DarkMatter>(CenterLeft - Vector2.UnitX))
            {
                zapLeft = true;
            }
            if (!this.CollideCheck<DarkMatter>(TopCenter - Vector2.UnitY))
            {
                zapTop = true;
            }
            if (!this.CollideCheck<DarkMatter>(BottomCenter + Vector2.UnitY))
            {
                zapBottom = true;
            }
            //scene.Tracker.GetEntity<DarkMatterRenderer>().Track(this, this.SceneAs<Level>());
            foreach (DarkMatter barrier in (scene as Level).Tracker.GetEntitiesCopy<DarkMatter>())
            {
                barriers.Add(barrier);
            }
            if ((scene as Level).Tracker.GetEntities<DarkMatterController>().Count >= 1)
            {
                controller = (this.Scene as Level).Tracker.GetEntity<DarkMatterController>();
            }
            else
            {
                scene.Add(controller = new DarkMatterController(Vector2.Zero, 0.6f, 4.0f, 96f, "3b0c1c", "3b0c2c", "3b0c3c", "3b0c4c", "3b0c5c", "3b0c6c", "5e0824", "5e0834", "5e0844", "5e0854", "5e0864", "5e0874", false, false));
            }
            staticController = controller;
            for (int i = 0; i < Width * Height / (controller.darkMatterParticleDensity); i++)
            {
                particles.Add(new Vector2(Calc.Random.NextFloat(Width - 1f), Calc.Random.NextFloat(Height - 1f)) * 2);
            }
            if ((scene as Level) != null)
            {
                DarkMatterRenderer x = scene.Tracker.GetEntity<DarkMatterRenderer>();
                if (x != null) x.Track(this, this.SceneAs<Level>(), controller); else { scene.Add(x = new DarkMatterRenderer()); x.Track(this, scene as Level, controller); } //if this breaks I'm gonna flip my shit.
                needToCall = false;
            }

            untracked = false;

            staticAccelerationModifier = (controller.darkMatterAccelerationModifier / 100 + 1);
            staticFinalSpeedModifier = controller.darkMatterFinalSpeedMultiplier;
            staticFlipX = controller.flipX;
            staticFlipY = controller.flipY;
            


        }

        public override void Update()
        {
            base.Update();
            if (this.SceneAs<Level>() != null && needToCall)
            {
                this.Scene.Tracker.GetEntity<DarkMatterRenderer>().Track(this, this.SceneAs<Level>(), controller);
                needToCall = false;
            }
            if (TwigModule.GetLevel() == null)
            {
                Visible = false;
            }
            else
            {
                Visible = InView(TwigModule.GetLevel().Camera, Math.Max(Width, Height));
            }

            totalTime += Engine.DeltaTime;
            warpSprite.Play("boost");
            player = TwigModule.GetPlayer();
            if (player != null)
            {
                if (!spriteCreated)
                {
                    player.Add(warpSprite = TwigModule.spriteBank.Create("darkMatter"));
                    warpSprite.Visible = false;
                    spriteCreated = true;

                }
                warpSprite.RenderPosition = player.Position + playerOffset;

                if (this.InBoundsCheck(player, this) && (player.StateMachine.State != TwigModule.DarkMatterState))
                {
                    if (!isFrozen && player.StateMachine.State != TwigModule.DarkMatterState)
                    {
                        TwigModule.Session.frozenXSpeed = player.Speed.X;
                        TwigModule.Session.frozenYSpeed = player.Speed.Y;
                    }
                    isFrozen = true;
                    player.StateMachine.State = TwigModule.DarkMatterState;
                }
                else if (!this.InBoundsCheck(player, this) && !InBoundsCheckAny(player, barriers))
                {
                    isFrozen = false;
                    if (player.StateMachine.State == TwigModule.DarkMatterState)
                    {
                        player.StateMachine.State = 0;
                    }

                }
            }
            for (int i = 0; i < particles.Count; i++)
            {
                int random = i % 8;
                Vector2 dir = Vector2.Zero;
                switch (random)
                {
                    case 0:
                        dir = Vector2.UnitX;
                        break;
                    case 1:
                        dir = -Vector2.UnitX;
                        break;
                    case 2:
                        dir = Vector2.UnitY;
                        break;
                    case 3:
                        dir = -Vector2.UnitY;
                        break;
                    case 4:
                        dir = Vector2.UnitX + Vector2.UnitY;
                        break;
                    case 5:
                        dir = Vector2.UnitX - Vector2.UnitY;
                        break;
                    case 6:
                        dir = -Vector2.UnitX + Vector2.UnitY;
                        break;
                    case 7:
                        dir = -Vector2.UnitX - Vector2.UnitY;
                        break;

                }
                Vector2 value = particles[i];
                value += (dir * speeds[i % speeds.Length] * Engine.DeltaTime);
                value.Y += Height;
                value.X += Width;
                value.Y %= Height;
                value.X %= Width;
                particles[i] = value;
            }
        }


        public bool InView(Camera camera, float extraSpacing)
        {
            if (base.X > camera.X - extraSpacing && base.Y > camera.Y - extraSpacing && base.X < camera.X + 320 + extraSpacing)
            {
                return base.Y < camera.Y + 180 + extraSpacing;
            }
            return false;
        }


        public override void Render()
        {
            mainColor = color = indigoCycle();
            if (!Invisible)
            {
                Color color2 = Color.White * 0.5f;
                foreach (Vector2 particle in particles)
                {
                    Draw.Pixel.Draw(Position + particle, Vector2.Zero, color2);
                }
                Draw.Rect(this.Collider, mainColor * 0.6f);
            }

        }

        public bool InBoundsCheck(Player player, DarkMatter self)
        {

            if (player.Position.X < self.Position.X)
            {
                return false;
            }
            if ((player.Position.X > (self.Position.X + Width)))
            {
                return false;
            }
            if ((player.Position.Y < self.Position.Y))
            {
                return false;
            }
            if ((player.Position.Y > (self.Position.Y + Height)))
            {
                return false;
            }
            return true;
        }



        public bool InBoundsCheckAny(Player player, List<DarkMatter> barriers)
        {
            foreach (DarkMatter barrier in barriers)
            {
                if (barrier.InBoundsCheck(player, barrier))
                {
                    return true;
                }
            }
            return false;
        }


        public bool InBoundsCheckAny(Vector2 playerPos, List<DarkMatter> barriers)
        {
            foreach (DarkMatter barrier in barriers)
            {
                if (barrier.InBoundsCheck(playerPos, barrier))
                {
                    return true;
                }
            }
            return false;
        }



        public bool InBoundsCheck(Vector2 playerPos, DarkMatter self)
        {
            if (playerPos.X < self.Position.X)
            {
                return false;
            }
            if ((playerPos.X > (self.Position.X + Width)))
            {
                return false;
            }
            if ((playerPos.Y < self.Position.Y))
            {
                return false;
            }
            if ((playerPos.Y > (self.Position.Y + Height)))
            {
                return false;
            }
            return true;
        }

        public static IEnumerator DarkMatterCoroutine()
        {
            Player player = TwigModule.GetPlayer();
            Level level = TwigModule.GetLevel();
            playerData = new DynData<Player>(player);
            yield return 0.1f;
            //level.Displacement.AddBurst(player.Center, 0.25f, 16f, 32f);
            player.RefillStamina();
            Vector2 unitSpd = spdVector;
            unitSpd.Normalize();
            Vector2 launch = new Vector2(TwigModule.Session.frozenXSpeed, TwigModule.Session.frozenYSpeed);
            float amplitude = (float)Math.Sqrt(Math.Pow(launch.X, 2) + Math.Pow(launch.Y, 2));
            if (amplitude < 200f)
            {
                amplitude = 200f;
            }
            launch.Normalize();
            //float accel = amplitude * (float)Math.Pow(1.5, boostTime);
            player.Speed = launch * amplitude;
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            level.DirectionalShake(unitSpd);
        }

        public static void DarkMatterBegin()
        {
            MessWithScreenShake(true);
            Player player = TwigModule.GetPlayer();
            playerData = new DynData<Player>(player);
            dashCountCurrent = player.Dashes;
            if (dashCountCurrent < 1)
            {
                dashCountCurrent = 1;
            }
            player.Dashes = 0;
            warpSprite.Visible = true;
            player.Speed.X *= (staticFlipX ? -1f : 1f);
            player.Speed.Y *= (staticFlipY ? -1f : 1f);
            initialSpd = player.Speed;
            finalSpd = initialSpd * (staticHasController ? staticFinalSpeedModifier : 4.0f);
        }

        public static void DarkMatterEnd()
        {
            MessWithScreenShake(false);
            Player player = TwigModule.GetPlayer();
            playerData = new DynData<Player>(player);
            player.Dashes = dashCountCurrent;
            player.RefillDash();
            player.Sprite.HairCount = playerData.Get<int>("startHairCount");
            player.Sprite.Visible = true;
            player.Hair.Visible = true;
            warpSprite.Visible = false;
            initialSpd = Vector2.Zero;
            finalSpd = Vector2.Zero;
        }



        public static int DarkMatterUpdate()
        {
            Player player = TwigModule.GetPlayer();
            player.Dashes = 0;
            if (player.Speed == Vector2.Zero)
            {
                player.Die(Vector2.Zero, true, true);
            }

            float amplitude = (float)Math.Sqrt(Math.Pow(player.Speed.X, 2) + Math.Pow(player.Speed.Y, 2));
            Vector2 unitMovement = (player == null ? Vector2.One : Vector2.Normalize(player.Speed));

            if (amplitude < 200f)
            {
                amplitude = 200f;
            }
            player.Speed = unitMovement * amplitude;
            player.Speed *= new Vector2(staticAccelerationModifier, staticAccelerationModifier);
            if (finalSpd != null)
            {
                player.Speed.X = Math.Min(Math.Abs(player.Speed.X), Math.Abs(finalSpd.X)) * (unitMovement.X > 0 ? 1 : -1);
            }
            if ((float)unitMovement.Y != 0f && finalSpd != null)
            {
                player.Speed.Y = Math.Min(Math.Abs(player.Speed.Y), Math.Abs(finalSpd.Y)) * (unitMovement.Y > 0 ? 1 : -1);
            }
            Input.Rumble(RumbleStrength.Climb, RumbleLength.Short);
            player.Sprite.Visible = false;
            player.Hair.Visible = false;
            return TwigModule.DarkMatterState;
        }


        public bool camCheck()
        {
            Camera camera = (this.Scene as Level).Camera;
            if (base.X > camera.X - 16f && base.Y > camera.Y - 16f && base.X < camera.X + 320f + 16f)
            {
                return base.Y < camera.Y + 180f + 16f;
            }
            return false;
        }

        private static void MessWithScreenShake(bool begin)
        {
            //Checks for if DisableScreenShake exists, if it does we're in Nonbeta.
            if (settingsDisableScreenShake != null)
            { //That means we're gonna fuck with disabling it for the time being.
                if (begin)
                {
                    screenShakeNonbeta = (bool)settingsDisableScreenShake.GetValue(Settings.Instance);
                    settingsDisableScreenShake.SetValue(Settings.Instance, true);
                }
                else
                {
                    settingsDisableScreenShake.SetValue(Settings.Instance, screenShakeNonbeta);
                    screenShakeNonbeta = true;
                }
            }
            else  //This is the part of the timeline where we are in Beta. Beta is structured a little differently.
            { // settingsScreenShake is a field, meaning running = is not set_Property()
                if (settingsScreenShake.Length < 1) throw new Exception("This crashed because of a bug with Celeste. Please report this error_log.txt and your log.txt to the Winter Collab Bug Reports discord at https://discord.gg/YN8VaAVcPN \n Game crashed due to being unable to find Member ScreenshakeAmount ScreenShake.");
                if (begin)
                {
                    screenShakeBeta = (ScreenshakeAmount)(settingsScreenShake[0] as FieldInfo).GetValue(Settings.Instance);
                    (settingsScreenShake[0] as FieldInfo).SetValue(Settings.Instance, ScreenshakeAmount.Off);
                }
                else
                {
                    (settingsScreenShake[0] as FieldInfo).SetValue(Settings.Instance, screenShakeBeta);
                    screenShakeBeta = ScreenshakeAmount.Off;
                }
            }
        }
    }
}
