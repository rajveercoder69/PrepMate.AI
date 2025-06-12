using System.ComponentModel.DataAnnotations;
using WebApiCRUDOps.DataUtility.Model;

namespace WebApiCRUDOps.DataUtility
{
    public class UserDefinedValidateioncsAttribute: ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var shirt = validationContext.ObjectInstance as Person;

            if (shirt != null&&!string.IsNullOrWhiteSpace(shirt.gender))
            {
                if(shirt.gender.Equals("men",StringComparison.OrdinalIgnoreCase)&&shirt.size<8)
                {
                    return new ValidationResult("Shirt size for the Men must be greater than 8");
                }
                else if(shirt.price<50&&shirt.gender.Equals("women",StringComparison.OrdinalIgnoreCase))
                {
                    return new ValidationResult("Shirt price for women cannot be less than 50 Rupees");
                }
            }
            return ValidationResult.Success;

        }
    }
}
