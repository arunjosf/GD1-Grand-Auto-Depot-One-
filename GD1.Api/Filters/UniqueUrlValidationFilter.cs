using GD1.Application.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GD1.Api.Filters
{
    public class UniqueUrlValidationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var urls = new List<string>();

            // Scan all action arguments
            foreach (var arg in context.ActionArguments.Values)
            {
                if (arg != null)
                {
                    ExtractUrls(arg, urls);
                }
            }

            // Filter out empty URLs
            var validUrls = urls.Where(u => !string.IsNullOrWhiteSpace(u)).ToList();

            // Check for duplicates
            var duplicates = validUrls.GroupBy(x => x)
                                      .Where(g => g.Count() > 1)
                                      .Select(y => y.Key)
                                      .ToList();

            if (duplicates.Any())
            {
                context.Result = new BadRequestObjectResult(
                    BaseResponse<string>.Fail("Duplicate URLs found in the request. Each file/image URL must be unique.")
                );
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Do nothing
        }

        private void ExtractUrls(object obj, List<string> urls)
        {
            if (obj == null) return;

            var type = obj.GetType();

            // If it's a string, check if it looks like a URL property based on naming or we just ignore direct string arguments
            // We usually care about object properties.
            if (type == typeof(string)) return;

            // If it's a collection, iterate through it
            if (obj is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    ExtractUrls(item, urls);
                }
                return;
            }

            // Process object properties
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                 .Where(p => p.CanRead);

            foreach (var prop in properties)
            {
                if (prop.PropertyType == typeof(string))
                {
                    var val = prop.GetValue(obj) as string;
                    if (!string.IsNullOrWhiteSpace(val))
                    {
                        // Check if property name ends with "Url" or has [Url] attribute
                        bool isUrlProperty = prop.Name.EndsWith("Url", System.StringComparison.OrdinalIgnoreCase) ||
                                             prop.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.UrlAttribute), true).Any();

                        if (isUrlProperty)
                        {
                            urls.Add(val);
                        }
                    }
                }
                else if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
                {
                    // Prevent infinite recursion on self-referencing types if any, but typical DTOs are safe
                    var val = prop.GetValue(obj);
                    ExtractUrls(val, urls);
                }
            }
        }
    }
}
