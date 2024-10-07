module JackalHelperCryoRefill
using ..Ahorn, Maple

@mapdef Entity "JackalHelper/CryoRefill" CryoRefill(x::Integer, y::Integer, oneUse::Bool=false, RefillDashOnUse::Bool=true, AddDash::Bool=false, Radius::Integer=25)

const placements = Ahorn.PlacementDict(
   "Cryo Refill (Jackal Helper)" => Ahorn.EntityPlacement(
        CryoRefill, 
        "point")
)


const sprite = "objects/refillCryo/idle00"

function getSprite(entity::CryoRefill)
    return sprite
end

function Ahorn.selection(entity::CryoRefill)
    x, y = Ahorn.position(entity)
    sprite = getSprite(entity)

    return Ahorn.getSpriteRectangle(sprite, x, y)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CryoRefill, room::Maple.Room)
    sprite = getSprite(entity)
    Ahorn.drawSprite(ctx, sprite, 0, 0)
end

end
