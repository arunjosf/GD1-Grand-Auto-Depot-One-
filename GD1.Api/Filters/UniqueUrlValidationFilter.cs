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
            // Only validate POST, PUT, PATCH requests
            var method = context.HttpContext.Request.Method;
            if (method == "GET" || method == "DELETE") return;

            var urls = new List<string>();
            var visited = new HashSet<object>();

            // Scan all action arguments
            foreach (var arg in context.ActionArguments.Values)
            {
                if (arg != null)
                {
                    if (arg is Microsoft.EntityFrameworkCore.DbContext) continue;
                    ExtractUrls(arg, urls, visited);
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

        private void ExtractUrls(object obj, List<string> urls, HashSet<object> visited)
        {
            if (obj == null) return;
            if (obj.GetType().IsValueType) return; // Value types don't have cyclic reference issues we care about here
            if (visited.Contains(obj)) return; // Prevent infinite recursion
            
            // Skip traversing file uploads and streams which can cause deep/infinite reflection issues
            if (obj is Microsoft.AspNetCore.Http.IFormFile || obj is System.IO.Stream) return;

            visited.Add(obj);

            var type = obj.GetType();

            if (type == typeof(string)) return;

            if (obj is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    ExtractUrls(item, urls, visited);
                }
                return;
            }

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                 .Where(p => p.CanRead && p.GetIndexParameters().Length == 0); // Ignore indexers

            foreach (var prop in properties)
            {
                if (prop.PropertyType == typeof(string))
                {
                    try 
                    {
                        var val = prop.GetValue(obj) as string;
                        if (!string.IsNullOrWhiteSpace(val))
                        {
                            bool isUrlProperty = prop.Name.EndsWith("Url", System.StringComparison.OrdinalIgnoreCase) ||
                                                 prop.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.UrlAttribute), true).Any();

                            if (isUrlProperty)
                            {
                                urls.Add(val);
                            }
                        }
                    } 
                    catch { /* Ignore properties that throw on read */ }
                }
                else if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
                {
                    try 
                    {
                        var val = prop.GetValue(obj);
                        ExtractUrls(val, urls, visited);
                    }
                    catch { /* Ignore properties that throw on read */ }
                }
            }
        }
    }
}
