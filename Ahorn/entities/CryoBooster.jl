module JackalHelperCryoBooster

using ..Ahorn, Maple

@mapdef Entity "JackalHelper/CryoBooster" CryoBooster(x::Integer, y::Integer)


const placements = Ahorn.PlacementDict(
   "Cryo Booster (Jackal Helper)" => Ahorn.EntityPlacement(
	  CryoBooster,
	  "point"
   )
)

sprite = "objects/boosterIce/boosterIce05"

function Ahorn.selection(entity::CryoBooster)
    x, y = Ahorn.position(entity)
    return Ahorn.getSpriteRectangle(sprite, x, y)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CryoBooster, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0, 0)

end

