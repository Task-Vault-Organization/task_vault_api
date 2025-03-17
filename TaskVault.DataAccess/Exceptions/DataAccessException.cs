namespace TaskVault.DataAccess.Exceptions;

public class DataAccessException(string errorMessage) : Exception
{
    public string ErrorMessage { get; set; } = errorMessage;
}