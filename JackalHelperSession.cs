using Celeste.Mod;
using Celeste.Mod.JackalHelper.Entities;
using Microsoft.Xna.Framework;

public class JackalHelperSession : EverestModuleSession
{

	public bool hasGrapple { get; set; } = false;

	public bool HasCryoDash { get; set; } = false;
	public bool CryoDashActive { get; set; } = false;
	public float CryoRadius { get; set; } = 25f;

	public bool HasGaleDash { get; set; } = false;
	public bool GaleDashActive { get; set; } = false;

	public bool PowerDashActive { get; set; } = false;
	public bool HasPowerDash { get; set; } = false;

	public float lastBird { get; set; } = 0f;
	public float lastAltBird { get; set; } = 0f;

	public Color color { get; set; } = Color.White;

	public bool dashQueue { get; set; } = false;

	// COLOURSOFNOISE: it's generally a bad idea to store entities in session/save data
	public CustomRedBooster lastBooster { get; set; } = null;
}