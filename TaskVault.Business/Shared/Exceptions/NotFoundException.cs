using Microsoft.AspNetCore.Http;

namespace TaskVault.Business.Shared.Exceptions;

public class NotFoundException(string message) : ServiceException(StatusCodes.Status404NotFound, message) { }