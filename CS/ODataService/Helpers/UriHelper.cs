using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.OData.UriParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Routing;

namespace ODataService.Helpers {
    public static class UriHelper {
        public static TKey GetKeyFromUri<TKey>(HttpRequestMessage request, Uri uri) {
            if(uri == null) {
                throw new ArgumentNullException("uri");
            }
            var urlHelper = request.GetUrlHelper() ?? new UrlHelper(request);
            var pathHandler = (IODataPathHandler)request.GetRequestContainer().GetService(typeof(IODataPathHandler));
            string serviceRoot = urlHelper.CreateODataLink(
                request.ODataProperties().RouteName,
                pathHandler, new List<ODataPathSegment>());
            var odataPath = pathHandler.Parse(serviceRoot, uri.LocalPath, request.GetRequestContainer());
            var keySegment = odataPath.Segments.OfType<KeySegment>().FirstOrDefault();
            if(keySegment == null) {
                throw new InvalidOperationException("The link does not contain a key.");
            }
            var value = keySegment.Keys.FirstOrDefault().Value;
            return (TKey)value;
        }

        public static string GetServiceRootUri(ODataController controller) {
            var routeName = controller.Request.ODataProperties().RouteName;
            ODataRoute odataRoute = controller.Configuration.Routes[routeName] as ODataRoute;
            var prefixName = odataRoute.RoutePrefix;
            if(!string.IsNullOrEmpty(prefixName)) {
                var requestUri = controller.Request.RequestUri.ToString();
                var serviceRootUri = requestUri.Substring(0, requestUri.IndexOf(prefixName, StringComparison.InvariantCultureIgnoreCase) + prefixName.Length);
                return serviceRootUri;
            } else {
                return controller.Url.Content("~/");
            }
        }
    }
}