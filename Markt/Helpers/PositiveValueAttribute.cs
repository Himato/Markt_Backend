using System;
using System.ComponentModel.DataAnnotations;

namespace Markt.Helpers
{
    public class PositiveValueAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (double.TryParse(value.ToString(), out var x))
            {
                return x > 0;
            }

            return false;
        }
    }
}
