using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Markt.Helpers
{
    public class ApiError
    {
        public ApiError(string message = null)
        {
            Message = message;
        }

        public ApiError(ModelStateDictionary modelState)
        {
            Message = modelState
                .FirstOrDefault(x => x.Value.Errors.Any()).Value.Errors
                .FirstOrDefault()
                ?.ErrorMessage;
        }

        public string Message { get; set; }
    }
}
