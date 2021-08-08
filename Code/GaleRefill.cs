using System;
using System.Collections;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;


namespace Celeste.Mod.JackalHelper.Entities
{

	[Tracked]
	[CustomEntity("JackalHelper/GaleRefill")]

	public class GaleRefill : Entity
	{

		public WindController.Patterns Pattern;

		public static ParticleType P_Shatter;

		public static ParticleType P_Regen;

		public static ParticleType P_Glow;

		public static ParticleType P_ShatterTwo;

		public static ParticleType P_RegenTwo;

		public static ParticleType P_GlowTwo;

		private Sprite sprite;

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

		public bool check = false;

		public bool refillDash;

		public bool refillStamina;

		public bool last = false;

		public bool windNext = false;
		public Vector2 lastPos;

		public GaleRefill(Vector2 position, bool oneUse, bool refillDash, bool refillStamina)
			: base(position)
		{
			this.oneUse = oneUse;
			this.refillDash = refillDash;
			this.refillStamina = refillStamina;
			base.Collider = new Hitbox(16f, 16f, -8f, -8f);
			Add(new PlayerCollider(OnPlayer));
			p_shatter = P_ShatterTwo;
			p_regen = P_RegenTwo;
			p_glow = P_GlowTwo;
			Add(outline = new Image(GFX.Game["objects/refillCandy/outline"]));
			outline.CenterOrigin();
			outline.Visible = false;
			Add(sprite = JackalModule.spriteBank.Create("StarCandy"));
			sprite.Play("idle");
			sprite.CenterOrigin();
			Add(wiggler = Wiggler.Create(1f, 4f, delegate (float v)
			{
				sprite.Scale = Vector2.One * (1f + v * 0.2f);
			}));
			Add(new MirrorReflection());
			Add(bloom = new BloomPoint(0.8f, 16f));
			Add(light = new VertexLight(Color.White, 1f, 16, 48));
			Add(sine = new SineWave(0.6f, 0f));
			sine.Randomize();
			UpdateY();
			base.Depth = -100;
			JackalModule.Session.HasGaleDash = false;
			JackalModule.Session.GaleDashActive = false;
		}

		public GaleRefill(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Bool("oneUse", false), data.Bool("refillDash", true), data.Bool("refillStamina", true))
		{
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			level = SceneAs<Level>();
		}

		public override void Update()
		{
            if (windNext)
            {
				windNext = false;
				//beginWind(lastPos);
            }
			base.Update();
			if (respawnTimer > 0f)
			{
				respawnTimer -= Engine.DeltaTime;
				if (respawnTimer <= 0f)
				{
					Respawn();
				}
			}
			UpdateY();
			light.Alpha = Calc.Approach(light.Alpha, sprite.Visible ? 1f : 0f, 4f * Engine.DeltaTime);
			bloom.Alpha = light.Alpha * 0.8f;
			if(last ==  false && JackalModule.Session.GaleDashActive)
            {
				windNext = true;
				lastPos = JackalModule.GetPlayer().Position;
            }
			last = JackalModule.Session.GaleDashActive;
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
				Audio.Play("event:/new_content/game/10_farewell/pinkdiamond_return", Position);
			}
		}

		private void UpdateY()
		{
			Sprite obj = sprite;
			Sprite obj2 = sprite;
			float num2 = (bloom.Y = sine.Value * 2f);
			float num5 = (obj.Y = (obj2.Y = num2));

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
			Audio.Play("event:/new_content/game/10_farewell/pinkdiamond_touch", Position);
			Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
			Collidable = false;
			Add(new Coroutine(RefillRoutine(player)));
			respawnTimer = 2.5f;
			if (refillDash)
			{
				player.RefillDash();
			}
			if (refillStamina)
			{
				player.RefillStamina();
			}
			JackalModule.Session.HasGaleDash = true;
		}

		private IEnumerator RefillRoutine(Player player)
		{
			Celeste.Freeze(0.05f);
			yield return null;
			level.Shake();
			sprite.Visible = false;
			if (!oneUse)
			{
				outline.Visible = true;
			}
			Depth = 8999;
			yield return 0.05f;
			float angle = player.Speed.Angle();
			SlashFx.Burst(Position, angle);
			if (oneUse)
			{
				RemoveSelf();
			}
		}
		public void beginWind(Vector2 pos)
		{
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
				Pattern = JackalModule.GetPlayer().Facing == Facings.Right ? WindController.Patterns.Right : WindController.Patterns.Left;
			}
			WindController windController = base.Scene.Entities.FindFirst<WindController>();
			if (windController == null)
			{
				windController = new WindController(Pattern);
				base.Scene.Add(windController);
			}
			else
			{
				windController.SetPattern(WindController.Patterns.None);
				windController.SetPattern(Pattern);
			}
		}
	}
}