using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Globalization;

namespace Validation
{
    public class SceneNameValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string[] ng = new string[] { ",", "\"", "\'" , "\\", ">", "<", "&"};
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
