using System;
using System.Collections;
using System.Collections.Generic;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;



namespace TwigHelper.ARC_Project
{
	[Tracked]
	[CustomEntity("TwigHelper/Inkrail")]
	public class Inkrail : Entity
	{
		private const float RespawnTime = 1f;

		public static ParticleType P_Burst;

		public static ParticleType P_BurstRed;

		public static ParticleType P_Appear;

		public static ParticleType P_RedAppear;

		public static readonly Vector2 playerOffset = new Vector2(0f, -2f);

		private Sprite sprite;

		private Entity outline;

		private Wiggler wiggler;

		private BloomPoint bloom;

		private VertexLight light;

		private Coroutine dashRoutine;

		private DashListener dashListener;

		private ParticleType particleType;

		private float respawnTimer;

		private float cannotUseTimer;

		private bool red;

		private SoundSource loopingSfx;

		public bool Ch9HubInkrail;

		public bool Ch9HubTransition;

		public float BoostTime = .25f;

		public static Vector2 aim;

		public EntityData entityData;



		public string XMLreference = "boosterBase";

		public string tint = "ffffff";

		public float launchSpeed = 240f;

		public float decayRate = 1f;

		public bool overrideDashes = false;

		public int dashes = 1;

		public bool canJump = false;

		public float xSinAmp = 0f;

		public float ySinAmp = 0f;

		public float xSinFreq = 0f;

		public float ySinFreq = 0f;

		public int state = 0;

		public List<Inkrail> neighbors = new List<Inkrail>();

		public bool BoostingPlayer
		{
			get;
			private set;
		}

		public Inkrail(Vector2 position, float launchSpeed, float decayRate, float xsinAmp, float xSinFreq, float ySinAmp, float ySinFreq, bool overrideDashes, int dashes, bool canJump, string tint)
			: base(position)
		{
			this.launchSpeed = launchSpeed;
			this.decayRate = decayRate;
			xSinAmp = xsinAmp;
			this.ySinAmp = ySinAmp;
			this.xSinFreq = xSinFreq;
			this.ySinFreq = ySinFreq;
			this.overrideDashes = overrideDashes;
			this.dashes = dashes;
			this.canJump = canJump;
			this.tint = tint;
			if (Calc.HexToColor(tint) == null)
			{
				tint = "ffffff";
			}
			if (dashes < 0)
			{
				dashes = 0;
			}
			base.Depth = -8500;
			base.Collider = new Circle(10f, 0f, 2f);
			red = true;
			Add(sprite = TwigModule.spriteBank.Create("boosterBase"));
			sprite.Color = Calc.HexToColor(tint);
			sprite.Color.Invert();
			Add(new PlayerCollider(OnPlayer));
			Add(light = new VertexLight(Calc.HexToColor(tint), 1f, 16, 32));
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
			particleType = Booster.P_BurstRed;
		}

		public Inkrail(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Float("launchSpeed"), data.Float("decayRate"), data.Float("xSineAmplitude"), data.Float("xSineFrequency"), data.Float("ySineAmplitude"), data.Float("ySineFrequency"), data.Bool("overrideDashes"), data.Int("dashes"), data.Bool("canJumpFromInkrail"), data.Attr("tint"))
		{
			entityData = data;
		}

		public override void Added(Scene scene)
		{

			//neighbors.Add(this);
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
			foreach(Inkrail i in (scene as Level).Tracker.GetEntities<Inkrail>())
            {
				if(!(i.neighbors.Contains(this) && this.neighbors.Contains(i)))
                {
					neighbors.Add(i);
					i.neighbors.Add(this);
					scene.Add(new Edge(this, i));
                }
            }
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
				particlesBG.Emit(Booster.P_RedAppear, 1, base.Center, Vector2.One * 2f, i * ((float)Math.PI / 180f));
			}
		}

		private void OnPlayer(Player player)
		{
			if (respawnTimer <= 0f && cannotUseTimer <= 0f && !BoostingPlayer)
			{
				if(player.StateMachine.State == TwigModule.InkrailState)
                {
					player.StateMachine.State = 0;
				}
				TwigModule.Session.lastInkrail = this;
				TwigModule.Session.lastInkrailPos = Position;
				cannotUseTimer = 0.45f;
				Inkrailer(player, this);
				Audio.Play("event:/game/05_mirror_temple/redbooster_enter", Position);
				wiggler.Start();
				sprite.Play("inside");
				sprite.FlipX = player.Facing == Facings.Left;
				//player.StateMachine.State = TwigModule.InkrailState;
			}
		}


		private static IEnumerator Sequence(Player player, Inkrail booster)
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
				base.Tag = 0;
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
			Player player = TwigModule.GetPlayer();
			if (player != null)
			{
				state = player.StateMachine.State;
			}
			if (player == null && TwigModule.Session.lastInkrail == this && state == TwigModule.InkrailState)
			{
				sprite.Play("pop");
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
				Player entity = base.Scene.Tracker.GetEntity<Player>();
				if (entity != null && CollideCheck(entity))
				{
					target = entity.Center + playerOffset - Position;
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

		public override void Removed(Scene scene)
		{
			if (Ch9HubTransition)
			{
				Level level = scene as Level;
				foreach (Backdrop item in level.Background.GetEach<Backdrop>("bright"))
				{
					item.ForceVisible = false;
					item.FadeAlphaMultiplier = 1f;
				}
				level.Bloom.Base = AreaData.Get(level).BloomBase + 0.25f;
				level.Session.BloomBaseAdd = 0.25f;
			}
			base.Removed(scene);
		}

		public static void Inkrailer(Player player, Inkrail booster)
		{
			player.StateMachine.State = TwigModule.InkrailState;
			player.Position = booster.Center;
			player.Speed = Vector2.Zero;
			new DynData<Player>(player).Set("boostTarget", booster.Center);
			booster.Add(new Coroutine(Sequence(player, booster)));
		}

		public void PlayerBoosted(Player player)
		{
			player.Center = base.Center;
			Audio.Play("event:/game/05_mirror_temple/redbooster_dash", Position);
			loopingSfx.Play("event:/game/05_mirror_temple/redbooster_move");
			loopingSfx.DisposeOnTransition = false;
			BoostingPlayer = true;
			base.Tag = Tags.Persistent | Tags.TransitionUpdate;
			sprite.Play("spin");
			sprite.FlipX = player.Facing == Facings.Left;
			outline.Visible = true;
			wiggler.Start();
			dashRoutine.Replace(BoostRoutine(player, player.DashDir));
		}
		

		private IEnumerator BoostRoutine(Player player, Vector2 dir)
		{
			float angle = (-dir).Angle();
			while ((player.StateMachine.State == TwigModule.InkrailState || player.StateMachine.State == 2) && BoostingPlayer)
			{
				sprite.RenderPosition = player.Center + playerOffset;
				loopingSfx.Position = sprite.Position;
				if (base.Scene.OnInterval(0.02f))
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
			base.Tag = 0;
		}



	}
}
