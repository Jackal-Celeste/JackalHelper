using System;
using System.Collections;
using System.Reflection;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;


namespace Celeste.Mod.JackalHelper.Entities
{
	[CustomEntity("JackalHelper/CryoBooster")]
	[Tracked]
	public class CryoBooster : Entity
	{
		internal static readonly FieldInfo player_boostTarget = typeof(Player).GetField("boostTarget", BindingFlags.Instance | BindingFlags.NonPublic);

		private const string reappearSfx = "event:/game/04_cliffside/greenbooster_reappear";
		private const string enterSfx = "event:/game/04_cliffside/greenbooster_enter";
		private const string boostSfx = "event:/game/04_cliffside/greenbooster_dash";
		private const string endSfx = "event:/game/04_cliffside/greenbooster_end";

		/// <summary>
		/// Temporary variable used to store the player's dashcount when entering the CryoBooster
		/// </summary>
		internal static int startingDashes = 1;

		/// <summary>
		/// Controls whether the player can refill their dashes
		/// </summary>
		public bool FrozenDash = false;

		// COLOURSOFNOISE: Not entirely sure what the purpose of this one is
		public static bool hasCryoDash;

		public bool StartedBoosting;

		// Components
		private Coroutine dashRoutine;

		public Sprite sprite;
		private Entity outline;

		private Wiggler wiggler;

		private ParticleType particleType;
		private Color particleColor;

		// Timers
		public readonly float FreezeTime = 0.25f;
		public readonly float RespawnTime = 1f;
		public readonly float BoostTime = .25f;

		public float FreezeTimer = 0f;
		private float respawnTimer;
		private float cannotUseTimer;

		public bool BoostingPlayer { get; private set; }

		[Pooled]
		private class BreakDebris : Entity
		{
			private Image sprite;

			private Vector2 speed;

			private float percent;

			private float duration;

			public BreakDebris()
			{
				Add(sprite = new Image(Calc.Random.Choose(GFX.Game.GetAtlasSubtextures("objects/boosterIce/chunk"))));
				sprite.CenterOrigin();
			}

			public BreakDebris Init(Vector2 position, Vector2 direction)
			{
				Depth = -1000;
				Position = position;
				Visible = true;

				direction = Calc.AngleToVector(direction.Angle() + Calc.Random.Range(-0.1f, 0.1f), 1f);
				direction.X += Calc.Random.Range(-0.3f, 0.3f);
				direction.Normalize();
				speed = direction * Calc.Random.Range(140, 180);
				percent = 0f;
				// COLOURSOFNOISE: Calc.Random.Range(2, 3) would only ever return 2
				duration = Calc.Random.Range(2, 4);
				return this;
			}

			public override void Update()
			{
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
				sprite.DrawOutline(Color.Black);
				base.Render();
			}
		}

		public CryoBooster(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			FrozenDash = false;
			Depth = -8500;
			Collider = new Circle(10f, 0f, 2f);

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

			Add(wiggler = Wiggler.Create(0.5f, 4f, delegate (float f)
			{
				sprite.Scale = Vector2.One * (1f + f * 0.25f);
			}));

			Add(new VertexLight(Color.White, 1f, 16, 32));
			Add(new BloomPoint(0.1f, 16f));
			Add(new MirrorReflection());

			Add(new PlayerCollider(OnPlayer));
			Add(dashRoutine = new Coroutine(removeOnComplete: false));
			Add(new DashListener(OnPlayerDashed));

			particleType = Booster.P_Burst;
			particleColor = Color.LightBlue;
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
			outline.Add(image, new MirrorReflection());
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
				particlesBG.Emit(Booster.P_Appear, 1, Center, Vector2.One * 2f, particleColor, i * ((float)Math.PI / 180f));
			}
		}

		private void OnPlayer(Player player)
		{
			hasCryoDash = JackalModule.Session.HasCryoDash;
			if (respawnTimer <= 0f && cannotUseTimer <= 0f && !BoostingPlayer)
			{
				cannotUseTimer = 0.45f;
				Boost(player);
				Audio.Play(enterSfx, Position);
				wiggler.Start();
				sprite.Play("inside");
				sprite.FlipX = player.Facing == Facings.Left;
			}
		}

		public void Boost(Player player)
		{
			player.StateMachine.State = JackalModule.CryoBoostState;
			player.Speed = Vector2.Zero;
			player_boostTarget.SetValue(player, Center);
			StartedBoosting = true;
		}

		public void PlayerBoosted(Player player, Vector2 direction)
		{
			StartedBoosting = false;
			Audio.Play(boostSfx, Position);
			BoostingPlayer = true;
			Tag = Tags.Persistent | Tags.TransitionUpdate;
			sprite.Play("spin");
			sprite.FlipX = player.Facing == Facings.Left;
			outline.Visible = true;
			wiggler.Start();
			dashRoutine.Replace(BoostRoutine(player, direction));
		}

