using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.JackalHelper.Entities
{
    public class CustomRedBoost
    {
		private static float timer;

		private static DynData<Player> dyn;

		public static int time = 0;

		public static MethodInfo rdU = typeof(Player).GetMethod("RedDashUpdate", BindingFlags.Instance | BindingFlags.NonPublic);

		public static Vector2 dir;

		public static float totalTime = 0f;

		public static Vector2 lastPos;


		public static void Begin()
		{
			Player player = JackalModule.GetPlayer();
			player.RefillDash();
			player.RefillStamina();
			timer = 0.26f;
			dyn = null;
			totalTime = 0f;
			time = 0;
			//dir = Input.Aim.Value;
			time++;
			
		}

		public static IEnumerator Coroutine()
		{
			yield return 0.25f;
			dir = (Input.Aim.Value == Vector2.Zero ? Vector2.UnitX : Input.Aim.Value);
			
			Player player = JackalModule.GetPlayer();
			if (dyn == null)
			{
				dyn = new DynData<Player>(player);
			}
			
			//player.Speed = CorrectDashPrecision(dir) * JackalModule.Session.lastBooster.launchSpeed * (float)Math.Pow(1 + (double)JackalModule.Session.lastBooster.decayRate, (double)time);
			_ = player.Speed;
			Vector2 value = Vector2.Zero;
			DynData<Level> i = new DynData<Level>(player.Scene as Level);
			while (true)
			{
				Vector2 v = (player.DashDir = player.Speed);
				dyn.Set("gliderBoostDir", v);
				(player.Scene as Level).DirectionalShake(player.DashDir, 0.2f);
				if (player.DashDir.X != 0f)
				{
					player.Facing = (Facings)Math.Sign(player.DashDir.X);
				}
				yield return null;
				
			}
		}

		public static int Update()
		{
			Player player = JackalModule.GetPlayer();
			
			
			player.LastBooster = null;
			time++;
			totalTime += Engine.DeltaTime;

			if (timer > 0f)
			{
				timer -= Engine.DeltaTime;
				player.Center = new DynData<Player>(player).Get<Vector2>("boostTarget");
				return JackalModule.customRedBoostState;
			}
			int num = (int)rdU.Invoke(player, new object[0]);
			if (JackalModule.Session.lastBooster == null)
			{
				JackalModule.Session.lastBooster = JackalModule.GetLevel().Tracker.GetNearestEntity<CustomRedBooster>(player.Position);
			}
			else
			{
				float sinDeltaX = JackalModule.Session.lastBooster.xSinAmp * (float)Math.Sin(JackalModule.Session.lastBooster.xSinFreq * totalTime);
				float sinDeltaY = JackalModule.Session.lastBooster.ySinAmp * (float)Math.Sin(JackalModule.Session.lastBooster.ySinFreq * totalTime);
				Vector2 quirky = new Vector2(sinDeltaX, sinDeltaY);
				if (JackalModule.Session.lastBooster.overrideDashes)
				{
					player.Dashes = Math.Max(player.Dashes, JackalModule.Session.lastBooster.dashes);
				}
				if(dir == null)
                {
					dir = Vector2.UnitX * (float)JackalModule.GetPlayer().Facing;
                }
				//dir.Normalize();
				
				player.Speed = ((dir.X != 0 && dir.Y != 0) ? 1f/(float)Math.Sqrt(2) : 1f) * (quirky + /*CorrectDashPrecision(dir)*/ dir *   JackalModule.Session.lastBooster.launchSpeed * (float)Math.Pow((double)JackalModule.Session.lastBooster.decayRate, (double)time));

				//player.Speed = quirky + CorrectDashPrecision(dir * JackalModule.Session.lastBooster.launchSpeed * (float)Math.Pow((double)JackalModule.Session.lastBooster.decayRate, (double)time));
			}

			if (num != 5)
            {
				time = 0;
            }
            if (Input.Jump.Pressed && JackalModule.Session.lastBooster.canJump)
            {
				JackalModule.GetPlayer().Jump();
				JackalModule.GetPlayer().Speed.Y *= 2f;
				return 0;
            }
            if (Input.Dash.Pressed)
            {
				JackalModule.GetPlayer().StartDash();
				return 2;
            }
			if(JackalModule.GetPlayer().Facing == Facings.Right && JackalModule.GetPlayer().CollideCheck<SolidTiles>(JackalModule.GetPlayer().CenterRight + Vector2.UnitX) || JackalModule.GetPlayer().CollideCheck<Solid>(JackalModule.GetPlayer().CenterRight + Vector2.UnitX))
            {
				return 0;
            }
			if (JackalModule.GetPlayer().Facing == Facings.Left && JackalModule.GetPlayer().CollideCheck<SolidTiles>(JackalModule.GetPlayer().CenterLeft - Vector2.UnitX) || JackalModule.GetPlayer().CollideCheck<Solid>(JackalModule.GetPlayer().CenterLeft - Vector2.UnitX))
			{
				return 0;
			}
			if (JackalModule.GetPlayer().Speed.Y < -20f && JackalModule.GetPlayer().CollideCheck<SolidTiles>(JackalModule.GetPlayer().TopCenter - Vector2.UnitY) || JackalModule.GetPlayer().CollideCheck<Solid>(JackalModule.GetPlayer().TopCenter - Vector2.UnitY))
			{
				return 0;
			}
			if (JackalModule.GetPlayer().Speed.Y > 20f && JackalModule.GetPlayer().CollideCheck<SolidTiles>(JackalModule.GetPlayer().BottomCenter + Vector2.UnitY) || JackalModule.GetPlayer().CollideCheck<Solid>(JackalModule.GetPlayer().BottomCenter + Vector2.UnitY))
			{
				return 0;
			}
			player.Sprite.Visible = (num != 5);
			player.Hair.Visible = (num != 5);
			return (num == 5) ? JackalModule.customRedBoostState : num;
		}

		public static void End()
		{
			Player player = JackalModule.GetPlayer();
			player.Sprite.Visible = true;
			player.Hair.Visible = true;
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
