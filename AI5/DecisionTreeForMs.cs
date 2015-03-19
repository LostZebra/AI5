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
        /// <param name="propertiesSet"></param>
        /// <returns></returns>
		private DtNode MakeRoot(List<MathStudent> list, HashSet<string> propertiesSet)
        {
			// No samples, return healthy as the default classification
			if (list.Count == 0)
			{
				return new DtNode(true);
			}

			// If all samples are within the same class, return a final node with the classification
			if (list.TrueForAll(studentInstance => studentInstance.Result) || list.TrueForAll(studentInstance => !studentInstance.Result))
			{
				return new DtNode(list[0].Result);
			}

			// No attributes, return the majority classification
			if (propertiesSet.Count == 0)
			{
				var pos = list.Count(studentInstance => studentInstance.Result);
				return pos >= list.Count() / 2 ? new DtNode(true) : new DtNode(false);
			}

			var p = list.Count(studentInstance => studentInstance.Result);            // Number of positive instance, aka, colic
			var n = list.Count - p;                                                                           // Number of negative instance, aka, healthy
			var ic = InformationContent(p, n);                                                        // Information content of current data set

			var maxIga = double.MinValue;                                                            // Max information gain
			var bestAttribute = string.Empty;                                                         // Best attribute
			var bestThreshold = 0.0;                                                                      // Best threshold

			foreach (var attributeName in propertiesSet)
			{
				// Get all distinct values for certain attribute
				var valuesForAttributeName = list.Select(studentInstance => studentInstance.ValueOfPropertyByName(attributeName)).Distinct().ToList(); 
				// Sort all values in non-descending order
				valuesForAttributeName.Sort ((first, second) => Comparer<double>.Default.Compare (first, second));

				if (valuesForAttributeName.Count == 1) {
					var mid = valuesForAttributeName [0];
					// Get all instance of which the value of <attributeName> is GreaterThanOrEqualTo or Less than mid
					var greaterOrEqualTo = list.Where (studentInstance => studentInstance.ValueOfPropertyByName (attributeName) >= mid).ToList ();
					var less = list.Where (studentInstance => studentInstance.ValueOfPropertyByName (attributeName) < mid).ToList ();	
					// Get the number of positive and negative instance for both groups above
					var posG = greaterOrEqualTo.Count (studentInstance => studentInstance.Result);
					var negG = greaterOrEqualTo.Count - posG;
					var posL = less.Count (studentInstance => studentInstance.Result);
					var negL = less.Count - posL;
					var iga = ic - (((double)posG + negG) / (p + n) * InformationContent (posG, negG) + ((double)posL + negL) / (p + n) * InformationContent (posL, negL));
					if (iga > maxIga) {
						maxIga = iga;
						bestThreshold = mid;
						bestAttribute = attributeName;
					}
				} 
				else
				{
					for (int i = 1; i < valuesForAttributeName.Count; ++i) 
					{
						var mid = (valuesForAttributeName [i - 1] + valuesForAttributeName [i]) / 2;
						// Get all instance of which the value of <attributeName> is GreaterThanOrEqualTo or Less than mid
						var greaterOrEqualTo = list.Where (studentInstance => studentInstance.ValueOfPropertyByName (attributeName) >= mid).ToList ();
						var less = list.Where (studentInstance => studentInstance.ValueOfPropertyByName (attributeName) < mid).ToList ();	
						// Get the number of positive and negative instance for both groups above
						var posG = greaterOrEqualTo.Count (studentInstance => studentInstance.Result);
						var negG = greaterOrEqualTo.Count - posG;
						var posL = less.Count (studentInstance => studentInstance.Result);
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
			}

			propertiesSet.Remove(bestAttribute);

			var newNode = new DtNode(bestAttribute, bestThreshold);
			newNode.GreaterOrEqualTo = MakeRoot(list.Where(studentInstance => studentInstance.ValueOfPropertyByName(bestAttribute) >= bestThreshold).ToList(), propertiesSet);
			newNode.Less = MakeRoot(list.Where(studentInstance => studentInstance.ValueOfPropertyByName(bestAttribute) < bestThreshold).ToList(), propertiesSet);

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
        /// Calculate the Information Content of a data set given the number of Positive and Negative instance
        /// </summary>
        /// <param name="p"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        private double InformationContent(int p, int n)
        {
			if (p == 0 && n == 0)
			{
				return 0;
			}
            return -(((double)p / (p + n)) * (p != 0 ? Math.Log((double)p / (p + n), 2) : 0) + ((double)n / (p + n)) * (n != 0 ? Math.Log((double)n / (p + n), 2) : 0));
        }
    }
}
