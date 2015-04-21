﻿using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using RecSys.Core;
using RecSys.Evaluation;
using RecSys.Numerical;
using RecSys.Ordinal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RecSys.ExperimentOfCIKM2015
{
    class Experiment
    {
        /************************************************************
         *   R_train     => Rating Matrix train set
         *   R_test      => Rating Matrix test set
         *   R_unknown   => Rating Matrix with ones indicating unknown entries in the R_test
         *   PR_train    => Preference relations constructed from R_train
         *   PR_test     => Preference relations constructed from R_test
         *   UserSimilaritiesOfRating    => The user-user similarities from R_train
         *   ItemSimilaritiesOfRating    => The item-item similarities from R_train
         *   UserSimilaritiesOfPref      => The user-user similarities from PR_train
         *   ItemSimilaritiesOfPref      => The user-user similarities from PR_train
         *   RelevantItemsByUser         => The relevant items of each user based on R_test, 
         *                                  is used as ground truth in all ranking evalution
        ************************************************************/

        #region Experiment settings
        public RatingMatrix R_train { get; set; }
        public RatingMatrix R_test { get; set; }
        public RatingMatrix R_unknown { get; set; }
        public PrefRelations PR_train { get; set; }
        public PrefRelations PR_test { get; set; }
        public Matrix<double> UserSimilaritiesOfRating { get; set; }
        public Matrix<double> UserSimilaritiesOfPref { get; set; }
        public Matrix<double> ItemSimilaritiesOfRating { get; set; }
        public Matrix<double> ItemSimilaritiesOfPref { get; set; }
        public bool ReadyForNumerical { get; set; }
        public bool ReadyForOrdinal { get; set; }
        public string DataSetFile { get; set; }
        public int MinCountOfRatings { get; set; }
        public int CountOfRatingsForTrain { get; set; }
        public bool ShuffleData { get; set; }
        public int Seed { get; set; }
        public double RelevantItemCriteria { get; set; }
        public Dictionary<int, List<int>> RelevantItemsByUser { get; set; }
        #endregion

        #region Constructor
        public Experiment(string dataSetFile, int minCountOfRatings,
            int countOfRatingsForTrain, bool shuffleData, int seed, double relevantItemCriteria)
        {
            DataSetFile = dataSetFile;
            MinCountOfRatings = minCountOfRatings;
            CountOfRatingsForTrain = countOfRatingsForTrain;
            ShuffleData = shuffleData;
            Seed = seed;
            RelevantItemCriteria = relevantItemCriteria;
            ReadyForNumerical = false;
            ReadyForOrdinal = false;
        }
        #endregion

        #region Get ready for numerical methods
        public string GetReadyForNumerical(bool saveLoadedData = false,
            string userSimilaritiesOfRatingFile = "", string itemSimilaritiesOfRatingFile = "",
            bool loadSavedData = false)
        {
            StringBuilder log = new StringBuilder();
            Utils.StartTimer();

            log.Append(Utils.PrintHeading("Create R_train/R_test sets from " + DataSetFile));
            RatingMatrix R_train_out;
            RatingMatrix R_test_out;
            Utils.LoadMovieLensSplitByCount(Config.Ratings.DataSetFile, out R_train_out,
                out R_test_out, MinCountOfRatings, CountOfRatingsForTrain, ShuffleData, Seed);
            R_train = R_train_out;
            R_test = R_test_out;

            Console.WriteLine(R_train.DatasetBrief("Train set"));
            Console.WriteLine(R_test.DatasetBrief("Test set"));
            log.AppendLine(R_train.DatasetBrief("Train set"));
            log.AppendLine(R_test.DatasetBrief("Test set"));

            R_unknown = R_test.IndexesOfNonZeroElements();

            log.Append(Utils.PrintValue("Relevant item criteria", RelevantItemCriteria.ToString("0.0")));
            RelevantItemsByUser = ItemRecommendationCore.GetRelevantItemsByUser(R_test, RelevantItemCriteria);
            log.Append(Utils.PrintValue("Mean # of relevant items per user",
                RelevantItemsByUser.Average(k => k.Value.Count).ToString("0")));
            log.Append(Utils.StopTimer());

            #region Prepare similarity data
            if (loadSavedData)
            {
                Utils.StartTimer();
                Utils.PrintHeading("Load user-user similarities from R_train");
                UserSimilaritiesOfRating = Utils.ReadDenseMatrix(userSimilaritiesOfRatingFile);
                Utils.PrintValue("Sum of similarities", UserSimilaritiesOfRating.RowSums().Sum().ToString("0.0000"));
                Utils.PrintValue("Abs sum of similarities", UserSimilaritiesOfRating.RowAbsoluteSums().Sum().ToString("0.0000"));
                Utils.StopTimer();

                Utils.StartTimer();
                Utils.PrintHeading("Load item-item similarities from R_train");
                ItemSimilaritiesOfRating = Utils.ReadDenseMatrix(itemSimilaritiesOfRatingFile);
                Utils.PrintValue("Sum of similarities", ItemSimilaritiesOfRating.RowSums().Sum().ToString("0.0000"));
                Utils.PrintValue("Abs sum of similarities", ItemSimilaritiesOfRating.RowAbsoluteSums().Sum().ToString("0.0000"));
                Utils.StopTimer();
            }
            else
            {
                Utils.StartTimer();
                Utils.PrintHeading("Compute user-user similarities from R_train");
                UserSimilaritiesOfRating = Metric.GetPearsonOfRows(R_train);
                if (saveLoadedData) { Utils.WriteMatrix(UserSimilaritiesOfRating, userSimilaritiesOfRatingFile); }
                Utils.PrintValue("Sum of similarities", UserSimilaritiesOfRating.RowSums().Sum().ToString("0.0000"));
                Utils.PrintValue("Abs sum of similarities", UserSimilaritiesOfRating.RowAbsoluteSums().Sum().ToString("0.0000"));
                Utils.StopTimer();

                Utils.StartTimer();
                Utils.PrintHeading("Compute item-item similarities from R_train");
                ItemSimilaritiesOfRating = Metric.GetPearsonOfColumns(R_train);
                if (saveLoadedData) { Utils.WriteMatrix(ItemSimilaritiesOfRating, itemSimilaritiesOfRatingFile); }
                Vector<double> rowSums = ItemSimilaritiesOfRating.RowSums();
                double sum = checked(rowSums.Sum());
                Utils.PrintValue("Sum of similarities", ItemSimilaritiesOfRating.RowSums().Sum().ToString("0.0000"));
                Utils.PrintValue("Abs sum of similarities", ItemSimilaritiesOfRating.RowAbsoluteSums().Sum().ToString("0.0000"));
                Utils.StopTimer();
            }
            #endregion

            ReadyForNumerical = true;

            return log.ToString();
        }
        #endregion

        #region Get ready for ordinal methods
        private string GetReadyForOrdinal(bool saveLoadedData = false,
            string userSimilaritiesOfPrefFile = "", string itemSimilaritiesOfPrefFile = "",
            bool loadSavedData = false)
        {
            StringBuilder log = new StringBuilder();
            Utils.StartTimer();
            log.Append(Utils.PrintHeading("Prepare preferecen relation data"));

            Console.WriteLine("Converting R_train into PR_train");
            log.AppendLine("Converting R_train into PR_train");
            PrefRelations PR_train = PrefRelations.CreateDiscrete(R_train);

            Console.WriteLine("Converting R_test into PR_test");
            log.AppendLine("Converting R_test into PR_test");
            PrefRelations PR_test = PrefRelations.CreateDiscrete(R_test);

            log.Append(Utils.StopTimer());

            #region Prepare similarity data
            if (Config.LoadSavedData)
            {

                Utils.StartTimer();
                Utils.PrintHeading("Load user-user similarities from PR_train");
                UserSimilaritiesOfPref = Utils.ReadDenseMatrix(userSimilaritiesOfPrefFile);
                Utils.PrintValue("Sum of similarities", UserSimilaritiesOfPref.RowSums().Sum().ToString("0.0000"));
                Utils.PrintValue("Abs sum of similarities", UserSimilaritiesOfPref.RowAbsoluteSums().Sum().ToString("0.0000"));
                Utils.StopTimer();

                // TODO: add PR based item-item similarities

            }
            else
            {
                Utils.StartTimer();
                Utils.PrintHeading("Compute user-user similarities from PR_train");
                UserSimilaritiesOfPref = Metric.GetCosineOfPrefRelations(PR_train);
                if (saveLoadedData) { Utils.WriteMatrix(UserSimilaritiesOfPref, userSimilaritiesOfPrefFile); }
                Utils.PrintValue("Sum of similarities", UserSimilaritiesOfPref.RowSums().Sum().ToString("0.0000"));
                Utils.PrintValue("Abs sum of similarities", UserSimilaritiesOfPref.RowAbsoluteSums().Sum().ToString("0.0000"));
                Utils.StopTimer();

                // TODO: add PR based item-item similarities

            }
            #endregion

            ReadyForOrdinal = true;

            return log.ToString();
        }
        #endregion

        #region Get ready for all methods
        public string GetReady()
        {
            StringBuilder log = new StringBuilder();
            log.Append(GetReadyForNumerical());
            log.Append(GetReadyForOrdinal());

            return log.ToString();
        }
        #endregion

        #region Global Mean
        /// <summary>
        /// Predict all unknown values as global mean rating.
        /// </summary>
        public string RunGlobalMean()
        {
            if (!ReadyForNumerical) { return "Please setup experiment first."; }
            StringBuilder log = new StringBuilder();
            log.Append(Utils.PrintHeading("Global Mean"));

            // Prediction
            Utils.StartTimer();
            double globalMean = R_train.GetGlobalMean();
            RatingMatrix R_predicted = R_unknown.Multiply(globalMean);
            log.Append(Utils.StopTimer());

            // Numerical Evaluation
            log.Append(Utils.PrintValue("RMSE", RMSE.Evaluate(R_test, R_predicted).ToString("0.0000")));
            log.Append(Utils.PrintValue("MAE", MAE.Evaluate(R_test, R_predicted).ToString("0.0000")));

            return log.ToString();
        }
        #endregion

        #region Most Popular
        /// <summary>
        /// Recommend the most popular (measured by mean rating) items to all users.
        /// </summary>
        public string RunMostPopular(int topN)
        {
            if (!ReadyForNumerical) { return "Please setup experiment first."; }
            StringBuilder log = new StringBuilder();
            log.Append(Utils.PrintHeading("Most popular"));

            // Prediction
            Utils.StartTimer();
            var meanByItem = R_train.GetItemMeans();
            RatingMatrix R_predicted = new RatingMatrix(R_unknown.UserCount, R_unknown.ItemCount);
            foreach (var element in R_unknown.Matrix.EnumerateIndexed(Zeros.AllowSkip))
            {
                int indexOfUser = element.Item1;
                int indexOfItem = element.Item2;
                R_predicted[indexOfUser, indexOfItem] = meanByItem[indexOfItem];
            }
            var topNItemsByUser = ItemRecommendationCore.GetTopNItemsByUser(R_predicted, Config.TopN);
            log.Append(Utils.StopTimer());

            // TopN Evaluation
            for (int n = 1; n <= Config.TopN; n++)
            {
                log.Append(Utils.PrintValue("NCDG@" + n, NCDG.Evaluate(RelevantItemsByUser, topNItemsByUser, n).ToString("0.0000")));
            }

            return log.ToString();
        }
        #endregion

        #region NMF
        /// <summary>
        /// Rating based Non-negative Matrix Factorization
        /// </summary>
        public string RunNMF(int maxEpoch, double learnRate, double regularization,
            int factorCount, int topN = 0)
        {
            if (!ReadyForNumerical) { return "Please setup experiment first."; }
            StringBuilder log = new StringBuilder();
            log.Append(Utils.PrintHeading("NMF"));

            // Prediction
            Utils.StartTimer();
            RatingMatrix R_predicted = NMF.PredictRatings(R_train, R_unknown, maxEpoch,
                learnRate, regularization, factorCount);
            log.Append(Utils.StopTimer());

            // Numerical Evaluation
            log.Append(Utils.PrintValue("RMSE", RMSE.Evaluate(R_test, R_predicted).ToString("0.0000")));
            log.Append(Utils.PrintValue("MAE", MAE.Evaluate(R_test, R_predicted).ToString("0.0000")));

            // TopN Evaluation
            if (topN != 0)
            {
                var topNItemsByUser = ItemRecommendationCore.GetTopNItemsByUser(R_predicted, Config.TopN);
                for (int n = 1; n <= Config.TopN; n++)
                {
                    log.Append(Utils.PrintValue("NCDG@" + n, NCDG.Evaluate(RelevantItemsByUser, topNItemsByUser, n).ToString("0.0000")));
                }
            }

            return log.ToString();
        }
        #endregion

        #region UserKNN
        public string RunUserKNN(int neighborCount, int topN = 0)
        {
            if (!ReadyForNumerical) { return "Please setup experiment first."; }
            StringBuilder log = new StringBuilder();
            log.Append(Utils.PrintHeading("UserKNN"));

            // Prediction
            Utils.StartTimer();
            RatingMatrix R_predicted = Numerical.UserKNN.PredictRatings(R_train, R_unknown, UserSimilaritiesOfRating, neighborCount);
            log.Append(Utils.StopTimer());

            // Numerical Evaluation
            log.Append(Utils.PrintValue("RMSE", RMSE.Evaluate(R_test, R_predicted).ToString("0.0000")));
            log.Append(Utils.PrintValue("MAE", MAE.Evaluate(R_test, R_predicted).ToString("0.0000")));

            // TopN Evaluation
            if (topN != 0)
            {
                var topNItemsByUser = ItemRecommendationCore.GetTopNItemsByUser(R_predicted, Config.TopN);
                for (int n = 1; n <= Config.TopN; n++)
                {
                    Utils.PrintValue("NCDG@" + n, NCDG.Evaluate(RelevantItemsByUser, topNItemsByUser, n).ToString("0.0000"));
                }
            }

            return log.ToString();
        }
        #endregion

        #region PrefNMF
        public string RunPrefNMF(int maxEpoch, double learnRate, double regularizationOfUser,
            double regularizationOfItem, int factorCount, int topN = 10)
        {
            if (!ReadyForOrdinal) { return "Please setup experiment first."; }
            StringBuilder log = new StringBuilder();
            log.Append(Utils.PrintHeading("PrefNMF"));

            // Prediction
            Utils.StartTimer();
            RatingMatrix R_predicted = PrefNMF.PredictRatings(PR_train, R_unknown,
                maxEpoch, learnRate, regularizationOfUser, regularizationOfItem, factorCount);
            log.Append(Utils.StopTimer());

            // Evaluation
            var topNItemsByUser = ItemRecommendationCore.GetTopNItemsByUser(R_predicted, topN);
            for (int n = 1; n <= Config.TopN; n++)
            {
                Utils.PrintValue("NCDG@" + n, NCDG.Evaluate(RelevantItemsByUser, topNItemsByUser, n).ToString("0.0000"));
            }

            return log.ToString();
        }
        #endregion

        #region PrefKNN
        public string RunPrefKNN(int neighborCount, int topN = 10)
        {
            if (!ReadyForOrdinal) { return "Please setup experiment first."; }
            StringBuilder log = new StringBuilder();
            log.Append(Utils.PrintHeading("PrefKNN"));

            // Prediction
            Utils.StartTimer();
            RatingMatrix R_predicted = PrefUserKNN.PredictRatings(PR_train, R_unknown, neighborCount, UserSimilaritiesOfPref);
            log.Append(Utils.StopTimer());

            // TopN Evaluation
            var topNItemsByUser = ItemRecommendationCore.GetTopNItemsByUser(R_predicted, topN);
            for (int n = 1; n <= Config.TopN; n++)
            {
                Utils.PrintValue("NCDG@" + n, NCDG.Evaluate(RelevantItemsByUser, topNItemsByUser, n).ToString("0.0000"));
            }

            return log.ToString();
        }
        #endregion

        #region PrefMRF: PrefNMF based ORF
        public string RunPrefMRF(Dictionary<Tuple<int, int>, List<double>> OMFDistributionByUserItem,
            double regularization, double learnRate, double minSimilarity, int maxEpoch, List<double> quantizer,
            int topN = 10)
        {
            if (!ReadyForOrdinal) { return "Please setup experiment first."; }
            StringBuilder log = new StringBuilder();
            log.Append(Utils.PrintHeading("PrefMRF: PrefNMF based ORF"));

            // Prediction
            Utils.StartTimer();
            RatingMatrix R_predicted_expectations;
            RatingMatrix R_predicted_mostlikely;

            // Convert PR_train into user-wise preferences
            RatingMatrix R_train_positions = new RatingMatrix(PR_train.GetPositionMatrix());
            R_train_positions.Quantization(quantizer[0], quantizer[quantizer.Count - 1] - quantizer[0], quantizer);

            ORF orf = new ORF();
            orf.PredictRatings(
                R_train_positions, R_unknown, ItemSimilaritiesOfPref, OMFDistributionByUserItem,
                regularization, learnRate, minSimilarity, maxEpoch, quantizer.Count,
                out R_predicted_expectations, out R_predicted_mostlikely);
            log.Append(Utils.StopTimer());

            // Evaluation
            var topNItemsByUser_expectations = ItemRecommendationCore.GetTopNItemsByUser(R_predicted_expectations, Config.TopN);
            var topNItemsByUser_mostlikely = ItemRecommendationCore.GetTopNItemsByUser(R_predicted_mostlikely, Config.TopN);
            for (int n = 1; n <= Config.TopN; n++)
            {
                log.Append(Utils.PrintValue("NCDG@" + n, NCDG.Evaluate(RelevantItemsByUser,
                    topNItemsByUser_expectations, n).ToString("0.0000")));
            }
            for (int n = 1; n <= Config.TopN; n++)
            {
                log.Append(Utils.PrintValue("NCDG@" + n, NCDG.Evaluate(RelevantItemsByUser,
                    topNItemsByUser_mostlikely, n).ToString("0.0000")));
            }

            return log.ToString();
        }
        #endregion

        #region NMF based ORF
        public string RunNMFbasedORF(Dictionary<Tuple<int, int>, List<double>> OMFDistributionByUserItem,
            double regularization, double learnRate, double minSimilarity, int maxEpoch, List<double> quantizer,
            int topN = 0)
        {
            if (!ReadyForNumerical) { return "Please setup experiment first."; }
            StringBuilder log = new StringBuilder();
            log.Append(Utils.PrintHeading("NMF based ORF"));

            // Prediction
            Utils.StartTimer();
            RatingMatrix R_predicted_expectations;
            RatingMatrix R_predicted_mostlikely;
            ORF orf = new ORF();
            orf.PredictRatings(
                R_train, R_unknown, ItemSimilaritiesOfRating, OMFDistributionByUserItem,
                regularization, learnRate, minSimilarity, maxEpoch, quantizer.Count,
                out R_predicted_expectations, out R_predicted_mostlikely);
            log.Append(Utils.StopTimer());

            // Numerical Evaluation
            log.Append(Utils.PrintValue("RMSE", RMSE.Evaluate(R_test, R_predicted_expectations).ToString("0.0000")));
            log.Append(Utils.PrintValue("MAE", RMSE.Evaluate(R_test, R_predicted_mostlikely).ToString("0.0000")));

            // Top-N Evaluation
            if (topN != 0)
            {
                var topNItemsByUser_expectations = ItemRecommendationCore.GetTopNItemsByUser(R_predicted_expectations, topN);
                var topNItemsByUser_mostlikely = ItemRecommendationCore.GetTopNItemsByUser(R_predicted_mostlikely, topN);
                for (int n = 1; n <= Config.TopN; n++)
                {
                    log.Append(Utils.PrintValue("NCDG@" + n, NCDG.Evaluate(RelevantItemsByUser, topNItemsByUser_expectations, n).ToString("0.0000")));
                }
                for (int n = 1; n <= Config.TopN; n++)
                {
                    log.Append(Utils.PrintValue("NCDG@" + n, NCDG.Evaluate(RelevantItemsByUser, topNItemsByUser_mostlikely, n).ToString("0.0000")));
                }
            }

            return log.ToString();
        }
        #endregion

        #region PrefNMF based OMF
        public string RunPrefNMFbasedOMF(int maxEpoch, double learnRate, double regularizationOfUser,
            double regularizationOfItem, int factorCount, List<double> quantizer,
            out Dictionary<Tuple<int, int>, List<double>> OMFDistributionByUserItem,
            int topN, Dictionary<int, List<int>> relevantItemsByUser)
        {
            if (!ReadyForOrdinal)
            {
                OMFDistributionByUserItem = null;
                return "Please setup experiment first.";
            }
            StringBuilder log = new StringBuilder();
            log.Append(Utils.PrintHeading("PrefNMF based OMF"));

            // =============PrefNMF prediction on Train+Unknown============
            // Get ratings from scorer, for both train and test
            // R_all contains indexes of all ratings both train and test
            RatingMatrix R_all = new RatingMatrix(R_unknown.UserCount, R_unknown.ItemCount);
            R_all.MergeNonOverlap(R_unknown);
            R_all.MergeNonOverlap(R_train.IndexesOfNonZeroElements());
            PrefRelations PR_unknown = PrefRelations.CreateDiscrete(R_all);

            Utils.StartTimer();
            // PR_test should be replaced with PR_unknown, but it is the same
            PrefRelations PR_predicted = PrefNMF.PredictPrefRelations(PR_train, PR_unknown,
                maxEpoch, learnRate, regularizationOfUser, regularizationOfItem, factorCount);

            // Both predicted and train need to be quantized
            // otherwise OMF won't accept
            PR_predicted.quantization(0, 1.0,
                new List<double> { Config.Preferences.LessPreferred, 
                        Config.Preferences.EquallyPreferred, Config.Preferences.Preferred });
            RatingMatrix R_predictedByPrefNMF = new RatingMatrix(PR_predicted.GetPositionMatrix());

            // PR_train itself is already in quantized form!
            //PR_train.quantization(0, 1.0, new List<double> { Config.Preferences.LessPreferred, Config.Preferences.EquallyPreferred, Config.Preferences.Preferred });
            RatingMatrix R_train_positions = new RatingMatrix(PR_train.GetPositionMatrix());
            R_train_positions.Quantization(quantizer[0], quantizer[quantizer.Count - 1] - quantizer[0], quantizer);
            log.Append(Utils.StopTimer());

            // =============OMF prediction on Train+Unknown============
            log.Append(Utils.PrintHeading("Ordinal Matrix Factorization with PrefNMF as scorer"));
            Utils.StartTimer();
            RatingMatrix R_predicted;
            log.Append(OMF.PredictRatings(R_train_positions.Matrix, R_unknown.Matrix, R_predictedByPrefNMF.Matrix,
                quantizer, out R_predicted, out OMFDistributionByUserItem));
            log.Append(Utils.StopTimer());

            // TopN Evaluation
            var topNItemsByUser = ItemRecommendationCore.GetTopNItemsByUser(R_predicted, Config.TopN);
            for (int n = 1; n <= Config.TopN; n++)
            {
                log.Append(Utils.PrintValue("NCDG@" + n, NCDG.Evaluate(relevantItemsByUser, topNItemsByUser, n).ToString("0.0000")));
            }

            return log.ToString();
        }
        #endregion

        #region NMF based OMF
        public string RunNMFbasedOMF(int maxEpoch, double learnRate, double regularization, int factorCount,
            List<double> quantizer, out Dictionary<Tuple<int, int>, List<double>> OMFDistributionByUserItem,
            int topN = 0)
        {
            if (!ReadyForNumerical)
            {
                OMFDistributionByUserItem = null;
                return "Please setup experiment first.";
            }
            StringBuilder log = new StringBuilder();
            log.Append(Utils.PrintHeading("NMF based OMF"));

            // NMF Prediction
            // Get ratings from scorer, for both train and test
            // R_all contains indexes of all ratings both train and test
            RatingMatrix R_all = new RatingMatrix(R_unknown.UserCount, R_unknown.ItemCount);
            R_all.MergeNonOverlap(R_unknown);
            R_all.MergeNonOverlap(R_train.IndexesOfNonZeroElements());
            Utils.StartTimer();
            RatingMatrix R_predictedByNMF = NMF.PredictRatings(R_train, R_all, Config.NMF.MaxEpoch,
                Config.NMF.LearnRate, Config.NMF.Regularization, Config.NMF.K);
            log.Append(Utils.StopTimer());

            // OMF Prediction
            log.Append(Utils.PrintHeading("Ordinal Matrix Factorization with NMF as scorer"));
            Utils.StartTimer();
            RatingMatrix R_predicted;
            log.Append(OMF.PredictRatings(R_train.Matrix, R_unknown.Matrix, R_predictedByNMF.Matrix,
                quantizer, out R_predicted, out OMFDistributionByUserItem));
            log.Append(Utils.StopTimer());

            // Numerical Evaluation
            log.Append(Utils.PrintValue("RMSE", RMSE.Evaluate(R_test, R_predicted).ToString("0.0000")));
            log.Append(Utils.PrintValue("MAE", MAE.Evaluate(R_test, R_predicted).ToString("0.0000")));

            // TopN Evaluation
            if (topN != 0)
            {
                var topNItemsByUser = ItemRecommendationCore.GetTopNItemsByUser(R_predicted, Config.TopN);
                for (int n = 1; n <= Config.TopN; n++)
                {
                    log.Append(Utils.PrintValue("NCDG@" + n, NCDG.Evaluate(RelevantItemsByUser, topNItemsByUser, n).ToString("0.0000")));
                }
            }

            return log.ToString();
        }
        #endregion

        #region MML
        /*
            Utils.PrintHeading("MML");
            if (Utils.Ask())
            {
                // load the data
                Utils.WriteMovieLens(R_train, "R_train_1m.data");
                Utils.WriteMovieLens(R_test, "R_test_1m.data");
                var training_data = RatingData.Read("R_train_1m.data");
                var test_data = RatingData.Read("R_test_1m.data");

                var m_data = RatingData.Read("1m_comma.data");
                var k_data = RatingData.Read("100k_comma.data");


                var mf = new MatrixFactorization() { Ratings = m_data };
                Console.WriteLine("CV on 1m all data "+mf.DoCrossValidation());
                mf = new MatrixFactorization() { Ratings = k_data };
                Console.WriteLine("CV on 100k all data " + mf.DoCrossValidation());
                mf = new MatrixFactorization() { Ratings = training_data };
                Console.WriteLine("CV on 1m train data " + mf.DoCrossValidation());
                mf = new MatrixFactorization() { Ratings = k_data };
                Console.WriteLine("CV on 100k train data " + mf.DoCrossValidation());


                var bmf = new BiasedMatrixFactorization { Ratings = training_data };
                Console.WriteLine("BMF CV on 1m train data " + bmf.DoCrossValidation());

                // set up the recommender
                var recommender = new MatrixFactorization();// new UserItemBaseline();
                recommender.Ratings = training_data;
                recommender.Train();
                RatingMatrix R_predicted = new RatingMatrix(R_test.UserCount, R_test.ItemCount);
                foreach (var element in R_test.Matrix.EnumerateIndexed(Zeros.AllowSkip))
                {
                    int indexOfUser = element.Item1;
                    int indexOfItem = element.Item2;
                    R_predicted[indexOfUser, indexOfItem] = recommender.Predict(indexOfUser, indexOfItem);
                }

                // Evaluation
                Utils.PrintValue("RMSE of MF on 1m train data, mine RMSE", 
                    RMSE.Evaluate(R_test, R_predicted).ToString("0.0000"));
                var topNItemsByUser = ItemRecommendationCore.GetTopNItemsByUser(R_predicted, Config.TopN);

                Dictionary<int, List<int>> relevantItemsByUser2 = ItemRecommendationCore
    .GetRelevantItemsByUser(R_test, Config.Ratings.RelevanceThreshold);

                for (int n = 1; n <= Config.TopN; n++)
                {
                    Utils.PrintValue("NCDG@" + n, NCDG.Evaluate(relevantItemsByUser2, topNItemsByUser, n).ToString("0.0000"));
                }


                // measure the accuracy on the test data set
                var results = recommender.Evaluate(test_data);
                Console.WriteLine("1m train/test, Their RMSE={0} MAE={1}", results["RMSE"], results["MAE"]);
                Console.WriteLine(results);


            }
         */
        #endregion

    }
}