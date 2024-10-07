using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace TwigHelper.ARC_Project
{
    [CustomEntity("TwigHelper/AutoRun")]
    public class AutoRunTrigger : Trigger
    {
        public bool right = true;
        public readonly float swapTimer = 0.2f;
        public float timer = 0f;
        public AutoRunTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {

        }
        public override void OnEnter(Player player)
        {
            right = true;
            base.OnEnter(player);
        }
        public override void OnStay(Player player)
        {
            if (right)
            {
                player.Facing = Facings.Right;
            }
            else
            {
                player.Facing = Facings.Left;
            }
            base.OnStay(player);
            if (right) 
            { 
                player.DummyMoving = true;
                player.Speed.X = Calc.Approach(player.Speed.X, 100f, 8000f * Engine.DeltaTime);
                player.Facing = Facings.Right;
                if (Input.Jump.Pressed && player.CollideCheck<Solid>(player.CenterRight + new Vector2(-1f, 0f)) && !Input.GrabCheck) right = false;
                else if (player.CollideCheck<Solid>(player.CenterRight + new Vector2(-1f, 0f)) && player.OnGround() && !Input.GrabCheck) right = false;
                player.DummyMoving = false;
            }
            else
            {
                player.DummyMoving = true;
                player.Speed.X = Calc.Approach(player.Speed.X, -100f, 8000f * Engine.DeltaTime);
                player.Facing = Facings.Left;
                if (Input.Jump.Pressed && player.CollideCheck<Solid>(player.CenterLeft + new Vector2(1f, 0f)) && !Input.GrabCheck) right = true;
                else if (player.CollideCheck<Solid>(player.CenterLeft + new Vector2(1f, 0f)) && player.OnGround() && !Input.GrabCheck) right = true;
                player.DummyMoving = false;
            }
        }
    }
}
