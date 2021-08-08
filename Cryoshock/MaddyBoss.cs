using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using MonoMod.Utils;
using Celeste;
using System.Threading;
using System.Xml.Serialization;
using FMOD;


namespace Celeste.Mod.JackalHelper.Entities
{
    [CustomEntity("JackalHelper/MaddyBoss.cs")]
    public class MaddyBoss : Entity
    {
        public FinalBoss madeline;

        public Level level;

        public Player player;

        private DynData<FinalBoss> madelineInfo;

        public Sprite sprite;

        public int wave = 0;

        public MaddyBoss(EntityData data, Vector2 offset)
        {
            madeline = new FinalBoss(data, offset);
            madeline.Remove(madeline.Get<CameraLocker>());
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(madeline);

            level = SceneAs<Level>();
            player = SceneAs<Level>().Tracker.GetEntity<Player>();



        }
        public override void Update()
        {
            madelineInfo = new DynData<FinalBoss>(madeline);
            int node = madelineInfo.Get<int>("nodeIndex");


            Console.WriteLine(node);

        }



    }
}