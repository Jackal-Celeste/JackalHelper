using System;
using System.Collections;
using System.Collections.Generic;
using Celeste;
using Celeste.Mod.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace TwigHelper.ARC_Project
{
	[CustomEntity("TwigHelper/SnakeBlock")]
	[Tracked]
	public class SnakeBlock : Solid
	{
		public enum Directions
		{
			Right,
			Up,
			Left,
			Down
		}

		private enum MovementState
		{
			Idling,
			Moving,
			Breaking
		}




		private const float Accel = 300f;



		public Directions direction;
		public Vector2 directionVector;
		private Vector2 startPosition;
		private MovementState state = MovementState.Idling;



		public float targetSpeed;
		private List<Image> body = new List<Image>();

		private Color fillColor = idleBgFill;
		private float flash;
		private SoundSource moveSfx;

		public bool triggered;

		private static readonly Color idleBgFill = Calc.HexToColor("474070");
		private static readonly Color pressedBgFill = Calc.HexToColor("30b335");
		private static readonly Color breakingBgFill = Calc.HexToColor("cc2541");

		private const float minScrapeSpeed = 10f;
		private bool cornerClipped = false;
		private float cornerClippedTimer = 0f;
		private float cornerClippedResetTime = 0.1f;

		private Vector2 targetOffset = Vector2.Zero;
		private const float regenWaitTime = 1f;

		private Vector2 moveLiftSpeed = Vector2.Zero;


		public Vector2 initPos;
		public Image i;
		public bool inactive = true;
		public int length;
		public bool fast = false;

		public SnakeBlock(Vector2 position, int width, int height, Directions direction, float speed, int length)
			: base(position, width, height, safe: false)
		{
			this.length = Math.Abs(length);
			base.Depth = -11503;
			startPosition = initPos = position;
			this.direction = direction;
			targetSpeed = speed;
			switch (direction)
			{
				case Directions.Right:
					directionVector = Vector2.UnitX;
					break;
				case Directions.Up:
					directionVector = -Vector2.UnitY;
					break;
				case Directions.Left:
					directionVector = -Vector2.UnitX;
					break;
				default:
					directionVector = Vector2.UnitY;
					break;
			}
			Add(new Coroutine(Controller()));
			Add(new LightOcclude(0.5f));
			Add(i = new Image(GFX.Game["objects/snakeBlock/Left/snek_simple2"]));
			fast = targetSpeed > 60;
			if (fast) i.Color = Color.OrangeRed;
		}

		public SnakeBlock(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Width, data.Height, data.Enum("direction", Directions.Left), data.Float("speed", defaultValue: 60f), data.Int("length", defaultValue: 3))
		{
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);

		}

		public void activate()
		{
			triggered = true;
			Remove(i);
			inactive = false;
			Add(i = new Image(GFX.Game["objects/snakeBlock/Left/snek_simple1"]));
			if (fast) i.Color = Color.OrangeRed;
		}

		public SnakeBlockTail tail = null;

		private IEnumerator Controller()
		{
			while (true)
			{
				#region Idle and triggering
				if (!activated)
				{
					triggered = false;
					state = MovementState.Idling;
					while (!triggered && !HasPlayerRider())
					{
						yield return null;
					}
					state = MovementState.Moving;
					tail = new SnakeBlockTail(Position, (int)Width, (int)Height, Speed, this);
					TwigModule.GetLevel().Add(tail);
					StopPlayerRunIntoAnimation = false;


					SnakeBlockBody n = new SnakeBlockBody(Position, (int)Width, (int)Height, this);
					//n.Collidable = false;
					list.Add(n);
					TwigModule.GetLevel().Add(n);
				}
				#endregion

				#region Moving
				while (true)
				{


					Vector2 move = directionVector * targetSpeed * Engine.DeltaTime;

                    if (inactive && triggered)
                    {
						Remove(i);
						Add(i = new Image(GFX.Game["objects/snakeBlock/Left/snek_simple2"]));
						if (fast) i.Color = Color.OrangeRed;
					}

					bool hit = false;
					if (move == Vector2.Zero)
					{
						hit = true;
					}
					else
					{
						if (move.X != 0)
						{
							List<Entity> collidedSolids = CollideAll<Solid>(Position + new Vector2(Math.Sign(move.X), 0));
							if (MoveHCheck(move.X) || collidedSolids.Count != 0)
							{
								hit = true;
								move.X = 0;
								targetOffset.X = 0;
							}
						}
						if (move.Y != 0)
						{
							List<Entity> collidedSolids = CollideAll<Solid>(Position + new Vector2(0, Math.Sign(move.Y)));
							if (MoveVCheck(move.Y) || collidedSolids.Count != 0)
							{
								hit = directionVector.X == 0 || hit;
								move.Y = 0;
								targetOffset.Y = 0;
							}
							else
							{
								hit = false;
							}

							if (directionVector.Y > 0 && Top > (float)(SceneAs<Level>().Bounds.Bottom + 32))
							{
								hit = true;
							}
						}
					}

					if (move.X == 0)
					{
						LiftSpeed.X = 0;
					}
					if (move.Y == 0)
					{
						LiftSpeed.Y = 0;
					}




					moveLiftSpeed = (hit ? Vector2.Zero : LiftSpeed);
					Level level = Scene as Level;
					if (Left <= (float)level.Bounds.Left || Top <= (float)level.Bounds.Top || Right >= (float)level.Bounds.Right)
					{
						RemoveSelf();
						break;
					}
					#region Reproducing

					if (Math.Abs(Position.X - initPos.X) >= Width || Math.Abs(Position.Y - initPos.Y) >= Height)
					{
						SnakeBlockBody n = new SnakeBlockBody(Position, (int)Width, (int)Height, this);
						//list[list.Count - 1].Collidable = true;
						//n.Collidable = false;
						list.Add(n);
						TwigModule.GetLevel().Add(n);
						initPos = Position;
						if (list.Count > length)
						{
							SnakeBlockBody m = list[0];
							list.RemoveAt(0);
							if (!tailMade)
							{
								tail.start = true;
								tailMade = true;
							}
							m.RemoveSelf();

						}
					}


					#endregion
					yield return null;
				}

				#endregion

				#region Breaking and reforming
				activated = true;
				#endregion
			}
		}

		public List<SnakeBlockBody> list = new List<SnakeBlockBody>();
		public bool activated = false;
		public bool tailMade = false;

		public override void Update()
		{
			List<Entity> snakeBlocks = null;
			List<Entity> snakeTail = null;
			//Collidable = true;
			if (TwigModule.GetLevel() != null)
			{
				snakeBlocks = TwigModule.GetLevel().Tracker.GetEntities<SnakeBlockBody>();
				snakeBlocks.ForEach(entity => entity.Collidable = false);

			}
			base.Update();

			LiftSpeed = moveLiftSpeed;
			if (moveSfx != null && moveSfx.Playing)
			{
				float num = (directionVector * new Vector2(-1f, 1f)).Angle();
				int num2 = (int)Math.Floor((0f - num + (float)Math.PI * 2f) % ((float)Math.PI * 2f) / ((float)Math.PI * 2f) * 8f + 0.5f);
				moveSfx.Param("arrow_influence", num2 + 1);

				if (cornerClipped)
				{
					cornerClippedTimer -= Engine.DeltaTime;
					if (cornerClippedTimer <= 0)
					{
						cornerClipped = false;
					}
				}
			}
			if (list.Count > 0)
			{
				list[0].large = true;
				if (list.Count > 1)
				{
					for (int i = 1; i < list.Count; i++)
					{
						list[i].large = true;
					}

				}
			}

			if (TwigModule.GetLevel() != null)
			{
				snakeBlocks.ForEach(entity => entity.Collidable = true);
			}
			if (fast) i.Color = Color.OrangeRed;
		}

		public void changeDirection(bool left)
		{
			
			directionVector = directionVector.Rotate((float)(Math.PI / 2* (left ? 1:-1)));
			Player player = TwigModule.GetPlayer();
			if (directionVector.Y > 0 && HasPlayerOnTop())
			{
				player.Position.Y -= 1f;
			}
			else if(directionVector.X > 0 && HasPlayerClimbing() && player?.Facing == Facings.Right)
            {
				player.Position.X -= 1f;
            }
			else if (directionVector.X < 0 && HasPlayerClimbing() && player?.Facing == Facings.Left)
			{
				player.Position.X += 1f;
			}

		}



		private bool MoveHCheck(float move)
		{
			if (MoveHCollideSolids(move, thruDashBlocks: false))
			{
				if (!cornerClipped)
				{
					for (int i = 1; i <= 3; i++)
					{
						for (int num = 1; num >= -1; num -= 2)
						{
							Vector2 value = new Vector2(Math.Sign(move), i * num);
							if (!CollideCheck<Solid>(Position + value))
							{
								int offset = i * num;
								MoveVExact(offset);
								if (targetOffset.Y != 0)
								{
									targetOffset.Y -= offset;
								}
								MoveHExact(Math.Sign(move));
								cornerClipped = true;
								cornerClippedTimer = cornerClippedResetTime;
								return false;
							}
						}
					}
				}
				return true;
			}
			return false;
		}

		private bool MoveVCheck(float move)
		{
			if (MoveVCollideSolids(move, thruDashBlocks: false))
			{
				if (!cornerClipped)
				{
					for (int j = 1; j <= 3; j++)
					{
						for (int num2 = 1; num2 >= -1; num2 -= 2)
						{
							Vector2 value2 = new Vector2(j * num2, Math.Sign(move));
							if (!CollideCheck<Solid>(Position + value2))
							{
								int offset = j * num2;
								MoveHExact(offset);
								if (targetOffset.X != 0)
								{
									targetOffset.X -= offset;
								}
								MoveVExact(Math.Sign(move));
								cornerClipped = true;
								cornerClippedTimer = cornerClippedResetTime;
								return false;
							}
						}
					}
				}
				return true;
			}
			return false;
		}
	}
}

