﻿using RecSys.Numerical;
using RecSys.Ordinal;
using System;
using System.Collections.Generic;
using System.Linq;
using RecSys.Core;
using RecSys.Evaluation;
using MathNet.Numerics;
using MathNet.Numerics.Providers.LinearAlgebra.Mkl;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Data.Text;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Statistics;
using System.IO;
using MyMediaLite.IO;
using MyMediaLite.RatingPrediction;
using MyMediaLite.Eval;
using RecSys.ExperimentOfCIKM2015;

namespace RecSys
{
    class Program
    {
        static void Main(string[] args)
        {
            Control.UseMultiThreading();

            Experiment aExperiment = new Experiment("100k.data", 60, 50, false, 1, 5.0);
            aExperiment.GetReadyForNumerical();
        }
    }
}
