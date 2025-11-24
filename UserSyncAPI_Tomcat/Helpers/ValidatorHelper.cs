using System.ComponentModel.DataAnnotations;

namespace UserSyncAPI_Tomcat.Helpers
{
    public class ValidatorHelper
    {
        public static IList<ValidationResult> ValidateModel(object model)
        {
            var results = new List<ValidationResult>();
            if (model == null)
            {
                results.Add(new ValidationResult("Request body cannot be null."));
                return results;
            }
            var context = new ValidationContext(model, serviceProvider: null, items: null);
            Validator.TryValidateObject(model, context, results, validateAllProperties: true);
            return results;
        }
    }
}