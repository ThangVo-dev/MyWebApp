namespace WebApp.Service.Enums
{
    public enum ApiStatusCode
    {
        // Success codes
        OK = 200,
        Created = 201,
        NoContent = 204,

        // Client error codes
        BadRequest = 400,
        Unauthorized = 401,
        Forbidden = 403,
        NotFound = 404,
        Conflict = 409,

        // Server error codes
        InternalServerError = 500,
        NotImplemented = 501,
        BadGateway = 502,
        ServiceUnavailable = 503
    }
}