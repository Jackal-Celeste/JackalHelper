module JackalHelperStaminaLockTrigger

using ..Ahorn, Maple

@mapdef Trigger "JackalHelper/StaminaLockTrigger" StaminaLockTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16)


const placements = Ahorn.PlacementDict(
   "Stamina Lock Trigger (Jackal Helper)" => Ahorn.EntityPlacement(
	  StaminaLockTrigger,
	  "rectangle"
   )
)


end