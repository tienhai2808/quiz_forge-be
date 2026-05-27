namespace QuizForge.Common;

public static class Constants
{
    public const string AccessToken = "_at";
    public const string RefreshToken = "_rt";

    public const string ExchangeDocument = "extract.document";
    public const string QueueNameExtractDocument = "document.extract";
    public const string RoutingKeyExtractDocument = "document.extract";

    public const string ExchangeDocumentRetry = "extract.document.retry";
    public const string QueueNameExtractDocumentRetry = "document.extract.retry";
    public const string RoutingKeyExtractDocumentRetry = "document.extract.retry";

    public const string ExchangeDocumentDlq = "extract.document.dlq";
    public const string QueueNameExtractDocumentDlq = "document.extract.dlq";
    public const string RoutingKeyExtractDocumentDlq = "document.extract.dlq";
}
