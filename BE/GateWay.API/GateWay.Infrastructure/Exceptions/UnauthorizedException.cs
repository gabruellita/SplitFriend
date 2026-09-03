namespace GateWay.Infrastructure.Exceptions;

/// <summary>Aruncată când o cerere nu are un JWT valid sau lipsit (401).</summary>
public class UnauthorizedException(string message) : Exception(message);
