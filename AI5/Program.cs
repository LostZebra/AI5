namespace AI5
{
    class Program
    {
        static void Main(string[] args)
        {
            var dt = DecisionTreeForEqd.CreateFromDataInstance(@"E:\horseTrain.txt");
            var root = dt.MakeDecisionTree();
            dt.PerformTest(root, @"E:\horseTrain.txt");
            dt.PerformTest(root, @"E:\horseTest.txt");

            var vi = new ValueIteration();
            vi.PerformIteration();
        }
    }
}
