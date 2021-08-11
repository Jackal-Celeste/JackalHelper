// Example usings.
using System;
using System.Collections;
using System.Reflection;
using Celeste;
using Celeste.Mod;
using Celeste.Mod.JackalHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;


public class JackalModule : EverestModule
{

	// Only one alive module instance can exist at any given time.
	public static JackalModule Instance;

	public static SpriteBank spriteBank;

	public static SpriteBank guiSpriteBank;

	public static FieldInfo player_boostTarget = typeof(Player).GetField("boostTarget", BindingFlags.Instance | BindingFlags.NonPublic);

	public static int cryoBoostState;

	public static int braveBirdState;

	public static int AltbraveBirdState;

	public static int customRedBoostState;

	public float timer;

	private DynData<Glider> gliderData;
	public override Type SessionType => typeof(JackalHelperSession);

	public OuiChapterPanel recent;

	public static JackalHelperSession Session => (JackalHelperSession)Instance._Session;

	public float totalTime;

	public float grappleRespawnTime = 0.4f;


	public JackalModule()
	{
		Instance = this;
	}

	// Check the next section for more information about mod settings, save data and session.
	// Those are optional: if you don't need one of those, you can remove it from the module.

	// If you need to store settings:

	// Set up any hooks, event handlers and your mod in general here.
	// Load runs before Celeste itself has initialized properly.
	public override void Load()
	{
		On.Celeste.PlayerHair.GetHairColor += CryoDashHairColor;
		On.Celeste.Player.Render += CryoBubbleRender;
		//On.Celeste.Spikes.OnCollide += CryoDash;
		//On.Celeste.TriggerSpikes.OnCollide += CryoDash;
		//On.Celeste.Player.DashEnd += CryoDashEnd;
		On.Celeste.FireBarrier.OnPlayer += SafeLava;
		//On.Celeste.Player.ClimbBoundsCheck += PlayerOnClimbBoundsCheck;
		On.Celeste.IceBlock.OnPlayer += SafeIce;
		On.Celeste.SandwichLava.OnPlayer += CryoStand;
		On.Celeste.Glider.Update += directionalJelly;
		On.Celeste.Player.ctor += Player_ctor;
		On.Celeste.Player.CallDashEvents += Player_CallDashEvents;
		On.Celeste.Player.Die += remove_CryoEffects;
		IL.Celeste.OuiChapterPanel.Option.ctor += CryoshockCustomTag;
		On.Celeste.Player.DashBegin += CryoDashBegin;
		On.Celeste.Player.DashEnd += CryoDashEnd;
		On.Celeste.Player.SwimJumpCheck += DeadlyWaterJump;
		On.Celeste.Player.UpdateHair += GaleHairUpdate;
		On.Celeste.Player.Update += PlayerUpdate;
		On.Celeste.Player.ReflectionFallCoroutine += BadelineBoostDownFall;
		On.Celeste.Player.Update += grappleUpdate;
		On.Celeste.Level.Render += bloodRender;
		On.Celeste.CrystalStaticSpinner.Destroy += optimizeDestroy;
		//On.Celeste.HeartGemDisplay.ctor += HeartGemDisplay_ctor;
		//On.Celeste.HeartGemDisplay.Update += HeartGemDisplay_Update;
		On.Celeste.GFX.Load += GFX_Load;
		On.Celeste.DeathsCounter.SetMode += SetDeathsCounterIcon;
		On.Celeste.HeartGemDisplay.SetCurrentMode += HeartGemDisplay_SetCurrentMode;
		//On.Celeste.HeartGemDisplay.ctor += HeartGemDisplay_ctor;
		//On.Celeste.HeartGemDisplay.Render += HeartGemDisplay_Render;
		On.Celeste.Player.OnCollideH += BouncyBoosterReflectH;
		On.Celeste.Player.OnCollideV += BouncyBoosterReflectV;
		//On.Celeste.Booster.OnPlayer += pyroBoosterMelt;
		//IL.Celeste.OuiChapterPanel.Option.Render += CryoshockCustomTag3;

		//Player.add_DashEnd((On.Celeste.Player.hook_DashEnd)(object)new On.Celeste.Player.hook_DashEnd(CryoDashEnd));
		//Player.add_DashBegin((On.Celeste.Player.hook_DashBegin)(object)new On.Celeste.Player.hook_DashBegin(CryoDashBegin));
		//PlayerHair.add_GetHairColor((On.Celeste.PlayerHair.hook_GetHairColor)(object)new On.Celeste.PlayerHair.hook_GetHairColor(CryoDashHairColor));
	}

