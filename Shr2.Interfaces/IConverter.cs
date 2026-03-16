namespace Shr2.Interfaces
{
    public interface IConverter
    {

        Task<string> TryEncodeUrl(string url);

        //bool TryEncodeUrl(string url, out string idcode);

        Task<(string Url, bool Permanent, bool PreserveMethod)> TryDecode(string shortcode);
    }
}
