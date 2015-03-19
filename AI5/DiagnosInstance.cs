using System.Text;

namespace AI5
{
    class DiagnosInstance
    {
        private readonly int _numOfItems;

        public double K { get; private set; }
        public double Na { get; private set; }
        public double Cl { get; private set; }
        public double Hco3 { get; private set; }
        public double Endotoxin { get; private set; }
        public double Aniongap { get; private set; }
        public double Pla2 { get; private set; }
        public double Sdh { get; private set; }
        public double Gldh { get; private set; }
        public double Tpp { get; private set; }
        public double BreathRate { get; private set; }
        public double Pcv { get; private set; }
        public double PulseRate { get; private set; }
        public double Fibrinogen { get; private set; }
        public double Dimer { get; private set; }
        public double FibPerDim { get; private set; }
        public bool Result { get; private set; }

        /// <summary>
        /// Initialize a new instance of DiagnosInstance using data provided.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="parameters"></param>
        public DiagnosInstance(bool result, params double[] parameters)
        {
            var typeOfThisClass = this.GetType();
            for (int i = 0; i < parameters.Length; ++i)
            {
                typeOfThisClass.GetProperty(PropertyNames[i]).SetValue(this, parameters[i], null);
            }
            this.Result = result;
            _numOfItems = parameters.Length;
        }

        /// <summary>
        /// String representation of this class
        /// </summary>
        /// <returns>String representation of this class</returns>
        public override string ToString()
        {
            var diagnostics = new StringBuilder();
            var typeOfThisClass = this.GetType();
            for (int i = 0; i < _numOfItems; ++i)
            {
                var property = typeOfThisClass.GetProperty(PropertyNames[i]);
                diagnostics.Append(property.Name + ":" +
                                   property.GetValue(this) + "\n");
            }

            return diagnostics.ToString();
        }

        public static readonly string[] PropertyNames =
        {
            "K", "Na", "Cl", "Hco3", "Endotoxin", "Aniongap", "Pla2",
            "Sdh", "Gldh", "Tpp", "BreathRate", "Pcv", "PulseRate", "Fibrinogen",
            "Dimer", "FibPerDim"
        };
    }
}
