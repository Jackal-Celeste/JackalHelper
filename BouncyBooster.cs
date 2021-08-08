using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.JackalHelper.Entities
{
	[Tracked]
	[CustomEntity("JackalHelper/BouncyBooster")]
	public class BouncyBooster : Booster
	{
		public BouncyBooster(Vector2 position) : base(position, true)
        {

        }


		/*
		public override void PlayerReleased()
		{
			Audio.Play("event:/game/05_mirror_temple/redbooster_end",  base.sprite.RenderPosition);
			sprite.Play("pop");
			cannotUseTimer = 0f;
			respawnTimer = 1f;
			BoostingPlayer = false;
			wiggler.Stop();
			loopingSfx.Stop();
		}*/



	}
}

