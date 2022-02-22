using System;
using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

/*Linear:
 * initial speed, 
 * acceleration constant
 * 
 * Circular:
 * frequency,
 * amplitude,
 * 
 * Visual:
 * directory, ***
 * sprite tint,
 * 
 * General: 
 * can jump out of, 
 * amount of dashes to give
 */

namespace Celeste.Mod.JackalHelper.Entities
{
	[Tracked]
	[CustomEntity("JackalHelper/CustomRedBooster")]
	public class CustomRedBooster : Entity
	{
		public static ParticleType P_Burst;
		public static ParticleType P_BurstRed;
		public static ParticleType P_Appear;
		public static ParticleType P_RedAppear;

		public static readonly Vector2 playerOffset = new Vector2(0f, -2f);

		private Sprite sprite;

		private Entity outline;

		private Wiggler wiggler;

		private Coroutine dashRoutine;

		private DashListener dashListener;

		private ParticleType particleType;

		private float respawnTimer;

		private float cannotUseTimer;

		private SoundSource loopingSfx;

		private Color tint = Color.White;

		public float launchSpeed = 240f;

		public float decayRate = 1f;

		public bool overrideDashes = false;

		public int dashes = 1;

		public bool canJump = false;

		public Vector2 SinAmp;
		public Vector2 SinFreq;

		public bool BoostingPlayer { get; private set; }

		public CustomRedBooster(Vector2 position, float launchSpeed, float decayRate, float xSinAmp, float xSinFreq, float ySinAmp, float ySinFreq, bool overrideDashes, int dashes, bool canJump, string tint)
			: base(position)
		{
			this.launchSpeed = launchSpeed;
			this.decayRate = decayRate;
			SinAmp = new Vector2(xSinAmp, ySinAmp);
			SinFreq = new Vector2(xSinFreq, ySinFreq);
			this.overrideDashes = overrideDashes;
			this.dashes = dashes;
			this.canJump = canJump;
			this.tint = Calc.HexToColor(tint);
			if (this.tint == null)
			{
				this.tint = Color.White;
			}
			if (dashes < 0)
			{
				dashes = 0;
			}
			Depth = Depths.Above;
			Collider = new Circle(10f, 0f, 2f);
			Add(sprite = JackalModule.spriteBank.Create("boosterBase"));
			sprite.Color = this.tint;
			sprite.Color.Invert();
			Add(new PlayerCollider(OnPlayer));
			Add(new VertexLight(this.tint, 1f, 16, 32));
			Add(new BloomPoint(0.1f, 16f));
			Add(wiggler = Wiggler.Create(0.5f, 4f, delegate (float f)
			{
				sprite.Scale = Vector2.One * (1f + f * 0.25f);
			}));
			Add(dashRoutine = new Coroutine(removeOnComplete: false));
			Add(dashListener = new DashListener());
			Add(new MirrorReflection());
			Add(loopingSfx = new SoundSource());
			dashListener.OnDash = OnPlayerDashed;
			particleType = Booster.P_BurstRed;
		}

		public CustomRedBooster(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Float("launchSpeed"), data.Float("decayRate"), data.Float("xSineAmplitude"), data.Float("xSineFrequency"), data.Float("ySineAmplitude"), data.Float("ySineFrequency"), data.Bool("overrideDashes"), data.Int("dashes"), data.Bool("canJumpFromBooster"), data.Attr("tint"))
		{
		}

		public override void Added(Scene scene)
		{

			base.Added(scene);
			Image image = new Image(GFX.Game["objects/booster/outline"]);
			image.CenterOrigin();
			image.Color = Color.Lerp(Color.White, Calc.HexToColor("f48b95"), 0.2f) * 0.75f;
			outline = new Entity(Position);
			outline.Depth = 8999;
			outline.Visible = false;
			outline.Add(image);
			outline.Add(new MirrorReflection());
			scene.Add(outline);
		}

		public void Appear()
		{
			Audio.Play("event:/game/05_mirror_temple/redbooster_reappear", Position);
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
				particlesBG.Emit(Booster.P_RedAppear, 1, Center, Vector2.One * 2f, i * ((float)Math.PI / 180f));
			}
		}

