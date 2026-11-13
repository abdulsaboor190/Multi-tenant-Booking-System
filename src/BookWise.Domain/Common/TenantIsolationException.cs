namespace BookWise.Domain.Common;

public sealed class TenantIsolationException(string message) : Exception(message);
