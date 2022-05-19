module JackalHelperObsidianBlock

using ..Ahorn, Maple

@mapdef Entity "JackalHelper/CryoLava" ObsidianBlock(x::Integer, y::Integer, width::Integer=Maple.defaultBlockWidth, height::Integer=Maple.defaultBlockHeight, isIce::Bool=false, surfaceColor::String="ff8933", edgeColor::String="f25e29", centerColor::String="d01c01", linearMotion::Bool=false, linSpeedY::Integer=0, linSpeedX::Integer=0, sineMotion::Bool=false, sineAmplitudeY::Integer=0, sineFrequencyY::Integer=0, sineAmplitudeX::Integer=0, sineFrequencyX::Integer=0)

const placements = Ahorn.PlacementDict(
    "Obsidian Block (Jackal Helper)" => Ahorn.EntityPlacement(
        ObsidianBlock,
        "rectangle"
    ),
)

Ahorn.minimumSize(entity::ObsidianBlock) = 8, 8
Ahorn.resizable(entity::ObsidianBlock) = true, true

Ahorn.selection(entity::ObsidianBlock) = Ahorn.getEntityRectangle(entity)

edgeColor = (18, 18, 18, 255) ./ 255
centerColor = (54, 54, 54, 102) ./ 255

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ObsidianBlock, room::Maple.Room)
    x = Int(get(entity.data, "x", 0))
    y = Int(get(entity.data, "y", 0))

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    Ahorn.drawRectangle(ctx, 0, 0, width, height, centerColor, edgeColor)
end

end