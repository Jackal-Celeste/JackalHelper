using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.JackalCollabHelper.Entities
{
    [CustomEntity("JackalCollabHelper/CornerGoldenBerry")]
    [RegisterStrawberry(tracked: false, blocksCollection: true)]
    [Tracked]
    class CornerGoldenBerry : Strawberry
    {
        public static void Load()
        {
            On.Celeste.Session.UpdateLevelStartDashes += onUpdateLevelStartDashes;
            On.Celeste.Level.Reload += onLevelReload;
            On.Celeste.Player.ClimbJump += onWhatever;
        }

        public static void Unload()
        {
            On.Celeste.Session.UpdateLevelStartDashes -= onUpdateLevelStartDashes;
            On.Celeste.Level.Reload -= onLevelReload;
            On.Celeste.Player.ClimbJump -= onWhatever;
        }

        private static void onUpdateLevelStartDashes(On.Celeste.Session.orig_UpdateLevelStartDashes orig, Session self)
        {
            orig(self);

            // "commit" the Corner berry state
            TwigModule.Session.CornerBerryFlewAway = TwigModule.Session.CornerBerryWillFlyAway;
        }

        private static void onLevelReload(On.Celeste.Level.orig_Reload orig, Level self)
        {
            if (!self.Completed)
            {
                // "reset" the Corner berry state
                TwigModule.Session.CornerBerryWillFlyAway = TwigModule.Session.CornerBerryFlewAway;
            }
            orig(self);
        }

        private static void onWhatever(On.Celeste.Player.orig_ClimbJump orig, Player self)
        {
            orig(self);
            if ((self.Speed.X > 140f && self.Facing == Facings.Right) || (self.Speed.X < -140f && self.Facing == Facings.Left))
            {
                // that is what you're doing when you detect that the golden berry should fly away.
                TwigModule.Session.CornerBerryWillFlyAway = true;
                self.Scene?.Tracker.GetEntity<CornerGoldenBerry>()?.flyAway?.Invoke();
            }
        }


        private Action flyAway;

        public static Entity Load(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
        {
            // when instanciating the berry, pretend it's a Memorial Text Controller (tm), so that we get a shiny dashless golden berry.
            entityData.Name = "memorialTextController";
            CornerGoldenBerry result = new CornerGoldenBerry(entityData, offset, new EntityID(levelData.Name, entityData.ID));
            entityData.Name = "JackalCollabHelper/CornerGoldenBerry";
            return result;
        }

        public CornerGoldenBerry(EntityData data, Vector2 offset, EntityID id)
            : base(data, offset, id)
        {

            DashListener dashListener = Get<DashListener>();
            if (dashListener != null)
            {
                // prevent the dashless golden from reacting to dash, but save the dash handler somewhere
                // so that we can call it when the player is doing what makes this berry disappear...
                Remove(dashListener);
                flyAway = () => dashListener.OnDash(Vector2.Zero);
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            if (!SceneAs<Level>().Session.StartedFromBeginning || TwigModule.Session.CornerBerryFlewAway)
            {
                // Corner golden is gone :crab: because the player started from a checkpoint or did the forbidden thing.
                RemoveSelf();
            }
        }
    }
}