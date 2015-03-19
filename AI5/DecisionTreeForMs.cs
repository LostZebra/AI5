using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AI5
{
    class DecisionTreeForMs
    {
        private static readonly List<MathStudent> StudentData = new List<MathStudent>();

        private static readonly DecisionTreeForMs Dt = new DecisionTreeForMs();

        private DecisionTreeForMs() {}

        /// <summary>
        /// Load data from the trainning set of students.
        /// </summary>
        /// <param name="dataPath"></param>
        /// <returns></returns>
        public static DecisionTreeForMs CreateFromStudentInstance(string dataPath)
        {
            if (StudentData.Count != 0)
            {
                StudentData.Clear();
            }

            using (var sw = new StreamReader(dataPath))
            {
                int count = 0;
                string newLine;
                while ((newLine = sw.ReadLine()) != null)
                {
                    if (count == 0)
                    {
                        count = 1;
                        continue;
                    }

                    var dataArray = newLine.Split(',').Select(int.Parse).ToArray();
                    StudentData.Add(new MathStudent(dataArray.Last() == 1, dataArray));
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
            return MakeRoot(StudentData, new HashSet<string>(MathStudent.PropertyNames));
        }

        /// <summary>
        /// Construct the root of each subtree.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="properitesSet"></param>
        /// <returns></returns>
        private DtNode MakeRoot(List<MathStudent> list, HashSet<string> properitesSet)
        {
            // No samples, return healthy as the default classification
            if (list.Count == 0)
            {
                return new DtNode(true);
            }

            // If all samples are within the same class, return a final node with the classification
            if (list.TrueForAll(mathStudent => mathStudent.Result) || list.TrueForAll(mathStudent => !mathStudent.Result))
            {
                return new DtNode(list[0].Result);
            }

            // No attributes, return the majority classification
            if (properitesSet.Count == 0)
            {
                var pos = list.Count(mathStudent => mathStudent.Result);
                return pos >= list.Count() / 2 ? new DtNode(true) : new DtNode(false);
            }

            var p = list.Count(mathStudent => mathStudent.Result);            // Number of positive instance, aka, colic
            var n = list.Count - p;                                                   // Number of negative instance, aka, healthy
            var ic = InformationContent(p, n);                                        // Information content of current data set

            var attributesToIga = new Dictionary<string, double>();                   // Attribute and its corresponding Information Gain

            foreach (var attributeName in properitesSet)
            {
                // Get all values for certain attribute
                var valuesForAttributeName = new HashSet<int>(list.Select(mathStudent => mathStudent.ValueOfPropertyByName(attributeName)));
                // Remainder of the attribute
                var remainder = 0.0;
                foreach (var value in valuesForAttributeName)
                {
                    // Calculate the number of positive and negative instance of which the value of <attributeName> equals <value>
                    var itemsMatched = list.Where(mathStudent => mathStudent.ValueOfPropertyByName(attributeName) == value).ToList();
                    var pos = itemsMatched.Count(mathStudent => mathStudent.Result);
                    var neg = itemsMatched.Count() - pos;
                    remainder += ((double)pos + neg) / (p + n) * InformationContent(pos, neg);
                }
                attributesToIga.Add(attributeName, ic - remainder);
            }

            var attributeChosen = attributesToIga.Aggregate((first, second) => first.Value > second.Value ? first : second).Key;

            // <!--Still need to compute the threshold--!>
            var threshold = CalculateThreshold(list, attributeChosen);

            properitesSet.Remove(attributeChosen);

            var newNode = new DtNode(attributeChosen, threshold);
            newNode.GreaterOrEqualTo = MakeRoot(list.Where(mathStudent => mathStudent.ValueOfPropertyByName(attributeChosen) >= threshold).ToList(), properitesSet);
            newNode.Less = MakeRoot(list.Where(mathStudent => mathStudent.ValueOfPropertyByName(attributeChosen) < threshold).ToList(), properitesSet);

            return newNode;
        }

        /// <summary>
        /// Perform test on decision tree using test data.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="dataPath"></param>
        public void PerformTest(DtNode root, string dataPath)
        {
            var success = 0;
            var fail = 0;
            var testData = new List<MathStudent>();

            using (var sw = new StreamReader(dataPath))
            {
                int count = 0;
                string newLine;
                while ((newLine = sw.ReadLine()) != null)
                {
                    if (count == 0)
                    {
                        count = 1;
                        continue;
                    }

                    var dataArray = newLine.Split(',').Select(int.Parse).ToArray();
                    testData.Add(new MathStudent(dataArray.Last() == 1, dataArray));
                }
            }

            for (int i = 0; i < testData.Count; ++i)
            {
                var mathStudent = testData[i];
                if (TestOnInstance(root, mathStudent) == mathStudent.Result)
                {
                    Console.WriteLine("The classification on {0} is successful", i + 1);
                    success++;
                }
                else
                {
                    Console.WriteLine("The classification on {0} is failed", i + 1);
                    fail++;
                }
            }

            Console.WriteLine("Success: {0}, Failed: {1}, Rate: {2}", success, fail, (double)success / (success + fail));
        }

        /// <summary>
        /// Test the classification of certain instance.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="mathStudent"></param>
        /// <returns></returns>
        private bool TestOnInstance(DtNode root, MathStudent mathStudent)
        {
            while (!root.Classification.HasValue)
            {
                var attributeName = root.AttributeName;
                root = mathStudent.ValueOfPropertyByName(attributeName) >= root.Threshold ? root.GreaterOrEqualTo : root.Less;
            }

            return root.Classification.Value;
        }

        /// <summary>
        /// Calculate the best threshold for certain attribute.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private double CalculateThreshold(List<MathStudent> list, string propertyName)
        {
            var potentionThresholds = new List<double>();
            // Sort the remaining list on <propertyName>
            list.Sort((element1, element2) => Comparer<int>.Default.Compare(element1.ValueOfPropertyByName(propertyName), element2.ValueOfPropertyByName(propertyName)));
            // Get all potential thresholds by getting the average of every two successive value of <propertyName> 
            list.Aggregate((first, second) =>
            {
                var mid = ((double)first.ValueOfPropertyByName(propertyName) + second.ValueOfPropertyByName(propertyName)) / 2;
                potentionThresholds.Add(mid);
                return second;
            });

            // The threshold with maximum information gain has minimum remainder
            var minRemainder = double.MaxValue;
            var threshold = double.MaxValue;
            // Get the threshold which has the minimum remainder
            foreach (var potentialThreshold in potentionThresholds)
            {
                var greaterOrEqualTo = list.Where(mathStudent => mathStudent.ValueOfPropertyByName(propertyName) >= potentialThreshold).ToList();
                var less = list.Where(mathStudent => mathStudent.ValueOfPropertyByName(propertyName) < potentialThreshold).ToList();

                var pos = greaterOrEqualTo.Count(mathStudent => mathStudent.Result);
                var neg = greaterOrEqualTo.Count - pos;
                var remainder = ((double)greaterOrEqualTo.Count / list.Count) * InformationContent(pos, neg);
                pos = less.Count(mathStudent => mathStudent.Result);
                neg = less.Count - pos;
                remainder += ((double)less.Count / list.Count) * InformationContent(pos, neg);

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
            return -(((double)p / (p + n)) * (p != 0 ? Math.Log((double)p / (p + n), 2) : 0) + ((double)n / (p + n)) * (n != 0 ? Math.Log((double)n / (p + n), 2) : 0));
        }
    }
}