		private IEnumerator BoostRoutine(Player player, Vector2 dir)
		{
			float angle = (-dir).Angle();
			while ((player.StateMachine.State == Player.StDash || player.StateMachine.State == Player.StRedDash) && BoostingPlayer)
			{
				if (player.Dead)
				{
					PlayerDied(player);
					continue;
				}
				sprite.RenderPosition = player.Center + Booster.playerOffset;
				if (Scene.OnInterval(0.02f))
				{
					(Scene as Level).ParticlesBG.Emit(particleType, 2, player.Center - dir * 3f + new Vector2(0f, -2f), new Vector2(3f, 3f), particleColor, angle);
				}
				yield return null;
			}
			PlayerReleased(player);
			if (player.StateMachine.State == Player.StBoost)
			{
				//sprite.Visible = false;
			}
			while (SceneAs<Level>().Transitioning)
			{
				yield return null;
			}
			Tag = 0;
		}

		public void OnPlayerDashed(Vector2 direction)
		{
			BoostingPlayer = false;
		}

		public void PlayerReleased(Player player)
		{
			Audio.Play(endSfx, sprite.RenderPosition);
			sprite.Play("pop");
			cannotUseTimer = 0f;
			respawnTimer = RespawnTime;
			BoostingPlayer = false;
			wiggler.Stop();
			for (int x = -12; x < 12; x += 8)
			{
				for (int y = -12; y < 12; y += 8)
				{
					Scene.Add(Engine.Pooler.Create<BreakDebris>().Init(new Vector2(player.X + x, player.Y + y), -(Position - player.Position)));
				}
			}
			FreezeTimer += Engine.DeltaTime;
			JackalModule.Session.HasCryoDash = hasCryoDash;
		}

		public void PlayerDied(Player player)
		{
			if (BoostingPlayer)
			{
				PlayerReleased(player);
				dashRoutine.Active = false;
				Tag = 0;
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
			Player player = Scene.Tracker.GetEntity<Player>();

			if (FreezeTimer <= 0f || FreezeTimer > FreezeTime)
			{
				FreezeTimer = 0f;
				FrozenDash = false;
			}
			else
			{
				FreezeTimer += Engine.DeltaTime;
				FrozenDash = true;
				if (player != null)
				{
					if (player.Dashes > startingDashes && startingDashes == 0)
					{
						player.Dashes = startingDashes;
					}
				}

			}

			if (player != null)
			{
				if (player.StateMachine.State == JackalModule.CryoBoostState)
				{
					FrozenDash = true;
				}
			}

			SceneAs<Level>().Session.Inventory.NoRefills = FrozenDash;

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
				Player entity = Scene.Tracker.GetEntity<Player>();
				if (entity != null && CollideCheck(entity))
				{
					target = entity.Center + Booster.playerOffset - Position;
				}
				sprite.Position = Calc.Approach(sprite.Position, target, 80f * Engine.DeltaTime);
			}

			if (sprite.CurrentAnimationID == "inside" && !BoostingPlayer && !CollideCheck<Player>())
			{
				sprite.Play("loop", randomizeFrame: true);
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

	}
	
	public static class CryoBoostStateExt {

		public static void CryoBoostBegin(this Player player)
		{
			CryoBooster.hasCryoDash = JackalModule.Session.HasCryoDash;
			bool? theoInBubble = player.SceneAs<Level>()?.Session.MapData.GetMeta()?.TheoInBubble;
			player.RefillStamina();
			if (!theoInBubble.GetValueOrDefault())
			{
				player.Drop();
			}
			CryoBooster.startingDashes = player.Dashes;
			player.SceneAs<Level>().Session.Inventory.NoRefills = true;
		}

		public static int CryoBoostUpdate(this Player player)
		{
			Vector2 boostTarget = (Vector2)CryoBooster.player_boostTarget.GetValue(player);
			Vector2 value = Input.Aim.Value * 3f;
			Vector2 vector = Calc.Approach(player.ExactPosition, boostTarget - player.Collider.Center + value, 80f * Engine.DeltaTime);
			player.MoveToX(vector.X);
			player.MoveToY(vector.Y);
			player.SceneAs<Level>().Session.Inventory.NoRefills = true;
			bool pressed = (Input.Dash.Pressed || Input.CrouchDashPressed);
			if (pressed)
			{
				Input.CrouchDash.ConsumePress();
				Input.Dash.ConsumePress();
				return Player.StDash;
			}
			JackalModule.Session.HasCryoDash = CryoBooster.hasCryoDash;
			return JackalModule.CryoBoostState;
		}

		public static IEnumerator CryoBoostCoroutine(this Player player)
		{
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
			player.StateMachine.State = Player.StDash;
			JackalModule.Session.HasCryoDash = CryoBooster.hasCryoDash;
		}

		public static void CryoBoostEnd(this Player player)
		{
			Vector2 boostTarget = (Vector2)CryoBooster.player_boostTarget.GetValue(player);
			Vector2 vector = (boostTarget - player.Collider.Center).Floor();
			player.MoveToX(vector.X);
			player.MoveToY(vector.Y);
			JackalModule.Session.HasCryoDash = CryoBooster.hasCryoDash;
		}
	}
}