namespace ForgetMeNot.Common.Transport
{
    public class HandlerResponse
    {
        public HandlerResponse(HandlerResponseCode code)
        {
            Code = code;
        }

        public HandlerResponseCode Code { get; }
    }

    public enum HandlerResponseCode
    {
        Success = 1,
        QuoteAlreadySaved = 2,
        UnknownQuote = 3,
        Error = 99,
    }
}
