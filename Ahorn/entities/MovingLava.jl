module JackalHelperMovLava

using ..Ahorn, Maple

@mapdef Entity "JackalHelper/MovingLava" MovLava(x::Integer, y::Integer, width::Integer=Maple.defaultBlockWidth, height::Integer=Maple.defaultBlockHeight, linSpeedY::Integer=0, linSpeedX::Integer=0, sineAmplitudeY::Integer=0, sineFrequencyY::Integer=0, sineAmplitudeX::Integer=0, sineFrequencyX::Integer=0, flagLinSpeedY::Integer=0, flagLinSpeedX::Integer=0, flagSineAmplitudeY::Integer=0, flagSineFrequencyY::Integer=0, flagSineAmplitudeX::Integer=0, flagSineFrequencyX::Integer=0, flag::String="", flagToStart::String="")

const placements = Ahorn.PlacementDict(
    "Moving Fire Barrier (Jackal Helper)" => Ahorn.EntityPlacement(
        MovLava,
        "rectangle"
    ),
)

Ahorn.minimumSize(entity::MovLava) = 8, 8
Ahorn.resizable(entity::MovLava) = true, true

Ahorn.selection(entity::MovLava) = Ahorn.getEntityRectangle(entity)

edgeColor = (246, 98, 18, 255) ./ 255
centerColor = (209, 9, 1, 102) ./ 255

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MovLava, room::Maple.Room)
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