using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;


namespace Celeste.Mod.JackalHelper.Entities
{
	[CustomEntity("JackalHelper/CryoBooster")]
	[Tracked]
	public class CryoBooster : Entity
	{
		public string reappearSfx;

		public string enterSfx;

		public string boostSfx;

		public string endSfx;

		public float BoostTime;

		public bool StartedBoosting;

		public Color ParticleColor;

		private float RespawnTime;

		public static readonly Vector2 playerOffset;

		public Sprite sprite;

		private Entity outline;

		private Wiggler wiggler;

		private BloomPoint bloom;

		private VertexLight light;

		private Coroutine dashRoutine;

		private DashListener dashListener;

		private ParticleType particleType;

		private float respawnTimer;

		private float cannotUseTimer;

		private SoundSource loopingSfx;

		public static Vector2 staticPosition;

		public static Level staticScene;

		public bool FrozenDash = false;

		public float freezeTime = 0.25f;

		public float timer = 0f;

		public static int startingDashes = 1;

		public bool crystal = false;


		public static bool hasCryoDash;

		public bool storedCryoDash = false;

		public bool BoostingPlayer
		{
			get;
			private set;
		}


		private class BreakDebris : Entity
		{
			private Image sprite;

			private Vector2 speed;

			private float percent;

			private float duration;

			public BreakDebris Init(Vector2 position, Vector2 direction)
			{
				//IL_0067: Unknown result type (might be due to invalid IL or missing references)
				//IL_0068: Unknown result type (might be due to invalid IL or missing references)
				//IL_006d: Unknown result type (might be due to invalid IL or missing references)
				//IL_008d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0092: Unknown result type (might be due to invalid IL or missing references)
				//IL_0095: Unknown result type (might be due to invalid IL or missing references)
				//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
				Depth = -1000;
				List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures("objects/boosterIce/chunk");
				MTexture texture = Calc.Random.Choose(atlasSubtextures);
				if (sprite == null)
				{
					Add(sprite = new Image(texture));
					sprite.CenterOrigin();
				}
				else
				{
					sprite.Texture = texture;
				}
				Position = JackalModule.GetPlayer().Position;
				direction = Calc.AngleToVector(direction.Angle() + Calc.Random.Range(-0.1f, 0.1f), 1f);
				direction.X += Calc.Random.Range(-0.3f, 0.3f);
				direction.Normalize();
				speed = direction * Calc.Random.Range(140, 180);
				percent = 0f;
				duration = Calc.Random.Range(2, 3);
				return this;
			}

			public override void Update()
			{
				//IL_002d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Unknown result type (might be due to invalid IL or missing references)
				//IL_003d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0042: Unknown result type (might be due to invalid IL or missing references)
				//IL_0047: Unknown result type (might be due to invalid IL or missing references)
				//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
				base.Update();
				if (percent >= 1f)
				{
					RemoveSelf();
					return;
				}
				Position += speed * Engine.DeltaTime;
				speed.X = Calc.Approach(speed.X, 0f, 180f * Engine.DeltaTime);
				speed.Y += 200f * Engine.DeltaTime;
				percent += Engine.DeltaTime / duration;
				sprite.Color = Color.White * (1f - percent);
			}

			public override void Render()
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				sprite.DrawOutline(Color.Black);
				base.Render();
			}
		}

		public static ParticleType P_Burst => Booster.P_Burst;

		public static ParticleType P_BurstRed => Booster.P_BurstRed;

		public static ParticleType P_Appear => Booster.P_Appear;

		public static ParticleType P_RedAppear => Booster.P_RedAppear;

		public CryoBooster(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			FrozenDash = false;
			staticPosition = Position;
			base.Depth = -8500;
			base.Collider = new Circle(10f, 0f, 2f);
			sprite = new Sprite(GFX.Game, "objects/boosterIce/");
			sprite.Visible = true;
			sprite.CenterOrigin();
			sprite.Justify = new Vector2(0.5f, 0.5f);
			sprite.AddLoop("loop", "boosterIce", 0.1f, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9);
			sprite.AddLoop("inside", "boosterIce", 0.1f, 10, 11, 12, 13);
			sprite.AddLoop("spin", "boosterIce", 0.06f, 23, 24, 25, 26, 27, 28, 29, 30);
			sprite.Add("pop", "boosterIce", 0.08f, 14, 15, 16, 17, 18, 19, 20, 21, 22);
			sprite.Play("loop", false, true);
			Add(sprite);
			Add(new PlayerCollider(OnPlayer));
			Add(light = new VertexLight(Color.White, 1f, 16, 32));
			Add(bloom = new BloomPoint(0.1f, 16f));
			Add(wiggler = Wiggler.Create(0.5f, 4f, delegate (float f)
			{
				sprite.Scale = Vector2.One * (1f + f * 0.25f);
			}));
			Add(dashRoutine = new Coroutine(removeOnComplete: false));
			Add(dashListener = new DashListener());
			Add(new MirrorReflection());
			Add(loopingSfx = new SoundSource());
			dashListener.OnDash = OnPlayerDashed;
			particleType = Booster.P_Burst;
			RespawnTime = 1f;
			BoostTime = .25f;
			ParticleColor = Color.LightBlue;
			reappearSfx = "event:/game/04_cliffside/greenbooster_reappear";
			enterSfx = "event:/game/04_cliffside/greenbooster_enter";
			boostSfx = "event:/game/04_cliffside/greenbooster_dash";
			endSfx = "event:/game/04_cliffside/greenbooster_end";
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			Image image = new Image(GFX.Game["objects/booster/outline"]);
			image.CenterOrigin();
			image.Color = Color.White * 0.75f;
			outline = new Entity(Position);
			outline.Depth = 8999;
			outline.Visible = false;
			outline.Add(image);
			outline.Add(new MirrorReflection());
			scene.Add(outline);
		}

