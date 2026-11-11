# Design Decisions

## D1.4: Cross-cutting API basics

### Structured logging with Serilog
- Context: we need searchable logs that can later carry the tenant id.
- Options: built-in logger, Serilog, NLog.
- Choice: Serilog, configured from appsettings.json, with one summary line per HTTP request.
- Trade-off: one extra dependency, in exchange for structured, filterable log output.

### Errors returned as ProblemDetails (RFC 7807)
- Context: clients need one predictable error shape, and internal details must not leak.
- Options: default error pages, custom error JSON, ProblemDetails.
- Choice: one global IExceptionHandler returning ProblemDetails with a traceId; full details only in server logs.
- Trade-off: debugging needs the log (matched by traceId) instead of the response body.

### Health check with a database probe
- Context: hosting platforms and Docker need to know when the app cannot serve requests.
- Choice: /health runs an EF Core connectivity check, returning 503 when the database is unreachable.
- Trade-off: a database outage marks the whole app unhealthy, which is correct because it cannot serve real requests anyway.

### OpenAPI documentation with Scalar
- Context: a browsable API reference helps development and recruiter demos; the OpenAPI document also feeds the frontend client later.
- Choice: built-in OpenAPI document plus the Scalar UI, enabled in Development only.

### Validation with FluentValidation and an endpoint filter
- Context: minimal APIs do not validate request bodies automatically.
- Options: DataAnnotations, manual checks in each handler, FluentValidation.
- Choice: FluentValidation validators live in the Application layer and are auto-registered; a generic endpoint filter runs them and returns a 400 with field errors.
- Trade-off: one extra filter per endpoint, but validation rules stay out of endpoint code and are easy to unit test.

### Proof (sample requests and responses)

All requests were sent to `http://localhost:5099` with the `http` launch profile.

Valid request, `POST /dev/echo` with body `{"name":"Ali"}`:

```
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8

{"message":"Hello, Ali!"}
```

Empty name, `POST /dev/echo` with body `{"name":""}`:

```
HTTP/1.1 400 Bad Request
Content-Type: application/problem+json

{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"Name":["'Name' must not be empty."]},"traceId":"00-b2eed629e82b54b8cfb39d364c4be07e-ecb4f9d623458770-00"}
```

Name too long, `POST /dev/echo` with 51 "a" characters:

```
HTTP/1.1 400 Bad Request
Content-Type: application/problem+json

{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"Name":["The length of 'Name' must be 50 characters or fewer. You entered 51 characters."]},"traceId":"00-d8551e2bccbd2e83e0e043213144129c-eb75360ebd5f88b4-00"}
```

Missing body, `POST /dev/echo` with `Content-Type: application/json` and no body:

```
HTTP/1.1 400 Bad Request
Content-Type: application/problem+json

{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"The request could not be processed.","status":400,"traceId":"00-d02cdf5c29adb9efac9340a686e25268-81ffee804910bc82-00"}
```

Deliberate exception, `GET /dev/boom` (the exception text and stack trace stay in the server log only):

```
HTTP/1.1 500 Internal Server Error
Content-Type: application/problem+json

{"type":"https://tools.ietf.org/html/rfc9110#section-15.6.1","title":"An unexpected error occurred.","status":500,"detail":"Something went wrong on our side. Please try again, and quote the traceId if you contact support.","traceId":"00-c80680e44b2a5daeb2752a4ab2c3f098-aa4ba1d1f533ad70-00"}
```
