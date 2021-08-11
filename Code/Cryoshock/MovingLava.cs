using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using MonoMod.Utils;

namespace Celeste.Mod.JackalHelper.Entities
{

	[CustomEntity("JackalHelper/MovingLava")]
	public class MovingLava : FireBarrier
	{
		private DynData<FireBarrier> solidData;
		public float linSpdY = 0f;
		public float linSpdX = 0f;
		public float sineAmpY = 0f;
		public float sineFreqY = 0f;
		public float sineAmpX = 0f;
		public float sineFreqX = 0f;
		public float flaglinSpdY = 0f;
		public float flaglinSpdX = 0f;
		public float flagsineAmpY = 0f;
		public float flagsineFreqY = 0f;
		public float flagsineAmpX = 0f;
		public float flagsineFreqX = 0f;
		public float cumulativeTime = 0f;
		public string flag = "";
		public Sprite mask;
		public Vector2 maskOffset = new Vector2(350f, 0f);
		public string startFlag = "";

		public MovingLava(Vector2 position, float width, float height, float linSpdY, float linSpdX, float sineAmpY, float sineFreqY, float sineAmpX, float sineFreqX, float linSpdYFlag, float linSpdXFlag, float sineAmpYFlag, float sineFreqYFlag, float sineAmpXFlag, float sineFreqXFlag, string flag, string startFlag) : base(position, width, height)
		{
			Depth = -1000000;
			this.linSpdY = linSpdY;
			this.linSpdX = linSpdX;
			this.sineAmpY = sineAmpY;
			this.sineFreqY = sineFreqY;
			this.sineAmpX = sineAmpX;
			this.sineFreqX = sineFreqX;
			this.flaglinSpdY = linSpdYFlag;
			this.flaglinSpdX = linSpdXFlag;
			this.flagsineAmpY = sineAmpYFlag;
			this.flagsineFreqY = sineFreqYFlag;
			this.flagsineAmpX = sineAmpXFlag;
			this.flagsineFreqX = sineFreqXFlag;
			this.flag = flag;
			this.startFlag = startFlag;
            mask = JackalModule.spriteBank.Create("voidTear");
			mask.FlipX = false;
			maskOffset.X *= -1f;
			Add(mask);
			mask.Play("boost");
			if(sineAmpX == 0 && linSpdX == 0)
            {
				mask.Rotation = (float)Math.PI * 3 / 2;
				maskOffset = new Vector2(0, 352);

            }
            else
            {
				maskOffset = new Vector2(-350f + Width, 0);
            }
			Depth = -1500000;
			mask.Visible = false;
		}

		public MovingLava(EntityData data, Vector2 offset) : this(data.Position + offset, data.Width, data.Height, data.Float("linSpeedY", defaultValue: 0f), data.Float("linSpeedX", defaultValue: 0f), data.Float("sineAmplitudeY", defaultValue: 0f), data.Float("sineFrequencyY", defaultValue: 0f), data.Float("sineAmplitudeX", defaultValue: 0f), data.Float("sineFrequencyX", defaultValue: 0f), data.Float("flagLinSpeedY", defaultValue: 0f), data.Float("flagLinSpeedX", defaultValue: 0f), data.Float("flagSineAmplitudeY", defaultValue: 0f), data.Float("flagSineFrequencyY", defaultValue: 0f), data.Float("flagSineAmplitudeX", defaultValue: 0f), data.Float("flagSineFrequencyX", defaultValue: 0f), data.Attr("flag", ""), data.Attr("flagToStart", ""))
		{
		}

		public override void Added(Scene scene)
		{
			sineAmpX *= 0.25f;
			sineAmpY *= 0.25f;
			base.Added(scene);
			Depth = -1500000;
			Collidable = true;
		}

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
        }


        public override void Update()
		{
			if (JackalModule.GetLevel() != null && JackalModule.GetPlayer() != null)
			{
                if (JackalModule.GetLevel().Session.GetFlag(startFlag))
                {
					Motion();
				}
			}
			if (JackalModule.GetLevel() != null)
            {
				solidData = new DynData<FireBarrier>(this);
				solidData.Get<LavaRect>("Lava").CenterColor = Color.Black * 0f;
				solidData.Get<LavaRect>("Lava").SurfaceColor = Color.Black * 0f;
				solidData.Get<LavaRect>("Lava").EdgeColor = Color.Black * 0f;
            }

			Collidable = true;
			mask.Position = this.Position + maskOffset;
			mask.Visible = false;
			mask.FlipX = false;
			mask.Play("boost");
			//mask.Active = true;
			


			base.Update();
		}

        public override void Render()
        {
			
				mask.Position = Position + maskOffset;
				mask.RenderPosition = Position + maskOffset;
			mask.Visible = true;
			mask.Play("boost");
			base.Render();
		}


        public void Motion()
		{

			float linDeltaX = (JackalModule.GetLevel().Session.GetFlag(flag) ? flaglinSpdX : linSpdX) * Engine.DeltaTime;
			float linDeltaY = (JackalModule.GetLevel().Session.GetFlag(flag) ? flaglinSpdY : linSpdY) * Engine.DeltaTime;
			float sinDeltaX = (JackalModule.GetLevel().Session.GetFlag(flag) ? flagsineAmpX : sineAmpX * (float)Math.Sin(JackalModule.GetLevel().Session.GetFlag(flag) ? flagsineFreqX : sineFreqX * cumulativeTime));
			float sinDeltaY = (JackalModule.GetLevel().Session.GetFlag(flag) ? flagsineAmpY : sineAmpY * (float)Math.Sin((JackalModule.GetLevel().Session.GetFlag(flag) ? flagsineFreqY : sineFreqY * cumulativeTime) + Math.PI / 2));
			float totalDeltaX = linDeltaX + sinDeltaX;
			float totalDeltaY = linDeltaY + sinDeltaY;


			X += totalDeltaX;
			Y += totalDeltaY;
			solidData = new DynData<FireBarrier>(this);
			solidData.Get<Solid>("solid").Speed.X = 0;
			solidData.Get<Solid>("solid").Speed.Y = 0;
			solidData.Get<Solid>("solid").MoveH(totalDeltaX);
			solidData.Get<Solid>("solid").MoveV(totalDeltaY);

			cumulativeTime += Engine.DeltaTime;


		}
	}
}
