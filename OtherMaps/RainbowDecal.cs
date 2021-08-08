using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.JackalHelper.Entities
{
    [Tracked]
    [CustomEntity("JackalHelper/RainbowDecal")]
    public class RainbowDecal : Solid
    {
        private Image sprite;
        public float cumulativeTime = 0f;
        //vate Wiggler wiggler;
        public RainbowDecal(Vector2 position, string directory, bool wiggle) : base(position, 8,8f, false)
        {
            Add(sprite = new Image(GFX.Game[directory]));
            sprite.Visible = true;
            Collidable = true;
            sprite.Position -= (4 * Vector2.One);
            Depth = 1000;
            
        }

        public RainbowDecal(EntityData data, Vector2 offset) : this(data.Position + offset, data.Attr("directory"), data.Bool("wiggle"))
        {

        }

        public override void Render()
        {
            base.Render();
            //sprite.Color = JackalModule.Session.color;
        }

        public override void Update()
        {
          //Motion();
            base.Update();
            float distance = JackalModule.GetPlayer() != null ? (float)Vector2.Distance(sprite.Position + 4* Vector2.One, JackalModule.GetPlayer().Position) : 99999f;
            bool alive = false;
            if(JackalModule.GetLevel() != null)
            {
                if(JackalModule.GetLevel().Tracker.GetEntities<GrapplingHook>().Count > 0)
                {
                    foreach(GrapplingHook hook in JackalModule.GetLevel().Tracker.GetEntities<GrapplingHook>())
                    {
                        if (hook.thrown && !hook.grappled)
                        {
                            alive = true;
                        }
                    }
                }
                else{
                    alive = false;
                }
            }

            Collidable = alive;
        }

        public void Motion()
        {


            float sinDeltaX = 0f * (float)Math.Sin(2f * cumulativeTime);
            float sinDeltaY = 0.05f * (float)Math.Sin(2f * cumulativeTime + Math.PI / 2);
            float totalDeltaX = sinDeltaX;
            float totalDeltaY = sinDeltaY;


            X += totalDeltaX;
            Y += totalDeltaY;

            sprite.Position += new Vector2(totalDeltaX, totalDeltaY);

            cumulativeTime += Engine.DeltaTime;


        }
    }
}
