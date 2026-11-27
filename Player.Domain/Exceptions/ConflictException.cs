namespace Player.Domain.Exceptions;

public class ConflictException(string message) : Exception(message);
