module JackalHelperGrappleGem

using ..Ahorn, Maple

@mapdef Entity "JackalHelper/GrappleGem" GrappleGem(x::Integer, y::Integer, topText::String="You Got a Grappling Hook!", bottomText="Press Grab to Launch It!", topTextColor::String="00FFFF", bottomTextColor::String="00FFFF", particleSpeed::Number=0.25, skipCutscene::Bool=false)


const placements = Ahorn.PlacementDict(
    "Grappling Hook Gem (WIP) (Jackal Helper)" => Ahorn.EntityPlacement(
        GrappleGem,
        "point",
    )
)


sprite = "collectables/grappleGem/0/00"


function Ahorn.selection(entity::GrappleGem)
    x, y = Ahorn.position(entity)
    return Ahorn.getSpriteRectangle(sprite, x, y)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::GrappleGem, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0, 0)

end