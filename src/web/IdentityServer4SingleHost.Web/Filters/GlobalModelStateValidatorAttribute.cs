using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IdentityServer4SingleHost.Web.Filters
{
    public class GlobalModelStateValidatorAttribute : Attribute, IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {

        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                Controller controller = context.Controller as Controller;

                object model = context.ActionArguments.Any()
                    ? context.ActionArguments.First().Value
                    : null;

                context.Result = (IActionResult) controller?.View(model)
                                 ?? new BadRequestResult();
            };

        }
    }
}
