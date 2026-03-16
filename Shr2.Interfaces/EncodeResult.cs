namespace Shr2.Interfaces
{
    public class EncodeResult
    {
        public bool Success { get; }
        public string? ShortCode { get; }
        public EncodeError Error { get; }

        private EncodeResult(bool success, string? shortCode, EncodeError error)
        {
            Success = success;
            ShortCode = shortCode;
            Error = error;
        }

        public static EncodeResult Ok(string shortCode) => new(true, shortCode, EncodeError.None);
        public static EncodeResult Fail(EncodeError error) => new(false, null, error);
    }

    public enum EncodeError
    {
        None,
        StorageError,
        Conflict
    }
}
