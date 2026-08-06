namespace Erp.Application.Common.Exceptions;

public class AppException : Exception
{
    public int StatusCode { get; }

    public AppException(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }
}

public sealed class ForbiddenException : AppException
{
    public ForbiddenException(string message = "Không có quyền thực hiện thao tác.") : base(message, 403) { }
}

public sealed class UnauthorizedAppException : AppException
{
    public UnauthorizedAppException(string message = "Chưa đăng nhập hoặc phiên hết hạn.") : base(message, 401) { }
}
