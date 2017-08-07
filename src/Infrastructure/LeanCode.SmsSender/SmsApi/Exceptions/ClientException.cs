namespace LeanCode.SmsSender.SmsApi.Exceptions
{
    public class ClientException : ResponseException
    {
        public ClientException(int errorCode, string message)
            : base(errorCode, message)
        { }
    }
}