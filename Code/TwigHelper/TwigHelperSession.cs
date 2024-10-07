using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod;
using Celeste.Mod.TwigHelper.Entities;
using Microsoft.Xna.Framework;
using TwigHelper.ARC_Project;

public class TwigHelperSession : EverestModuleSession
{
	public float frozenXSpeed
	{
		get;
		set;
	} = 0f;

	public float frozenYSpeed
	{
		get;
		set;
	} = 0f;

	public bool inDarkMatter
	{
		get;
		set;
	} = false;

	public bool hasScanner
	{
		get;
		set;
	} = false;

	public bool spaceBoosterFlight
	{
		get;
		set;
	} = false;

	public bool inBoundsAlready
	{
		get;
		set;
	} = false;

	public bool usingSkin
	{
		get;
		set;
	} = false;

	public bool inBoundsAny
	{
		get;
		set;
	} = false;

	public String skinName
	{
		get;
		set;
	} = "";

	public bool CornerBerryWillFlyAway
	{
		get;
		set;
	} = false;

	public bool CornerBerryFlewAway
	{
		get;
		set;
	} = false;
	public bool HasShroomDash
	{
		get;
		set;
	} = false;

	public float lastEdward { get; set; } = 0f;


	public bool ShroomDashActive
	{
		get;
		set;
	} = false;

	public bool ShroomDashTrailActive
	{
		get;
		set;
	} = false;

	public Inkrail lastInkrail { get; set; } = null;
	public Vector2 lastInkrailPos { get; set; } = Vector2.Zero;

	public BossNode currentBossNode { get; set; } = null;

	public bool inDDRZone { get; set; } = false;

}