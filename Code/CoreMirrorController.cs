using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.JackalHelper.Entities
{
	[CustomEntity("JackalHelper/RoundKevin2")]
	public class CoreMirrorController : Entity
	{
		public CoreBooster booster;

		public bool red;

		public Scene scene;

		public bool hasLaunched = false;

		public bool firstLaunch = true;

		public Session.CoreModes mode;

		private DynData<Booster> dyn;

		public Vector2 playerPosition;

		public bool redo;



		public CoreMirrorController(Vector2 position)
			: base(position)
		{
		}

		public CoreMirrorController(EntityData data, Vector2 offset)
			: this(data.Position)
		{
		}

		public override void Awake(Scene scene)
		{
			if (JackalModule.GetLevel() != null)
			{
				redo = false;
				if (firstLaunch)
				{
					if (JackalModule.GetLevel().CoreMode == Session.CoreModes.None)
					{
						JackalModule.GetLevel().CoreMode = Session.CoreModes.Hot;
					}
					firstLaunch = false;
				}
				mode = JackalModule.GetLevel().CoreMode;
				red = (JackalModule.GetLevel().CoreMode == Session.CoreModes.Hot || JackalModule.GetLevel().CoreMode == Session.CoreModes.None);
				JackalModule.GetLevel().Add(booster = new CoreBooster(Position, red, false));
				booster.Position = Position + JackalModule.GetLevel().LevelOffset;
				dyn = new DynData<Booster>(booster);
				dyn.Get<Sprite>("sprite").Play("swap");
				Audio.Play(red ? "event:/game/05_mirror_temple/redbooster_reappear" : "event:/game/04_cliffside/greenbooster_reappear", Position);
			}
			else
			{
				redo = true;
			}
		}


		public override void Removed(Scene scene)
		{
			base.Removed(scene);
			redo = true;
		}

		public override void Update()
		{
			if (redo)
			{
				if (JackalModule.GetLevel() != null)
				{
					redo = false;
					if (firstLaunch)
					{
						if (JackalModule.GetLevel().CoreMode == Session.CoreModes.None)
						{
							JackalModule.GetLevel().CoreMode = Session.CoreModes.Hot;
						}
						firstLaunch = false;
					}
					mode = JackalModule.GetLevel().CoreMode;
					red = (JackalModule.GetLevel().CoreMode == Session.CoreModes.Hot || JackalModule.GetLevel().CoreMode == Session.CoreModes.None);
					JackalModule.GetLevel().Add(booster = new CoreBooster(Position, red, false));
					dyn = new DynData<Booster>(booster);
					dyn.Get<Sprite>("sprite").Play("swap");
					Audio.Play(red ? "event:/game/05_mirror_temple/redbooster_reappear" : "event:/game/04_cliffside/greenbooster_reappear", Position);
				}
			}
			else
			{
				if (JackalModule.GetPlayer() != null)
				{

					StateCheck(JackalModule.GetPlayer(), booster);
				}
				if (JackalModule.GetLevel() != null)
				{
					mode = JackalModule.GetLevel().CoreMode;
				}

			}
		}


		public void Destroy(CoreBooster booster)
		{
			dyn = new DynData<Booster>(booster);
			//sprite.Visible = false;
			Remove(dyn.Get<Sprite>("sprite"));
			booster.RemoveSelf();
			JackalModule.GetLevel().CoreMode = (red ? Session.CoreModes.Cold : Session.CoreModes.Hot);
			Awake(Scene);
		}

		public void StateCheck(Player player, CoreBooster booster)
		{
			if (player.LastBooster is CoreBooster || player.LastBooster == null)
			{

				if (player.StateMachine.State == 4)
				{
					hasLaunched = true;

					playerPosition = player.Position;

				}
				if ((player.StateMachine.State != 4 && player.StateMachine.State != 5) && hasLaunched && (Vector2.Distance(player.Position, player.LastBooster.Position) > 28f))
				{
					hasLaunched = false;
					Destroy(booster);
				}
				else if (mode != JackalModule.GetLevel().CoreMode)
				{
					hasLaunched = false;
					Destroy(booster);
				}
			}

		}
	}
}



