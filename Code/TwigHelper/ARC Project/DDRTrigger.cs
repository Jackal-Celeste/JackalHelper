using System;
using System.Collections;
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
    [Tracked]
    [CustomEntity("TwigHelper/DDRTrigger")]
    public class DDRTrigger : Trigger
    {
        public string[] inputs;
        public string flag;
        public Directions[] dirs;
        public int index = -1;
        public bool disabled;
        public enum Directions
        {
            U,
            UL,
            L,
            DL,
            D,
            DR,
            R,
            UR
        };
        public DDRTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            this.inputs = data.Attr("inputs", defaultValue:"").Split(',');
            this.flag = data.Attr("flag", defaultValue: "sample_DDR");
            dirs = new Directions[this.inputs.Length];
            for(int i = 0; i < dirs.Length; i++)
            {
                switch (this.inputs[i].ToString())
                {
                    case "U":
                        dirs[i] = (Directions)0;
                        break;
                    case "UL":
                        dirs[i] = (Directions)1;
                        break;
                    case "L":
                        dirs[i] = (Directions)2;
                        break;
                    case "DL":
                        dirs[i] = (Directions)3;
                        break;
                    case "D":
                        dirs[i] = (Directions)4;
                        break;
                    case "DR":
                        dirs[i] = (Directions)5;
                        break;
                    case "R":
                        dirs[i] = (Directions)6;
                        break;
                    case "UR":
                        dirs[i] = (Directions)7;
                        break;
                    default:
                        dirs[i] = (Directions)0;
                        break;
                }
            }
            TwigModule.Session.inDDRZone = false;
        }

        public override void OnEnter(Player player)
        {
            index = 0;
            base.OnEnter(player);
        }
        public override void OnLeave(Player player)
        {
            index = -1;
            base.OnLeave(player);
            TwigModule.Session.inDDRZone = false;
        }

        public IEnumerator AwardWinner()
        {
            yield return 2f;
            TwigModule.GetLevel()?.Session.SetFlag(flag, true);
            index = -1;
        }
        public override void OnStay(Player player)
        {
            TwigModule.Session.inDDRZone = false;
            if (index >= dirs.Length)
            {
                disabled = true;
                player.Add(new Coroutine(AwardWinner()));
            }
            Player p = TwigModule.GetPlayer();
            if (index >= 0 && index < dirs.Length)
            {
                //run code to check for 
            }
            base.OnStay(player);
        }

    }
}
