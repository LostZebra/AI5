using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AI5
{
    /// <summary>
    /// Directions of movement
    /// </summary>
    internal enum MoveDirection
    {
        Up = 0, Down, Left, Right, Stay
    }

    internal class ValueIteration
    {
        // Discount factor
        private const double Discount = 0.9;

        // Define the given reward matrix, the top-left corner value r
        // can be reassigned in the constructor.
        private readonly double[,] _rewardMatrix =
        {
            {100, -1, 10},
            {-1, -1, -1},
            {-1, -1, -1}
        };

        public ValueIteration(int r = 100)
        {
            _rewardMatrix[0, 0] = r;
        }

        public void PerformIteration()
        {
            int stepCount = 1;                                                  // Step

            var prevMatrix = _rewardMatrix.Clone() as double[,];                // Vt
            var currMatrix = new double[3, 3];                                  // Vt+1
            currMatrix[0, 2] = 10;

            Debug.Assert(prevMatrix != null, "prevMatrix != null");

            MoveDirection[,] bestDirectionMatrix = new MoveDirection[3, 3];     // Best direction to move next for each point
            while (stepCount != 26)
            {
                // For each point, calculate the maximum expected utility earned after certain
                // steps as indicated by stepCount
                for (int row = 0; row < 3; ++row)
                {
                    for (int col = 0; col < 3; ++col)
                    {
                        if (row == 0 && col == 2)
                        {
                            continue;
                        }
                        double maxVal = double.MinValue;
                        MoveDirection bestDirection = MoveDirection.Stay;
                        // For each chosen direction to move
                        foreach (var direction in _directionsAsInt)
                        {
                            double value = _rewardMatrix[row, col];             // ri
                            var actualDirection = (MoveDirection)direction;
                            // For each of the next possible positions
                            for (int i = 0; i < _probabilityDistributionMatrix[row, col][actualDirection].Length; ++i)
                            {
                                var nextPosProbability = _probabilityDistributionMatrix[row, col][actualDirection][i];
                                var nextPos = (MoveDirection)i;
                                value += Math.Abs(nextPosProbability - 0.0)  < 1e-13 
                                    ? 0
                                    : Discount * nextPosProbability * prevMatrix[row + _directionToOffset[nextPos][0], col + _directionToOffset[nextPos][1]];
                            }
                            if (value > maxVal)
                            {
                                maxVal = value;
                                bestDirection = actualDirection;
                            }
                        }
                        currMatrix[row, col] = maxVal;
                        bestDirectionMatrix[row, col] = bestDirection;
                    }
                }
                prevMatrix = currMatrix;
                currMatrix = new double[3, 3];
                currMatrix[0, 2] = 10;
                stepCount++;
            }

            for (int row = 0; row < 3; ++row)
            {
                for (int col = 0; col < 3; ++col)
                {
                    Console.Write(prevMatrix[row, col]);
                    Console.Write(col == 2 ? "\n" : " ");
 
                }
            }

            Console.WriteLine("=======================华丽丽的分割线=======================");

            for (int row = 0; row < 3; ++row)
            {
                for (int col = 0; col < 3; ++col)
                {
                    Console.Write(bestDirectionMatrix[row, col]);
                    Console.Write(col == 2 ? "\n" : " ");
                }
            }
        }

        // Directions represented as Integers
        private readonly int[] _directionsAsInt = { 0, 1, 2, 3 };

        // The offset matrix of x-axis and y-axis after taking certain
        // direction.
        private readonly Dictionary<MoveDirection, int[]> _directionToOffset = new Dictionary<MoveDirection, int[]>
        {
            { MoveDirection.Up, new[]{-1, 0} },
            { MoveDirection.Down, new[]{1, 0} },
            { MoveDirection.Left, new[]{0, -1} },
            { MoveDirection.Right, new[]{0, 1} },
            { MoveDirection.Stay, new[]{0, 0} }
        };

        // The probability distribution matrix, it defines the probability
        // of reaching the next possible position(s) starting from current 
        // position after taking certain direction of movement, the sequence 
        // of the next possible positions are Up, Down, Left, Right 
        private readonly Dictionary<MoveDirection, double[]>[,] _probabilityDistributionMatrix =
        {
            {
                new Dictionary<MoveDirection, double[]>
                {
                    {MoveDirection.Up, new[]{0.0, 0.0, 0.0, 0.1, 0.9}},
                    {MoveDirection.Down, new[]{0.0, 0.8, 0.0, 0.1, 0.1}},
                    {MoveDirection.Left, new[]{0.0, 0.1, 0.0, 0.0, 0.9}},
                    {MoveDirection.Right, new[]{0.0, 0.1, 0.0, 0.8, 0.1}}
                }, 
                new Dictionary<MoveDirection, double[]>
                {
                    {MoveDirection.Up, new[]{0.0, 0.0, 0.1, 0.1, 0.8}},
                    {MoveDirection.Down, new[]{0.0, 0.8, 0.1, 0.1, 0.0}},
                    {MoveDirection.Left, new[]{0.0, 0.1, 0.8, 0.0, 0.1}},
                    {MoveDirection.Right, new[]{0.0, 0.1, 0.0, 0.8, 0.1}}
                } ,
                new Dictionary<MoveDirection, double[]>
                {
                    {MoveDirection.Up, new[]{0.0, 0.0, 0.1, 0.0, 0.9}},
                    {MoveDirection.Down, new[]{0.0, 0.8, 0.1, 0.0, 0.1}},
                    {MoveDirection.Left, new[]{0.0, 0.1, 0.8, 0.0, 0.1}},
                    {MoveDirection.Right, new[]{0.0, 0.1, 0.0, 0.0, 0.9}}
                }
            },
            {    
                new Dictionary<MoveDirection, double[]>
                {
                    {MoveDirection.Up, new[]{0.8, 0.0, 0.0, 0.1, 0.1}},
                    {MoveDirection.Down, new[]{0.0, 0.8, 0.0, 0.1, 0.1}},
                    {MoveDirection.Left, new[]{0.1, 0.1, 0.0, 0.0, 0.8}},
                    {MoveDirection.Right, new[]{0.1, 0.1, 0.0, 0.8, 0.0}}
                }, 
                new Dictionary<MoveDirection, double[]>
                {
                    {MoveDirection.Up, new[]{0.8, 0.0, 0.1, 0.1, 0.0}},
                    {MoveDirection.Down, new[]{0.0, 0.8, 0.1, 0.1, 0.0}},
                    {MoveDirection.Left, new[]{0.1, 0.1, 0.8, 0.0, 0.0}},
                    {MoveDirection.Right, new[]{0.1, 0.1, 0.0, 0.8, 0.0}}
                }, 
                new Dictionary<MoveDirection, double[]>
                {
                    {MoveDirection.Up, new[]{0.8, 0.0, 0.1, 0.0, 0.1}},
                    {MoveDirection.Down, new[]{0.0, 0.8, 0.1, 0.0, 0.1}},
                    {MoveDirection.Left, new[]{0.1, 0.1, 0.8, 0.0, 0.0}},
                    {MoveDirection.Right, new[]{0.1, 0.1, 0.0, 0.0, 0.8}}
                }
            },
            {
                new Dictionary<MoveDirection, double[]>
                {
                    {MoveDirection.Up, new[]{0.8, 0.0, 0.0, 0.1, 0.1}},
                    {MoveDirection.Down, new[]{0.0, 0.0, 0.0, 0.1, 0.9}},
                    {MoveDirection.Left, new[]{0.1, 0.0, 0.0, 0.0, 0.9}},
                    {MoveDirection.Right, new[]{0.1, 0.0, 0.0, 0.8, 0.1}}
                }, 
                new Dictionary<MoveDirection, double[]>
                {
                    {MoveDirection.Up, new[]{0.8, 0.0, 0.1, 0.1, 0.0}},
                    {MoveDirection.Down, new[]{0.0, 0.0, 0.1, 0.1, 0.8}},
                    {MoveDirection.Left, new[]{0.1, 0.0, 0.8, 0.0, 0.1}},
                    {MoveDirection.Right, new[]{0.1, 0.0, 0.0, 0.8, 0.1}}
                }, 
                new Dictionary<MoveDirection, double[]>
                {
                    {MoveDirection.Up, new[]{0.8, 0.0, 0.1, 0.0, 0.1}},
                    {MoveDirection.Down, new[]{0.0, 0.0, 0.1, 0.0, 0.9}},
                    {MoveDirection.Left, new[]{0.1, 0.0, 0.8, 0.0, 0.1}},
                    {MoveDirection.Right, new[]{0.1, 0.0, 0.0, 0.0, 0.9}}
                }
            }
        };
    }
}
