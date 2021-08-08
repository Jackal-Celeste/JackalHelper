using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod;
using Celeste.Mod.JackalHelper.Entities;
using Microsoft.Xna.Framework;

public class JackalHelperSession : EverestModuleSession
{

	public bool hasGrapple
	{
		get;
		set;
	} = false;
	public bool HasCryoDash
	{
		get;
		set;
	} = false;


	public bool CryoDashActive
	{
		get;
		set;
	} = false;


	public bool HasGaleDash
	{
		get;
		set;
	} = false;


	public bool PowerDashActive
	{
		get;
		set;
	} = false;

	public bool HasPowerDash
	{
		get;
		set;
	} = false;


	public bool GaleDashActive
	{
		get;
		set;
	} = false;

	public float CryoRadius
	{
		get;
		set;
	} = 25f;

	public float lastBird
	{
		get;
		set;
	} = 0f;


	public float lastAltBird
	{
		get;
		set;
	} = 0f;

	public Color color
	{
		get;
		set;
	} = Color.White;

	public bool dashQueue
	{
		get;
		set;
	} = false;

	public CustomRedBooster lastBooster
	{
		get;
		set;
	} = null;

	public BouncyBooster lastBouncyBooster
	{
		get;
		set;
	} = null;
}