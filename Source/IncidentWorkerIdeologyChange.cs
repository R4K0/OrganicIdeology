using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace OrganicIdeology
{
    public class OrganicIdeologyDefExtension : DefModExtension
    {
        public bool canBeOrganicallyAdopted = true;
    }

    public class IncidentWorker_IdeologyChange : IncidentWorker
    {
        public IncidentWorker_IdeologyChange()
        {
        }

        public static bool IsMixedIdeology()
        {
            return Faction.OfPlayer.ideos.AllIdeos.Count() > 1;
        }

        private static Ideo GetRandomIdeology()
        {
            var otherIdeos =
                Faction.OfPlayer.ideos.AllIdeos.Where(ideo => Faction.OfPlayer.ideos.PrimaryIdeo.id != ideo.id);
            otherIdeos.TryRandomElement(out var foundRandom);

            return foundRandom;
        }

        private static List<Precept> GetRandomCompatiblePrecept(Ideo from, Ideo compatibleWith, int amount = 1)
        {
            return from.PreceptsListForReading
                .Where(precept => precept.GetType() == typeof(Precept) )
                .Where(precept =>
                    compatibleWith.CanAddPreceptAllFactions(precept.def).Accepted && !compatibleWith.HasPrecept(precept.def) &&
                    (precept.def.GetModExtension<OrganicIdeologyDefExtension>()?.canBeOrganicallyAdopted ?? true))
                .InRandomOrder()
                .Take(amount).ToList();
        }

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (OrganicSettings.PlayerOnly && !Faction.OfPlayer.ideos.PrimaryIdeo.initialPlayerIdeo)
                return false;
            
            return ModLister.IdeologyInstalled && IsMixedIdeology();
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (OrganicSettings.PlayerOnly && !Faction.OfPlayer.ideos.PrimaryIdeo.initialPlayerIdeo)
                return false;
            
            var randomIdeo = GetRandomIdeology();

            if (randomIdeo is null)
                return false;

            var randomPrecepts = GetRandomCompatiblePrecept(randomIdeo, Faction.OfPlayer.ideos.PrimaryIdeo,
                OrganicSettings.PreceptRange.RandomInRange);

            if (randomPrecepts.Count < 1)
                return false;

            var builder = new StringBuilder();

            foreach (var randomPrecept in randomPrecepts)
            {
                builder.AppendLine($"{randomPrecept.TipLabel}");
            }

            if (Faction.OfPlayer.ideos.PrimaryIdeo.PreceptsListForReading.Select(precept => precept.def.defName)
                .Intersect(OrganicSettings.BigotDefs).Any())
            {
                builder.AppendLine("\nDue to your ideology precepts, this change will upset some of your colonists.");
            }
            
            var letter = (IdeologyChangeLetter) LetterMaker.MakeLetter(def.letterLabel,
                def.letterText.Formatted(randomIdeo.Named(randomIdeo.name), builder), def.letterDef);

            letter.IncomingIdeo = randomIdeo;
            letter.RequestedPrecept = randomPrecepts;
            letter.ChangedIdeo = Faction.OfPlayer.ideos.PrimaryIdeo;
            letter.StartTimeout( 5000 * 5 );

            Find.LetterStack.ReceiveLetter(letter);

            return true;
        }
    }
}