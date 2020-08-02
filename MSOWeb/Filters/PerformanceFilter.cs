using MSOCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MSOWeb.Filters
{
    public class PerformanceFilter : IActionFilter
    {
        private Stopwatch stopWatch = new Stopwatch();

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            stopWatch.Reset();
            stopWatch.Start();
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            stopWatch.Stop();
            var executionTime = stopWatch.ElapsedMilliseconds;

            var context = DataEntitiesProvider.Provide();
            context.Database.ExecuteSqlCommand($"INSERT INTO [PageLoadTime] VALUES ('{DateTime.Now.ToString("yyyy-MM-dd HH:mm")}', '{filterContext.HttpContext.Request.Url}', {executionTime})");

        }
    }

    public class PerformanceTestFilterProvider : IFilterProvider
    {
        public IEnumerable<Filter> GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            return new[] { new Filter(new PerformanceFilter(), FilterScope.Global, 0) };
        }
    }
}