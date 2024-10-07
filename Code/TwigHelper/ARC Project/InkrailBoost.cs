using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace TwigHelper.ARC_Project
{
	public class InkrailBoost
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
			Player player = TwigModule.GetPlayer();
			player.RefillDash();
			player.RefillStamina();
			timer = 0.26f;
			dyn = null;
			totalTime = 0f;
			time = 0;
			//dir = Input.Aim.Value;
			time++;

		}

		public static Vector2 findClosest(Vector2 aim, Inkrail[] n)
        {
			float angle = aim.Angle();
			float minDiff = float.MaxValue;
			Vector2 smol = Vector2.UnitY;
			foreach(Inkrail i in n)
            {
				Vector2 e = i.Position - TwigModule.Session.lastInkrailPos;
				if(Math.Abs(Math.Abs(e.Angle())-Math.Abs(angle)) < minDiff)
                {
					minDiff = Math.Abs(Math.Abs(e.Angle()) - Math.Abs(angle));
					smol = e;
				}
            }
			return smol;
        }

		public static Vector2 GetClosest(Inkrail n, Vector2 aim, Player p)
		{
			Vector2[] proxies = new Vector2[n.neighbors.Count];
			int closestIndex = 0;
			for (int i = 0; i < proxies.Length; i++)
			{
				Vector2 offset = n.neighbors[i].Position - n.Position;
				offset.Normalize();
				offset *= 10;
				proxies[i] = n.Position + offset;
			}
			for (int i = 1; i < proxies.Length; i++)
			{

				float a0 = Vector2.Distance(proxies[closestIndex], aim);
				float a1 = Vector2.Distance(proxies[i], aim);
				if (a1 < a0) closestIndex = i;
			}
			return  n.neighbors[closestIndex].Position - p.Position;
		}

		public static IEnumerator Coroutine()
		{
			yield return 0.25f;
			dir = (Input.Aim.Value == Vector2.Zero ? Vector2.UnitX : Input.Aim.Value);
			dir = GetClosest(TwigModule.Session.lastInkrail, dir, TwigModule.GetPlayer());
			//dir = findClosest(dir, positions);
			dir.Normalize();
			dir *= 2;
			//dir = findClosest(dir, TwigModule.Session.lastInkrail.neighbors);
			Player player = TwigModule.GetPlayer();
			if (dyn == null)
			{
				dyn = new DynData<Player>(player);
			}

			//player.Speed = CorrectDashPrecision(dir) * TwigModule.Session.lastInkrail.launchSpeed * (float)Math.Pow(1 + (double)TwigModule.Session.lastInkrail.decayRate, (double)time);
			_ = player.Speed;
			Vector2 value = Vector2.Zero;
			DynData<Level> i = new DynData<Level>(player.Scene as Level);
			while (true)
			{
				Vector2 v = (player.DashDir = player.Speed);
				dyn.Set("gliderBoostDir", v);
				(player.Scene as Level).DirectionalShake(player.DashDir, 0.2f);
				/*if (player.DashDir.X != 0f && player.DashDir != null)
				{
					player.Facing = (Facings)Math.Sign(player.DashDir.X);
				}*/
				yield return null;

			}
		}

		public static int Update()
		{
			Player player = TwigModule.GetPlayer();


			player.LastBooster = null;
			time++;
			totalTime += Engine.DeltaTime;

			if (timer > 0f)
			{
				timer -= Engine.DeltaTime;
				player.Center = new DynData<Player>(player).Get<Vector2>("boostTarget");
				return TwigModule.InkrailState;
			}
			int num = (int)rdU.Invoke(player, new object[0]);


				if (dir == null)
				{
					dir = Vector2.UnitX * (float)TwigModule.GetPlayer().Facing;
				}
				//dir.Normalize();

				player.Speed = ((dir.X != 0 && dir.Y != 0) ? 1f / (float)Math.Sqrt(2) : 1f) * (Vector2.Zero + /*CorrectDashPrecision(dir)*/ dir * 40f);
				//player.Speed.Rotate( = 

			if (num != 5)
			{
				time = 0;
			}
			if (Input.Jump.Pressed /*TwigModule.Session.lastInkrail.canJump*/)
			{
				TwigModule.GetPlayer().Jump();
				TwigModule.GetPlayer().Speed.Y *= 2f;
				return 0;
			}
			if (Input.Dash.Pressed)
			{
				TwigModule.GetPlayer().StartDash();
				return 2;
			}
			if (TwigModule.GetPlayer().Facing == Facings.Right && TwigModule.GetPlayer().CollideCheck<SolidTiles>(TwigModule.GetPlayer().CenterRight + Vector2.UnitX) || TwigModule.GetPlayer().CollideCheck<Solid>(TwigModule.GetPlayer().CenterRight + Vector2.UnitX))
			{
				return 0;
			}
			if (TwigModule.GetPlayer().Facing == Facings.Left && TwigModule.GetPlayer().CollideCheck<SolidTiles>(TwigModule.GetPlayer().CenterLeft - Vector2.UnitX) || TwigModule.GetPlayer().CollideCheck<Solid>(TwigModule.GetPlayer().CenterLeft - Vector2.UnitX))
			{
				return 0;
			}
			if (TwigModule.GetPlayer().Speed.Y < -20f && TwigModule.GetPlayer().CollideCheck<SolidTiles>(TwigModule.GetPlayer().TopCenter - Vector2.UnitY) || TwigModule.GetPlayer().CollideCheck<Solid>(TwigModule.GetPlayer().TopCenter - Vector2.UnitY))
			{
				return 0;
			}
			if (TwigModule.GetPlayer().Speed.Y > 20f && TwigModule.GetPlayer().CollideCheck<SolidTiles>(TwigModule.GetPlayer().BottomCenter + Vector2.UnitY) || TwigModule.GetPlayer().CollideCheck<Solid>(TwigModule.GetPlayer().BottomCenter + Vector2.UnitY))
			{
				return 0;
			}
			player.Sprite.Visible = (num != 5);
			player.Hair.Visible = (num != 5);
			return (num == 5) ? TwigModule.InkrailState : num;
		}

		public static void End()
		{
			Player player = TwigModule.GetPlayer();
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
