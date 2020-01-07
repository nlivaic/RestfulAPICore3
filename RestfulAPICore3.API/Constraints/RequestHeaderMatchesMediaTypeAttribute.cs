using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Formatters;
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
        private readonly MediaTypeCollection _mediaTypeValues = new MediaTypeCollection();

        public int Order => 0;

        public RequestHeaderMatchesMediaTypeAttribute(string requestHeader, string mediaTypeValue, params string[] otherMediaTypeValues)
        {
            _requestHeader = requestHeader;
            if (!MediaTypeHeaderValue.TryParse(mediaTypeValue, out MediaTypeHeaderValue mediaTypeHeaderValue))
            {
                throw new ArgumentException($"Invalid media type: {mediaTypeValue} declared in {nameof(RequestHeaderMatchesMediaTypeAttribute)}.");
            }
            _mediaTypeValues.Add(mediaTypeHeaderValue);
            foreach (var otherMediaTypeValue in otherMediaTypeValues)
            {
                if (!MediaTypeHeaderValue.TryParse(otherMediaTypeValue, out mediaTypeHeaderValue))
                {
                    throw new ArgumentException($"Invalid media type: {mediaTypeValue} declared in {nameof(RequestHeaderMatchesMediaTypeAttribute)}.");
                }
                _mediaTypeValues.Add(mediaTypeHeaderValue);
            }
        }

        public bool Accept(ActionConstraintContext context)
        {
            if (!context.RouteContext.HttpContext.Request.Headers.TryGetValue(_requestHeader, out StringValues mediaTypeStringValueFromRequest))
            {
                return false;
            }
            var mediaTypesFromRequest = mediaTypeStringValueFromRequest.ToString().Split(",").Select(m => new MediaType(m));
            IList<MediaTypeHeaderValue> parsedMediaTypes = new List<MediaTypeHeaderValue>();
            return _mediaTypeValues.Select(m => new MediaType(m)).Any(
                m => mediaTypesFromRequest.Contains(m));
        }
    }
}
