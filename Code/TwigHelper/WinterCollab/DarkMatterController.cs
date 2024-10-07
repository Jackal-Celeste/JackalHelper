using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.JackalCollabHelper.Entities
{
    /*
     * Lightning Colors
     * Lightning Final Speed
     * Lightning Speed Growth (x100)
     * Particle Density
     * Lightning Border Colors
     */

    [Tracked]
    [CustomEntity("JackalCollabHelper/DarkMatterController")]
    public class DarkMatterController : Entity
    {
        public float darkMatterAccelerationModifier = 0.6f;
        public float darkMatterFinalSpeedMultiplier = 4.0f;
        public float darkMatterParticleDensity = 96f;
        public string[] darkMatterBaseColors = new string[6];
        public string[] darkMatterLightningColors = new string[6];
        public bool flipX = false;
        public bool flipY = false;

        public DarkMatterController(Vector2 position, float acceleration, float finalMultiplier, float particleDensity, string baseColor1, string baseColor2, string baseColor3, string baseColor4, string baseColor5, string baseColor6, string edgeColor1, string edgeColor2, string edgeColor3, string edgeColor4, string edgeColor5, string edgeColor6, bool flipX, bool flipY) : base(position)
        {
            
            darkMatterAccelerationModifier = acceleration;
            darkMatterParticleDensity = particleDensity;
            darkMatterFinalSpeedMultiplier = finalMultiplier;
            darkMatterBaseColors[0] = baseColor1;
            darkMatterBaseColors[1] = baseColor2;
            darkMatterBaseColors[2] = baseColor3;
            darkMatterBaseColors[3] = baseColor4;
            darkMatterBaseColors[4] = baseColor5;
            darkMatterBaseColors[5] = baseColor6;

            darkMatterLightningColors[0] = edgeColor1;
            darkMatterLightningColors[1] = edgeColor2;
            darkMatterLightningColors[2] = edgeColor3;
            darkMatterLightningColors[3] = edgeColor4;
            darkMatterLightningColors[4] = edgeColor5;
            darkMatterLightningColors[5] = edgeColor6;


        }

        public DarkMatterController(EntityData data, Vector2 offset) : this(data.Position + offset, data.Float("acceleration"), data.Float("finalMultiplier"), data.Float("particleDensity"), data.Attr("baseColor1"), data.Attr("baseColor2"), data.Attr("baseColor3"), data.Attr("baseColor4"), data.Attr("baseColor5"), data.Attr("baseColor6"), data.Attr("edgeColor1"), data.Attr("edgeColor2"), data.Attr("edgeColor3"), data.Attr("edgeColor4"), data.Attr("edgeColor5"), data.Attr("edgeColor6"), data.Bool("flipX"), data.Bool("flipY"))
        {

        }
    }
}
