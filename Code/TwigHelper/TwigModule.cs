using Celeste;
using Celeste.Mod;
using Celeste.Mod.JackalCollabHelper.Entities;
using Celeste.Mod.TwigHelper;
using Celeste.Mod.TwigHelper.Entities;
using JackalCollabHelper;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using TwigHelper.ARC_Project;

public class TwigModule : EverestModule
    {
    public static TwigModule Instance;

    public static SpriteBank spriteBank;

    public static SpriteBank spriteBank2;
    public static int DarkMatterState;
    public static int FlagBoosterState;
    public static int InkrailState;
    public static int EdwardState { get; private set; }

    private float lowSpeedTimer = 0.2f;
    private static Color ShroomHairColor = Calc.HexToColor("f20024");
    private static Color ShroomDashTrailColor = Calc.HexToColor("f22745");
    public override Type SettingsType => typeof(TwigModuleSettings);
    public static TwigModuleSettings Settings => (TwigModuleSettings)Instance._Settings;

    public override Type SessionType => typeof(TwigHelperSession);

    public static TwigHelperSession Session => (TwigHelperSession)Instance._Session;

    public TwigModule()
    {
        Instance = this;
    }


    private static void onUpdateLevelStartDashes(On.Celeste.Session.orig_UpdateLevelStartDashes orig, Session self)
    {
        orig(self);

        // "commit" the Cornerberry state
        TwigModule.Session.CornerBerryFlewAway = TwigModule.Session.CornerBerryWillFlyAway;
    }
    private static void onLevelReload(On.Celeste.Level.orig_Reload orig, Level self)
    {
        if (!self.Completed)
        {
            // "reset" the Cornerberry state
            TwigModule.Session.CornerBerryWillFlyAway = TwigModule.Session.CornerBerryFlewAway;
        }
        orig(self);
    }

    private static void onLevelLoadingThread(On.Celeste.LevelLoader.orig_LoadingThread orig, LevelLoader self)
    {
        orig(self);

        // spawn a Kevin barrier renderer if there are Kevin barriers in the map.
        if (self.Level.Session.MapData?.Levels?.Any(level => level.Entities?.Any(entity => entity.Name == "JackalCollabHelper/DarkMatter") ?? false) ?? false)
        {
            self.Level.Add(new DarkMatterRenderer());
        }
    }
    private void Player_ctor(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode)
    {
        orig.Invoke(self, position, spriteMode);
        DarkMatterState = self.StateMachine.AddState(DarkMatter.DarkMatterUpdate, DarkMatter.DarkMatterCoroutine, DarkMatter.DarkMatterBegin, DarkMatter.DarkMatterEnd);
        EdwardState = self.StateMachine.AddState(Edward.EdwardUpdate, Edward.EdwardCoroutine, Edward.EdwardBegin, Edward.EdwardEnd);
        InkrailState = self.StateMachine.AddState(InkrailBoost.Update, InkrailBoost.Coroutine, InkrailBoost.Begin, InkrailBoost.End);
    }

    public Color onGetHairColor(On.Celeste.PlayerHair.orig_GetHairColor orig, PlayerHair self, int index)
    {
        if (Session.HasShroomDash)
        {
            return ShroomHairColor;
        }
        else
        {
            return orig.Invoke(self, index);
        }
    }

    private void onPlayerRender(On.Celeste.Player.orig_Render orig, Player self)
    {
        orig.Invoke(self);
        if (Session.HasShroomDash || Session.ShroomDashTrailActive)
        {
            Vector2 scale = new Vector2(Math.Abs(self.Sprite.Scale.X) * (float)self.Facing, self.Sprite.Scale.Y);
            TrailManager.Add(self, scale, ShroomDashTrailColor);
        }
    }

    private void onPlayerUpdate(On.Celeste.Player.orig_Update orig, Player self)
    {
        // Use player walk speed as a lower bound
        if (self.Speed.X < 64f)
        {
            lowSpeedTimer -= Engine.DeltaTime;
            if (lowSpeedTimer < 0f)
            {
                Session.ShroomDashTrailActive = false;
            }
        }
        else
        {
            lowSpeedTimer = 0.2f;
        }
        orig.Invoke(self);
    }
    public static Player GetPlayer()
    {
        try
        {
            return (Engine.Scene as Level).Tracker.GetEntity<Player>();
        }
        catch (NullReferenceException)
        {
            return null;
        }
    }
    public static Level GetLevel()
    {
        try
        {
            return (Engine.Scene as Level);
        }
        catch (NullReferenceException)
        {
            return new Level();
            return null;
        }
    }
    private void OnCollideHAltered(On.Celeste.Player.orig_OnCollideH orig, Player self, CollisionData data)
    {
        if (self.StateMachine.State == DarkMatterState)
        {
            self.Die(Vector2.Zero, true, true);
        }
        else
        {
            orig.Invoke(self, data);
        }
    }
    private void OnCollideVAltered(On.Celeste.Player.orig_OnCollideV orig, Player self, CollisionData data)
    {
        if (self.StateMachine.State == DarkMatterState)
        {
            self.Die(Vector2.Zero, true, true);
        }
        else
        {
            orig.Invoke(self, data);
        }
    }

    public bool InView(Entity self, Camera camera, float extraSpacing)
    {
        if (self.X > camera.X - extraSpacing && self.Y > camera.Y - extraSpacing && self.X < camera.X + 320 + extraSpacing)
        {
            return self.Y < camera.Y + 180 + extraSpacing;
        }
        return false;
    }

    private void lazyRender(On.Celeste.FloatingDebris.orig_Update orig, FloatingDebris self)
    {
        if(GetLevel() == null)
        {
            self.Visible = false;
        }
        else
        {
            self.Visible = InView(self, GetLevel().Camera, 16f);
        }
        orig(self);
    }
    private void changeCharacter(On.Celeste.Spring.orig_BounceAnimate orig, Spring self)
    {
        if (self is VentSpring)
        {
            (self as VentSpring).ChangeSprite();
        }
        orig.Invoke(self);
    }

    // Set up any hooks, event handlers and your mod in general here.
    // Load runs before Celeste itself has initialized properly.
    public override void Load()
    {
        On.Celeste.Player.DashBegin += ShroomDashBegin;
        On.Celeste.Player.DashEnd += ShroomDashEnd;
        On.Celeste.LevelLoader.LoadingThread += onLevelLoadingThread;
        //On.Celeste.FlyFeather.OnPlayer += DarkMatterOnPlayer;
        On.Celeste.Player.ctor += Player_ctor;
        //On.Celeste.Player.StartDash += Player_StartDash;
        On.Celeste.Player.OnCollideH += OnCollideHAltered;
        On.Celeste.Player.OnCollideV += OnCollideVAltered;
        On.Celeste.Player.Render += onPlayerRender;
        On.Celeste.Player.Update += onPlayerUpdate;
        On.Celeste.PlayerHair.GetHairColor += onGetHairColor;
        On.Celeste.Spring.BounceAnimate += changeCharacter;
        On.Celeste.Session.UpdateLevelStartDashes += onUpdateLevelStartDashes;
        On.Celeste.Level.Reload += onLevelReload;
        CornerGoldenBerry.Load();
        On.Celeste.FloatingDebris.Update += lazyRender;
        //On.Celeste.PlayerSprite.ctor += CustomPlayerSprite;
    }

    // Optional, initialize anything after Celeste has initialized itself properly.



    private void ShroomDashBegin(On.Celeste.Player.orig_DashBegin orig, Player self)
    {
        if (Session.HasShroomDash)
        {
            Session.ShroomDashActive = true;
            Session.ShroomDashTrailActive = true;
            self.Speed *= 1.25f;
        }
        else
        {
            Session.ShroomDashTrailActive = false;
        }
        if(TwigModule.Session.inDDRZone){
            Vector2 aim = new DynData<Player>(self).Get<Vector2>("lastAim");
            aim.Normalize();
            aim.EightWayNormal();
            DDRTrigger d = TwigModule.GetLevel().Tracker.GetNearestEntity<DDRTrigger>(self.Position);
            if (!d.disabled)
            {
                if (GetAim(aim) != d.dirs[d.index])
                {
                    self.Die(Vector2.Zero);
                }
                else
                {
                    d.index++;
                }
            }
        }
        orig.Invoke(self);
    }

    private DDRTrigger.Directions GetAim(Vector2 a)
    {
        int b = -1;
        if(a.X == 0)
        {
            b = (a.Y < 0) ? 0 : 4;
        }
        else if(a.X > 0)
        {
            b = (a.Y < 0) ? 7 : (a.Y == 0) ? 6 : 5;
        }
        else
        {
            b = (a.Y < 0) ? 1 : (a.Y == 0) ? 2 : 3;
        }
        
        return (DDRTrigger.Directions)b;
    }

    private void ShroomDashEnd(On.Celeste.Player.orig_DashEnd orig, Player self)
    {
        orig.Invoke(self);
        if (self.StateMachine.State != 2 && Session.ShroomDashActive)
        {
            Session.ShroomDashActive = false;
            Session.HasShroomDash = false;
            self.Speed *= 1.25f;
        }
    }

    public override void Initialize()
    {
    }

    // Optional, do anything requiring either the Celeste or mod content here.
    public override void LoadContent(bool firstLoad)
    {
        base.LoadContent(firstLoad);
        spriteBank = new SpriteBank(GFX.Game, "Graphics/TwigHelper/Sprites.xml");
        spriteBank2 = new SpriteBank(GFX.Game, "Graphics/TwigHelper/Sprites2.xml");
    }

    // Unload the entirety of your mod's content. Free up any native resources.
    public override void Unload()
    {
        On.Celeste.FloatingDebris.Update -= lazyRender;
        On.Celeste.Player.DashBegin -= ShroomDashBegin;
        On.Celeste.Player.DashEnd -= ShroomDashEnd;
        On.Celeste.LevelLoader.LoadingThread -= onLevelLoadingThread;
        //On.Celeste.FlyFeather.OnPlayer += DarkMatterOnPlayer;
        On.Celeste.Player.ctor -= Player_ctor;
        //On.Celeste.Player.StartDash -= Player_StartDash;
        On.Celeste.Player.OnCollideH -= OnCollideHAltered;
        On.Celeste.Player.OnCollideV -= OnCollideVAltered;
        On.Celeste.Player.Render -= onPlayerRender;
        On.Celeste.Player.Update -= onPlayerUpdate;
        On.Celeste.PlayerHair.GetHairColor -= onGetHairColor;
        //On.Celeste.PlayerSprite.ctor -= CustomPlayerSprite;
        On.Celeste.Spring.BounceAnimate -= changeCharacter;
        On.Celeste.Session.UpdateLevelStartDashes -= onUpdateLevelStartDashes;
        On.Celeste.Level.Reload -= onLevelReload;
        CornerGoldenBerry.Unload();
    }

}
