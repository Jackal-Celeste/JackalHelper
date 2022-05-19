module JackalHelperRainbowDecal

using ..Ahorn, Maple

@mapdef Entity "JackalHelper/RainbowDecal" RainbowDecal(x::Integer, y::Integer, directory::String="JackalHelper/cryobotRainbow", wiggle::Bool=true)


const placements = Ahorn.PlacementDict(
   "Grapple Star (WIP) (Jackal Helper)" => Ahorn.EntityPlacement(
	  RainbowDecal,
	  "point"
   )
)

sprite = "JackalHelper/cryobotRainbow"

function Ahorn.selection(entity::RainbowDecal)
    x, y = Ahorn.position(entity)
    return Ahorn.getSpriteRectangle(sprite, x, y)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RainbowDecal, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0, 0)

end

