using System;

namespace AI5
{
    static class Extension
    {
        public static bool InRange(this int x)
        {
            return x < 3 && x >= 0;
        }

        public static double ValueOfPropertyByName(this DiagnosInstance diagnosInstance, string propertyName)
        {
            return Convert.ToDouble(diagnosInstance.GetType().GetProperty(propertyName).GetValue(diagnosInstance));
        }
    }
}