	private void HeartGemDisplay_Render1(On.Celeste.HeartGemDisplay.orig_Render orig, HeartGemDisplay self)
	{
		throw new NotImplementedException();
	}

	private void optimizeDestroy(On.Celeste.CrystalStaticSpinner.orig_Destroy orig, CrystalStaticSpinner self, bool boss = false)
	{
		if (GetLevel() != null && GetLevel().Tracker.GetEntities<PowBlock>().Count > 0)
		{
			DynData<CrystalStaticSpinner> spinData = new DynData<CrystalStaticSpinner>(self);

			Audio.Play("event:/game/06_reflection/fall_spike_smash", self.Position);
			Color color = Color.White;
			if (spinData.Get<CrystalColor>("color") == CrystalColor.Red)
			{
				color = Calc.HexToColor("ff4f4f");
			}
			else if (spinData.Get<CrystalColor>("color") == CrystalColor.Blue)
			{
				color = Calc.HexToColor("639bff");
			}
			else if (spinData.Get<CrystalColor>("color") == CrystalColor.Purple)
			{
				color = Calc.HexToColor("ff4fef");
			}
			CrystalDebris.Burst(self.Position, color, boss, 2);

			self.RemoveSelf();
		}
		else
		{
			orig.Invoke(self, boss);
		}
	}

	private void PlayerUpdate(On.Celeste.Player.orig_Update orig, Player self)
	{
		totalTime += Engine.DeltaTime;
		Session.color = rainbowCycle();
		orig.Invoke(self);
	}


	public Color rainbowCycle()
	{
		Color color;
		float time = (float)(totalTime % 6);
		int timeInt = (int)time;
		switch (timeInt)
		{
			case 1:
				color = Color.Lerp(Color.Red, Color.Orange, time % 1f);
				break;
			case 2:
				color = Color.Lerp(Color.Orange, Color.Yellow, time % 1f);
				break;
			case 3:
				color = Color.Lerp(Color.Yellow, Color.Green, time % 1f);
				break;
			case 4:
				color = Color.Lerp(Color.Green, Color.Blue, time % 1f);
				break;
			case 5:
				color = Color.Lerp(Color.Blue, Color.Purple, time % 1f);
				break;
			default:
				color = Color.Lerp(Color.Purple, Color.Red, time % 1f);
				break;
		}
		return color * 0.8f;
	}

	// Optional, initialize anything after Celeste has initialized itself properly.

	// Optional, do anything requiring either the Celeste or mod content here.
	public override void LoadContent(bool firstLoad)
	{
		base.LoadContent(firstLoad);
		spriteBank = new SpriteBank(GFX.Game, "Graphics/JackalHelper/Sprites.xml");
		guiSpriteBank = new SpriteBank(GFX.Gui, "Graphics/JackalHelper/GuiSprites.xml");
	}

