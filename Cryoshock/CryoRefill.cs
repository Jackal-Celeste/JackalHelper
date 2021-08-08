using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using Celeste.Mod.JackalHelper;

namespace Celeste.Mod.JackalHelper.Entities
{


	[CustomEntity("JackalHelper/CryoRefill")]
	[Tracked]
	public class CryoRefill : Entity
	{
		private Sprite sprite;

		private Sprite flash;

		private Image outline;

		private Wiggler wiggler;

		private BloomPoint bloom;

		private VertexLight light;

		private Level level;

		private SineWave sine;

		private bool oneUse;

		private ParticleType p_shatter;

		private ParticleType p_regen;

		private ParticleType p_glow;

		private float respawnTimer;

		private bool refillDash;

		public bool addDash;
		public Player player;
		public bool storedCryoDash = false;
		public bool cryoDashing = false;
		public bool startPhase = true;
		public bool cryoDashReady = false;
		public float timer = 0f;
		public float freezeTime = 0.35f;
		public bool FrozenDash = false;
		public bool dashQueued = false;


		public CryoRefill(Vector2 position, bool oneUse, bool refillDash, bool addDash, float radius)
			: base(position)
		{
			JackalModule.Session.CryoRadius = radius;
			base.Collider = new Hitbox(16f, 16f, -8f, -8f);
			Add(new PlayerCollider(OnPlayer));
			this.oneUse = oneUse;
			this.refillDash = refillDash;
			this.addDash = addDash;
			string str = "objects/refillCryo/";
			p_shatter = Refill.P_Shatter;
			p_regen = Refill.P_Regen;
			p_glow = Refill.P_Glow;
			Add(outline = new Image(GFX.Game[str + "outline"]));
			outline.CenterOrigin();
			outline.Visible = false;
			Add(sprite = new Sprite(GFX.Game, str + "idle"));
			sprite.AddLoop("idle", "", 0.1f);
			sprite.Play("idle");
			sprite.CenterOrigin();
			Add(flash = new Sprite(GFX.Game, str + "flash"));
			flash.Add("flash", "", 0.05f);
			flash.OnFinish = delegate
			{
				flash.Visible = false;
			};
			flash.CenterOrigin();
			Add(wiggler = Wiggler.Create(1f, 4f, delegate (float v)
			{
				sprite.Scale = (flash.Scale = Vector2.One * (1f + v * 0.2f));
			}));
			Add(new MirrorReflection());
			Add(bloom = new BloomPoint(0.8f, 16f));
			Add(light = new VertexLight(Color.White, 1f, 16, 48));
			Add(sine = new SineWave(0.6f));
			sine.Randomize();
			//UpdateY();
			base.Depth = -100;
		}

		public CryoRefill(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Bool("oneUse", defaultValue: false), data.Bool("RefillDashOnUse", defaultValue: true), data.Bool("AddDash", defaultValue: false), data.Float("Radius", defaultValue: 20f))
		{
		}

		public override void Update()
		{
			
			if (JackalModule.GetPlayer() != null)
			{
				if(JackalModule.GetPlayer().StateMachine.State != 2 && JackalModule.Session.dashQueue)
                {
					JackalModule.Session.dashQueue = false;
                }
				if (respawnTimer > 0f)
				{
					respawnTimer -= Engine.DeltaTime;
					if (respawnTimer <= 0f)
					{
						Respawn();
					}
				}
				else if (base.Scene.OnInterval(0.1f) && JackalModule.GetLevel() != null)
				{
					//JackalModule.GetLevel().ParticlesFG.Emit(p_glow, 1, Position, Vector2.One * 5f);
				}
				UpdateY();
				light.Alpha = Calc.Approach(light.Alpha, sprite.Visible ? 1f : 0f, 4f * Engine.DeltaTime);
				bloom.Alpha = light.Alpha * 0.8f;
				if (base.Scene.OnInterval(2f) && sprite.Visible)
				{
					flash.Play("flash", restart: true);
					flash.Visible = true;
				}
			}
			pityTimer();
				base.Update();
		}




		public void pityTimer()
		{
			if (timer <= 0f || timer > freezeTime)
			{
				timer = 0f;
				FrozenDash = false;
			}
			else
			{
				timer += Engine.DeltaTime;
				FrozenDash = true;
			}

		}

		private void Respawn()
		{
			if (!Collidable)
			{
				Collidable = true;
				sprite.Visible = true;
				outline.Visible = false;
				base.Depth = -100;
				wiggler.Start();
				Audio.Play("event:/game/general/diamond_return", Position);
				//level.ParticlesFG.Emit(p_regen, 16, Position, Vector2.One * 2f);
			}
		}

		private void UpdateY()
		{
			Sprite sprite = flash;
			Sprite sprite2 = this.sprite;
			float num2 = bloom.Y = sine.Value * 2f;
			float num3 = num2;
			float num5 = sprite2.Y = num3;
			num2 = (sprite.Y = num5);
			float num7 = num2;
		}

		public override void Render()
		{
			if (sprite.Visible)
			{
				sprite.DrawOutline();
			}
			base.Render();
		}

		private void OnPlayer(Player player)
		{
			if (Collidable)
			{
				if (addDash)
				{
					player.Dashes = Math.Min(player.Dashes + 1, 2);
				}
				
				JackalModule.Session.HasCryoDash = true;
				if(player.StateMachine.State == 2 || player.StateMachine.State == JackalModule.cryoBoostState || player.StateMachine.State == 4 || player.StateMachine.State == 5)
                {
					JackalModule.Session.dashQueue = true;
                }
				Audio.Play("event:/game/general/diamond_touch", Position);
				Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
				Collidable = false;
				Add(new Coroutine(RefillRoutine(player)));
				respawnTimer = 2.5f;
				storedCryoDash = true;
			}
		}

		private IEnumerator RefillRoutine(Player player)
		{
			level = JackalModule.GetLevel();
			Celeste.Freeze(0.05f);
			yield return null;
			level.Shake();
			sprite.Visible = (flash.Visible = false);
			if (refillDash && player != null)
			{
				player.RefillDash();
				player.RefillStamina();
			}
			if (!oneUse)
			{
				outline.Visible = true;
			}
			base.Depth = 8999;
			yield return 0.05f;
			float num = player.Speed.Angle();
			//level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, num - (float)Math.PI / 2f);
			//level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, num + (float)Math.PI / 2f);
			SlashFx.Burst(Position, num);
			if (oneUse)
			{
				RemoveSelf();
			}
		}
	}
}