using Bookstore.Application.Common.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Bookstore.PublicAPI.Filters
{
    public class ValidationFilter : IActionFilter
    {
        private readonly IServiceProvider serviceProvider;
        public ValidationFilter(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            foreach (var arg in context.ActionArguments.Values)
            {
                if (arg == null)
                {
                    continue;
                }

                var validatorType = typeof(IValidator<>).MakeGenericType(arg.GetType());
                var validator = serviceProvider.GetService(validatorType);

                if (validator == null)
                {
                    continue;
                }

                var method = validatorType.GetMethod("Validate")!;
                var errors = (IReadOnlyList<string>)method.Invoke(validator, [arg])!;

                if (errors.Count > 0)
                {
                    context.Result = new BadRequestObjectResult(new { errors });
                    return;
                }
            }
        }
        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
