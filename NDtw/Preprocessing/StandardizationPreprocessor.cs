﻿using System;
using System.Linq;
using System.Collections.Generic;

namespace NDtw.Preprocessing
{
    public class StandardizationPreprocessor : IPreprocessor
    {
        public IList<double> Preprocess(IList<double> data)
        {
            //http://stats.stackexchange.com/questions/1944/what-is-the-name-of-this-normalization
            //http://stats.stackexchange.com/questions/13412/what-are-the-primary-differences-between-z-scores-and-t-scores-and-are-they-bot
            //http://mathworld.wolfram.com/StandardDeviation.html

            // x = (x - mean) / std dev
            var mean = data.Average();
            var stdDev = Math.Sqrt(data.Select(x => x - mean).Sum(x => x * x) / (data.Count - 1));

            return data.Select(x => (x - mean) / stdDev).ToArray();
        }

        public override string ToString()
        {
            return "Standardization";
        }
    }
}
