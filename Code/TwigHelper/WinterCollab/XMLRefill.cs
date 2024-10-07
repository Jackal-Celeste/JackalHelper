using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste;
using MonoMod;
using Celeste.Mod.Entities;
using MonoMod.Utils;
using Celeste.Mod.JackalCollabHelper;
using Celeste.Mod.JackalCollabHelper.Entities;
using System.Collections;

namespace Celeste.Mod.JackalCollabHelper.Entities
{
	[CustomEntity("JackalCollabHelper/DonkerRefill")]
	public class DonkerRefill : Entity
	{
		public static ParticleType P_Shatter;

		public static ParticleType P_Regen;

		public static ParticleType P_Glow;

		public static ParticleType P_ShatterTwo;

		public static ParticleType P_RegenTwo;

		public static ParticleType P_GlowTwo;

		private Sprite sprite;

		private Sprite flash;

		private Image outline;

		private Wiggler wiggler;

		private BloomPoint bloom;

		private VertexLight light;

		public Level level;

		private SineWave sine;

		private bool twoDashes;

		private bool oneUse;

		public ParticleType p_shatter;

		public ParticleType p_regen;

		public ParticleType p_glow;

		private float respawnTimer;

		public String dashPath;

		public String doubleDashPath;

		public DonkerRefill(Vector2 position, bool twoDashes, bool oneUse, String dashPath, String doubleDashPath)
			: base(position)
		{
			base.Collider = new Hitbox(16f, 16f, -8f, -8f);
			Add(new PlayerCollider(OnPlayer));

			this.twoDashes = twoDashes;
			this.oneUse = oneUse;
			this.dashPath = dashPath;
			this.doubleDashPath = doubleDashPath;
			if (dashPath.Equals(""))
            {
				dashPath = "refill";
			}
            if (doubleDashPath.Equals(""))
            {
				doubleDashPath = "refillTwo";
			}
			String folderPath = "objects/" + (twoDashes ? doubleDashPath : dashPath) + "/";
			p_shatter = Refill.P_ShatterTwo;
			p_regen = Refill.P_RegenTwo;
			p_glow = Refill.P_GlowTwo;
			Add(outline = new Image(GFX.Game[folderPath + "outline"]));
			outline.CenterOrigin();
			outline.Visible = false;
			Add(sprite = new Sprite(GFX.Game, folderPath + "idle"));
			sprite.AddLoop("idle", "", 0.1f);
			sprite.Play("idle");
			sprite.CenterOrigin();
			Add(flash = new Sprite(GFX.Game, folderPath + "flash"));
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
			Add(sine = new SineWave(0.6f, 0f));
			sine.Randomize();
			UpdateY();
			base.Depth = -100;
		}

		public DonkerRefill(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Bool("twoDash"), data.Bool("oneUse"), data.Attr("singleFolderName", defaultValue: "refill"), data.Attr("doubleFolderName", defaultValue: "refillTwo"))
		{
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			level = SceneAs<Level>();
		}

		public override void Update()
		{
			base.Update();
			if (respawnTimer > 0f)
			{
				respawnTimer -= Engine.DeltaTime;
				if (respawnTimer <= 0f)
				{
					Respawn();
				}
			}
			else if (base.Scene.OnInterval(0.1f))
			{
				level.ParticlesFG.Emit(p_glow, 1, Position, Vector2.One * 5f);
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

		private void Respawn()
		{
			if (!Collidable)
			{
				Collidable = true;
				sprite.Visible = true;
				outline.Visible = false;
				base.Depth = -100;
				wiggler.Start();
				Audio.Play(twoDashes ? "event:/new_content/game/10_farewell/pinkdiamond_return" : "event:/game/general/diamond_return", Position);
				level.ParticlesFG.Emit(p_regen, 16, Position, Vector2.One * 2f);
			}
		}

		private void UpdateY()
		{
			Sprite obj = flash;
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
			if (player.UseRefill(twoDashes))
			{
				Audio.Play(twoDashes ? "event:/new_content/game/10_farewell/pinkdiamond_touch" : "event:/game/general/diamond_touch", Position);
				Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
				Collidable = false;
				Add(new Coroutine(RefillRoutine(player)));
				respawnTimer = 2.5f;
			}
		}

		public IEnumerator RefillRoutine(Player player)
		{
			Celeste.Freeze(0.05f);
			yield return null;
			level.Shake();
			sprite.Visible = (flash.Visible = false);
			if (!oneUse)
			{
				outline.Visible = true;
			}
			Depth = 8999;
			yield return 0.05f;
			float angle = player.Speed.Angle();
			level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, angle - (float)Math.PI / 2f);
			level.ParticlesFG.Emit(p_shatter, 5, Position, Vector2.One * 4f, angle + (float)Math.PI / 2f);
			SlashFx.Burst(Position, angle);
			if (oneUse)
			{
				RemoveSelf();
			}
		}
	}
}