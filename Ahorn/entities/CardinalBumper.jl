module JackalCardinalBumper
using ..Ahorn, Maple
@mapdef Entity "JackalHelper/CardinalBumper" CardinalBumper(x::Integer, y::Integer, alwaysBumperBoost::Bool=false, wobble::Bool=false, spriteDirectory::String="bumperCardinal", overrideString::String="")
const placements = Ahorn.PlacementDict(
    "Cardinal Direction Bumper (Jackal Helper)" => Ahorn.EntityPlacement(
        CardinalBumper
    )
)

Ahorn.nodeLimits(entity::CardinalBumper) = 0, -1

sprite = "objects/bumperCoral/idle22.png"

function Ahorn.selection(entity::CardinalBumper)
    nodes = get(entity.data, "nodes", ())
    x, y = Ahorn.position(entity)

    res = Ahorn.Rectangle[Ahorn.getSpriteRectangle(sprite, x, y)]
    
    for node in nodes
        nx, ny = Int.(node)

        push!(res, Ahorn.getSpriteRectangle(sprite, nx, ny))
    end

    return res
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::CardinalBumper)
    px, py = Ahorn.position(entity)

    for node in get(entity.data, "nodes", ())
        nx, ny = Int.(node)

        Ahorn.drawArrow(ctx, px, py, nx, ny, Ahorn.colors.selection_selected_fc, headLength=6)
        Ahorn.drawSprite(ctx, sprite, nx, ny)

        px, py = nx, ny
    end
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::CardinalBumper, room::Maple.Room)
    x, y = Ahorn.position(entity)
    Ahorn.drawSprite(ctx, sprite, x, y, sx=-1)
end

end