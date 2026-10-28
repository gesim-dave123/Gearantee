namespace ASI.Basecode.Services.Common
{
    /// <summary>
    /// Base type for all service operation outcomes. Carries the universal
    /// succeeded/message pair; concrete results extend it with domain-specific
    /// state flags (stale, missing, denied, conflict, ...).
    /// </summary>
    public abstract class ServiceResult
    {
        public bool Succeeded { get; protected set; }
        public string Message { get; protected set; } = string.Empty;
    }

    /// <summary>
    /// Generic outcome wrapper carrying a payload alongside the outcome.
    /// </summary>
    public class ServiceResult<TData> : ServiceResult
    {
        public TData Data { get; protected set; }

        public static ServiceResult<TData> Success(TData data, string message = null) =>
            new ServiceResult<TData>
            {
                Succeeded = true,
                Message = message ?? string.Empty,
                Data = data
            };

        public static ServiceResult<TData> Failure(string message) =>
            new ServiceResult<TData>
            {
                Succeeded = false,
                Message = message
            };
    }
}
