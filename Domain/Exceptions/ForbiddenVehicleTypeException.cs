namespace Domain.Exceptions;

public class ForbiddenVehicleTypeException : Exception
{
    public ForbiddenVehicleTypeException(string message) : base(message)
    {
    }
}