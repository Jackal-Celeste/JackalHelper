module JackalHelperGrappleTrigger

using ..Ahorn, Maple

@mapdef Trigger "JackalHelper/GrappleTrigger" GrappleTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16, removeOnLeave::Bool=false)


const placements = Ahorn.PlacementDict(
   "Grappling Hook Trigger (Jackal Helper)" => Ahorn.EntityPlacement(
	  GrappleTrigger,
	  "rectangle"
   )
)


end