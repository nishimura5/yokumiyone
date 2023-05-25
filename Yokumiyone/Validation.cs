using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace Validation
{
    public class NumericValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (Regex.IsMatch(value.ToString(), @"^([1-9]\d*)$"))
            {
                return new ValidationResult(true, null);

            }
            else
            {
                return new ValidationResult(false, @"整数を入力してください");
            }
        }
    }
    public class SceneNameValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string[] ng = new string[] { ",", "\"", "\'", "\\", ">", "<", "&" };
            if (ng.Where(x => ((string)value).Contains(x) == true).Count() > 0)
            {
                return new ValidationResult(false, @"次の記号は使用できません(, \ ' "" < > &)");
            }
            else
            {
                return new ValidationResult(true, null);
            }
        }
    }
}
