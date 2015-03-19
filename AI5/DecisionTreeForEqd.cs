using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AI5
{
    internal class DtNodeForEqd
    {
        public string AttributeName { get; private set; }
        public double Threshold { get; private set; }
        public DtNodeForEqd GreaterOrEqualTo { get; set; }
        public DtNodeForEqd Less { get; set; }
        public bool? Classification { get; private set; }

        /// <summary>
        /// Initialize a normal node for decision tree which has children, attribute name and threshold.
        /// </summary>
        /// <param name="attributeName"></param>
        /// <param name="threshold"></param>
        public DtNodeForEqd(string attributeName, double threshold)
        {
            this.AttributeName = attributeName;
            this.Threshold = threshold;
        }

        /// <summary>
        /// Initialize an end node for decision tree to represent a final classification
        /// </summary>
        /// <param name="classification"></param>
        public DtNodeForEqd(bool classification)
        {
            this.Classification = classification;
        }
    }

    class DecisionTreeForEqd
    {
        private static readonly List<DiagnosInstance> Data = new List<DiagnosInstance>();

        private static readonly DecisionTreeForEqd Dt = new DecisionTreeForEqd();

        private DecisionTreeForEqd() {}

        /// <summary>
        /// Load data from trainning set.
        /// </summary>
        /// <param name="dataPath"></param>
        /// <returns></returns>
        public static DecisionTreeForEqd CreateFromDataInstance(string dataPath)
        {
            if (Data.Count != 0)
            {
                Data.Clear();
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
                    Data.Add(new DiagnosInstance(!dataArray.Last().Equals("healthy."), dataArrayAsDouble));
                }
            }

            return Dt;
        }

        /// <summary>
        /// Construct the decision tree for the trainning set.
        /// </summary>
        /// <returns></returns>
        public DtNodeForEqd MakeDecisionTree()
        {
            return MakeRoot(Data, new HashSet<string>(DiagnosInstance.PropertyNames));
        }

        /// <summary>
        /// Construct the root of each subtree.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="properitesSet"></param>
        /// <returns></returns>
        private DtNodeForEqd MakeRoot(List<DiagnosInstance> list, HashSet<string> properitesSet)
        {
            // No samples, return healthy as the default classification
            if (list.Count == 0)
            {
                return new DtNodeForEqd(true);
            }

            // If all samples are within the same class, return a final node with the classification
            if (list.TrueForAll(diagnosInstance => diagnosInstance.Result) || list.TrueForAll(diagnosInstance => !diagnosInstance.Result))
            {
                return new DtNodeForEqd(list[0].Result);
            }

            // No attributes, return the majority classification
            if (properitesSet.Count == 0)
            {
                var pos = list.Count(diagnosInstance => diagnosInstance.Result);
                return pos >= list.Count() / 2 ? new DtNodeForEqd(true) : new DtNodeForEqd(false);
            }

            var p = list.Count(diagnosInstance => diagnosInstance.Result);            // Number of positive instance, aka, colic
            var n = list.Count - p;                                                   // Number of negative instance, aka, healthy
            var ic = InformationContent(p, n);                                        // Information content of current data set

            var attributesToIga = new Dictionary<string, double>();                   // Attribute and its corresponding Information Gain

            foreach (var attributeName in properitesSet)
            {
                // Get all values for certain attribute
                var valuesForAttributeName = new HashSet<double>(list.Select(diagnosInstance => diagnosInstance.ValueOfPropertyByName(attributeName)));  
                // Remainder of the attribute
                var remainder = 0.0;
                foreach (var value in valuesForAttributeName)
                {
                    // Calculate the number of positive and negative instance of which the value of <attributeName> equals <value>
                    var itemsMatched = list.Where(diagnosInstance => Math.Abs(diagnosInstance.ValueOfPropertyByName(attributeName) - value) < 1e-13).ToList();
                    var pos = itemsMatched.Count(diagnosInstance => diagnosInstance.Result);
                    var neg = itemsMatched.Count() - pos;
                    remainder += ((double)pos + neg) / (p + n) * InformationContent(pos, neg);
                }
                attributesToIga.Add(attributeName, ic - remainder);
            }

            var attributeChosen = attributesToIga.Aggregate((first, second) => first.Value > second.Value ? first : second).Key;
            
            // <!--Still need to compute the threshold--!>
            var threshold = CalculateThreshold(list, attributeChosen);
            
            properitesSet.Remove(attributeChosen);

            var newNode = new DtNodeForEqd(attributeChosen, threshold);
            newNode.GreaterOrEqualTo = MakeRoot(list.Where(diagnosInstance => diagnosInstance.ValueOfPropertyByName(attributeChosen) >= threshold).ToList(), properitesSet);
            newNode.Less = MakeRoot(list.Where(diagnosInstance => diagnosInstance.ValueOfPropertyByName(attributeChosen) < threshold).ToList(), properitesSet);

            return newNode;
        }

        /// <summary>
        /// Perform test on decision tree using test data.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="dataPath"></param>
        public void PerformTest(DtNodeForEqd root, string dataPath)
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
                    testData.Add(new DiagnosInstance(!dataArray.Last().Equals("healthy."), dataArrayAsDouble));
                }
            }
            for (int i = 0; i < testData.Count; ++i)
            {
                var diagnosInstance = testData[i];
                Console.WriteLine("The classification on {0} is {1}",  i + 1, TestOnInstance(root, diagnosInstance) == diagnosInstance.Result ? "successful" : "failed");
            }
        }

        /// <summary>
        /// Test the classification of certain instance.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="diagnosInstance"></param>
        /// <returns></returns>
        private bool TestOnInstance(DtNodeForEqd root, DiagnosInstance diagnosInstance)
        {
            while (!root.Classification.HasValue)
            {
                var attributeName = root.AttributeName;
                root = diagnosInstance.ValueOfPropertyByName(attributeName) >= root.Threshold ? root.GreaterOrEqualTo : root.Less;
            }

            return root.Classification.Value;
        }

        /// <summary>
        /// Calculate the best threshold for certain attribute.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private double CalculateThreshold(List<DiagnosInstance> list, string propertyName)
        {
            var potentionThresholds = new List<double>();
            // Sort the remaining list on <propertyName>
            list.Sort((element1, element2) => Comparer<double>.Default.Compare(element1.ValueOfPropertyByName(propertyName), element2.ValueOfPropertyByName(propertyName)));
            // Get all potential thresholds by getting the average of every two successive value of <propertyName> 
            list.Aggregate((first, second) =>
            {
                var mid = (first.ValueOfPropertyByName(propertyName) + second.ValueOfPropertyByName(propertyName)) / 2;
                potentionThresholds.Add(mid);
                return second;
            });

            // The threshold with maximum information gain has minimum remainder
            var minRemainder = double.MaxValue;
            var threshold = double.MaxValue;
            // Get the threshold which has the minimum remainder
            foreach (var potentialThreshold in potentionThresholds)
            {
                var greaterOrEqualTo = list.Where(diagnosInstance => diagnosInstance.ValueOfPropertyByName(propertyName) >= potentialThreshold).ToList();
                var less = list.Where(diagnosInstance => diagnosInstance.ValueOfPropertyByName(propertyName) < potentialThreshold).ToList();

                var pos = greaterOrEqualTo.Count(diagnosInstance => diagnosInstance.Result);
                var neg = greaterOrEqualTo.Count - pos;
                var remainder = ((double)greaterOrEqualTo.Count/list.Count)*InformationContent(pos, neg);
                pos = less.Count(diagnosInstance => diagnosInstance.Result);
                neg = less.Count - pos;
                remainder += ((double)less.Count/list.Count)*InformationContent(pos, neg);
                
                if (remainder < minRemainder)
                {
                    minRemainder = remainder;
                    threshold = potentialThreshold;
                }
            }

            return threshold;
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
