using NCalc;

namespace Lib.Utilities
{
    public class MedicalUtil
    {
        /// <summary>
        /// 劑量格式化
        /// </summary>
        /// <param name="dose">分子</param>
        /// <param name="doseDe">分母</param>
        /// <param name="doseUnit">單位</param>
        /// <param name="fraction">分數格式</param>
        /// <returns></returns>
        public string DoseFormat(object dose, object doseDe, bool fraction = true, string doseUnit = "")
        {
            string result = string.Empty;
            double dDose, dDoseDe;

            try
            {
                if (double.TryParse(dose.NullableToStr(), out dDose))
                    result += dDose.NullableToStr();
                else
                    result += dose.NullableToStr();

                if (double.TryParse(doseDe.NullableToStr(), out dDoseDe))
                {
                    if (dDoseDe != 1)
                        result += "/" + dDoseDe.NullableToStr();
                }
                else
                    result += "/" + doseDe.NullableToStr();

                if (!fraction)
                {
                    Expression e = new Expression(result);
                    var exp = e.Evaluate().NullableToStr();
                    if (!double.IsInfinity(double.Parse(exp)))
                        result = exp;
                }
            }
            catch (Exception) { }
            finally
            {
                if (result == "/") result = string.Empty;
                if (doseUnit != string.Empty)
                    result += doseUnit;
            }

            return result;
        }

    }
}
