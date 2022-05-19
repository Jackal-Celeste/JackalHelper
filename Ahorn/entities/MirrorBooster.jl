module JackalHelperMirrorBoost

using ..Ahorn, Maple

@mapdef Entity "JackalHelper/RoundKevin2" MirrorBoost(x::Integer, y::Integer)


const placements = Ahorn.PlacementDict(
   "Core Booster (Mirror)(Jackal Helper)" => Ahorn.EntityPlacement(
	  MirrorBoost,
	  "point"
   )
)

sprite = "JackalHelper/coreBooster/coreBoostAhornAlt"

function Ahorn.selection(entity::MirrorBoost)
    x, y = Ahorn.position(entity)
    return Ahorn.getSpriteRectangle(sprite, x, y)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MirrorBoost, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0, 0)

end

