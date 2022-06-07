module JackalHelperRoundKevin

using ..Ahorn, Maple

@mapdef Entity "JackalHelper/RoundKevin" RoundKevin(x::Integer, y::Integer, controlCoreMode::Bool=true)


const placements = Ahorn.PlacementDict(
   "Core Booster (Jackal Helper)" => Ahorn.EntityPlacement(
	  RoundKevin,
	  "point"
   )
)

sprite = "JackalHelper/coreBooster/coreBoostAhorn"

function Ahorn.selection(entity::RoundKevin)
    x, y = Ahorn.position(entity)
    return Ahorn.getSpriteRectangle(sprite, x, y)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RoundKevin, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0, 0)

end

