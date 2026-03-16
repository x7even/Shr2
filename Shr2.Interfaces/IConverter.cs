namespace Shr2.Interfaces
{
    public interface IConverter
    {

        Task<EncodeResult> TryEncodeUrl(string url);

        Task<(string Url, bool Permanent, bool PreserveMethod)> TryDecode(string shortcode);
    }
}
