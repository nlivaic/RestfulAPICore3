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
        private readonly IList<MediaTypeHeaderValue> _mediaTypeValues = new List<MediaTypeHeaderValue>();

        public int Order => 0;

        public RequestHeaderMatchesMediaTypeAttribute(string requestHeader, string mediaTypeValue, params string[] otherMediaTypeValues)
        {
            _requestHeader = requestHeader;
            if (otherMediaTypeValues.Length > 0 && !MediaTypeHeaderValue.TryParseList(otherMediaTypeValues, out _mediaTypeValues))
            {
                throw new ArgumentException($"Invalid media types: {string.Join(',', otherMediaTypeValues)} declared in {nameof(RequestHeaderMatchesMediaTypeAttribute)}.");
            }
            if (!MediaTypeHeaderValue.TryParse(mediaTypeValue, out MediaTypeHeaderValue mediaTypeHeaderValue))
            {
                throw new ArgumentException($"Invalid media type: {mediaTypeValue} declared in {nameof(RequestHeaderMatchesMediaTypeAttribute)}.");
            }
            _mediaTypeValues.Add(mediaTypeHeaderValue);
        }

        public bool Accept(ActionConstraintContext context)
        {
            if (!context.RouteContext.HttpContext.Request.Headers.TryGetValue(_requestHeader, out StringValues mediaTypeStringValueFromRequest))
            {
                return false;
            }
            var mediaTypes = mediaTypeStringValueFromRequest.ToString().Split(",");
            IList<MediaTypeHeaderValue> parsedMediaTypes = new List<MediaTypeHeaderValue>();
            if (!MediaTypeHeaderValue.TryParseList(mediaTypes, out parsedMediaTypes))
            {
                return false;
            }
            return _mediaTypeValues.Any(
                m => mediaTypes.Contains(m.ToString()));
        }
    }
}