		public void Appear()
		{
			Audio.Play(reappearSfx, Position);
			sprite.Play("appear");
			wiggler.Start();
			Visible = true;
			AppearParticles();
		}

		private void AppearParticles()
		{
			ParticleSystem particlesBG = SceneAs<Level>().ParticlesBG;
			for (int i = 0; i < 360; i += 30)
			{
				particlesBG.Emit(Booster.P_Appear, 1, base.Center, Vector2.One * 2f, ParticleColor, i * ((float)Math.PI / 180f));
			}
		}

		private void OnPlayer(Player player)
		{
			hasCryoDash = JackalModule.Session.HasCryoDash;
			if (respawnTimer <= 0f && cannotUseTimer <= 0f && !BoostingPlayer)
			{
				cannotUseTimer = 0.45f;
				Boost(player, this);
				Audio.Play(enterSfx, Position);
				wiggler.Start();
				sprite.Play("inside");
				sprite.FlipX = player.Facing == Facings.Left;
			}
		}

		public static void Boost(Player player, CryoBooster booster)
		{
			player.StateMachine.State = JackalModule.cryoBoostState;
			player.Speed = Vector2.Zero;
			JackalModule.player_boostTarget.SetValue(player, booster.Center);
			booster.StartedBoosting = true;
		}

		public void PlayerBoosted(Player player, Vector2 direction)
		{
			StartedBoosting = false;
			Audio.Play(boostSfx, Position);
			BoostingPlayer = true;
			base.Tag = Tags.Persistent | Tags.TransitionUpdate;
			sprite.Play("spin");
			sprite.FlipX = player.Facing == Facings.Left;
			outline.Visible = true;
			wiggler.Start();
			dashRoutine.Replace(BoostRoutine(player, direction));
		}

		private IEnumerator BoostRoutine(Player player, Vector2 dir)
		{
			float angle = (-dir).Angle();
			while ((player.StateMachine.State == 2 || player.StateMachine.State == 5) && BoostingPlayer)
			{
				if (player.Dead)
				{
					PlayerDied();
					continue;
				}
				sprite.RenderPosition = player.Center + Booster.playerOffset;
				loopingSfx.Position = sprite.Position;
				if (base.Scene.OnInterval(0.02f))
				{
					(base.Scene as Level).ParticlesBG.Emit(particleType, 2, player.Center - dir * 3f + new Vector2(0f, -2f), new Vector2(3f, 3f), ParticleColor, angle);
				}
				yield return null;
			}
			PlayerReleased();
			if (player.StateMachine.State == 4)
			{
				//sprite.Visible = false;
			}
			while (SceneAs<Level>().Transitioning)
			{
				yield return null;
			}
			base.Tag = 0;
		}

		public void OnPlayerDashed(Vector2 direction)
		{
			if (BoostingPlayer)
			{
				BoostingPlayer = false;
			}
		}

		public void PlayerReleased()
		{
			Audio.Play(endSfx, sprite.RenderPosition);
			sprite.Play("pop");
			cannotUseTimer = 0f;
			respawnTimer = RespawnTime;
			BoostingPlayer = false;
			wiggler.Stop();
			loopingSfx.Stop();
			if (JackalModule.GetPlayer() != null)
			{
				for (int i = 0; (float)i < 24; i += 8)
				{
					for (int j = 0; (float)j < 24; j += 8)
					{
						base.Scene.Add(Engine.Pooler.Create<BreakDebris>().Init(new Vector2(base.X + i + 4f, base.Y + j + 4f), -(Position - JackalModule.GetPlayer().Position)));
					}
				}
			}
			timer += Engine.DeltaTime;
			JackalModule.Session.HasCryoDash = hasCryoDash;
		}

		public void PlayerDied()
		{
			if (BoostingPlayer)
			{
				PlayerReleased();
				dashRoutine.Active = false;
				base.Tag = 0;
			}
		}

