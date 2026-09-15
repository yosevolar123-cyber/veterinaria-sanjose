namespace VetSanJose.Application.Common;

public class AppException(string message, int statusCode = 400) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

public class NotFoundException(string message) : AppException(message, statusCode: 404);

public class UnauthorizedAppException(string message) : AppException(message, statusCode: 401);

public class ForbiddenAppException(string message) : AppException(message, statusCode: 403);

public class ConflictException(string message) : AppException(message, statusCode: 409);
