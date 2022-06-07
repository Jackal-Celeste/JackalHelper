module JackalHelperBraveBird

using ..Ahorn, Maple

@mapdef Entity "JackalHelper/BraveBird" BraveBird(x::Integer, y::Integer, waiting::Bool=false, spritePath::String="characters/bird/", launchSpeedX::Number=380, launchSpeedY::Number=-100, canSkipNode::Bool=false, stressedAtLastNode::Bool=true, particleColor::String="639bff", skipDistance::Number=100, toggleZoom::Bool=false)

const placements = Ahorn.PlacementDict(
    "Custom Fling Bird (Jackal Helper)" => Ahorn.EntityPlacement(
        BraveBird
    ),
)

Ahorn.nodeLimits(entity::BraveBird) = 0, -1

sprite = "characters/bird/Hover04"

function Ahorn.selection(entity::BraveBird)
    nodes = get(entity.data, "nodes", ())
    x, y = Ahorn.position(entity)

    res = Ahorn.Rectangle[Ahorn.getSpriteRectangle(sprite, x, y)]
    
    for node in nodes
        nx, ny = Int.(node)

        push!(res, Ahorn.getSpriteRectangle(sprite, nx, ny))
    end

    return res
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::BraveBird)
    px, py = Ahorn.position(entity)

    for node in get(entity.data, "nodes", ())
        nx, ny = Int.(node)

        theta = atan(py - ny, px - nx)
        Ahorn.drawArrow(ctx, px, py, nx + cos(theta) * 8, ny + sin(theta) * 8, Ahorn.colors.selection_selected_fc, headLength=6)
        Ahorn.drawSprite(ctx, sprite, nx, ny)

        px, py = nx, ny
    end
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::BraveBird, room::Maple.Room)
    x, y = Ahorn.position(entity)
    Ahorn.drawSprite(ctx, sprite, x, y, sx=-1)
end

end