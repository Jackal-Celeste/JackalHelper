using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.JackalHelper.Code.Geoshock
{
	[Tracked]
	[CustomEntity("JackalHelper/InsightCrystal")]
	public class InsightCrystal : Entity
	{
		//public static ParticleType P_OnLight;

		public const float BloomAlpha = 0.5f;

		public const int StartRadius = 48;

		public const int EndRadius = 64;

		public static readonly Color Color = Color.Lerp(Color.White, Color.Cyan, 0.5f);

		public static readonly Color CollectedColor = Color.Lerp(Color.White, Color.Orange, 0.5f);

		private EntityID id;

		private bool lit;

		private VertexLight light;

		private BloomPoint bloom;

		private Sprite sprite;

		private bool alreadyLit;

		private string FlagName => "InsightCrystal_" + id.Key;

		public InsightCrystal(EntityID id, Vector2 position)
			: base(position)
		{
			this.id = id;
			base.Collider = new Hitbox(32f, 32f, -16f, -16f);
			Add(new PlayerCollider(OnPlayer));
			Add(light = new VertexLight(Color, 1f, 48, 64));
			Add(bloom = new BloomPoint(0.5f, 8f));
			bloom.Visible = false;
			light.Visible = false;
			base.Depth = 2000;
			alreadyLit = JackalModule.SaveData.insightCrystals.Contains(id);
			if (alreadyLit) light.Color = CollectedColor;
			Add(sprite = GFX.SpriteBank.Create("litTorch"));
		}

		public InsightCrystal(EntityData data, Vector2 offset, EntityID id)
			: this(id, data.Position + offset)
		{
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			if (alreadyLit)
			{
				bloom.Visible = (light.Visible = true);
				lit = true;
				Collidable = false;
				sprite.Play("on");
			}
		}

		private void OnPlayer(Player player)
		{
			if (!lit)
			{
				Audio.Play("event:/game/05_mirror_temple/torch_activate", Position);
				lit = true;
				bloom.Visible = true;
				light.Visible = true;
				Collidable = false;
				sprite.Play("turnOn");
				Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.BackOut, 1f, start: true);
				tween.OnUpdate = delegate (Tween t)
				{
					light.Color = Color.Lerp(Color.White, Color, t.Eased);
					light.StartRadius = 48f + (1f - t.Eased) * 32f;
					light.EndRadius = 64f + (1f - t.Eased) * 32f;
					bloom.Alpha = 0.5f + 0.5f * (1f - t.Eased);
				};
				Add(tween);
				SceneAs<Level>().Session.SetFlag(FlagName);
				ParticleType type = new ParticleType();
				type.Color = CollectedColor;
				SceneAs<Level>().ParticlesFG.Emit(new ParticleType(), 12, Position, new Vector2(3f, 3f));
				JackalModule.SaveData.insightCrystals.Add(id);
			}
		}
	}

}
