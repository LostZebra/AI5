namespace AI5
{
    class Program
    {
        static void Main(string[] args)
        {
			/*
			var dt = DecisionTreeForH.CreateFromHorseInstance(@"/Users/xiaoyong/Downloads/horseTrain.txt");
			var root = dt.MakeDecisionTree();
			dt.PerformTest(root, @"/Users/xiaoyong/Downloads/horseTrain.txt");
			dt.PerformTest(root, @"/Users/xiaoyong/Downloads/horse.txt");
			*/

			var dt2 = DecisionTreeForMs.CreateFromStudentInstance(@"/Users/xiaoyong/Downloads/porto_math_train.csv");
            var root2 = dt2.MakeDecisionTree();
			dt2.PerformTest(root2, @"/Users/xiaoyong/Downloads/porto_math_train.csv");
			dt2.PerformTest(root2, @"/Users/xiaoyong/Downloads/porto_math_test.csv");

            var vi = new ValueIteration();
            vi.PerformIteration();
        }
    }
}