		public void Respawn()
		{
			Audio.Play(reappearSfx, Position);
			sprite.Position = Vector2.Zero;
			sprite.Play("loop", restart: true);
			wiggler.Start();
			sprite.Visible = true;
			outline.Visible = false;
			AppearParticles();
		}

		public override void Update()
		{
			base.Update();
			staticScene = JackalModule.GetLevel();
			if (!FrozenDash)
			{
				crystal = false;
			}
			if (timer <= 0f || timer > freezeTime)
			{
				timer = 0f;
				FrozenDash = false;
			}
			else
			{
				timer += Engine.DeltaTime;
				FrozenDash = true;
				if (JackalModule.GetPlayer() != null)
				{
					if (JackalModule.GetPlayer().Dashes > startingDashes && FrozenDash && startingDashes == 0)
					{
						JackalModule.GetPlayer().Dashes = startingDashes;
					}
				}

			}
			if (JackalModule.GetPlayer() != null && JackalModule.GetLevel() != null)
			{
				if (JackalModule.GetPlayer().StateMachine.State == JackalModule.cryoBoostState)
				{
					FrozenDash = true;
				}
			}
			if (JackalModule.GetLevel() != null)
			{
				JackalModule.GetLevel().Session.Inventory.NoRefills = FrozenDash;
			}
			if (cannotUseTimer > 0f)
			{
				cannotUseTimer -= Engine.DeltaTime;
			}
			if (respawnTimer > 0f)
			{
				respawnTimer -= Engine.DeltaTime;
				if (respawnTimer <= 0f)
				{
					Respawn();
				}
			}
			if (!dashRoutine.Active && respawnTimer <= 0f)
			{
				Vector2 target = Vector2.Zero;
				Player entity = base.Scene.Tracker.GetEntity<Player>();
				if (entity != null && CollideCheck(entity))
				{
					target = entity.Center + Booster.playerOffset - Position;
				}
				sprite.Position = Calc.Approach(sprite.Position, target, 80f * Engine.DeltaTime);
			}
			if (sprite.CurrentAnimationID == "inside" && !BoostingPlayer && !CollideCheck<Player>())
			{
				sprite.Play("loop", false, true);
			}
		}

		public override void Render()
		{
			Vector2 position = sprite.Position;
			sprite.Position = position.Floor();
			if (sprite.CurrentAnimationID != "pop" && sprite.Visible)
			{
				sprite.DrawOutline();
			}
			base.Render();
			sprite.Position = position;
		}


		public static void CryoBoostBegin()
		{
			Player player = JackalModule.GetPlayer();
			hasCryoDash = JackalModule.Session.HasCryoDash;
			bool? flag = player.SceneAs<Level>()?.Session.MapData.GetMeta()?.TheoInBubble;
			bool? flag2 = flag;
			player.RefillStamina();
			if (!flag2.GetValueOrDefault())
			{
				player.Drop();
			}
			startingDashes = player.Dashes;
			staticScene.Session.Inventory.NoRefills = true;
		}

		public static int CryoBoostUpdate()
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			Player player = JackalModule.GetPlayer();
			Vector2 boostTarget = (Vector2)JackalModule.player_boostTarget.GetValue(player);
			Vector2 value = Input.Aim.Value * 3f;
			Vector2 vector = Calc.Approach(player.ExactPosition, boostTarget - player.Collider.Center + value, 80f * Engine.DeltaTime);
			player.MoveToX(vector.X);
			player.MoveToY(vector.Y);
			staticScene.Session.Inventory.NoRefills = true;
			if (Input.Dash.Pressed)
			{
				Input.Dash.ConsumePress();
				return 2;
			}
			JackalModule.Session.HasCryoDash = hasCryoDash;
			return JackalModule.cryoBoostState;
		}

		public static void CryoBoostEnd()
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			Player player = JackalModule.GetPlayer();
			Vector2 boostTarget = (Vector2)JackalModule.player_boostTarget.GetValue(player);
			Vector2 vector = (boostTarget - player.Collider.Center).Floor();
			player.MoveToX(vector.X);
			player.MoveToY(vector.Y);
			JackalModule.Session.HasCryoDash = hasCryoDash;
		}

		public static IEnumerator CryoBoostCoroutine()
		{
			Player player = JackalModule.GetPlayer();
			CryoBooster booster = null;
			foreach (CryoBooster b in player.Scene.Tracker.GetEntities<CryoBooster>())
			{
				if (b.StartedBoosting)
				{
					booster = b;
					break;
				}
			}
			yield return booster.BoostTime;
			player.StateMachine.State = 2;
			JackalModule.Session.HasCryoDash = hasCryoDash;

		}

		static CryoBooster()
		{
			playerOffset = new Vector2(0f, -2f);
		}
	}
}