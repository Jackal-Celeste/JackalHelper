// Example usings.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Celeste.Mod.JackalHelper.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace Celeste.Mod.JackalHelper
{
	public class JackalModule : EverestModule
	{
		// Only one alive module instance can exist at any given time.
		public static JackalModule Instance;

		public override Type SessionType => typeof(JackalHelperSession);
		public static JackalHelperSession Session => (JackalHelperSession)Instance._Session;



		// If you need to store settings:
		public override Type SettingsType => typeof(JackalModuleSettings);
		public static JackalModuleSettings Settings => (JackalModuleSettings)Instance._Settings;


		// If you need to store save data:
		public override Type SaveDataType => typeof(JackalModuleSaveData);
		public static JackalModuleSaveData SaveData => (JackalModuleSaveData)Instance._SaveData;

		public static SpriteBank spriteBank;
		public static SpriteBank guiSpriteBank;

		public static int CryoBoostState { get; private set; }

		public static int BraveBirdState { get; private set; }

		public static int AltbraveBirdState { get; private set; }

		public static int CustomRedBoostState { get; private set; }

		public static int HellBoostState { get; private set; }

		private static IDetour mod_OuiFileSelectSlot_orig_Render;

		private OuiChapterPanel recent;

		// COLOURSOFNOISE: consider using Scene.TimeActive for this calculation
		private float totalTime;

		private float grappleRespawnTime = 0.4f;

		public JackalModule()
		{
			Instance = this;
		}

		// do anything requiring either the Celeste or mod content here.
		public override void LoadContent(bool firstLoad)
		{
			base.LoadContent(firstLoad);
			spriteBank = new SpriteBank(GFX.Game, "Graphics/JackalHelper/Sprites.xml");
			guiSpriteBank = new SpriteBank(GFX.Gui, "Graphics/JackalHelper/GuiSprites.xml");
		}

		public static Player GetPlayer()
		{
			return (Engine.Scene as Level)?.Tracker?.GetEntity<Player>();
		}

		public static bool TryGetPlayer(out Player player)
		{
			player = GetPlayer();
			return player != null;
		}

		public static Level GetLevel()
		{
			return (Engine.Scene as Level);
		}

		#region Hooks

		// Check the next section for more information about mod settings, save data and session.
		// Those are optional: if you don't need one of those, you can remove it from the module.

		// If you need to store settings:

		// Set up any hooks, event handlers and your mod in general here.
		// Load runs before Celeste itself has initialized properly.

		public override void Load()
		{
			Logger.SetLogLevel("JackalHelper", LogLevel.Info);

			// PLAYER STATES
			// State Handling
			On.Celeste.Player.ctor += Player_ctor;
			On.Celeste.Player.DashBegin += Player_DashBegin;
			On.Celeste.Player.DashEnd += Player_DashEnd;
			On.Celeste.Player.Die += remove_CryoEffects;
			// Other Effects
			On.Celeste.IceBlock.OnPlayer += SafeIce;
			On.Celeste.SandwichLava.OnPlayer += CryoStand;
			On.Celeste.Player.SuperJump += cryoBubbleHyper;
			// Rendering
			On.Celeste.PlayerHair.GetHairColor += CryoDashHairColor;

			// Player TrailManager Rendering
			On.Celeste.Player.Render += Player_Render;
			// CryoBooster
			On.Celeste.Player.CallDashEvents += Player_CallDashEvents;

			// POW Block
			On.Celeste.CrystalStaticSpinner.Destroy += optimizeDestroy;
			// Rainbow Color Cycle
			On.Celeste.Player.Update += PlayerUpdate;

			// CRYOSHOCK
			On.Celeste.HeartGemDisplay.SetCurrentMode += HeartGemDisplay_SetCurrentMode;
			On.Celeste.DeathsCounter.SetMode += SetDeathsCounterIcon;
			IL.Celeste.OuiChapterPanel.Option.ctor += CryoshockCustomTag;

			On.Celeste.Glider.Update += directionalJelly;

			On.Celeste.Player.SwimJumpCheck += DeadlyWaterJump;

			On.Celeste.Player.OnCollideH += BouncyBoosterReflectH;
			On.Celeste.Player.OnCollideV += BouncyBoosterReflectV;

			On.Celeste.FireBarrier.OnPlayer += SafeLava;

			On.Celeste.Player.Update += grappleUpdate;
			IL.Celeste.OuiJournalProgress.ctor += ModJournalProgressPageConstructCryo;
			mod_OuiFileSelectSlot_orig_Render = new ILHook(
				typeof(OuiFileSelectSlot).GetMethod("orig_Render", BindingFlags.Public | BindingFlags.Instance),
				ModFileSelectSlotRenderCryo
			);

			On.Celeste.Player.ClimbJump += Player_ClimbJump;
			On.Celeste.Player.ClimbUpdate += Player_ClimbUpdate;
			On.Celeste.Level.ctor += Level_ctor;
		}

		#region PlayerStates

		private void Player_ctor(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode)
		{
			orig.Invoke(self, position, spriteMode);
			CryoBoostState = self.StateMachine.AddState(self.CryoBoostUpdate, self.CryoBoostCoroutine, self.CryoBoostBegin, self.CryoBoostEnd);
			BraveBirdState = self.StateMachine.AddState(BraveBird.BraveBirdUpdate, BraveBird.BraveBirdCoroutine, BraveBird.BraveBirdBegin, BraveBird.BraveBirdEnd);
			AltbraveBirdState = self.StateMachine.AddState(AltBraveBird.AltBraveBirdUpdate, AltBraveBird.AltBraveBirdCoroutine, AltBraveBird.AltBraveBirdBegin, AltBraveBird.AltBraveBirdEnd);
			CustomRedBoostState = self.StateMachine.AddState(CustomRedBoost.Update, CustomRedBoost.Coroutine, CustomRedBoost.Begin, CustomRedBoost.End);
			//FlagBoosterState = self.StateMachine.AddState(FlagBooster.FlagDashUpdate, FlagBooster.FlagDashCoroutine, FlagBooster.FlagDashBegin, FlagBooster.FlagDashEnd);
		}

		private void Player_DashBegin(On.Celeste.Player.orig_DashBegin orig, Player self)
		{
			Session.PowerDashActive = Session.HasPowerDash;
			Session.CryoDashActive = Session.HasCryoDash;
			orig.Invoke(self);
		}

		private void Level_ctor(On.Celeste.Level.orig_ctor orig, Level self)
		{
			if (JackalModule.Settings.ResetOnChapterLoad && JackalModule.SaveData.insightCrystals.Count > 0)
			{
				JackalModule.SaveData.insightCrystals.Clear();
			}
			orig.Invoke(self);
		}


		private void Player_ClimbJump(On.Celeste.Player.orig_ClimbJump orig, Player self)
		{
			if (!ClimbBlocker.Check(self.Scene, self, self.Position + Vector2.UnitX * (float)self.Facing))
			{
				if (Session.inStaminaZone)
				{
					self.RefillStamina();
				}
			}
			orig.Invoke(self);
		}



		private int Player_ClimbUpdate(On.Celeste.Player.orig_ClimbUpdate orig, Player self)
		{

			if (ClimbBlocker.Check(self.Scene, self, self.Position + Vector2.UnitX * (float)self.Facing))
			{
				return orig.Invoke(self);
			}
			if (Session.inStaminaZone) 
			{ 
				self.RefillStamina();	
			}
			return orig.Invoke(self);
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

		private void Player_DashEnd(On.Celeste.Player.orig_DashEnd orig, Player self)
		{
			orig.Invoke(self);
			if (self.StateMachine.State != Player.StDash && Session.PowerDashActive)
			{
				Session.PowerDashActive = false;
				Session.HasPowerDash = false;
			}
			if (Session.HasCryoDash && self.StateMachine.State != CryoBoostState && !Session.dashQueue)
			{
				Session.CryoDashActive = false;
				Session.HasCryoDash = false;
			}
		}


		private void cryoBubbleHyper(On.Celeste.Player.orig_SuperJump orig, Player self)
		{
			bool ducking = self.Ducking;
			foreach (CryoBooster entity in self.Scene.Tracker.GetEntities<CryoBooster>())
			{
				if (!self.OnGround() && Vector2.Distance(entity.Position + 8* Vector2.One, self.Center) < 12f)
				{
					self.Ducking = true;
				}
			}
			orig.Invoke(self);
			//self.Ducking = ducking;
		}

		private PlayerDeadBody remove_CryoEffects(On.Celeste.Player.orig_Die orig, Player player, Vector2 direction, bool evenIfInvincible = false, bool registerDeathInStats = true)
		{
			foreach (CryoBooster b2 in player.Scene.Tracker.GetEntities<CryoBooster>())
			{
				b2.FreezeTimer = 0f;
			}
			Session.CryoDashActive = false;
			Session.HasCryoDash = false;
			return orig.Invoke(player, direction, evenIfInvincible, registerDeathInStats);
		}

		private void SafeIce(On.Celeste.IceBlock.orig_OnPlayer orig, IceBlock self, Player player)
		{
			// COLOURSOFNOISE: Why check if self is Collidable?
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

		public Color CryoDashHairColor(On.Celeste.PlayerHair.orig_GetHairColor orig, PlayerHair self, int index)
		{
			if (Session.HasCryoDash)
			{
				return Color.AliceBlue;
			}
			else if (Session.HasPowerDash || Session.PowerDashActive)
			{
				return Color.Yellow;
			}
			return orig.Invoke(self, index);
		}



		#endregion

	


		private void optimizeDestroy(On.Celeste.CrystalStaticSpinner.orig_Destroy orig, CrystalStaticSpinner self, bool boss = false)
		{
			if (self.Scene.Tracker.GetEntities<PowBlock>().Count > 0)
			{
				DynData<CrystalStaticSpinner> spinData = new DynData<CrystalStaticSpinner>(self);

				Audio.Play("event:/game/06_reflection/fall_spike_smash", self.Position);
				Color color = spinData.Get<CrystalColor>("color") switch
				{
					CrystalColor.Blue => Calc.HexToColor("639bff"),
					CrystalColor.Red => Calc.HexToColor("ff4f4f"),
					CrystalColor.Purple => Calc.HexToColor("ff4fef"),
					_ => Color.White,
				};
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
			float time = (float)(totalTime % 6);
			int timeInt = (int)time;
			var color = timeInt switch
			{
				1 => Color.Lerp(Color.Red, Color.Orange, time % 1f),
				2 => Color.Lerp(Color.Orange, Color.Yellow, time % 1f),
				3 => Color.Lerp(Color.Yellow, Color.Green, time % 1f),
				4 => Color.Lerp(Color.Green, Color.Blue, time % 1f),
				5 => Color.Lerp(Color.Blue, Color.Purple, time % 1f),
				_ => Color.Lerp(Color.Purple, Color.Red, time % 1f),
			};
			return color * 0.8f;
		}

		private void Player_Render(On.Celeste.Player.orig_Render orig, Player self)
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
						TrailManager.Add(self, scale, (Color.SkyBlue * ((entity.FreezeTime - entity.FreezeTimer) / entity.FreezeTime)));
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

		private void Player_CallDashEvents(On.Celeste.Player.orig_CallDashEvents orig, Player self)
		{
			// COLOURSOFNOISE: using a try/catch block without specifying the exception to catch and providing additional information/logging is generally a bad practice
			foreach (CryoBooster booster in self.Scene.Tracker.GetEntities<CryoBooster>())
			{
				if (booster.StartedBoosting)
				{
					booster.PlayerBoosted(self, self.DashDir);
					return;
				}
				if (booster.BoostingPlayer)
				{
					return;
				}
			}
			orig.Invoke(self);
		}

		private void grappleUpdate(On.Celeste.Player.orig_Update orig, Player self)
		{
			foreach (Follower follower in self.Leader.Followers)
        	{
            	if (follower.Entity is Strawberry && (follower.Entity as Strawberry).Golden)
            	{
                	GetLevel().Session.SetFlag("cryoGoldenRun", true); 
            	}
        	}
			GrapplingHook hook;
			if (Input.Grab.Pressed && (Session.hasGrapple || Session.grappleStored) && !self.CollideCheck<Solid>(self.Position + (self.Facing == Facings.Right ? 16f : -16f) * Vector2.UnitX) && grappleRespawnTime < 0f)
			{
				foreach (GrapplingHook currentHook in GetLevel().Tracker.GetEntities<GrapplingHook>())
				{
					currentHook.Die();
				}
				GetLevel().Add(hook = new GrapplingHook(self.Position));
				grappleRespawnTime = 0.4f;
				if (Session.grappleStored)
				{
					Session.grappleStored = false;
				}
			}
			else
			{
				grappleRespawnTime -= Engine.DeltaTime;
			}
			orig.Invoke(self);
		}

		#region CryoShock


		private static int curSlot = -1;
		private void ModFileSelectSlotRenderCryo(ILContext il)
		{
			ILCursor cursor = new ILCursor(il);
			if (cursor.TryGotoNext(MoveType.After,
								instr => instr.MatchLdstr("cassette"),
								instr => instr.MatchCallvirt<Atlas>("get_Item")))
			{
				Logger.Log(LogLevel.Info, "AltSidesHelper", $"Modding file select slot at {cursor.Index} in IL for OuiFileSelectSlot.orig_Render, for custom cassettes (1/2).");
				cursor.Emit(OpCodes.Ldarg_0);
				cursor.Emit(OpCodes.Ldfld, typeof(OuiFileSelectSlot).GetField("SaveData"));
				cursor.Emit(OpCodes.Ldloc_S, il.Method.Body.Variables[11]);
				// literally just tell it to render nothing
				cursor.EmitDelegate<Func<MTexture, SaveData, int, MTexture>>((orig, save, index) => {
					AreaData data = FromIndexInSave(save, index);
					return orig;
				});
				if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<MTexture>("DrawCentered")))
				{
					Logger.Log(LogLevel.Info, "AltSidesHelper", $"Modding file select slot at {cursor.Index} in IL for OuiFileSelectSlot.orig_Render, for custom cassettes (2/2).");
					cursor.Emit(OpCodes.Ldarg_0);
					cursor.Emit(OpCodes.Ldfld, typeof(OuiFileSelectSlot).GetField("SaveData"));
					cursor.Emit(OpCodes.Ldloc_S, il.Method.Body.Variables[11]); // index
					cursor.Emit(OpCodes.Ldloc_S, il.Method.Body.Variables[8]); // vector
					cursor.EmitDelegate<Action<SaveData, int, Vector2>>((save, index, vector) => {
						AreaData data = FromIndexInSave(save, index);
					});
				}
			}
			if (cursor.TryGotoNext(MoveType.After,
								instr => instr.Match(OpCodes.Box),
								instr => instr.MatchCall<string>("Concat"),
								instr => instr.MatchCallvirt<Atlas>("get_Item")))
			{
				Logger.Log(LogLevel.Info, "AltSidesHelper", $"Modding file select slot at {cursor.Index} in IL for OuiFileSelectSlot.orig_Render, for custom hearts.");
				cursor.Emit(OpCodes.Ldarg_0);
				cursor.Emit(OpCodes.Ldfld, typeof(OuiFileSelectSlot).GetField("SaveData"));
				cursor.Emit(OpCodes.Ldloc_S, il.Method.Body.Variables[11]);
				cursor.EmitDelegate<Func<MTexture, SaveData, int, MTexture>>((orig, save, index) => {
					AreaData data = FromIndexInSave(save, index);
					if (data != null)
					{
						if (data.SID == "Jackal/Cryoshock/Cryoshock-D")
						{
							Logger.Log("AltSidesHelper", $"Changing file select heart texture for \"{data.SID}\".");
							// use *our* gem
							return MTN.Journal["Jackal/Cryoshock/Cryoshock-D"];
						}

					}
					return orig;
				});
			}
		}

		private void ModJournalProgressPageConstructCryo(ILContext il)
		{

			ILCursor cursor = new ILCursor(il);
			if (cursor.TryGotoNext(MoveType.After,
								instr => instr.Match(OpCodes.Box),
								instr => instr.MatchCall<string>("Concat")))
			{
				// now do that again :P
				if (cursor.TryGotoNext(MoveType.After,
								instr => instr.Match(OpCodes.Box),
								instr => instr.MatchCall<string>("Concat")))
				{
					cursor.Emit(OpCodes.Ldloc_2); // data
					cursor.EmitDelegate<Func<string, AreaData, string>>((orig, data) =>
					{

						if (data.SID == "Jackal/Cryoshock/Cryoshock-D")
						{
							return "Jackal/Cryoshock/Cryoshock-D";
						}
						// use *our* gem
						return orig;
					});
				}
			}
		}

		public void HeartGemDisplay_SetCurrentMode(On.Celeste.HeartGemDisplay.orig_SetCurrentMode orig, HeartGemDisplay self, AreaMode mode, bool has)
		{
			orig.Invoke(self, mode, has);
			if (self.Entity is OuiChapterPanel panel)
			{
				if (TryGetChapterPanel(out OuiChapterPanel p))
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
				if (TryGetChapterPanel(out OuiChapterPanel p))
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

					if (TryGetChapterPanel(out OuiChapterPanel p))
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
					return s;

				});

			}
		}

		private bool TryGetChapterPanel(out OuiChapterPanel panel)
		{
			if (Engine.Scene is Overworld o)
			{
				if (o.Current is OuiChapterPanel current)
				{
					recent = panel = current;
					return true;
				}
				if (o.Next is OuiChapterPanel next)
				{
					recent = panel = next;
					return true;
				}
			}
			else if (recent != null)
			{
				panel = recent;
				return true;
			}
			panel = null;
			return false;
		}

		#endregion

		private void directionalJelly(On.Celeste.Glider.orig_Update orig, Glider self)
		{
			orig.Invoke(self);
			foreach (OneWayJellyBarrier barrier in self.Scene.Tracker.GetEntities<OneWayJellyBarrier>())
			{
				if (barrier.InboundsCheck(self))
				{
					bool destroy = false;
					if (barrier.IsAgainst(self.Speed))
					{
						destroy = true;
					}

					Player player = self.Scene.Tracker.GetEntity<Player>();
					if (player?.Holding != null && !barrier.ignoreOnHeld && barrier.IsAgainst(player.Speed))
					{
						destroy = true;
					}

					if (destroy)
					{
						DynData<Glider> gliderData = new DynData<Glider>(self);
						gliderData.Set<bool>("destroyed", true);
						self.Collidable = false;
						if (self.Hold.IsHeld)
						{
							Vector2 speed = self.Hold.Holder.Speed;
							self.Hold.Holder.Drop();
							self.Speed = speed * 0.333f;
							Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
						}
						self.Add(new Coroutine(DestroyAnimationRoutine(self)));
						return;
					}
				}
			}
		}

		private IEnumerator DestroyAnimationRoutine(Glider self)
		{
			DynData<Glider> gliderData = new DynData<Glider>(self);
			gliderData.Set<bool>("destroyed", true);
			gliderData.Get<Sprite>("sprite").Play("death");
			yield return 0.5f;
			Audio.Play("event:/new_content/game/10_farewell/glider_emancipate", self.Position);
			self.RemoveSelf();
		}

		private bool DeadlyWaterJump(On.Celeste.Player.orig_SwimJumpCheck orig, Player player)
		{
			if (player.Scene.Tracker.GetNearestEntity<Water>(player.Position) is DeadlyWater)
				return false;
			return orig.Invoke(player);
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
			if (self.StateMachine.State == Player.StRedDash && GetLevel().Tracker.GetEntities<BouncyBooster>().Count > 1)
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
			if (self.StateMachine.State == Player.StRedDash && GetLevel().Tracker.GetEntities<BouncyBooster>().Count > 1)
			{
				self.Speed = floorBounce(self.Speed);
			}
			else
			{
				orig.Invoke(self, data);
			}
		}

		private void SafeLava(On.Celeste.FireBarrier.orig_OnPlayer orig, FireBarrier self, Player player)
		{
			if (self is ObsidianBlock block && block.safe)
				return;

			orig.Invoke(self, player);
		}


		// Unload the entirety of your mod's content. Free up any native resources.
		public override void Unload()
		{
			On.Celeste.Player.ctor -= Player_ctor;
			On.Celeste.Player.DashBegin -= Player_DashBegin;
			On.Celeste.Player.DashEnd -= Player_DashEnd;
			On.Celeste.Player.Die -= remove_CryoEffects;

			On.Celeste.IceBlock.OnPlayer -= SafeIce;
			On.Celeste.SandwichLava.OnPlayer -= CryoStand;
			On.Celeste.Player.SuperJump -= cryoBubbleHyper;

			On.Celeste.PlayerHair.GetHairColor -= CryoDashHairColor;

			On.Celeste.Player.Render -= Player_Render;
			On.Celeste.Player.CallDashEvents -= Player_CallDashEvents;

			On.Celeste.CrystalStaticSpinner.Destroy -= optimizeDestroy;
			On.Celeste.Player.Update -= PlayerUpdate;

			On.Celeste.HeartGemDisplay.SetCurrentMode -= HeartGemDisplay_SetCurrentMode;
			On.Celeste.DeathsCounter.SetMode -= SetDeathsCounterIcon;
			IL.Celeste.OuiChapterPanel.Option.ctor -= CryoshockCustomTag;

			On.Celeste.Glider.Update -= directionalJelly;

			On.Celeste.Player.SwimJumpCheck -= DeadlyWaterJump;

			On.Celeste.Player.OnCollideH -= BouncyBoosterReflectH;
			On.Celeste.Player.OnCollideV -= BouncyBoosterReflectV;

			On.Celeste.FireBarrier.OnPlayer -= SafeLava;

			On.Celeste.Player.Update -= grappleUpdate;


			IL.Celeste.OuiJournalProgress.ctor -= ModJournalProgressPageConstructCryo;
			mod_OuiFileSelectSlot_orig_Render.Dispose();

			On.Celeste.Player.ClimbJump -= Player_ClimbJump;
			On.Celeste.Player.ClimbUpdate -= Player_ClimbUpdate;
			On.Celeste.Level.ctor -= Level_ctor;
		}

		#endregion

		


		



		private static AreaData FromIndexInSave(SaveData save, int index)
		{
			var levelset = save.LevelSet;
			AreaData data = null; int i = 0;
			foreach (var item in AreaData.Areas)
			{
				if (item.GetLevelSet().Equals(levelset))
				{
					if (i == index)
					{
						data = item;
						break;
					}
					i++;
				}
			}
			return data;
		}


	}
}
