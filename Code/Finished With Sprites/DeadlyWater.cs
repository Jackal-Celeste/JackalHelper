using System;
using System.Collections.Generic;
using System.Reflection;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.JackalHelper.Entities
{
	[TrackedAs(typeof(Water))]
	[CustomEntity("JackalHelper/DeadlyWater")]
	public class DeadlyWater : Water
	{
		private Color baseColor;

		private Color surfaceColor;

		private Color fillColor;

		private Color rayTopColor;

		private bool fixedSurfaces;

		private bool visibleOnCamera;

		private List<Surface> emptySurfaces;

		private List<Surface> actualSurfaces;

		private Surface actualTopSurface;

		private Surface dummyTopSurface;

		private Surface actualBottomSurface;

		private Surface dummyBottomSurface;

		private static int horizontalVisiblityBuffer = 48;

		private static int verticalVisiblityBuffer = 48;

		public static FieldInfo fillColorField = typeof(Water).GetField("FillColor", BindingFlags.Static | BindingFlags.Public);

		public static FieldInfo surfaceColorField = typeof(Water).GetField("SurfaceColor", BindingFlags.Static | BindingFlags.Public);

		public static FieldInfo rayTopColorField = typeof(Water).GetField("RayTopColor", BindingFlags.Static | BindingFlags.Public);

		public static FieldInfo fillField = typeof(Water).GetField("fill", BindingFlags.Instance | BindingFlags.NonPublic);

		protected PlayerCollider playerCollider;

		protected Hitbox hitbox;

		protected float currentHeight;

		public KillBoxTrigger killbox;

		public float height;

		public float width;

		public Rectangle rect;

		public DeadlyWater(EntityData data, Vector2 offset)
			: base(data, offset)
		{
			baseColor = ColorHelper.GetColor(data.Attr("color", "#87CEFA"));
			surfaceColor = baseColor;
			fillColor = baseColor * 0.2f;
			rayTopColor = baseColor * 0f;
			fixedSurfaces = false;
			currentHeight = data.Height;
			data.Height = (int)currentHeight;

			killbox = new KillBoxTrigger(data, offset);

			Collidable = false;

			height = data.Height;
			width = data.Width;
			Console.WriteLine(Position);
			Console.WriteLine(killbox.Position);
			hitbox = new Hitbox(data.Width, currentHeight);
			rect = new Rectangle((int)Position.X, (int)Position.Y, data.Width, data.Height);

			Add(playerCollider = new PlayerCollider(OnPlayer, hitbox));
		}


		private void fixSurfaces()
		{
			if (!fixedSurfaces)
			{
				Color origFill = Water.FillColor;
				Color origSurface = Water.SurfaceColor;
				changeColor(fillColorField, origFill, fillColor);
				changeColor(surfaceColorField, origSurface, surfaceColor);
				bool hasTop = Surfaces.Contains(TopSurface);
				bool hasBottom = Surfaces.Contains(BottomSurface);
				Surfaces.Clear();
				if (hasTop)
				{
					TopSurface = new Surface(Position + new Vector2(base.Width / 2f, 8f), new Vector2(0f, -1f), base.Width, base.Height);
					Surfaces.Add(TopSurface);
					actualTopSurface = TopSurface;
					dummyTopSurface = new Surface(Position + new Vector2(base.Width / 2f, 8f), new Vector2(0f, -1f), base.Width, base.Height);
				}
				if (hasBottom)
				{
					BottomSurface = new Surface(Position + new Vector2(base.Width / 2f, base.Height - 8f), new Vector2(0f, 1f), base.Width, base.Height);
					Surfaces.Add(BottomSurface);
					actualBottomSurface = BottomSurface;
					dummyBottomSurface = new Surface(Position + new Vector2(base.Width / 2f, base.Height - 8f), new Vector2(0f, 1f), base.Width, base.Height);
				}
				fixedSurfaces = true;
				actualSurfaces = Surfaces;
				emptySurfaces = new List<Surface>();
				changeColor(fillColorField, fillColor, origFill);
				changeColor(surfaceColorField, surfaceColor, origSurface);
			}
		}

		private void updateSurfaces()
		{
			Surfaces = (visibleOnCamera ? actualSurfaces : emptySurfaces);
			TopSurface = (visibleOnCamera ? actualTopSurface : dummyTopSurface);
			BottomSurface = (visibleOnCamera ? actualBottomSurface : dummyBottomSurface);
			if (!visibleOnCamera)
			{
				dummyTopSurface?.Ripples?.Clear();
				dummyBottomSurface?.Ripples?.Clear();
			}
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);

		}

		private void updateVisiblity(Level level)
		{
			Camera camera = level.Camera;
			bool horizontalCheck = base.X < camera.Right + horizontalVisiblityBuffer && base.X + base.Width > camera.Left - horizontalVisiblityBuffer;
			bool verticalCheck = base.Y < camera.Bottom + verticalVisiblityBuffer && base.Y + base.Height > camera.Top - verticalVisiblityBuffer;
			visibleOnCamera = horizontalCheck && verticalCheck;
		}

		private void changeColor(FieldInfo fieldInfo, Color from, Color to)
		{
			if (from != to)
			{
				fieldInfo.SetValue(null, to);
			}
		}

		public override void Render()
		{
			if (visibleOnCamera)
			{
				Color origFill = Water.FillColor;
				Color origSurface = Water.SurfaceColor;
				changeColor(fillColorField, origFill, fillColor);
				changeColor(surfaceColorField, origSurface, surfaceColor);
				base.Render();
				changeColor(fillColorField, fillColor, origFill);
				changeColor(surfaceColorField, surfaceColor, origSurface);
			}
		}

		public bool moistCheck(Player player)
		{
			bool check = false;
			if (JackalModule.GetLevel() != null)
			{
				//Draw.Rect(rect, Color.Transparent);
				if (player.BottomCenter.X > rect.X && player.BottomCenter.X < rect.X + Width)
				{

					if (player.TopCenter.Y < rect.Y + rect.Height && player.TopCenter.Y > rect.Y)
					{
						return true;
					}
					if (player.BottomCenter.Y < rect.Y + rect.Height && player.BottomCenter.Y > rect.Y)
					{
						return true;
					}
					/*
					else if(player.Collider.BottomCenter.Y < this.Position.Y && player.Collider.TopCenter.Y > this.Position.Y - Height)
					{
						check = true;
					}*/
				}
			}
			return check;
		}


		private void OnPlayer(Player player)
		{
			player.Die(Vector2.Zero);
		}

		public override void Update()
		{
			Level level = base.Scene as Level;
			Color origRayTop = Water.RayTopColor;
			updateVisiblity(level);
			updateSurfaces();
			changeColor(rayTopColorField, origRayTop, rayTopColor);
			base.Update();
			changeColor(rayTopColorField, rayTopColor, origRayTop);
			Player player = JackalModule.GetPlayer();
			if (player != null)
			{

				if (moistCheck(player))
				{
					player.Die(Vector2.Zero);
				}
			}
		}

		public override void Added(Scene scene)
		{
			fixSurfaces();
			base.Added(scene);
		}
	}


	internal class ColorHelper
	{
		private static readonly PropertyInfo[] colorProps = typeof(Color).GetProperties();

		public static Color GetColor(string color)
		{
			PropertyInfo[] array = colorProps;
			foreach (PropertyInfo c in array)
			{
				if (color.Equals(c.Name, StringComparison.OrdinalIgnoreCase))
				{
					return (Color)c.GetValue(default(Color), null);
				}
			}
			try
			{
				return Calc.HexToColor(color.Replace("#", ""));
			}
			catch
			{
			}
			return Color.Transparent;
		}

		public static List<Color> GetColors(string rawColors, char sep = ',')
		{
			List<Color> colors = new List<Color>();
			string[] array = rawColors.Split(sep);
			foreach (string s in array)
			{
				colors.Add(GetColor(s));
			}
			return colors;
		}
	}


	public class KillBoxTrigger : Trigger
	{
		public KillBoxTrigger(EntityData data, Vector2 offset)
			: base(data, offset)
		{
		}//IL_0002: Unknown result type (might be due to invalid IL or missing references)


		public override void OnEnter(Player player)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			base.OnEnter(player);
			if (!SaveData.Instance.Assists.Invincible)
			{
				player.Die((player.Position - Position).SafeNormalize());
			}
		}
	}



}
