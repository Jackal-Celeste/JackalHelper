module JackalHelperStar
using ..Ahorn, Maple

@mapdef Entity "JackalHelper/StarRefill" JackalStar(x::Integer, y::Integer, oneUse::Bool=false, refillDash::Bool=true, refillStamina::Bool=true, time::Number=1.25, flag::String="")

const placements = Ahorn.PlacementDict(
   "Timed Flag Refill (Jackal Helper)" => Ahorn.EntityPlacement(
        JackalStar, 
        "point")
)


const sprite = "objects/refillTwo/idle00"

function getSprite(entity::JackalStar)
    return sprite
end

function Ahorn.selection(entity::JackalStar)
    x, y = Ahorn.position(entity)
    sprite = getSprite(entity)

    return Ahorn.getSpriteRectangle(sprite, x, y)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::JackalStar, room::Maple.Room)
    sprite = getSprite(entity)
    Ahorn.drawSprite(ctx, sprite, 0, 0)
end

end
