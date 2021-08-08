using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using MonoMod.Utils;

namespace Celeste.Mod.JackalHelper.Entities
{
    [CustomEntity("JackalHelper/LongFish")]
    [Tracked]
    public class WaterGeyser : JumpThru
    {
		private readonly int columns;
		public float totalTime = 0f;
		public bool canAdd;
		public float scale;
		public int terms;
		public int speed;
		public int riseRate;
		public int maxSpeed;
		public List<Sprite> bodySprites;



		public WaterGeyser(Vector2 position, int width, float scale, int terms, int speed, int rate, int maxSpeed)
				: base(position, width, safe: true)
		{
			bodySprites = new List<Sprite>();
			columns = width / 8;
			base.Depth = -60;
			totalTime = 0f;
			this.scale = scale;
			this.terms = terms;
			this.speed = speed;
			riseRate = rate;
			this.maxSpeed = maxSpeed;

			
		}

		public WaterGeyser(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Width, data.Int("oscillationScale"), data.Int("oscillationTerms"), data.Int("oscillationSpeed"), data.Int("riseRate"), data.Int("maxRiseSpeed"))

		{
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			//riseArea = new Rectangle((int)Position.X, (int)Position.Y, (int)Width, JackalModule.GetLevel().Bounds.Height);
			SurfaceSoundIndex = 7;
			MTexture mTexture = GFX.Game["objects/longFish/texture"];
			int num = mTexture.Width / 8;
			for (int i = 0; i < columns; i++)
			{
				int x;
				int y;
				if (i == 0)
				{
					x = 0;
					y = ((!CollideCheck<Solid>(Position + new Vector2(-1f, 0f))) ? 1 : 0);
				}
				else if (i == columns - 1)
				{
					x = num - 1;
					y = ((!CollideCheck<Solid>(Position + new Vector2(1f, 0f))) ? 1 : 0);
				}
				else
				{
					x = 1 + Calc.Random.Next(num - 2);
					y = Calc.Random.Choose(0, 1);
				}
				Image image = new Image(mTexture.GetSubtexture(x * 8, y * 8, 8, 8))
				{
					X = i * 8
				};
				Add(image);
			}
			for (int i = 0; i < columns; i += 2)
			{
				Sprite sprite = JackalModule.spriteBank.Create("geyser");
				//sprite.Position = Position;
				sprite.Position.X += (i * 8f);
				sprite.Position.Y += 3f;
				sprite.Visible = true;
				bodySprites.Add(sprite);
				
				Sprite sprite2 = JackalModule.spriteBank.Create("geyser");
				//sprite2.Position = Position;
				sprite2.Position.Y += (35f);
				sprite2.Position.X += (i * 8f);
				
				Sprite sprite3 = JackalModule.spriteBank.Create("geyser");
				
				sprite3.Position.Y += (67f);
				sprite3.Position.X += (i * 8f);

				Sprite sprite4 = JackalModule.spriteBank.Create("geyser");
				sprite4.Position.Y += (99f);
				sprite4.Position.X += (i * 8f);

				Sprite sprite5 = JackalModule.spriteBank.Create("geyser");
				sprite5.Position.Y += (131f);
				sprite5.Position.X += (i * 8f);

				Sprite sprite6 = JackalModule.spriteBank.Create("geyser");
				sprite6.Position.Y += (163f);
				sprite6.Position.X += (i * 8f);

				Sprite sprite7 = JackalModule.spriteBank.Create("geyser");
				sprite7.Position.Y += (195f);
				sprite7.Position.X += (i * 8f);

				Sprite sprite8 = JackalModule.spriteBank.Create("geyser");
				sprite8.Position.Y += (227f);
				sprite8.Position.X += (i * 8f);

				Sprite sprite9 = JackalModule.spriteBank.Create("geyser");
				sprite9.Position.Y += (259f);
				sprite9.Position.X += (i * 8f);

				Sprite sprite10 = JackalModule.spriteBank.Create("geyser");
				sprite10.Position.Y += (291f);
				sprite10.Position.X += (i * 8f);

				bodySprites.Add(sprite2);
				bodySprites.Add(sprite3);
				bodySprites.Add(sprite4);
				bodySprites.Add(sprite5);
				bodySprites.Add(sprite6);
				bodySprites.Add(sprite7);
				bodySprites.Add(sprite8);
				bodySprites.Add(sprite9);
				bodySprites.Add(sprite10);

			}
			foreach (Sprite sprite in bodySprites)
			{
				Add(sprite);
			}
			Depth = 400;
		}
        public override void Update()
        {
			if (JackalModule.GetLevel() != null && JackalModule.GetPlayer() != null)
            {
				if(JackalModule.GetPlayer().Position.X > Position.X && JackalModule.GetPlayer().Position.X < (Position.X + Width) && JackalModule.GetPlayer().Position.Y > Position.Y)
                {
					JackalModule.GetPlayer().Speed.Y -= riseRate;
					JackalModule.GetPlayer().Speed.Y = Math.Max(-maxSpeed, JackalModule.GetPlayer().Speed.Y);
                }
				
            }
			MoveV(fourierStepFunction(terms));
			
            base.Update();
        }

		public float fourierStepFunction(int terms)
        {
			totalTime += Engine.DeltaTime;
			float velocity = 0f;
			/*
			 * We want to follow a step function so our platform oscillates between two nodes.
			 * As the number of terms increases, we can limit our oscillations, but also make said oscillations "faster".
			 * Higher term counts may cause weird momentum.
			 */
			for(int i = 0; i <= terms; i++)
            {
				velocity += getFourierTerm(i);
            }
			return velocity * (scale/80) * (float)(2f / Math.PI);
        }


		public float getFourierTerm(int termIndex)
        {
			return (speed/10f)*(float)Math.Cos((totalTime * speed * 0.05f) + (termIndex * speed * 0.1f * totalTime));
        }

    }
}
