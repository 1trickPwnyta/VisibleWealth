using System;
using Verse;

namespace VisibleWealth
{
    public enum PawnCategory
    {
        Human,
        Animal,
        Mech,
        Mutant
    }

    public static class PawnCategoryUtility
    {
        public static string Label(this PawnCategory category)
        {
            switch (category)
            {
                case PawnCategory.Human: return "VisibleWealth_Humans".Translate();
                case PawnCategory.Animal: return "VisibleWealth_Animals".Translate();
                case PawnCategory.Mech: return "VisibleWealth_Mechs".Translate();
                case PawnCategory.Mutant: return "VisibleWealth_Mutants".Translate();
                default: throw new NotImplementedException("Invalid pawn category.");
            }
        }

        public static bool Matches(this PawnCategory category, Pawn pawn)
        {
            switch (category)
            {
                case PawnCategory.Human: return pawn.RaceProps.Humanlike && !pawn.IsMutant;
                case PawnCategory.Animal: return pawn.IsNonMutantAnimal;
                case PawnCategory.Mech: return pawn.RaceProps.IsMechanoid;
                case PawnCategory.Mutant: return pawn.IsMutant;
                default: throw new NotImplementedException("Invalid pawn category.");
            }
        }
    }
}
