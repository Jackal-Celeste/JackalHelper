using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.TwigHelper;
using Celeste.Mod.Entities;

namespace TwigHelper.ARC_Project
{
    [Tracked]
    [CustomEntity("TwigHelper/FazerBlaster")]
    public class FazerBlaster : Entity
    {
        [Pooled]
        private class RecoverBlast : Entity
        {
            private Sprite sprite;

            public override void Added(Scene scene)
            {
                base.Added(scene);
                base.Depth = -199;
                if (sprite == null)
                {
                    Add(sprite = GFX.SpriteBank.Create("seekerShockWave"));
                    sprite.OnLastFrame = delegate
                    {
                        RemoveSelf();
                    };
                }
                sprite.Play("shockwave", restart: true);
            }

            public static void Spawn(Vector2 position)
            {
                RecoverBlast recoverBlast = Engine.Pooler.Create<RecoverBlast>();
                recoverBlast.Position = position;
                Engine.Scene.Add(recoverBlast);
            }
        }
        public float distance;
        public Player p;
        public Entity e;
        public Image i;
        public bool canHit = false;
        public FazerBlaster(Vector2 position, float distance) : base(position)
        {
            Depth = -5000;
            this.distance = Math.Abs(distance);
            Add(i = new Image(GFX.Game["objects/TwigHelper/reticle"]));
            i.Visible = false;
            i.JustifyOrigin(0.5f, 0.5f);
        }
        public FazerBlaster(EntityData data, Vector2 offset) : this(data.Position + offset, data.Float("distance", defaultValue: 64f)) { }
        public override void Update()
        {
            p = TwigModule.GetPlayer();
            e = null;
            if (p != null)
            {
                
                Seeker s = TwigModule.GetLevel()?.Tracker.GetNearestEntity<Seeker>(p.Position);
                TouchSwitch t = TwigModule.GetLevel()?.Tracker.GetNearestEntity<TouchSwitch>(p.Position);
                if (s == null && t != null) e = t;
                else if (s != null && t == null) e = s;
                else if (s != null && t != null)
                {
                    if (Math.Min(Vector2.Distance(s.Position, p.Position), Vector2.Distance(t.Position, p.Position)) == Vector2.Distance(s.Position, p.Position))
                    {
                        //s is closer
                        e = s;
                    }
                    else { e = t; }
                }
                if (e != null && p != null && Vector2.Distance(e.Position, p.Position) <= distance)
                {
                    canHit = true;
                    if (TwigModule.Settings.LockOnFireKey.Pressed)
                    {
                        if (e is Seeker)
                        {
                            seekerKill(s);
                        }
                        else if (e is TouchSwitch)
                        {
                            (e as TouchSwitch).TurnOn();
                        }
                    }
                }
                else
                {
                    canHit = false;
                }
            }
            base.Update();
        }
        public override void Render()
        {
            base.Render();
            if (e != null && p != null)
            {
                if (canHit)
                {
                    i.Visible = true;
                    i.RenderPosition = e.Position;
                    i.Rotation += Engine.DeltaTime;
                }
                else
                {
                    i.Visible = false;
                }
            }
            else
            {
                i.Visible = false;
            }
        }
        public void seekerKill(Seeker s)
        {
            RecoverBlast.Spawn(s.Position);
            Entity entity = new Entity(s.Position);
            DeathEffect component = new DeathEffect(Color.HotPink, s.Center - s.Position)
            {
                OnEnd = delegate
                {
                    entity.RemoveSelf();
                }
            };
            entity.Add(component);
            entity.Depth = -1000000;
            base.Scene.Add(entity);
            Audio.Play("event:/game/05_mirror_temple/seeker_death", Position);
            s.RemoveSelf();
        }
    }
}
