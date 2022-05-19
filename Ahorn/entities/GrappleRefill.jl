module JackalHelperGrappleRefill
using ..Ahorn, Maple

@mapdef Entity "JackalHelper/GrappleRefill" GrappleRefill(x::Integer, y::Integer, oneUse::Bool=false, refillStamina::Bool=true)

const placements = Ahorn.PlacementDict(
   "Grapple Refill (WIP) (Jackal Helper)" => Ahorn.EntityPlacement(
        GrappleRefill, 
        "point")
)


const sprite = "objects/grapplingHook/refill/hookRefill00"

function getSprite(entity::GrappleRefill)
    return sprite
end

function Ahorn.selection(entity::GrappleRefill)
    x, y = Ahorn.position(entity)
    sprite = getSprite(entity)

    return Ahorn.getSpriteRectangle(sprite, x, y)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::GrappleRefill, room::Maple.Room)
    sprite = getSprite(entity)
    Ahorn.drawSprite(ctx, sprite, 0, 0)
end

end
