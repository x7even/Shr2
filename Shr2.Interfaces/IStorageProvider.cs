namespace Shr2.Interfaces
{
    public interface IStorageProvider
    {
        Task<bool> Init();

        Task<EncodeResult> TryAddNewUrlAsync(string url, bool permanent = false, bool preserve = true, bool statsCount = false);

        Task<(string Url, bool Permanent, bool PreserveMethod)> TryGetUrlAsync(string idcode);
    }
}
