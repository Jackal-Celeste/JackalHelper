module JackalHelperPowBlock

using ..Ahorn, Maple

@mapdef Entity "JackalHelper/PowBlock" PowBlock(x::Integer, y::Integer, explodeRange::Integer=64, threeHits::Bool=false)


const placements = Ahorn.PlacementDict(
    "POW Block (Jackal Helper)" => Ahorn.EntityPlacement(
        PowBlock,
        "point",
    )
)


sprite = "objects/powBlock/POW1_1"


function Ahorn.selection(entity::PowBlock)
    x, y = Ahorn.position(entity)
    return Ahorn.getSpriteRectangle(sprite, x, y)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::PowBlock, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0, 0)

end