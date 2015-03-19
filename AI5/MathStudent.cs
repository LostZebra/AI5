using System.Text;

namespace AI5
{
    class MathStudent
    {
        private readonly int _numOfItems;

        public int School { get; private set; }
        public int Sex { get; private set; }
        public int Age { get; private set; }
        public int Home { get; private set; }
        public int FamilySize { get; private set; }
        public int CohabitationStatus { get; private set; }
        public int MotherEducation { get; private set; }
        public int FatherEducation { get; private set; }
        public int PrimaryCaretaker { get; private set; }
        public int GenderCareTaker { get; private set; }
        public int TravelTime { get; private set; }
        public int StudyTime { get; private set; }
        public int Failures { get; private set; }
        public int ExtraEduSupport { get; private set; }
        public int FamilyEduSupport { get; private set; }
        public int ExtraPaidClasses { get; private set; }
        public int ExtraCurrActivities { get; private set; }
        public int AttendedNurserySchool { get; private set; }
        public int SeekHigherEdu { get; private set; }
        public int InternetAccess { get; private set; }
        public int InRelationship { get; private set; }
        public int QualityOfFamily { get; private set; }
        public int AmountOfFreeTime { get; private set; }
        public int FrequencyOfOut { get; private set; }
        public int AlcholComsumptionWeekdays { get; private set; }
        public int AlcholComsumptionSchoolDays { get; private set; }
        public int HealthStatus { get; private set; }
        public int ClassMissed { get; private set; }
        public bool Result { get; private set; }

        /// <summary>
        /// Initialize a new instance of MathStudent using data provided.
        /// </summary>
        /// <param name="parameters"></param>
        public MathStudent(bool result, params int[] parameters)
        {
            var typeOfThisClass = this.GetType();
            for (int i = 0; i < parameters.Length - 1; ++i)
            {
                typeOfThisClass.GetProperty(PropertyNames[i]).SetValue(this, parameters[i], null);
            }
            this.Result = result;
            _numOfItems = parameters.Length;
        }

        /// <summary>
        /// String representation of this class
        /// </summary>
        /// <returns></returns>
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
            "School", "Sex", "Age", "Home", "FamilySize", "CohabitationStatus", "MotherEducation", 
            "FatherEducation", "PrimaryCaretaker", "GenderCareTaker", "TravelTime", "StudyTime", "Failures", "ExtraEduSupport", 
            "FamilyEduSupport", "ExtraPaidClasses", "ExtraCurrActivities", "AttendedNurserySchool", "SeekHigherEdu", "InternetAccess", "InRelationship", 
            "QualityOfFamily", "AmountOfFreeTime", "FrequencyOfOut", "AlcholComsumptionWeekdays", "AlcholComsumptionSchoolDays", "HealthStatus", "ClassMissed"
        };
    }
}
