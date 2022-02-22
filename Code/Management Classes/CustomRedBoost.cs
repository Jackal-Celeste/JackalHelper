using System;
using System.Collections;
using System.Reflection;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.JackalHelper.Entities
{
	public static class CustomRedBoost
	{
		internal const string PLAYER_LASTBOOSTER = "JackalHelper_LastCustomRedBooster";

		public static MethodInfo m_Player_RedDashUpdate = typeof(Player).GetMethod("RedDashUpdate", BindingFlags.Instance | BindingFlags.NonPublic);

		// Only assigned when player is in the CustomRedBoost state
		private static DynData<Player> playerData;

		public static int time = 0;
		private static float timer;
		public static float totalTime = 0f;

		public static Vector2 dir;

		public static void CustomRedBoostBegin(this Player player)
		{
			playerData = new DynData<Player>(player);
			player.RefillDash();
			player.RefillStamina();
			timer = 0.26f;
			totalTime = 0f;
			time = 0;
			time++;
			// COLOURSOFNOISE: No DashAssist :(
		}

		public static IEnumerator CustomRedBoostCoroutine(this Player player)
		{
			if (playerData == null || !(playerData.Target == player))
			{
				playerData = new DynData<Player>(player);
			}

			yield return 0.25f;
			dir = (Input.Aim.Value == Vector2.Zero ? Vector2.UnitX : Input.Aim.Value);

			while (true)
			{
				Vector2 v = (player.DashDir = player.Speed);
				playerData.Set("gliderBoostDir", v);
				(player.Scene as Level).DirectionalShake(player.DashDir, 0.2f);
				if (player.DashDir.X != 0f)
				{
					player.Facing = (Facings)Math.Sign(player.DashDir.X);
				}
				yield return null;

			}
		}

		public static int CustomRedBoostUpdate(this Player player)
		{
			if (playerData == null || !(playerData.Target == player))
			{
				playerData = new DynData<Player>(player);
			}

			player.LastBooster = null;
			time++;
			totalTime += Engine.DeltaTime;

			if (timer > 0f)
			{
				timer -= Engine.DeltaTime;
				player.Center = playerData.Get<Vector2>("boostTarget");
				return JackalModule.CustomRedBoostState;
			}

			int nextState = (int)m_Player_RedDashUpdate.Invoke(player, null);

			// This *should* be safe to do without checking if the key exists first
			CustomRedBooster lastBooster = playerData.Get<CustomRedBooster>(PLAYER_LASTBOOSTER);

			float sinDeltaX = lastBooster.SinAmp.X * (float)Math.Sin(lastBooster.SinFreq.X * totalTime);
			float sinDeltaY = lastBooster.SinAmp.Y * (float)Math.Sin(lastBooster.SinFreq.Y * totalTime);
			Vector2 quirky = new Vector2(sinDeltaX, sinDeltaY);
			if (lastBooster.overrideDashes)
			{
				player.Dashes = Math.Max(player.Dashes, lastBooster.dashes);
			}
			if (dir == null)
			{
				dir = Vector2.UnitX * (float)player.Facing;
			}
			//dir.Normalize();

			player.Speed = ((dir.X != 0 && dir.Y != 0) ? 1f / (float)Math.Sqrt(2) : 1f) * (quirky + /*CorrectDashPrecision(dir)*/ dir * lastBooster.launchSpeed * (float)Math.Pow(lastBooster.decayRate, time));

			//player.Speed = quirky + CorrectDashPrecision(dir * JackalModule.Session.lastBooster.launchSpeed * (float)Math.Pow((double)JackalModule.Session.lastBooster.decayRate, (double)time));

			if (nextState != Player.StRedDash)
			{
				time = 0;
			}
			if (Input.Jump.Pressed && lastBooster.canJump)
			{
				player.Jump();
				player.Speed.Y *= 2f;
				return Player.StNormal;
			}
			if (Input.Dash.Pressed)
			{
				return player.StartDash();
			}
			if (player.Facing == Facings.Right && player.CollideCheck<SolidTiles>(player.CenterRight + Vector2.UnitX) || player.CollideCheck<Solid>(player.CenterRight + Vector2.UnitX))
			{
				return Player.StNormal;
			}
			if (player.Facing == Facings.Left && player.CollideCheck<SolidTiles>(player.CenterLeft - Vector2.UnitX) || player.CollideCheck<Solid>(player.CenterLeft - Vector2.UnitX))
			{
				return Player.StNormal;
			}
			if (player.Speed.Y < -20f && player.CollideCheck<SolidTiles>(player.TopCenter - Vector2.UnitY) || player.CollideCheck<Solid>(player.TopCenter - Vector2.UnitY))
			{
				return Player.StNormal;
			}
			if (player.Speed.Y > 20f && player.CollideCheck<SolidTiles>(player.BottomCenter + Vector2.UnitY) || player.CollideCheck<Solid>(player.BottomCenter + Vector2.UnitY))
			{
				return Player.StNormal;
			}
			player.Sprite.Visible = (nextState != Player.StRedDash);
			player.Hair.Visible = (nextState != Player.StRedDash);
			return (nextState == Player.StRedDash) ? JackalModule.CustomRedBoostState : nextState;
		}

		public static void CustomRedBoostEnd(this Player player)
		{
			if (playerData == null || !(playerData.Target == player))
			{
				playerData = new DynData<Player>(player);
			}
			playerData.Set<CustomRedBooster>(PLAYER_LASTBOOSTER, null);

			player.Sprite.Visible = true;
			player.Hair.Visible = true;
			dir = Vector2.Zero;
			time = 0;
		}

		private static Vector2 CorrectDashPrecision(Vector2 dir)
		{
			if (dir.X != 0f && Math.Abs(dir.X) < 0.001f)
			{
				dir.X = 0f;
				dir.Y = Math.Sign(dir.Y);
			}
			else if (dir.Y != 0f && Math.Abs(dir.Y) < 0.001f)
			{
				dir.Y = 0f;
				dir.X = Math.Sign(dir.X);
			}
			return dir;
		}
	}
}
