using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RestfulAPICore3.API.Constraints
{
    public class RequestHeaderMatchesMediaTypeAttribute : Attribute, IActionConstraint
    {
        private readonly string _requestHeader;
        private readonly List<string> _mediaTypeValues = new List<string>();

        public int Order => 0;

        public RequestHeaderMatchesMediaTypeAttribute(string requestHeader, string mediaTypeValues, params string[] otherMediaTypeValues)
        {
            _requestHeader = requestHeader;
            _mediaTypeValues.Add(mediaTypeValues);
            _mediaTypeValues.AddRange(otherMediaTypeValues);
        }

        public bool Accept(ActionConstraintContext context)
        {
            if (!context.RouteContext.HttpContext.Request.Headers.TryGetValue(_requestHeader, out StringValues mediaTypeStringValue))
            {
                return false;
            }
            var mediaTypes = mediaTypeStringValue.ToString().Split(",");
            IList<MediaTypeHeaderValue> parsedMediaTypes = new List<MediaTypeHeaderValue>();
            if (!MediaTypeHeaderValue.TryParseList(mediaTypes, out parsedMediaTypes))
            {
                return false;
            }
            return _mediaTypeValues.Any(
                m => mediaTypes.Contains(m));
        }
    }
}
