## RESTful API with .NET Core 3

### Return codes of interest

#### 200 - Success

* 200 OK
* 201 Created At - Returns Location header and resource as a body.
* 204 No Content - Empty response.

#### 400 - Client Errors

* 400 Bad Request - Cannot parse representation.
* 401 Not authorized - No authentication details provided.
* 403 Forbidden - Not authorized.
* 404 Not Found
* 405 Method Not Allowed - Posting to a single resource. .NET Core returns this automatically.
* 406 Not Acceptable - Media type provided through the `Accept` header is not known or we do not have an appropriate formatter. In other words client has requested a representation we do not support.
* 409 Conflict - State of the resource is not in line with the action you are trying to perform. I have not seen this used.
* 412 Precondition failed - Client is trying to manipulate a resource using an outdated ETag in a concurrency scenario. ETag is sent through the `If-Match` header.
* 415 Unsupported Media Type - `Content-Type` header in the request is unknown or cannot be parsed. E.g. when POSTing w/o a Media Type. Missing body (when expected) can cause it as well. I had trouble returning it.
* 422 Unprocessable Entity - Request body syntax is ok, can be parsed and is validated by .NET Core validators, however it causes an issue later on. Used for semantic errors. E.g. patching a resource leads to an invalid state.

#### 500 - Server Errors

* 500 Internal Server Error

## Some general unrelated pointers and tips

### Error vs Fault

#### Error

* **Consumer** passes invalid datato the API.
* Bad request.
* Bad credentials.
* Errors do not contribute to API availability.

#### Faults

* API fails to return a reponse to a valid request.
* Faults contribute to API availability.
* Level 500 errors.

### Best practices

* Return `Ok()` instead of `JsonResult()` as it allows for content negotiation.
* Problem Details object is a standardized way of formatting errors on an API. It is enabled by default when using `[ApiController]`.
* `Accept` header is client-side best practice.

### Content negotiation

* Client should provide `Accept` header. If they do not, we should default to some representation.
* If API does not support the desired representation format, we should **not** default, but rather return `406 Not Acceptable`.

### .NET Core 3.0 Web API

* Use `services.AddController()` instead of `.AddMvc()` since it does not register MVC's front end stuff like views etc.
* Your controllers should always inherit from `ControllerBase`.
* Faults:
    * Use `app.UseDeveloperExceptionPage()` to handle faults in the development environment.
    * Use `app.ExceptionHandler()` to handle faults in the staging/production environment.
* `[ApiController]`
    * In case of a failed binding or model validation returns Problem Details and a `400` Status Code. In the sample project we have created our own `InvalidStateModelFactory` returning a `ValidationProblemDetails`, which is in line with the standard. This gets triggered on:
        * POST w/o body
        * Failed binding (e.g. wrong data type)
        * Failed model validation
    * Changes the sources in which parameters are usually bound. Google this if needed.

## Outer Facing Contract

* Resource identifiers
    * Pluralized nouns: `AuthorsController`
    * Hierarchical resources: `Authors/{id}/Courses`
    * Be consistent, but it is ok to divert on some specific RPC calls.
