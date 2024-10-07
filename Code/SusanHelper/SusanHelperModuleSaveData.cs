using System;
using System.Collections.Generic;

namespace Celeste.Mod.SusanHelperNew;

public class SusanHelperNewModuleSaveData : EverestModuleSaveData {
    public List<Tuple<string, bool>> obligatoPrisms = new List<Tuple<string, bool>>()
    {
        new Tuple<string, bool>("Amber",false),
        new Tuple<string, bool>("Sapphire",false),
        new Tuple<string, bool>("Citrine",false),
        new Tuple<string, bool>("Emerald",false),
        new Tuple<string, bool>("Ruby",false),
        new Tuple<string, bool>("Amethyst",false),
    };

}