using System.Collections.Generic;
using RimWorld;
using Verse;

namespace OrganicIdeology
{
    public class OrganicSettings : ModSettings
    {
        public static IntRange PreceptRange = new IntRange(2, 3);
        public static List<string> BigotDefs = new List<string>
        {
            "IdeoDiversity_Horrible",
            "IdeoDiversity_Disapproved",
            "IdeoDiversity_Abhorrent"
        };

        public static bool PlayerOnly = false;

        public static void ToDefault( bool resetBigot, bool resetPrecept = false)
        {
            if (resetBigot)
            {
                BigotDefs = new List<string>
                {
                    "IdeoDiversity_Horrible",
                    "IdeoDiversity_Disapproved",
                    "IdeoDiversity_Abhorrent"
                };
            }

            if (resetPrecept)
            {
                PreceptRange = new IntRange(2, 3);
            }
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref PreceptRange, "OrganicPreceptAmount", new IntRange(2, 3));
            Scribe_Collections.Look(ref BigotDefs, "OrganicBigotDef", LookMode.Value);
            Scribe_Values.Look( ref PlayerOnly, "OrganicPlayerOnly", false);

            if (BigotDefs == null)
            {
                BigotDefs = new List<string>()
                {
                    "IdeoDiversity_Horrible",
                    "IdeoDiversity_Disapproved",
                    "IdeoDiversity_Abhorrent"
                };
            }

            base.ExposeData();
        }
    }
}