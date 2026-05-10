namespace Laddro.Career;

public class LaddroException : Exception
{
    public int Status { get; }
    public string? Code { get; }

    public LaddroException(string message, int status, string? code = null) : base(message)
    {
        Status = status;
        Code = code;
    }

    public bool IsAuthError => Status == 401;
    public bool IsUsageLimitError => Status == 402;
    public bool IsNotFound => Status == 404;
}
