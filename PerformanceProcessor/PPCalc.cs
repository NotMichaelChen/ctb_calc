using System;
using System.Globalization;

using BeatmapInfo;
using DifficultyProcessor;

namespace PerformanceProcessor
{
    //Copied from https://github.com/ppy/osu-performance
    public class PPCalc
    {
        private Modifiers mods;
        private int maxCombo, amount300, amount100, amountKatu, amount50, amountMiss;
        private Beatmap map;
        private DiffCalc difficulty;
        private double totalvalue;

        public PPCalc(int maxCombo, int amount300, int amount100, int amountKatu, int amount50, int amountMiss, Modifiers mods, Beatmap map)
        {
            this.maxCombo = maxCombo;
            this.amount300 = amount300;
            this.amount100 = amount100;
            this.amountKatu = amountKatu;
            this.amount50 = amount50;
            this.amountMiss = amountMiss;
            this.mods = mods;
            this.map = map;

            this.difficulty = new DiffCalc(map);

            ComputeTotalValue();
        }

        public double GetTotalValue()
        {
            return totalvalue;
        }

        private void ComputeTotalValue()
        {
            // Don't count scores made with supposedly unranked mods
            if(((int)mods & (int)Modifiers.Relax) > 0 ||
               ((int)mods & (int)Modifiers.Relax2) > 0 ||
               ((int)mods & (int)Modifiers.Autoplay) > 0)
            {
                totalvalue = 0;
                return;
            }

            // We are heavily relying on aim in catch the beat
            totalvalue = Math.Pow(5.0 * Math.Max(1.0, difficulty.GetDifficulty() / 0.0049) - 4.0, 2.0) / 100000.0;

            // Longer maps are worth more. "Longer" means how many hits there are which can contribute to combo
            int amountTotalHits = TotalHits();

            // Longer maps are worth more
            double lengthBonus = 0.95 + 0.4 * Math.Min(1.0, amountTotalHits / 3000.0) +
                                  (amountTotalHits > 3000 ? Math.Log10(amountTotalHits / 3000.0) * 0.5 : 0.0);

            // Longer maps are worth more
            totalvalue *= lengthBonus;

            // Penalize misses exponentially. This mainly fixes tag4 maps and the likes until a per-hitobject solution is available
            totalvalue *= Math.Pow(0.97, amountMiss);

            // Combo scaling
            double beatmapMaxCombo = TotalComboHits();
            if(beatmapMaxCombo > 0)
            {
                totalvalue *= Math.Min(Math.Pow(maxCombo, 0.8) / Math.Pow(beatmapMaxCombo, 0.8), 1.0);
            }


            double approachRate = Double.Parse(map.GetTag("Difficulty", "ApproachRate"), CultureInfo.InvariantCulture);
            double approachRateFactor = 1.0;
            if(approachRate > 9.0)
            {
                approachRateFactor += 0.1 * (approachRate - 9.0); // 10% for each AR above 9
            }
            else if(approachRate < 8.0)
            {
                approachRateFactor += 0.025 * (8.0 - approachRate); //2.5% for each AR below 8
            }

            totalvalue *= approachRateFactor;

            if(((int)mods & (int)Modifiers.Hidden) > 0)
            {
                // Hiddens gives nothing on max approach rate, and more the lower it is
                totalvalue *= 1.05 + 0.075 * (10.0 - Math.Min(10.0, approachRate)); // 7.5% for each AR below 10
            }

            // Scale the aim value with accuracy _slightly_
            totalvalue *= Math.Pow(Accuracy(), 5.5);

            // Custom multipliers for NoFail and SpunOut.
            if(((int)mods & (int)Modifiers.NoFail) > 0)
            {
                totalvalue *= 0.90;
            }

            if(((int)mods & (int)Modifiers.SpunOut) > 0)
            {
                totalvalue *= 0.95;
            }
        }

        private double Accuracy()
        {
            if(TotalHits() == 0)
            {
                return 0;
            }

            return Dewlib.Clamp((double)TotalSuccessfulHits() / TotalHits(), 0.0, 1.0);
        }

        private int TotalHits()
        {
            return amount50 + amount100 + amount300 + amountMiss + amountKatu;
        }

        private int TotalSuccessfulHits()
        {
            return amount50 + amount100 + amount300;
        }

        private int TotalComboHits()
        {
            return amount300 + amount100 + amountMiss;
        }
    }
}