	public static void GFX_Load(On.Celeste.GFX.orig_Load orig)
	{
		orig.Invoke();
		guiSpriteBank = new SpriteBank(GFX.Gui, "Graphics/JackalHelper/GuiSprites.xml");
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
			return null;
		}
	}





	public Color CryoDashHairColor(On.Celeste.PlayerHair.orig_GetHairColor orig, PlayerHair self, int index)
	{
		if (Session.HasCryoDash)
		{
			return Color.AliceBlue;
		}

		else if (Session.HasGaleDash || Session.GaleDashActive)
		{
			self.SimulateMotion = true;
			return Color.LightGreen;
		}
		else if (Session.HasPowerDash || Session.PowerDashActive)
		{
			return Color.Yellow;
		}
		/*
        if (GetLevel() != null && GetPlayer() != null && self.Scene.Tracker.GetEntities<CryoBooster>().Count > 0)
        {
            foreach (CryoBooster entity in self.Scene.Tracker.GetEntities<CryoBooster>())
            {
                if (entity.FrozenDash)
                {
                    Vector2 scale = new Vector2(Math.Abs(self.Sprite.Scale.X) * (float)self.Facing, self.Sprite.Scale.Y);
                    return Color.AliceBlue;
                }
            }
        }
        */
		return orig.Invoke(self, index);
	}


	public void GaleHairUpdate(On.Celeste.Player.orig_UpdateHair orig, Player self, bool applyGravity)
	{

		orig.Invoke(self, applyGravity);

	}






	private void bloodRender(On.Celeste.Level.orig_Render orig, Level self)
	{

		//ScreenWipe.WipeColor = Calc.HexToColor("440003");

		orig.Invoke(self);
	}

	private void CryoBubbleRender(On.Celeste.Player.orig_Render orig, Player self)
	{
		orig.Invoke(self);
		//New addon here
		if (Session.HasCryoDash)
		{ //idk whether Draw works but i hope it does
			Vector2 scale = new Vector2(Math.Abs(self.Sprite.Scale.X) * (float)self.Facing, self.Sprite.Scale.Y);
			TrailManager.Add(self, scale, Color.SkyBlue);
		}
		else
		{
			foreach (CryoBooster entity in self.Scene.Tracker.GetEntities<CryoBooster>())
			{
				if (entity.FrozenDash)
				{
					Vector2 scale = new Vector2(Math.Abs(self.Sprite.Scale.X) * (float)self.Facing, self.Sprite.Scale.Y);
					TrailManager.Add(self, scale, (Color.SkyBlue * ((entity.freezeTime - entity.timer) / entity.freezeTime)));
				}
			}
		}
		foreach (StopwatchRefill entity in self.Scene.Tracker.GetEntities<StopwatchRefill>())
		{
			if (entity.timed)
			{
				Vector2 scale = new Vector2(Math.Abs(self.Sprite.Scale.X) * (float)self.Facing * 0.8f, self.Sprite.Scale.Y * 0.8f);
				TrailManager.Add(self, scale, Color.Blue * ((entity.recallTime - entity.recallTimer) / entity.recallTimer));
			}
		}



	}



	private PlayerDeadBody remove_CryoEffects(On.Celeste.Player.orig_Die orig, Player player, Vector2 direction, bool evenIfInvincible = false, bool registerDeathInStats = true)
	{
		foreach (CryoBooster b2 in player.Scene.Tracker.GetEntities<CryoBooster>())
		{
			b2.timer = 0f;
		}
		Session.CryoDashActive = false;
		Session.HasCryoDash = false;
		return orig.Invoke(player, direction, evenIfInvincible, registerDeathInStats);
	}
	private void Player_CallDashEvents(On.Celeste.Player.orig_CallDashEvents orig, Player self)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			foreach (CryoBooster b2 in self.Scene.Tracker.GetEntities<CryoBooster>())
			{
				if (b2.StartedBoosting)
				{
					b2.PlayerBoosted(self, self.DashDir);
					return;
				}
				if (b2.BoostingPlayer)
				{
					return;
				}
			}
		}
		catch
		{
		}
		orig.Invoke(self);
	}


	private void grappleUpdate(On.Celeste.Player.orig_Update orig, Player self)
	{
		GrapplingHook hook;
		if (Input.Grab.Pressed && Session.hasGrapple && !self.CollideCheck<Solid>(self.Position + (self.Facing == Facings.Right ? 16f : -16f) * Vector2.UnitX) && grappleRespawnTime < 0f)
		{
			foreach (GrapplingHook currentHook in GetLevel().Tracker.GetEntities<GrapplingHook>())
			{
				currentHook.Die();
			}
			GetLevel().Add(hook = new GrapplingHook(self.Position));
			grappleRespawnTime = 0.4f;
		}
		else
		{
			grappleRespawnTime -= Engine.DeltaTime;
		}
		orig.Invoke(self);
	}
	private void Player_ctor(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode)
	{
		orig.Invoke(self, position, spriteMode);
		cryoBoostState = self.StateMachine.AddState(CryoBooster.CryoBoostUpdate, CryoBooster.CryoBoostCoroutine, CryoBooster.CryoBoostBegin, CryoBooster.CryoBoostEnd);
		braveBirdState = self.StateMachine.AddState(BraveBird.BraveBirdUpdate, BraveBird.BraveBirdCoroutine, BraveBird.BraveBirdBegin, BraveBird.BraveBirdEnd);
		AltbraveBirdState = self.StateMachine.AddState(AltBraveBird.AltBraveBirdUpdate, AltBraveBird.AltBraveBirdCoroutine, AltBraveBird.AltBraveBirdBegin, AltBraveBird.AltBraveBirdEnd);
		customRedBoostState = self.StateMachine.AddState(CustomRedBoost.Update, CustomRedBoost.Coroutine, CustomRedBoost.Begin, CustomRedBoost.End);
		//FlagBoosterState = self.StateMachine.AddState(FlagBooster.FlagDashUpdate, FlagBooster.FlagDashCoroutine, FlagBooster.FlagDashBegin, FlagBooster.FlagDashEnd);
	}


	private void HeartGemDisplay_ctor(On.Celeste.HeartGemDisplay.orig_ctor orig, HeartGemDisplay self, int heartGem, bool hasGem)
	{
		if (self.Entity is OuiChapterPanel panel)
		{
			if (CryoshockCustomTag2(out OuiChapterPanel p))
			{
				if (p != null)
				{
					if (p.Area.LevelSet == "Jackal/Cryoshock")
					{

						//if (p.Area.SID == "Jackal/Cryoshock/Cryoshock-D")
						//{
						Sprite[] sprites = new Sprite[3];
						sprites[0] = sprites[1] = sprites[2] = JackalModule.guiSpriteBank.Create("heartCryo");
						sprites[0].Scale = Vector2.One;
						//sprites[2].Visible = false;
						new DynData<HeartGemDisplay>(self).Get<Sprite[]>("Sprites")[2] = JackalModule.guiSpriteBank.Create("heartCryo");
						new DynData<HeartGemDisplay>(self).Set("Sprites", sprites);

						//}
					}
				}
			}
		}
		orig.Invoke(self, heartGem, hasGem);
	}

	public void HeartGemDisplay_Render(On.Celeste.HeartGemDisplay.orig_Render orig, HeartGemDisplay self)
	{
		orig.Invoke(self);
		/*
        if (self.Entity is OuiChapterPanel panel)
        {
            if (CryoshockCustomTag2(out OuiChapterPanel p))
            {
                if (p != null)
                {
                    if (p.Area.LevelSet == "Jackal/Cryoshock")
                    {

                        if (p.Area.SID == "Jackal/Cryoshock/Cryoshock-D")
                        {
                            Sprite[] sprites = new Sprite[3];
                            sprites[0] = sprites[1] = sprites[2] = JackalModule.guiSpriteBank.Create("heartCryo");
                            sprites[0].Scale = Vector2.One;
                            new DynData<HeartGemDisplay>(self).Get<Sprite[]>("Sprites")[2] = sprites[2];
                            new DynData<HeartGemDisplay>(self).Get<Sprite[]>("Sprites")[1] = sprites[1];
                            new DynData<HeartGemDisplay>(self).Get<Sprite[]>("Sprites")[0] = sprites[0];

                        }
                    }
                }
            }
        }*/

	}

	public void HeartGemDisplay_SetCurrentMode(On.Celeste.HeartGemDisplay.orig_SetCurrentMode orig, HeartGemDisplay self, AreaMode mode, bool has)
	{
		orig.Invoke(self, mode, has);
		if (self.Entity is OuiChapterPanel panel)
		{
			if (CryoshockCustomTag2(out OuiChapterPanel p))
			{
				if (p != null)
				{
					if (p.Area.LevelSet == "Jackal/Cryoshock")
					{

						if (p.Area.SID == "Jackal/Cryoshock/Cryoshock-D")
						{

							Sprite[] sprites = new Sprite[3];
							sprites[0] = sprites[1] = sprites[2] = JackalModule.guiSpriteBank.Create("heartCryo");
							sprites[0].Scale = Vector2.One;
							new DynData<HeartGemDisplay>(self).Set("Sprites", sprites);

						}
					}
				}
			}
		}
	}


	private void SetDeathsCounterIcon(On.Celeste.DeathsCounter.orig_SetMode orig, DeathsCounter self, AreaMode mode)
	{
		orig(self, mode);
		if (self.Entity is OuiChapterPanel panel)
		{
			if (CryoshockCustomTag2(out OuiChapterPanel p))
			{
				if (p != null)
				{
					if (p.Area.LevelSet == "Jackal/Cryoshock")
					{
						switch (p.Area.Mode)
						{
							case AreaMode.Normal:
								new DynData<DeathsCounter>(self).Set("icon", GFX.Gui["collectables/cryoDeaths/A"]);
								break;
							case AreaMode.BSide:
								new DynData<DeathsCounter>(self).Set("icon", GFX.Gui["collectables/cryoDeaths/B"]);
								break;
							default:
								new DynData<DeathsCounter>(self).Set("icon", GFX.Gui["collectables/cryoDeaths/C"]);
								break;
						}
						if (p.Area.GetSID() == "Jackal/Cryoshock/Cryoshock-D")
						{

							new DynData<DeathsCounter>(self).Set("icon", GFX.Gui["collectables/cryoDeaths/D"]);

						}

					}
				}
			}
			else if (recent != null)
			{
				if (p.Area.LevelSet == "Jackal/Cryoshock")
				{
					switch (p.Area.Mode)
					{
						case AreaMode.Normal:
							new DynData<DeathsCounter>(self).Set("icon", GFX.Gui["collectables/cryoDeaths/A"]);
							break;
						case AreaMode.BSide:
							new DynData<DeathsCounter>(self).Set("icon", GFX.Gui["collectables/cryoDeaths/B"]);
							break;
						default:
							new DynData<DeathsCounter>(self).Set("icon", GFX.Gui["collectables/cryoDeaths/C"]);
							break;
					}
					if (p.Area.GetSID() == "Jackal/Cryoshock/Cryoshock-D")
					{

						new DynData<DeathsCounter>(self).Set("icon", GFX.Gui["collectables/cryoDeaths/D"]);

					}

				}
			}
		}


	}


	private void HeartGemDisplay_Update(On.Celeste.HeartGemDisplay.orig_Update orig, HeartGemDisplay self)
	{
		orig.Invoke(self);
	}

	private void CryoshockCustomTag(ILContext il)
	{
		ILCursor cursor = new ILCursor(il);
		/*ILContext is the IL code for the method (oversimplification)
          while ILCursor is a cursor that runs through
          each individual instruction in order. We check the cursor
          for a specific IL function + data in order to figure out when the cursor reaches a specific
          instruction, which we can use as a reference point for our code modification.
          We do this in the next line.*/
		while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdstr("areaselect/tab")))
		{
			//At this point in this simple example, we have reached a specific line of code.
			//We wanna pop the old string and replace it with our new string "newText"

			cursor.EmitDelegate<Func<string, string>>(s =>
			{

				if (CryoshockCustomTag2(out OuiChapterPanel p))
				{

					if (p != null)
					{
						if (p.Area.LevelSet == "Jackal/Cryoshock")
						{

							return "areaselect/Cryoshock/tab";
						}
					}
					else if (recent != null)
					{
						if (recent.Area.LevelSet == "Jackal/Cryoshock")
						{
							return "areaselect/Cryoshock/tab";
						}
					}
				}
				return s;

			});

		}
	}

	private bool CryoshockCustomTag2(out OuiChapterPanel p)
	{
		Overworld i = Engine.Scene as Overworld;
		if (i == null)
		{
			p = recent;
			return true;

		}
		if (i != null)
		{
			bool flag2 = i.Current is OuiChapterPanel;
			if (flag2)
			{
				p = (OuiChapterPanel)i.Current;
				recent = p;
				return true;
			}
			bool flag3 = i.Next is OuiChapterPanel;
			if (flag2)
			{
				p = (OuiChapterPanel)i.Next;
				recent = p;
				return true;
			}
		}
		if (Engine.Scene is Overworld)
		{
			Overworld k = Engine.Scene as Overworld;
			if (k.Current is OuiChapterPanel)
			{
				p = (OuiChapterPanel)k.Current;
				recent = p;
				return true;
			}
			if (k.Next is OuiChapterPanel)
			{
				p = (OuiChapterPanel)k.Next;
				recent = p;
				return true;
			}
		}
		p = null;
		return false;
	}

	private void directionalJelly(On.Celeste.Glider.orig_Update orig, Glider self)
	{
		orig.Invoke(self);
		if (GetLevel() != null)
		{
			foreach (OneWayJellyBarrier entity in GetLevel().Tracker.GetEntities<OneWayJellyBarrier>())
			{
				if (entity.InboundsCheck(entity, self))
				{
					if (self.Speed.Y > 0 && entity.dir == 'U')
					{
						gliderData = new DynData<Glider>(self);
						gliderData.Set<bool>("destroyed", true);
						self.Collidable = false;
						if (self.Hold.IsHeld)
						{
							Vector2 speed2 = self.Hold.Holder.Speed;
							self.Hold.Holder.Drop();
							self.Speed = speed2 * 0.333f;
							Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
						}
						self.Add(new Coroutine(DestroyAnimationRoutine(self)));
						return;
					}
					else if (self.Speed.Y < 0 && entity.dir == 'D')
					{
						gliderData = new DynData<Glider>(self);
						gliderData.Set<bool>("destroyed", true);
						self.Collidable = false;
						if (self.Hold.IsHeld)
						{
							Vector2 speed2 = self.Hold.Holder.Speed;
							self.Hold.Holder.Drop();
							self.Speed = speed2 * 0.333f;
							Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
						}
						self.Add(new Coroutine(DestroyAnimationRoutine(self)));
						return;
					}
					else if (self.Speed.X > 0 && entity.dir == 'L')
					{
						gliderData = new DynData<Glider>(self);
						gliderData.Set<bool>("destroyed", true);
						self.Collidable = false;
						if (self.Hold.IsHeld)
						{
							Vector2 speed2 = self.Hold.Holder.Speed;
							self.Hold.Holder.Drop();
							self.Speed = speed2 * 0.333f;
							Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
						}
						self.Add(new Coroutine(DestroyAnimationRoutine(self)));
						return;
					}
					else if (self.Speed.X < 0 && entity.dir == 'R')
					{
						gliderData = new DynData<Glider>(self);
						gliderData.Set<bool>("destroyed", true);
						self.Collidable = false;
						if (self.Hold.IsHeld)
						{
							Vector2 speed2 = self.Hold.Holder.Speed;
							self.Hold.Holder.Drop();
							self.Speed = speed2 * 0.333f;
							Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
						}
						self.Add(new Coroutine(DestroyAnimationRoutine(self)));
						return;
					}

					if (JackalModule.GetPlayer() != null && !entity.ignoreOnHeld)
					{
						if (JackalModule.GetPlayer().Holding != null)
						{
							if ((JackalModule.GetPlayer().Speed.Y > 0) && entity.dir == 'U')
							{
								gliderData = new DynData<Glider>(self);
								gliderData.Set<bool>("destroyed", true);
								self.Collidable = false;
								if (self.Hold.IsHeld)
								{
									Vector2 speed2 = self.Hold.Holder.Speed;
									self.Hold.Holder.Drop();
									self.Speed = speed2 * 0.333f;
									Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
								}
								self.Add(new Coroutine(DestroyAnimationRoutine(self)));
								return;
							}
							else if ((JackalModule.GetPlayer().Speed.Y < 0) && entity.dir == 'D')
							{
								gliderData = new DynData<Glider>(self);
								gliderData.Set<bool>("destroyed", true);
								self.Collidable = false;
								if (self.Hold.IsHeld)
								{
									Vector2 speed2 = self.Hold.Holder.Speed;
									self.Hold.Holder.Drop();
									self.Speed = speed2 * 0.333f;
									Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
								}
								self.Add(new Coroutine(DestroyAnimationRoutine(self)));
								return;
							}
							else if ((JackalModule.GetPlayer().Speed.X > 0) && entity.dir == 'L')
							{
								gliderData = new DynData<Glider>(self);
								gliderData.Set<bool>("destroyed", true);
								self.Collidable = false;
								if (self.Hold.IsHeld)
								{
									Vector2 speed2 = self.Hold.Holder.Speed;
									self.Hold.Holder.Drop();
									self.Speed = speed2 * 0.333f;
									Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
								}
								self.Add(new Coroutine(DestroyAnimationRoutine(self)));
								return;
							}
							else if ((JackalModule.GetPlayer().Speed.X < 0) && entity.dir == 'R')
							{
								gliderData = new DynData<Glider>(self);
								gliderData.Set<bool>("destroyed", true);
								self.Collidable = false;
								if (self.Hold.IsHeld)
								{
									Vector2 speed2 = self.Hold.Holder.Speed;
									self.Hold.Holder.Drop();
									self.Speed = speed2 * 0.333f;
									Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
								}
								self.Add(new Coroutine(DestroyAnimationRoutine(self)));
								return;
							}
						}
					}
				}
			}
		}

	}

	private IEnumerator DestroyAnimationRoutine(Glider self)
	{

		gliderData = new DynData<Glider>(self);
		gliderData.Set<bool>("destroyed", true);
		gliderData.Get<Sprite>("sprite").Play("death");
		yield return 0.5f;
		Audio.Play("event:/new_content/game/10_farewell/glider_emancipate", self.Position);
		self.RemoveSelf();
	}

	private bool DeadlyWaterJump(On.Celeste.Player.orig_SwimJumpCheck orig, Player player)
	{
		if (JackalModule.GetLevel() != null)
		{
			if (JackalModule.GetLevel().Tracker.GetNearestEntity<Water>(player.Position) is DeadlyWater)
			{
				return false;
			}
		}
		return orig.Invoke(player);
	}

	private void CryoDashBegin(On.Celeste.Player.orig_DashBegin orig, Player self)
	{
		if (Session.HasGaleDash)
		{
			beginWind(Input.Aim.Value, JackalModule.GetLevel());
			Session.GaleDashActive = true;
		}
		Session.PowerDashActive = Session.HasPowerDash;
		Session.CryoDashActive = Session.HasCryoDash;
		orig.Invoke(self);
	}

	public void beginWind(Vector2 pos, Scene scene)
	{
		WindController.Patterns Pattern;
		if (pos.X == 0)
		{
			Pattern = (pos.Y > 0f ? WindController.Patterns.Down : WindController.Patterns.Up);
		}
		else if (pos.Y == 0)
		{
			Pattern = (pos.X > 0f ? WindController.Patterns.Right : WindController.Patterns.Left);
		}
		else
		{
			Pattern = WindController.Patterns.None;
		}
		WindController windController = scene.Entities.FindFirst<WindController>();
		if (windController == null)
		{
			windController = new WindController(Pattern);
			scene.Add(windController);
		}
		else
		{
			windController.SetPattern(WindController.Patterns.None);
			windController.SetPattern(Pattern);
		}
	}



	public Vector2 floorBounce(Vector2 input)
	{
		return new Vector2(input.X, -input.Y);
	}

	public Vector2 wallBounce(Vector2 input)
	{
		return new Vector2(-input.X, input.Y);
	}
	public void BouncyBoosterReflectH(On.Celeste.Player.orig_OnCollideH orig, Player self, CollisionData data)
	{
		if (self.StateMachine.State == 5 && GetLevel().Tracker.GetEntities<BouncyBooster>().Count > 1)
		{
			self.Speed = wallBounce(self.Speed);
		}
		else
		{
			orig.Invoke(self, data);
		}
	}

	public void BouncyBoosterReflectV(On.Celeste.Player.orig_OnCollideV orig, Player self, CollisionData data)
	{
		if (self.StateMachine.State == 5 && GetLevel().Tracker.GetEntities<BouncyBooster>().Count > 1)
		{
			self.Speed = floorBounce(self.Speed);
		}
		else
		{
			orig.Invoke(self, data);
		}
	}


	private void CryoDashEnd(On.Celeste.Player.orig_DashEnd orig, Player self)
	{
		orig.Invoke(self);
		if (self.StateMachine.State != 2 && Session.GaleDashActive)
		{
			Session.GaleDashActive = false;
			Session.HasGaleDash = false;
		}
		if (self.StateMachine.State != 2 && Session.PowerDashActive)
		{
			Session.PowerDashActive = false;
			Session.HasPowerDash = false;
		}
		if (Session.HasCryoDash && self.StateMachine.State != cryoBoostState && !Session.dashQueue)
		{
			dashEnd();
			Session.CryoDashActive = false;
			Session.HasCryoDash = false;
		}
	}

	private IEnumerator dashEnd()
	{
		yield return 0.1f;
	}


	private void SafeLava(On.Celeste.FireBarrier.orig_OnPlayer orig, FireBarrier self, Player player)
	{
		bool run = true;

		if (self is ObsidianBlock)
		{
			if ((self as ObsidianBlock).safe)
			{
				run = false;
			}
		}
		if (run)
		{
			orig.Invoke(self, player);
		}
	}




	private void SafeIce(On.Celeste.IceBlock.orig_OnPlayer orig, IceBlock self, Player player)
	{


		if (self.Collidable && !Session.CryoDashActive && !Session.HasCryoDash)
		{
			orig.Invoke(self, player);
		}
	}




	private void CryoStand(On.Celeste.SandwichLava.orig_OnPlayer orig, SandwichLava self, Player player)
	{
		if (!Session.HasCryoDash && !Session.CryoDashActive)
		{
			orig.Invoke(self, player);
		}
	}

	private static IEnumerator BadelineBoostDownFall(On.Celeste.Player.orig_ReflectionFallCoroutine orig, Player player)
	{
		if (player.SceneAs<Level>().Session.Area.GetLevelSet() == "Celeste")
		{
			IEnumerator enumerator = orig.Invoke(player);
			while (enumerator.MoveNext())
			{
				yield return enumerator.Current;
			}
			yield break;
		}
		player.Sprite.Play("bigFall");
		player.Speed.Y = 0f;
		yield return null;
		FallEffects.Show(visible: true);
		player.Speed.Y = 320f;
		while (!player.CollideCheck<Water>())
		{
			yield return null;
		}
		Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
		FallEffects.Show(visible: false);
		player.Sprite.Play("bigFallRecover");
		yield return 1.2f;
		player.StateMachine.State = 0;
	}




	public override void Initialize()
	{
	}

	// Unload the entirety of your mod's content. Free up any native resources.
	public override void Unload()
	{
		On.Celeste.PlayerHair.GetHairColor -= CryoDashHairColor;
		On.Celeste.Player.Render -= CryoBubbleRender;
		On.Celeste.Player.SwimJumpCheck -= DeadlyWaterJump;
		On.Celeste.Player.UpdateHair -= GaleHairUpdate;
		//On.Celeste.Booster.OnPlayer -= pyroBoosterMelt;
		//On.Celeste.Spikes.OnCollide -= CryoDash;
		//On.Celeste.TriggerSpikes.OnCollide -= CryoDash;
		//On.Celeste.Player.DashEnd -= CryoDashEnd;
		On.Celeste.FireBarrier.OnPlayer -= SafeLava;
		//On.Celeste.Player.ClimbBoundsCheck -= PlayerOnClimbBoundsCheck;
		On.Celeste.IceBlock.OnPlayer -= SafeIce;
		On.Celeste.SandwichLava.OnPlayer -= CryoStand;
		On.Celeste.Player.ctor -= Player_ctor;
		On.Celeste.Player.CallDashEvents -= Player_CallDashEvents;
		On.Celeste.Player.Die -= remove_CryoEffects;
		On.Celeste.Glider.Update -= directionalJelly;
		IL.Celeste.OuiChapterPanel.Option.ctor -= CryoshockCustomTag;
		On.Celeste.Player.DashBegin -= CryoDashBegin;
		On.Celeste.Player.DashEnd -= CryoDashEnd;
		On.Celeste.Player.ReflectionFallCoroutine -= BadelineBoostDownFall;
		On.Celeste.Player.Update -= grappleUpdate;
		On.Celeste.Level.Render -= bloodRender;
		On.Celeste.CrystalStaticSpinner.Destroy -= optimizeDestroy;
		//On.Celeste.HeartGemDisplay.Update -= HeartGemDisplay_Update;
		On.Celeste.GFX.Load -= GFX_Load;
		On.Celeste.DeathsCounter.SetMode -= SetDeathsCounterIcon;
		//On.Celeste.Player.Die -= visible;
		On.Celeste.HeartGemDisplay.SetCurrentMode -= HeartGemDisplay_SetCurrentMode;
		//On.Celeste.HeartGemDisplay.ctor -= HeartGemDisplay_ctor;
		//.Celeste.HeartGemDisplay.Render -= HeartGemDisplay_Render;
		//IL.Celeste.OuiChapterPanel.Option.Render -= CryoshockCustomTag3;
		On.Celeste.Player.OnCollideH -= BouncyBoosterReflectH;
		On.Celeste.Player.OnCollideV -= BouncyBoosterReflectV;
	}


}
