using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace OrganicIdeology
{
    public class IdeologyChangeLetter : ChoiceLetter
    {
        public Ideo IncomingIdeo;
        public Ideo ChangedIdeo;

        public List<Precept> RequestedPrecept;

        public static ThoughtDef PunishmentIdeologyChangeThought =
            DefDatabase<ThoughtDef>.GetNamed("OrganicIdeologySadChange");

        public IdeologyChangeLetter()
        {
        }

        private static Precept IsIdeologyBigoted(Ideo ideo)
        {
            return ideo.PreceptsListForReading.FirstOrDefault(precept =>
                OrganicSettings.BigotDefs.Contains(precept.def.defName));
        }

        public override IEnumerable<DiaOption> Choices
        {
            get
            {
                if (ArchivedOnly)
                {
                    yield return Option_Close;
                }
                else
                {
                    var accept = new DiaOption("Accept")
                    {
                        action = () =>
                        {
                            var bigotedPrecept = IsIdeologyBigoted(ChangedIdeo);
                            if (bigotedPrecept != null)
                            {
                                foreach (var bigotedPawn in PawnsFinder
                                    .AllMapsCaravansAndTravelingTransportPods_Alive_Colonists
                                    .Where(pawn => pawn.Ideo == ChangedIdeo))
                                {
                                    bigotedPawn.needs.mood.thoughts.memories.TryGainMemoryFast(
                                        PunishmentIdeologyChangeThought, bigotedPrecept);
                                }
                            }

                            var preceptsToApply = RequestedPrecept.ToList();
                            var currentPrecepts = ChangedIdeo.PreceptsListForReading.ToList();

                            var preceptsToDelete = currentPrecepts
                                .Where(precept => !precept.def.issue?.allowMultiplePrecepts ?? false)
                                .Where(precept => preceptsToApply.Any(testAgainst =>
                                    testAgainst.def.issue == precept.def.issue)).ToList();

                            Log.Message($"Found Precepts To Delete: {preceptsToDelete.Count}");

                            // First, add precepts that were requested
                            foreach (var addPrecept in preceptsToApply)
                            {
                                var newPrecept = PreceptMaker.MakePrecept(addPrecept.def);

                                ChangedIdeo.AddPrecept(newPrecept, true);
                            }

                            // Second, clean-up precepts that would otherwise be the same/conflicting
                            foreach (var deletePrecept in preceptsToDelete)
                            {
                                ChangedIdeo.RemovePrecept(deletePrecept);
                            }

                            Find.LetterStack.RemoveLetter(this);
                        },
                        resolveTree = true
                    };

                    yield return accept;
                    yield return Option_Postpone;
                    yield return Option_Reject;
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.Look(ref RequestedPrecept, "RequestedPrecepts", LookMode.Reference);

            Scribe_References.Look(ref IncomingIdeo, "ChangedIdeology");
            Scribe_References.Look(ref ChangedIdeo, "OriginalIdeology");
        }
    }
}