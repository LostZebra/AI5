namespace AI5
{
    class Program
    {
        static void Main(string[] args)
        {
            var dt = DecisionTreeForH.CreateFromHorseInstance(@"E:\horseTrain.txt");
            var root = dt.MakeDecisionTree();
            dt.PerformTest(root, @"E:\horseTrain.txt");
            dt.PerformTest(root, @"E:\horseTest.txt");

            var dt2 = DecisionTreeForMs.CreateFromStudentInstance(@"E:\porto_math_train.csv");
            var root2 = dt2.MakeDecisionTree();
            dt2.PerformTest(root2, @"E:\porto_math_train.csv");
            dt2.PerformTest(root2, @"E:\porto_math_test.csv");

            var vi = new ValueIteration();
            vi.PerformIteration();
        }
    }
}