* Use `ActionResult<T>` instead of `IActionResult`. Tools can deduce return types this way (e.g. Swashbuckle).
* One of REST constraints says responses must contain enough data so further manipulations (update, delete) can be done using the representation provided to the client. Strictly speaking, returning resource's `Id` is not enough, since resources are identified using the URI. Therefore entire URL should be included somewhere in the response. This is facilitated through HATEOAS.
* Use `ExceptionHandler` for production environment. Default handler returns an empty `500` response, but we should include some additional data in there. You can do this by consulting the [Startup.cs](https://github.com/nlivaic/RestfulAPICore3/blob/master/RestfulAPICore3.API/Startup.cs#L78). You can even log from here.

## Filtering and Searching

* Filtering involves finding relevant resources based on provided data. Filtering is strict, so when filtering you are actually targeting for some well-known resource instances through client provided data. E.g. client says it wants only resources whose `MainCategory` property equals `Rum`. Take a look at [AuthorsResourceParameters](https://github.com/nlivaic/RestfulAPICore3/blob/master/RestfulAPICore3.API/ResourceParameters/AuthorsResourceParameters.cs#L18).
* Searching is more relaxed. It involves using data provided by the client and searching through a number properties for parts of the data. E.g. client provides a string and the API already knows it should search `FirstName`, `LastName` and `MainCategory` for at least a partial match. Take a look at [AuthorsResourceParameters](https://github.com/nlivaic/RestfulAPICore3/blob/master/RestfulAPICore3.API/ResourceParameters/AuthorsResourceParameters.cs#L19).
* Please note that when clients say on which property they want to filter/search on, they should only provide properties found on the Dto. They should not know about the underlying entities and their properties.
* Combine multiple filtering and search criteria into one class, e.g. `AuthorsResourceParameters`.

## HTTP Methods

* Safe method does not change the resource's representation.
* Idempotent method means same call can be made without additional changes being done to the resource's representation.
* You must make sure your implementations conform to below specification.
* GET - safe and idempotent.
* POST - not safe and not idempotent.
* PUT - not safe, but is idempotent.
* PATCH - not safe, not idempotent. Personally I do not know how it could not be idempotent.
* DELETE - not safe, not idempotent. Deleting an already deleted resource results in `404`.

## Creating Resources

* Not safe, not idempotent
* Do not POST to URI containing an `Id`. This is called posting to a single resource and will cause a `405 Method Not Allowed`. This is built into .NET Core 3.
* Request must have `Content-Type` header, otherwise API returns `415`. This is built into .NET Core 3.
* When creating a child resource on an unexisting parent, return `404`.
* When created successfully, return `201` with the resource in the body.
* If you want to create a collection of resources, consider creating a new endpoint, e.g. `AuthorCollections` and POST the collection there. Bear in mind you will have to return a `Location` header with all the `Id`s in there, so you will have to have an accompanying `Get(ids)` action with a CSV list of ids. This, in turn, will entail having a model binder such as [ArrayModelBinder](https://github.com/nlivaic/RestfulAPICore3/blob/master/RestfulAPICore3.API/Helpers/ArrayModelBinder.cs).

### Validating input

* We validate Dtos entering the API. There are several methods.
* `DataAnnotations` are attributes provided by the framework. They cover simple, per property cases.
* `IValidatableObject` is an interface your Dto should implement. Allows for more complex validations spanning multiple properties on the Dto.
* `ValidationAttribute` also allows for more complex validations spanning multiple properties on the Dto. One advantage over `IValidatableObject` is you don't have to inherit anything, and since validation is a cross-cutting concern, attributes are a better fit.
* Fluent Validation API - more complex scenarios. Is testable?
* `[ApiController]` returns `400` and a Problem Details object on failed validation. Proper thing to do would be return a Validation Problem Details and (depending on the case) a `400` or a `422` return code:
    * `400` when syntax is wrong. I.e. model binder cannot bind due to an empty body or wrong data type.
    * `422 Unprocessable Entity` when input data is syntactically correct but validation fails. This covers cases both when the validation is done automatically by the framework or later on in our own code.
    * So to make `[ApiController]` return an appropriate Validation Problem Details object we have to provide a custom `InvalidModelStateResponseFactory`. Consult [here](https://github.com/nlivaic/RestfulAPICore3/blob/master/RestfulAPICore3.API/Startup.cs#L46).

### Put

* Not safe, but is idempotent.
* Client does a PUT against a single resource. Body must contain entire representation.
* If the resource does not exist, return `404`.
* Clients updating the representation of the resource. This is an important detail. You do not update the resource directly, but only its representation. Only then do you map the representation back to the resource.
* If PUT was successful, you must return a `204 No Content`.
* There is another use case for PUT inserting new entities. This use case makes sense only if you allow clients to generate entity identifiers, because PUT only works against a single resource. Return `201` in that case.
* Doing PUT against a collection of resources: even though nothing prevents you from doing so, you should be aware that such an action would require deleting the whole collection and recreating it, which could quickly become a very expensive move.

### Patch

* Not safe, not idempotent.
* Client does a PATCH against a single resource. Body contains a Patch Document.
* To work with Patch Document, you should turn to Newtonsoft Json, because the new Json Formatter does not support Patch Documents. Statements in the Patch Document are executed one by one.
* If the resource does not exist, return `404`.
* Clients updating the representation of the resource. This is an important detail. You do not update the resource directly, but only its representation. After you update the representation, you must do a validation using `TryValidate` to make sure no invariants are broken. Only then do you map the representation back to the resource. If the validation fails, you must return `422` with an accompanying Validation Problem Details object.
* If PATCH was successful, you must return a `204 No Content`.
* There is another use case for PATCH inserting new entities. This use case makes sense only if you allow clients to generate entity identifiers, because PATCH only works against a single resource. Return `201` in that case.

### Delete

* Not safe, not idempotent.
* If the resource does not exist, return `404`.
* On success return `204`.
* Removing parent entity removes children as well.
* Nothing prevents you from deleting entire collections, but that will lead to deleting their children, which might be a very destructive process (you could lose a lot of your data).

### Problems with InvalidModelStateResponseFactory

* We should provide our own implementation which return `400` Validation Problem Details.
* The problem is Model State errors cover issues that would (according to HTTP standard) result in `400` or `422`, but there is no way for us to discern between the two situations.
* `400` results from the binder not being able to read the request (no body, wrong data type) causing a model binding error.
`422` results from the validation not succeeding, causing a validation error.
* **Solution:** - just return a Validation Problem Details with a `400` error. `400` is a well-known status code and there is an accompanying error description in the Validation Problem Details.
* The only issue with this solution is when validation error is caught by the framework, it results in a `400`, but when I discover a faulty model in my own code, I return `422` manually, even though it is conceptually the same thing.

### Paging

* Always implement paging when fetching list of data. Not having paging is a drain on resources.
* Set some default values for page number and page size.
* One issue with paging is how you return paging data:
    * If you return paging data as part of the response body, you are effectively cheating. You are now returning a piece of data the client asked for and with it comes additional pieces of data related to paging.
    * Usually when clients issue a `GET` request against a specific resource, they ask for e.g. `application/json' representation of that specific resource.
    * However, what they get is an envelope containing the desired representation and an additional property holding paging data.
    * It is better to return only the requested data in the response body and move the paging data into a custom header, e.g. `X-Pagination`.
    * Another solution is to use HATEOAS by supplying links relevant to paging (previous, next).

### Sorting

* Use library `System.Linq.Dynamic.Core` to dynamically provide property names on which to sort. This allows clients to say on which Dto fields they want to sort and you have to map Dto fields to entity fields. This is done by returning a string representing entity property to order by and passing it to `System.Linq.Dynamic.Core`.

### Data shaping

* Data shaping is performed by the client sending a query string `?fields=name, age`.
* You can only shape on Dto properties as this is the only contract the client sees.
* Use an `ExpandoObject` as a target for shaping. You can return it from the controller as well.
* Downside might be performance of the dynamic object `ExpandoObject` and the fact you have to use `IActionResult` instead of `ActionResult<T>`.
* You can inject data shaping as a service (for a particular domain class).

### HATEOAS

* Return links along with the resource representation. To do this you add a new property to your resource representation called `links`. There you return an array of links describing other reachable resources and actions.
* Structure of each link:
    * `href` - URI
    * `rel` - type ("self", "post-new-course", "delete-author", etc)
    * `method` - Http method
* You only return `links` with responses that have a body. `GET`, `POST`, `PUT`, `PATCH` (when inserting), but not `PUT`, `PATCH` (when updating) nor `DELETE`.
* You link to other controllers as well, e.g. when fetching a single author you then link to other courses belonging to this author.
* When returning a single entity you add `links` simply as an additional property. You can use dynamic objects such as `ExpandoObject`.
* When returning an array of entities it is best to wrap everything and then have two properties, one for the payload (e.g. `authors`) and another one for `links`.
* Make sure you do not return HATEOAS responses when client requests `application/json` because HATEOAS is not a Json representation of the desired resource. Make use of custom vendor media types to singal to the API you want a HATEOAS response: `application/vnd.marvin.author.hateoas+json`.

### Advanced Content Negotiation

* Signals to the API we want a resource represented according to data in the `Accept` header.
* Good example (here) [https://github.com/nlivaic/RestfulAPICore3/blob/master/RestfulAPICore3.API/Controllers/AuthorsController.cs#L91].
* E.g. `Accept` header could contain a value such as `application/vnd.marvin.author.friendly.hateoas+json`. In our controller we can load the contents of this header (as a parameter), parse it and format our response accordingly.
* We can bind specific media types to a specific controller action using `[Produces]` and `[Consumes]` attributes, [here] (https://github.com/nlivaic/RestfulAPICore3/blob/master/RestfulAPICore3.API/Controllers/AuthorsController.cs#L91). These two attributes are filters which will either allow the action to be executed or cause the framework to return an error. Bear in mind these attributes **do not** take part in routing. 
    * [Produces] works in conjuction with the `Accept` header. If what is in the header is also in the [Produces] list, API will return a response with the desired media type.
    * However if a different value is in the header, you will get a `406 Not Acceptable`. This is done by the framework.
    * [Consumes] works in the same manner, except it is bound to the `Content-Type` header.
* Sometimes we will want to execute different actions based on the incoming `Accept` or `Content-Type` headers. Since [Produces] and [Consumes] have no bearing on the routing, we need a custom routing component that will route according to those headers. This requires an implementation of `IActionConstraint`. Consult [here] (https://github.com/nlivaic/RestfulAPICore3/blob/master/RestfulAPICore3.API/Constraints/RequestHeaderMatchesMediaTypeAttribute.cs).

### Caching

* To use ASP.NET caching we need two things:
    * `[ResponseCache]` on the action and/or controller level. This attribute makes the API return `cache-control` header.
    * Middleware component `services.AddResponseCaching()`. However, for reasons that will be explained later, ASP.NET Core's caching is not implemented properly when it comes to validation model caching. It is appropriate only for expiration model caching and it covers only simple cases.
* We can use caching profiles with `services.AddMvc()` to cut down on repetition with `[ResponseCache]`.
* Caching is performed on URI of the resource.
* Caching is done by the caching component. This component can be on the client and on the server. It can also be on various proxies along the way, but this is out of scope for this article.
* `cache-control` header covers two things:
    * Time the resource will be cached (in seconds)
    * Caching location: `public` (cached on server and/or on client) and `private` (cached on client only).
* Note: when using Postman, make sure to go to Settings -> General and turn of "Send no-cache header".
* A big issue with caching is when to invalidate cache. Two different models exist:
    * Expiration model:
        * Cache component holds resources for the amount of time as  specified by the `cache-control` header. When a request hits the cache component, the response (if not stale) is returned from the cache component, along with the `Age` header saying how long it will stay in the cache until it goes stale.
        * If the cache entry is stale, cache component goes to the API for a fresh response.
    * Validation model: 
        * Two ways to utilize validation model, depending on the response headers: strong validator (`ETag`, a.k.a. Entity Tag) and weak validator (`Last-Modified`).
        * Once cache component receives a request, it will call API **every** time. Cache component must send the ETag value in `If-None-Match` header. Cache component can also send `If-Modified-Since` as well. API will fetch the resource, generate current `ETag`, compare with `If-None-Match`. If the resource has **not changed**, response will have empty body with `304 Not Modified` status code. If the resource has changed, response will contain complete response body, along with current `ETag` and `Last-Modified`.
* Unfortunately, `[ResponseCache]` attribute does not generate an ETag. We must reach for external libraries to make it work. In the sample project we work with `Marvin.Cache.Headers`.
    * In Startup.cs we register the [service] (https://github.com/nlivaic/RestfulAPICore3/blob/master/RestfulAPICore3.API/Startup.cs#L52) and use it as a [middleware] (https://github.com/nlivaic/RestfulAPICore3/blob/master/RestfulAPICore3.API/Startup.cs#L94).
    * To cache a response we just add an attribute on top of an [action] (https://github.com/nlivaic/RestfulAPICore3/blob/master/RestfulAPICore3.API/Controllers/CoursesController.cs#L32) or a controller.
* `Vary` header allows cache components to cache same resource multiple times, based on properties in the header. E.g. if `Vary=Accept` then same resource is cached once for each distinct value of the `Accept` header that was requested + from the API.

#### Appropriate caching technique

* In order to cache effectively, expiration and validation model caching should be combined.
* Response should have both `cache-control` and `ETag` headers set to utilize both expiration and validation model caching.
* This way cached responses will get returned directly from cache component until the resource expires. This saves bandwidth and round-trip times (in case of private cache), resulting in a large number of client being able to be served efficiently. In case of public cache on the origin server, this saves time because database queries and server-side processing can be skipped.
* Once the resource expires, it can still be served from the cache if it has not gone stale. Checking whether it is still fresh is done by the caching component going to the API with the `If-None-Match` and `If-Modified-Since` headers set. API can then fetch the resources from the database, generate ETag, compare with the one just received and if the resource is still fresh return `304`. This saves bandwidth. Of course, if resource is stale, entire resource is returned along with a new `ETag` and `Last-Modified`.

![Appropriate caching technique](https://thepracticaldev.s3.amazonaws.com/i/0xqf6pncwx3az0igr57m.png)

#### Issues with ASP.NET Core caching middleware

* Expiration model caching works fine.
* Validation model caching is faulty because of two reasons:
    * Once a request comes in for an expired cached resource, caching middleware should forward the request and append `If-None-Match` and `If-Modified-Since` headers, so API can compare them with the fetched resource and determine whether the fetched resource is more fresh and return `304` or `200`. However, caching middleware forwards the `GET` request without those headers and then in turn API simply fetches and returns those resources as `200`. The response is then cached again and cache timer is restarted.
    * To make the validation caching work, clients (!) are expected to send `If-None-Match` and `If-Modified-Since`. This is not ok because clients should only send simple `GET` with the Uri, and the caching component should then append additional `If-None-Match` and `If-Modified-Since` headers to the request forwarded to the API. When clients do send those headers, caching middleware return `304` or `200`. Caching middleware should never return `304`.

![ASP.NET Core caching implementation](https://thepracticaldev.s3.amazonaws.com/i/e0qaqq4wwh40v9w6wu39.png)

#### Conclusion regarding caching

* Best results are achieved by combining expiration and validation models.
* ASP.NET Core's caching middleware is ok for expiration model and simple scenarios.
* More complex scenarios require use of dedicated caching components and/or CDN.

### Concurrency

* `ETag`s represent a specific version of a resource. This means client can send the ETag value it currently holds and API can use it to determine if it is the latest version or a stale version.
* To facilitate concurrency checks client should send the ETag in `If-Match`.
* In case of a concurrency issue, API responds with `412 Precondition Failed`.
* Please note that, even though `ETag`s are used, the process does not utilize cache in any way.

### Swagger/Swashbuckle

* Swagger (or OpenAPI) is a format for specifying and describing REST APIs.
* Swashbuckle is a library for .NET Core allowing Swagger docs and UI generation.
* Swagger UI is generated from Swagger doc.

#### Usage

* Add Swagger service in [Startup.cs] (https://github.com/nlivaic/RestfulAPICore3/blob/master/RestfulAPICore3.API/Startup.cs) using `services.AddSwaggerGen()`. Additional info on your API can be defined here as well. You can also include XML comments, but these have to be generated earlier (more on that below).
* Use Swagger middleware. There are two middlewares, one for the docs and one for  generated user interface:
```
app.UseSwagger();
app.UseSwaggerUI();
```
* When configuring the user interface middleware, you can also define the endpoint URIs for both Swagger URI and `Swagger.json` specification file.
* It is recommended to put the above middlewares after `.UseHttpsRedirection()` so the endpoints are accessible only via HTTPS.
* XML Comments:
    * Swagger UI displays additional info for each HTTP method on each of your endpoints. To display additional data on each parameter or return type, we can point it to an XML file containing all the XML comments from our source code.
    * To enable generating the XML file, we have to set in [.csproj] (https://github.com/nlivaic/RestfulAPICore3/blob/master/RestfulAPICore3.API/RestfulAPICore3.API.csproj) file an element `<GenerateDocumentationFile>true</GenerateDocumentationFile>`
    * Don't forget to tell Swagger where the file is: `setupAction.IncludeXmlComments(xmlPath)`
    * By enabling the XML comments, build system will warn us of each language construct without a comment. These can be suppressed in `.csproj` with `<NoWarn>0579;1591</NoWarn>`
* Additional custom comments can be provided by adding a XML element to comments above each method: `<remarks>Some example code here.</remarks>`
* Data annotations on each class and/or property also show up on Swagger UI.

#### Return Types
* Swagger returns a `200 text/plain` on every endpoint and every HTTP method.
* To tell Swagger the real status codes and media types, several attributes need to be employed on action methods, controllers or in Startup:
    * `[ProducesResponseType]` - allows for declaring both status code and return type.
    * `[Consumes]`
    * `[Produces]`
* Several status codes are returned by all endpoints and HTTP methods, so you can declare these in `services.AddControllers()`: `400`, `406`, `500`.
* `ApiExplorer` allows automatic analysis of our code. It helps with creating Swagger docs. `ApiExplorer` is registered with the DI container through e.g. `.AddMvc()`.
* If you return `IActionResult`, Swashbuckle cannot know the real return type. You can help out by providing type to the `[ProducesResponseType]`, or by returning `ActionResult<T>`.
* To provide additional information with each return status code, you can write an additional XML comment above your action methods within an element:
```
<response code="200">Return an author.</response>
```

#### Api Analyzer
* A tool that analyzes your code and tells you if you have any undeclared return status codes.
* Usually all you have to do is to decorate your action methods with appropriate `[ProducesResponseType]`s.
* You can turn it on by providing an entry in your [csproj] (https://github.com/nlivaic/RestfulAPICore3/blob/master/RestfulAPICore3.API/RestfulAPICore3.API.csproj) file
```
<IncludeOpenAPIAnalyzers>true</IncludeOpenAPIAnalyzers>
```
