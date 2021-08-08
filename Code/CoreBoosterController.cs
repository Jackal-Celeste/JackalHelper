using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using MonoMod.Utils;
using Celeste;
using System.Threading;
using System.Xml.Serialization;
using Celeste.Mod.JackalHelper.Entities;

namespace Celeste.Mod.JackalHelper.Entities
{
    [CustomEntity("JackalHelper/RoundKevin")]
    public class CoreBoosterController : Entity
    {
        public CoreBooster booster;

        public bool red;

        public Scene scene;

        public bool hasLaunched = false;

        public bool firstLaunch = true;

        public Session.CoreModes mode;

        private DynData<Booster> dyn;

        public Vector2 playerPosition;

        public bool redo;

        public bool controlCoreMode;
        

        public CoreBoosterController(Vector2 position, bool controlCoreMode)
            : base(position)
        {
            this.controlCoreMode = controlCoreMode;
        }

        public CoreBoosterController(EntityData data, Vector2 offset)
            : this(data.Position, data.Bool("controlCoreMode", defaultValue: true))
        {
        }

        public override void Awake(Scene scene)
        {
            if (JackalModule.GetLevel() != null)
            {
                redo = false;
                if (firstLaunch)
                {
                    if (JackalModule.GetLevel().CoreMode == Session.CoreModes.None)
                    {
                        JackalModule.GetLevel().CoreMode = Session.CoreModes.Hot;
                    }
                    firstLaunch = false;
                }
                mode = JackalModule.GetLevel().CoreMode;
                red = (JackalModule.GetLevel().CoreMode == Session.CoreModes.Hot || JackalModule.GetLevel().CoreMode == Session.CoreModes.None);
                JackalModule.GetLevel().Add(booster = new CoreBooster(Position, red, controlCoreMode));
                booster.Position = this.Position + JackalModule.GetLevel().LevelOffset;
                    dyn = new DynData<Booster>(booster);
                    dyn.Get<Sprite>("sprite").Play(booster.canChange ? "swap" : "swapSame");
                
                Audio.Play(red ? "event:/game/05_mirror_temple/redbooster_reappear" : "event:/game/04_cliffside/greenbooster_reappear", Position);
            }
            else
            {
                redo = true;
            }
        }


        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            redo = true;
        }

        public override void Update()
        {
            if (redo)
            {
                if (JackalModule.GetLevel() != null)
                {
                    redo = false;
                    if (firstLaunch)
                    {
                        if (JackalModule.GetLevel().CoreMode == Session.CoreModes.None)
                        {
                            JackalModule.GetLevel().CoreMode = Session.CoreModes.Hot;
                        }
                        firstLaunch = false;
                    }
                    mode = JackalModule.GetLevel().CoreMode;
                    red = (JackalModule.GetLevel().CoreMode == Session.CoreModes.Hot || JackalModule.GetLevel().CoreMode == Session.CoreModes.None);
                    JackalModule.GetLevel().Add(booster = new CoreBooster(Position, red, controlCoreMode));
                    dyn = new DynData<Booster>(booster);
                    dyn.Get<Sprite>("sprite").Play("swap");
                    Audio.Play(red ? "event:/game/05_mirror_temple/redbooster_reappear" : "event:/game/04_cliffside/greenbooster_reappear", Position);
                }
            }
            else
            {
                if (JackalModule.GetPlayer() != null)
                {

                    StateCheck(JackalModule.GetPlayer(), booster);
                }
                if (JackalModule.GetLevel() != null)
                {
                    mode = JackalModule.GetLevel().CoreMode;
                }

            }
        }


        public void Destroy(CoreBooster booster)
        {
            dyn = new DynData<Booster>(booster);
            //sprite.Visible = false;
            Remove(dyn.Get<Sprite>("sprite"));
            booster.RemoveSelf();
            if (booster.canChange)
            {
                JackalModule.GetLevel().CoreMode = (red ? Session.CoreModes.Cold : Session.CoreModes.Hot);
            }
            Awake(this.Scene);
        }

        public void StateCheck(Player player, CoreBooster booster)
        {
            if (player.LastBooster is CoreBooster || player.LastBooster == null)
            {

                    if (player.StateMachine.State == 4)
                    {
                        hasLaunched = true;

                        playerPosition = player.Position;

                    }
                    if ((player.StateMachine.State != 4 && player.StateMachine.State != 5) && hasLaunched && (Vector2.Distance(player.Position, player.LastBooster.Position) > 28f))
                    {
                        hasLaunched = false;
                        Destroy(booster);
                    }
                    else if (mode != JackalModule.GetLevel().CoreMode)
                    {
                        hasLaunched = false;
                        Destroy(booster);
                    }
                }
                
            }
        }
    }


