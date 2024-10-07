module JackalHelperPyroBooster

using ..Ahorn, Maple

@mapdef Entity "JackalHelper/PyroBoosterObject" PyroBooster(x::Integer, y::Integer)


const placements = Ahorn.PlacementDict(
   "Pyro Booster (Jackal Helper)" => Ahorn.EntityPlacement(
	  PyroBooster,
	  "point"
   )
)

sprite = "objects/boosterLava/BoosterPyro1"

function Ahorn.selection(entity::PyroBooster)
    x, y = Ahorn.position(entity)
    return Ahorn.getSpriteRectangle(sprite, x, y)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::PyroBooster, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0, 0)

end

