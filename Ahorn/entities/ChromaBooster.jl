module JackalHelperChromaBooster
using ..Ahorn, Maple

@mapdef Entity "JackalHelper/ChromaBooster" ChromaBooster(x::Integer, y::Integer, neo::Bool=false)

const placements = Ahorn.PlacementDict(
   "Chroma Booster (Jackal Collab Helper)" => Ahorn.EntityPlacement(
        ChromaBooster, 
        "point")
)


const sprite = "objects/boosterBase/boosterBase00.png"

function getSprite(entity::ChromaBooster)
    return sprite
end

function Ahorn.selection(entity::ChromaBooster)
    x, y = Ahorn.position(entity)
    sprite = getSprite(entity)

    return Ahorn.getSpriteRectangle(sprite, x, y)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ChromaBooster, room::Maple.Room)
    sprite = getSprite(entity)
    Ahorn.drawSprite(ctx, sprite, 0, 0)
end

end
