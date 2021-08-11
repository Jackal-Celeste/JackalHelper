using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.JackalHelper.Entities
{
	[CustomEntity("JackalHelper/BraveBird")]
	[Tracked]
	public class BraveBird : Entity
	{
		private enum States
		{
			Wait,
			Fling,
			Move,
			WaitForLightningClear,
			Leaving
		}

		public static ParticleType P_Feather;

		private Vector2 spriteOffset = new Vector2(0f, 0f);

		private Sprite sprite;

		private States state;

		private Vector2 flingSpeed;

		private Vector2 flingTargetSpeed;

		private float flingAccel;

		private EntityData entityData;

		private SoundSource moveSfx;

		private int segmentIndex = 0;

		public List<Vector2[]> NodeSegments;

		public List<bool> SegmentsWaiting;

		public bool LightningRemoved;

		private DynData<Player> dyn;

		//Customization Variables
		//Path is relative to gameplay folder

		public string spritePath;

		public Vector2 launchSpeed;

		public float launchSpeedX;

		public float launchSpeedY;

		public bool canSkipNode;

		public bool spriteFlip;

		public bool stressedAtLastNode;

		public Color particleColor;

		public float skipDistance;



		public BraveBird(Vector2[] nodes, bool skippable, string spritePath, float launchSpeedX, float launchSpeedY, bool canSkipNode, bool stressedAtLastNode, string particleColor, float skipDistance)
			: base(nodes[0])
		{
			this.spritePath = spritePath; /*need to do*/
			this.launchSpeedX = launchSpeedX;
			this.launchSpeedY = launchSpeedY;
			launchSpeed = new Vector2(launchSpeedX, launchSpeedY);
			spriteFlip = (launchSpeedX >= 0f);
			this.canSkipNode = canSkipNode;
			this.stressedAtLastNode = stressedAtLastNode;
			this.particleColor = Calc.HexToColor(particleColor);
			this.skipDistance = skipDistance;


			sprite = new Sprite(GFX.Game, spritePath);
			sprite.Visible = true;
			sprite.CenterOrigin();
			sprite.Justify = new Vector2(0.5f, 0.5f);
			sprite.AddLoop("hover", "hover", 0.1f, 0, 1, 2, 3, 4, 5);
			sprite.AddLoop("hoverStressed", "hoverStressed", 0.07f, 0, 1, 2, 3, 4, 5);
			sprite.AddLoop("fly", "fly", 0.1f, 3, 0, 1, 2);
			sprite.Add("throw", "throw", 0.08f, 0, 1, 2, 3, 4, 5, 6);

			Add(sprite);

			sprite.FlipX = spriteFlip;
			sprite.Play("hover");
			sprite.Scale.X = -1f;
			sprite.Position = spriteOffset;
			sprite.OnFrameChange = delegate
			{
				BirdNPC.FlapSfxCheck(sprite);
			};
			base.Collider = new Circle(16f);
			Add(new PlayerCollider(OnPlayer));
			Add(moveSfx = new SoundSource());
			NodeSegments = new List<Vector2[]>();
			NodeSegments.Add(nodes);
			SegmentsWaiting = new List<bool>();
			SegmentsWaiting.Add(skippable);
			Add(new TransitionListener
			{
				OnOut = delegate (float t)
				{
					sprite.Color = Color.White * (1f - Calc.Map(t, 0f, 0.4f));
				}
			});
		}

		public BraveBird(EntityData data, Vector2 levelOffset)
			: this(data.NodesWithPosition(levelOffset), data.Bool("waiting"), data.Attr("spritePath", "characters/bird/"), data.Float("launchSpeedX", 380), data.Float("launchSpeedY", -100), data.Bool("canSkipNode", false), data.Bool("stressedAtLastNode", true), data.Attr("particleColor", "639bff"), data.Float("skipDistance", 100))
		{
			entityData = data;
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			List<BraveBird> list = base.Scene.Entities.FindAll<BraveBird>();
			for (int num = list.Count - 1; num >= 0; num--)
			{
				if (list[num].entityData.Level.Name != entityData.Level.Name)
				{
					list.RemoveAt(num);
				}
			}
			list.Sort((BraveBird a, BraveBird b) => Math.Sign(a.X - b.X));
			if (list[0] == this)
			{
				for (int i = 1; i < list.Count; i++)
				{
					NodeSegments.Add(list[i].NodeSegments[0]);
					SegmentsWaiting.Add(list[i].SegmentsWaiting[0]);
					list[i].RemoveSelf();
				}
			}
			if (SegmentsWaiting[0])
			{
				sprite.Play(stressedAtLastNode ? "hoverStressed" : "hover");
				sprite.Scale.X = 1f;
			}
			Player entity = scene.Tracker.GetEntity<Player>();
			if (entity != null && entity.X > base.X)
			{
				RemoveSelf();
			}
		}

		private void Skip()
		{
			state = States.Move;
			Add(new Coroutine(MoveRoutine()));
		}

		private void OnPlayer(Player player)
		{
			if (state == States.Wait && DoFlingBird(player, this))
			{
				JackalModule.Session.lastBird = entityData.ID;
				flingSpeed = player.Speed * 0.4f;
				flingSpeed.Y = 120f;
				flingTargetSpeed = Vector2.Zero;
				flingAccel = 1000f;
				player.Speed = Vector2.Zero;
				state = States.Fling;
				Add(new Coroutine(DoFlingRoutine(player)));
				Audio.Play("event:/new_content/game/10_farewell/bird_throw", base.Center);
			}
		}


		public bool DoFlingBird(Player player, BraveBird bird)
		{
			if (!player.Dead && player.StateMachine.State != JackalModule.braveBirdState)
			{
				player.StateMachine.State = JackalModule.braveBirdState;
				if (player.Holding != null)
				{
					player.Drop();
				}
				return true;
			}
			return false;
		}


		public override void Update()
		{
			base.Update();
			if (state != 0)
			{
				sprite.Position = Calc.Approach(sprite.Position, spriteOffset, 32f * Engine.DeltaTime);
			}
			switch (state)
			{
				case States.Wait:
					{
						Player entity = base.Scene.Tracker.GetEntity<Player>();
						if (canSkipNode)
						{
							if (entity != null && launchSpeed.X >= 0f && entity.X - base.X >= skipDistance)
							{
								Skip();
							}
							else if (entity != null && launchSpeed.X < 0f && base.X - entity.X >= skipDistance)
							{
								Skip();
							}
							else if (SegmentsWaiting[segmentIndex] && LightningRemoved)
							{
								Skip();
							}
						}
						else if (entity != null)
						{
							float scaleFactor = Calc.ClampedMap((entity.Center - Position).Length(), 16f, 64f, 12f, 0f);
							Vector2 value = (entity.Center - Position).SafeNormalize();
							sprite.Position = Calc.Approach(sprite.Position, spriteOffset + value * scaleFactor, 32f * Engine.DeltaTime);
						}
						break;
					}
				case States.Fling:
					if (flingAccel > 0f)
					{
						flingSpeed = Calc.Approach(flingSpeed, flingTargetSpeed, flingAccel * Engine.DeltaTime);
					}
					Position += flingSpeed * Engine.DeltaTime;
					break;
				case States.WaitForLightningClear:
					if (base.Scene.Entities.FindFirst<Lightning>() == null || base.X > (Scene as Level).Bounds.Right)
					{
						sprite.Scale.X = 1f;
						state = States.Leaving;
						Add(new Coroutine(LeaveRoutine()));
					}
					break;
				case States.Move:
					break;
			}
		}

		private IEnumerator DoFlingRoutine(Player player)
		{
			Level level = Scene as Level;
			Vector2 camera = level.Camera.Position;
			Vector2 zoom = player.Position - camera;
			zoom.X = Calc.Clamp(zoom.X, 145f, 215f);
			zoom.Y = Calc.Clamp(zoom.Y, 85f, 95f);
			Add(new Coroutine(level.ZoomTo(zoom, 1.1f, 0.2f)));
			Engine.TimeRate = 0.8f;
			Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
			while (flingSpeed != Vector2.Zero)
			{
				yield return null;
			}
			sprite.Play("throw");
			sprite.Scale.X = 1f;
			flingSpeed = new Vector2(-140f, 140f);
			flingTargetSpeed = Vector2.Zero;
			flingAccel = 1400f;
			yield return 0.1f;
			Celeste.Freeze(0.05f);
			flingTargetSpeed = launchSpeed;
			flingAccel = 6000f;
			yield return 0.1f;
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
			Engine.TimeRate = 1f;
			level.Shake();
			Add(new Coroutine(level.ZoomBack(0.1f)));
			FinishFlingBird();
			flingTargetSpeed = Vector2.Zero;
			flingAccel = 4000f;
			yield return 0.3f;
			Add(new Coroutine(MoveRoutine()));
		}

		private IEnumerator MoveRoutine()
		{
			state = States.Move;
			sprite.Play("fly");
			sprite.Scale.X = 1f;
			moveSfx.Play("event:/new_content/game/10_farewell/bird_relocate");
			for (int nodeIndex = 1; nodeIndex < NodeSegments[segmentIndex].Length - 1; nodeIndex += 2)
			{
				Vector2 from = Position;
				Vector2 anchor = NodeSegments[segmentIndex][nodeIndex];
				Vector2 to = NodeSegments[segmentIndex][nodeIndex + 1];
				yield return MoveOnCurve(from, anchor, to);
			}
			segmentIndex++;
			bool atEnding = segmentIndex >= NodeSegments.Count;
			if (!atEnding)
			{
				Vector2 from2 = Position;
				Vector2 anchor2 = NodeSegments[segmentIndex - 1][NodeSegments[segmentIndex - 1].Length - 1];
				Vector2 to2 = NodeSegments[segmentIndex][0];
				yield return MoveOnCurve(from2, anchor2, to2);
			}
			sprite.Rotation = 0f;
			sprite.Scale = Vector2.One;
			if (atEnding)
			{
				sprite.Play(stressedAtLastNode ? "hoverStressed" : "hover");
				sprite.Scale.X = 1f;
				state = States.WaitForLightningClear;
				yield break;
			}
			if (SegmentsWaiting[segmentIndex])
			{
				sprite.Play(stressedAtLastNode ? "hoverStressed" : "hover");
			}
			else
			{
				sprite.Play("hover");
			}
			sprite.Scale.X = -1f;
			state = States.Wait;
		}

		private IEnumerator LeaveRoutine()
		{
			sprite.Scale.X = 1f;
			sprite.Play("fly");
			Vector2 to = new Vector2((Scene as Level).Bounds.Right + 32, Y);
			yield return MoveOnCurve(Position, (Position + to) * 0.5f - Vector2.UnitY * 12f, to);
			RemoveSelf();
		}

		private IEnumerator MoveOnCurve(Vector2 from, Vector2 anchor, Vector2 to)
		{
			SimpleCurve curve = new SimpleCurve(from, to, anchor);
			float duration = curve.GetLengthParametric(32) / 500f;
			Vector2 was = from;
			for (float t = 0.016f; t <= 1f; t += Engine.DeltaTime / duration)
			{
				Position = curve.GetPoint(t).Floor();
				sprite.Rotation = Calc.Angle(curve.GetPoint(Math.Max(0f, t - 0.05f)), curve.GetPoint(Math.Min(1f, t + 0.05f)));
				sprite.Scale.X = 1.25f;
				sprite.Scale.Y = 0.7f;
				if ((was - Position).Length() > 32f)
				{
					TrailManager.Add(this, particleColor, 1);
					was = Position;
				}
				yield return null;
			}
			Position = to;
		}

		public override void Render()
		{
			base.Render();
		}

		private void DrawLine(Vector2 a, Vector2 anchor, Vector2 b)
		{
			new SimpleCurve(a, b, anchor).Render(Color.Red, 32);
		}


		public static void BraveBirdBegin()
		{
			Player player = JackalModule.GetPlayer();
			player.RefillDash();
			player.RefillStamina();

		}

		public static void BraveBirdEnd()
		{
		}

		public static int BraveBirdUpdate()
		{
			Player player = JackalModule.GetPlayer();
			Level level = JackalModule.GetLevel();

			player.MoveTowardsX(level.Tracker.GetNearestEntity<BraveBird>(player.Position).Position.X, 250f * Engine.DeltaTime);
			player.MoveTowardsY(level.Tracker.GetNearestEntity<BraveBird>(player.Position).Position.Y + 8f + player.Collider.Height, 250f * Engine.DeltaTime);
			return JackalModule.braveBirdState;
		}

		public void FinishFlingBird()
		{
			Player player = JackalModule.GetPlayer();
			dyn = new DynData<Player>(player);
			player.StateMachine.State = 0;
			player.AutoJump = true;
			dyn.Set<int>("forceMoveX", 1);
			dyn.Set<float>("forceMoveXTimer", 0.2f);
			player.Speed = flingSpeed;
			/*
			foreach (BraveBird a in JackalModule.GetLevel().Tracker.GetEntities<BraveBird>())
			{
				if (a.entityData.ID == JackalModule.Session.lastAltBird)
				{
					player.Speed = a.flingSpeed;
				}
			}*/
			dyn.Set<float>("varJumpTimer", 0.2f);
			dyn.Set<float>("varJumpSpeed", player.Speed.Y);
			dyn.Set<bool>("lauched", true);
		}

		public static IEnumerator BraveBirdCoroutine()
		{
			yield break;
		}



	}
}