		private void OnPlayer(Player player)
		{
			if (respawnTimer <= 0f && cannotUseTimer <= 0f && !BoostingPlayer)
			{
				cannotUseTimer = 0.45f;
				CustomRedBoost(player, this);
				Audio.Play("event:/game/05_mirror_temple/redbooster_enter", Position);
				wiggler.Start();
				sprite.Play("inside");
				sprite.FlipX = player.Facing == Facings.Left;
			}
		}

		private static IEnumerator Sequence(Player player, CustomRedBooster booster)
		{
			yield return 0.25f;
			booster.PlayerBoosted(player);
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
			Audio.Play("event:/game/05_mirror_temple/redbooster_end", sprite.RenderPosition);
			sprite.Play("pop");
			cannotUseTimer = 0f;
			respawnTimer = 0.75f;
			BoostingPlayer = false;
			wiggler.Stop();
			loopingSfx.Stop();
		}

		public void PlayerDied()
		{
			if (BoostingPlayer)
			{
				PlayerReleased();
				dashRoutine.Active = false;
				Tag = 0;
			}
		}

		public void Respawn()
		{
			Audio.Play("event:/game/05_mirror_temple/redbooster_reappear", Position);
			sprite.Position = Vector2.Zero;
			sprite.Play("loop", restart: true);
			wiggler.Start();
			sprite.Visible = true;
			outline.Visible = false;
			AppearParticles();
		}

		public override void Update()
		{
			Player player = Scene.Tracker.GetEntity<Player>();
			//if (player == null && JackalModule.Session.lastBooster == this && state == JackalModule.CustomRedBoostState)
			{
				//sprite.Play("pop");
				//Visible = false;
			}
			base.Update();
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
				if (player != null && CollideCheck(player))
				{
					target = player.Center + playerOffset - Position;
				}
				sprite.Position = Calc.Approach(sprite.Position, target, 80f * Engine.DeltaTime);
			}
			if (sprite.CurrentAnimationID == "inside" && !BoostingPlayer && !CollideCheck<Player>())
			{
				sprite.Play("loop");
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

		public static void CustomRedBoost(Player player, CustomRedBooster booster)
		{
			player.StateMachine.State = JackalModule.CustomRedBoostState;
			player.Position = booster.Center;
			player.Speed = Vector2.Zero;

			var playerData = new DynData<Player>(player);
			playerData.Set("boostTarget", booster.Center);
			playerData.Set(Entities.CustomRedBoost.PLAYER_LASTBOOSTER, booster);

			booster.Add(new Coroutine(Sequence(player, booster)));
		}

		public void PlayerBoosted(Player player)
		{
			player.Center = Center;
			Audio.Play("event:/game/05_mirror_temple/redbooster_dash", Position);
			loopingSfx.Play("event:/game/05_mirror_temple/redbooster_move");
			loopingSfx.DisposeOnTransition = false;
			BoostingPlayer = true;
			Tag = Tags.Persistent | Tags.TransitionUpdate;
			sprite.Play("spin");
			sprite.FlipX = player.Facing == Facings.Left;
			outline.Visible = true;
			wiggler.Start();
			dashRoutine.Replace(BoostRoutine(player, player.DashDir));
		}


		private IEnumerator BoostRoutine(Player player, Vector2 dir)
		{
			while ((player.StateMachine.State == JackalModule.CustomRedBoostState || player.StateMachine.State == Player.StDash) && BoostingPlayer)
			{
				sprite.RenderPosition = player.Center + playerOffset;
				loopingSfx.Position = sprite.Position;
				if (Scene.OnInterval(0.02f))
				{
					//(base.Scene as Level).ParticlesBG.Emit(P_Burst, 2, player.Center - dir * 3f + new Vector2(0f, -2f), new Vector2(3f, 3f), angle);
				}
				yield return null;
			}
			PlayerReleased();
			sprite.Visible = false;
			while (SceneAs<Level>().Transitioning)
			{
				yield return null;
			}
			Tag = 0;
		}



	}
}
