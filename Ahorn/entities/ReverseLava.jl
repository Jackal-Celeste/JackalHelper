module JackalHelperRevLava

using ..Ahorn, Maple

@mapdef Entity "JackalHelper/RevLava" RevLava(x::Integer, y::Integer, width::Integer=Maple.defaultBlockWidth, height::Integer=Maple.defaultBlockHeight)

const placements = Ahorn.PlacementDict(
    "Reverse Lava (Jackal Helper)" => Ahorn.EntityPlacement(
        RevLava,
        "rectangle"
    ),
)

Ahorn.minimumSize(entity::RevLava) = 8, 8
Ahorn.resizable(entity::RevLava) = true, true

Ahorn.selection(entity::RevLava) = Ahorn.getEntityRectangle(entity)

edgeColor = (246, 98, 18, 255) ./ 255
centerColor = (209, 9, 1, 102) ./ 255

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RevLava, room::Maple.Room)
    x = Int(get(entity.data, "x", 0))
    y = Int(get(entity.data, "y", 0))

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

	edgeColor = Ahorn.argb32ToRGBATuple(parse(Int, get(entity.data, "edgeColor", "f25e29"), base=16))[1:3] ./ 255
	realEdgeColor = (edgeColor..., 1.0)
	
	centerColor = Ahorn.argb32ToRGBATuple(parse(Int, get(entity.data, "centerColor", "d01c01"), base=16))[1:3] ./ 255
	realCenterColor = (centerColor..., 1.0)
	
    Ahorn.drawRectangle(ctx, 0, 0, width, height, realCenterColor, realEdgeColor)
end

end