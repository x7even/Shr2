namespace Shr2.Interfaces
{
    public interface IStorageProvider
    {
        Task<bool> Init();

        Task<EncodeResult> TryAddNewUrlAsync(string url, bool permanent = false, bool preserve = true, bool statsCount = false);

        Task<(string url, bool permanent, bool preserveMethod)> TryGetUrlAsync(string idcode);
    }
}
