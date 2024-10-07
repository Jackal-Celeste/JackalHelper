using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste;
using SusanHelper.Entities.Paint;
using System.Collections.Generic;
using Celeste.Mod.SusanHelper;
using System.Linq;
using static SusanHelper.Entities.Paint.PaintSource;
using System.Runtime.CompilerServices;
using System.Globalization;
using System.Collections;
using Celeste.Mod.SusanHelperNew.Obligato;

public class JackalMaidCafePage04 : JackalMaidCafePage
{
    private string title;

    private string titleDisplayed;

    private MTexture clipArt;

    private float clipArtEase;

    private FancyText.Text infoText;

    private AreaCompleteTitle easyText;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public JackalMaidCafePage04()
    {
        Transition = Transitions.Blocky;
        ClearColor = Color.Gray * 0.5f;
        title = Dialog.Clean("JACKAL_PAGE3_TITLE1");
        titleDisplayed = "";
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Added(JackalMaidCafePresentation presentation)
    {
        base.Added(presentation);
        clipArt = presentation.Gfx["moveset"];
    }

    public override IEnumerator Routine()
    {
        while (titleDisplayed.Length < title.Length)
        {
            titleDisplayed += title[titleDisplayed.Length];
            yield return 0.05f;
        }
        yield return PressButton();
        //Audio.Play("event:/game/06_reflection/crushblock_move_loop_covert");
        while (clipArtEase < 1f)
        {
            clipArtEase = Calc.Approach(clipArtEase, 1f, Engine.DeltaTime);
            yield return null;
        }
        yield return 0.25f;
        infoText = FancyText.Parse(Dialog.Get("JACKAL_PAGE3_TITLE2"), Width - 240, 32, 1f, Color.Chartreuse * 0.7f);
        yield return PressButton();
        Audio.Play("event:/new_content/game/10_farewell/ppt_its_easy");
        easyText = new AreaCompleteTitle(new Vector2((float)Width / 2f, Height - 150), Dialog.Clean("JACKAL_PAGE3_PETERKEVINS"), 2f, rainbow: true);
        yield return 1f;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Update()
    {
        if (easyText != null)
        {
            easyText.Update();
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Render()
    {
        MTexture background = Presentation.Gfx["desktop/LavisBG"];
        background.Draw(Vector2.Zero,Vector2.Zero,Color.LightCoral);

        ActiveFont.DrawOutline(titleDisplayed, new Vector2(128f, 100f), Vector2.Zero, Vector2.One * 1.5f, Color.White, 2f, Color.Black);
        if (clipArtEase > 0f)
        {
            Vector2 scale = Vector2.One * (1f + (1f - clipArtEase) * 3f) * 0.8f;
            float rotation = (1f - clipArtEase) * 8f;
            Color color = Color.White * clipArtEase;
            clipArt.DrawCentered(new Vector2((float)base.Width / 2f, (float)base.Height / 2f - 90f), color, scale, rotation);
        }
        if (infoText != null)
        {
            infoText.Draw(new Vector2((float)base.Width / 2f, base.Height - 350), new Vector2(0.5f, 0f), Vector2.One, 1f);
        }
        if (easyText != null)
        {
            easyText.Render();
        }
    }
}
