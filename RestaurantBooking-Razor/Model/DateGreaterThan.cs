using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace RestaurantBooking_Razor.Model
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DateGreaterThan : ValidationAttribute, IClientValidatable
    {

        private string DateToCompareFieldName { get; set; }

        public DateGreaterThan(string dateToCompareFieldName)
        {
            DateToCompareFieldName = dateToCompareFieldName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            DateTime laterDate = DateTime.ParseExact((string)value, "HH:mm:ss", CultureInfo.InvariantCulture);

            DateTime earlierDate = (DateTime)validationContext.ObjectType.GetProperty(DateToCompareFieldName).GetValue(validationContext.ObjectInstance, null);

            if (laterDate > earlierDate)
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult(string.Format("{0} should be lower!", DateToCompareFieldName));
            }
        }

        //esse método retorna as validações que serão utilizadas no lado cliente
        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var clientValidationRule = new ModelClientValidationRule()
            {
                ErrorMessage = string.Format("{0} should be lower!", DateToCompareFieldName),
                ValidationType = "dategreaterthan"
            };

            clientValidationRule.ValidationParameters.Add("datetocomparefieldname", DateToCompareFieldName);

            return new[] { clientValidationRule };
        }

    }

}
