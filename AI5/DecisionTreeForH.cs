using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AI5
{
    internal class DtNode
    {
        public string AttributeName { get; private set; }
        public double Threshold { get; private set; }
        public DtNode GreaterOrEqualTo { get; set; }
        public DtNode Less { get; set; }
        public bool? Classification { get; private set; }

        /// <summary>
        /// Initialize a normal node for decision tree which has children, attribute name and threshold.
        /// </summary>
        /// <param name="attributeName"></param>
        /// <param name="threshold"></param>
        public DtNode(string attributeName, double threshold)
        {
            this.AttributeName = attributeName;
            this.Threshold = threshold;
        }

        /// <summary>
        /// Initialize an end node for decision tree to represent a final classification
        /// </summary>
        /// <param name="classification"></param>
        public DtNode(bool classification)
        {
            this.Classification = classification;
        }
    }

    class DecisionTreeForH
    {
        private static readonly List<DiagnosInstance> HorseData = new List<DiagnosInstance>();

        private static readonly DecisionTreeForH Dt = new DecisionTreeForH();

        private DecisionTreeForH() {}

        /// <summary>
        /// Load data from the trainning set of horses.
        /// </summary>
        /// <param name="dataPath"></param>
        /// <returns></returns>
        public static DecisionTreeForH CreateFromHorseInstance(string dataPath)
        {
            if (HorseData.Count != 0)
            {
                HorseData.Clear();
            }

            using (var sw = new StreamReader(dataPath))
            {
                string newLine;
                while ((newLine = sw.ReadLine()) != null)
                {
                    var dataArray = newLine.Split(',');
                    var dataArrayAsDouble = new double[dataArray.Length - 1];
                    for (int i = 0; i < dataArray.Length - 1; ++i)
                    {
                        dataArrayAsDouble[i] = double.Parse(dataArray[i]);
                    }
                    HorseData.Add(new DiagnosInstance(dataArray.Last().Equals("colic."), dataArrayAsDouble));
                }
            }

            return Dt;
        }

        /// <summary>
        /// Construct the decision tree for the trainning set.
        /// </summary>
        /// <returns></returns>
        public DtNode MakeDecisionTree()
        {
            return MakeRoot(HorseData, new HashSet<string>(DiagnosInstance.PropertyNames));
        }

        /// <summary>
        /// Construct the root of each subtree.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="propertiesSet"></param>
        /// <returns></returns>
        private DtNode MakeRoot(List<DiagnosInstance> list, HashSet<string> propertiesSet)
        {
            // No samples, return healthy as the default classification
            if (list.Count == 0)
            {
                return new DtNode(true);
            }

            // If all samples are within the same class, return a final node with the classification
            if (list.TrueForAll(diagnosInstance => diagnosInstance.Result) || list.TrueForAll(diagnosInstance => !diagnosInstance.Result))
            {
                return new DtNode(list[0].Result);
            }

            // No attributes, return the majority classification
            if (propertiesSet.Count == 0)
            {
                var pos = list.Count(diagnosInstance => diagnosInstance.Result);
                return pos >= list.Count() / 2 ? new DtNode(true) : new DtNode(false);
            }

            var p = list.Count(diagnosInstance => diagnosInstance.Result);            // Number of positive instance, aka, colic
            var n = list.Count - p;                                                                           // Number of negative instance, aka, healthy
            var ic = InformationContent(p, n);                                                        // Information content of current data set

			var maxIga = double.MinValue;                                                            // Max information gain
			var bestAttribute = string.Empty;                                                         // Best attribute
			var bestThreshold = 0.0;                                                                      // Best threshold

            foreach (var attributeName in propertiesSet)
            {
                // Get all distinct values for certain attribute
				var valuesForAttributeName = list.Select(diagnosInstance => diagnosInstance.ValueOfPropertyByName(attributeName)).Distinct().ToList(); 
				// Sort all values in non-descending order
				valuesForAttributeName.Sort ((first, second) => Comparer<double>.Default.Compare (first, second));

				for (int i = 1; i < valuesForAttributeName.Count; ++i) 
				{
					var mid = (valuesForAttributeName [i - 1] + valuesForAttributeName [i]) / 2;
					// Get all instance of which the value of <attributeName> is GreaterThanOrEqualTo or Less than mid
					var greaterOrEqualTo = list.Where (diagnosInstance => diagnosInstance.ValueOfPropertyByName (attributeName) >= mid).ToList ();
					var less = list.Where (diagnosInstance => diagnosInstance.ValueOfPropertyByName (attributeName) < mid).ToList ();	
					// Get the number of positive and negative instance for both groups above
					var posG = greaterOrEqualTo.Count (diagnosInstance => diagnosInstance.Result);
					var negG = greaterOrEqualTo.Count - posG;
					var posL = less.Count (diagnosInstance => diagnosInstance.Result);
					var negL = less.Count - posL;
					var iga = ic - (((double)posG + negG) / (p + n) * InformationContent (posG, negG) + ((double)posL + negL) / (p + n) * InformationContent (posL, negL));
					if (iga > maxIga) 
					{
						maxIga = iga;
						bestThreshold = mid;
						bestAttribute = attributeName;
					}
				}
            }

			propertiesSet.Remove(bestAttribute);
		
			var newNode = new DtNode(bestAttribute, bestThreshold);
			newNode.GreaterOrEqualTo = MakeRoot(list.Where(diagnosInstance => diagnosInstance.ValueOfPropertyByName(bestAttribute) >= bestThreshold).ToList(), propertiesSet);
			newNode.Less = MakeRoot(list.Where(diagnosInstance => diagnosInstance.ValueOfPropertyByName(bestAttribute) < bestThreshold).ToList(), propertiesSet);

            return newNode;
        }

        /// <summary>
        /// Perform test on decision tree using test set.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="dataPath"></param>
        public void PerformTest(DtNode root, string dataPath)
        {
            var testData = new List<DiagnosInstance>();

            using (var sw = new StreamReader(dataPath))
            {
                string newLine;
                while ((newLine = sw.ReadLine()) != null)
                {
                    var dataArray = newLine.Split(',');
                    var dataArrayAsDouble = new double[dataArray.Length - 1];
                    for (int i = 0; i < dataArray.Length - 1; ++i)
                    {
                        dataArrayAsDouble[i] = double.Parse(dataArray[i]);
                    }
                    testData.Add(new DiagnosInstance(dataArray.Last().Equals("colic."), dataArrayAsDouble));
                }
            }

			var numOfSuccess = 0;
			var numOfFailure = 0;
            for (int i = 0; i < testData.Count; ++i)
            {
                var diagnosInstance = testData[i];
				var result = TestOnInstance (root, diagnosInstance);
				if (result == diagnosInstance.Result) {
					Console.WriteLine ("The classification on instance {0} is successful!", i + 1);
					Console.WriteLine ("The actual classification is {0}, the decision tree classification is: {1}", diagnosInstance.Result, result);
					numOfSuccess++;
				} 
				else
				{
					Console.WriteLine ("The classification on instance {0} is failed!", i + 1);
					Console.WriteLine ("The actual classification is {0}, the decision tree classification is: {1}", diagnosInstance.Result, result);
					numOfFailure++;
				}
            }

			Console.WriteLine("Success: {0}, Failed: {1}, Rate: {2}", numOfSuccess, numOfFailure, (double)numOfSuccess / (numOfSuccess + numOfFailure));
        }

        /// <summary>
        /// Test the classification of certain instance, return the result of classification.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="diagnosInstance"></param>
        /// <returns></returns>
        private bool TestOnInstance(DtNode root, DiagnosInstance diagnosInstance)
        {
            while (!root.Classification.HasValue)
            {
                var attributeName = root.AttributeName;
                root = diagnosInstance.ValueOfPropertyByName(attributeName) >= root.Threshold ? root.GreaterOrEqualTo : root.Less;
            }

            return root.Classification.Value;
        }
			
        /// <summary>
        /// Calculate the Information Content of a data set given the number of Positive and Negative instance
        /// </summary>
        /// <param name="p"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        private double InformationContent(int p, int n)
        {
            return -(((double)p / (p + n)) * (p != 0 ? Math.Log((double) p / (p + n), 2) : 0) + ((double)n / (p + n)) * (n != 0 ? Math.Log((double) n / (p + n), 2) : 0));
        }
    }
}
