module JackalHelperStopwatch
using ..Ahorn, Maple

@mapdef Entity "JackalHelper/TracerRefill" JackalStopwatch(x::Integer, y::Integer, oneUse::Bool=false, RefillDashOnUse::Bool=true, time::Number=1.25)

const placements = Ahorn.PlacementDict(
   "Stopwatch Refill (Jackal Helper)" => Ahorn.EntityPlacement(
        JackalStopwatch, 
        "point")
)


const sprite = "objects/stopwatch/idle00"

function getSprite(entity::JackalStopwatch)
    return sprite
end

function Ahorn.selection(entity::JackalStopwatch)
    x, y = Ahorn.position(entity)
    sprite = getSprite(entity)

    return Ahorn.getSpriteRectangle(sprite, x, y)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::JackalStopwatch, room::Maple.Room)
    sprite = getSprite(entity)
    Ahorn.drawSprite(ctx, sprite, 0, 0)
end

end
