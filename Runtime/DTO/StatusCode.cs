namespace SmsAuthLibrary.DTO
{
    internal enum StatusCode : uint
    {
        NotFound = 404,
        ValidationError = 422,
    }

    public enum YdbStatusCode : uint
    {
        Unspecified = 0u,
        Success = 400000u,
        BadRequest = 400010u,
        Unauthorized = 400020u,
        InternalError = 400030u,
        Aborted = 400040u,
        Unavailable = 400050u,
        Overloaded = 400060u,
        SchemeError = 400070u,
        GenericError = 400080u,
        Timeout = 400090u,
        BadSession = 400100u,
        PreconditionFailed = 400120u,
        AlreadyExists = 400130u,
        NotFound = 400140u,
        SessionExpired = 400150u,
        Cancelled = 400160u,
        Undetermined = 400170u,
        Unsupported = 400180u,
        SessionBusy = 400190u,
        ClientResourceExhausted = 500010u,
        ClientInternalError = 500020u,
        ClientTransportUnknown = 600010u,
        ClientTransportUnavailable = 600020u,
        ClientTransportTimeout = 600030u,
        ClientTransportResourceExhausted = 600040u,
        ClientTransportUnimplemented = 600050u
    }
}
