module JackalHelperOneWayJellyBarrierModule

using ..Ahorn, Maple

@mapdef Entity "JackalHelper/OWJellyBarrier" OWJB(x::Integer, y::Integer, direction::String="Up", color::String="000000", ignoreOnHeld::Bool=false, alpha::Number=0.5)

directions = ["Up", "Down", "Left", "Right"]

const placements = Ahorn.PlacementDict(
    "One Way Jelly Barrier (Jackal Helper)" => Ahorn.EntityPlacement(
        OWJB,
        "rectangle"
    )
)

Ahorn.minimumSize(entity::OWJB) = 8, 8
Ahorn.resizable(entity::OWJB) = true, true

Ahorn.selection(entity::OWJB) = Ahorn.getEntityRectangle(entity)


function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::OWJB, room::Maple.Room)
    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))
        Ahorn.drawRectangle(ctx, 0, 0, width, height, (0.75, 0.75, 0.25, 0.8), (0.0, 0.0, 0.0, 0.0))
    end
end
