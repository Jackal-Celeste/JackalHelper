using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using MonoMod.Utils;
using Microsoft.Xna.Framework.Graphics;

namespace Celeste.Mod.JackalHelper.Triggers
{
    [Tracked]
    [CustomEntity("JackalHelper/ScreenWipeTrigger.cs")]
    public class ScreenWipeTrigger : Trigger
    {
        public FallWipe wipe;
        public Color tint;
        public Image background;
        public float timer = 1f;
        private DynData<FallWipe> dynData;
        public ScreenWipeTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            tint = Calc.HexToColor(data.Attr("color", "ffffff"));
            //wipe = new FallWipe(this.Scene, true);
            Add(background = new Image(GFX.Game["JackalHelper/background"]));
            //tint = Color.Indigo;
            //timer = 0f;


        }

        public override void Update()
        {
            base.Update();
            if (JackalModule.GetLevel() != null)
            {
                //background.Position = JackalModule.GetLevel().Camera.Position;
                background.Visible = true;
                Depth = -12500;
                background.Color = Color.Red * timer;
                timer = 1f;
                //timer = Math.Min(timer, 1f);
            }
            

        }
        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            wipe = new FallWipe(this.Scene, false);
            dynData = new DynData<FallWipe>(wipe);
            for(int i = 0; i < dynData.Get<VertexPositionColor[]>("vertexBuffer").Length; i++)
            {
                dynData.Get<VertexPositionColor[]>("vertexBuffer")[i].Color = tint;
                
            }
            ScreenWipe.WipeColor = tint;
           wipe.Render(this.Scene);
            timer += Engine.DeltaTime;
            
            
        }
        public override void OnStay(Player player)
        {
            base.OnStay(player);
            wipe.Render(this.Scene);
            timer += Engine.DeltaTime;
        }
        public override void OnLeave(Player player)
        {
            ScreenWipe.WipeColor = Color.Black;
            base.OnLeave(player);
            timer = 0f;
        }


    }
}
