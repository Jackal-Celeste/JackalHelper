module JackalHelperSombraCrystal

using ..Ahorn, Maple

@mapdef Entity "JackalHelper/DummyCrystal" SombraCrystal(x::Integer, y::Integer)


const placements = Ahorn.PlacementDict(
    "Dummy Crystal (Jackal Helper)" => Ahorn.EntityPlacement(
        SombraCrystal,
        "point",
    )
)


sprite = "characters/theoCrystal/idle00"


function Ahorn.selection(entity::SombraCrystal)
    x, y = Ahorn.position(entity)
    return Ahorn.getSpriteRectangle(sprite, x, y)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SombraCrystal, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0, 0)

end