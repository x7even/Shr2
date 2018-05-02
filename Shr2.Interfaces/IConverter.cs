using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shr2.Interfaces
{
    public interface IConverter
    {

        Task<string> TryEncodeUrl(string url);

        //bool TryEncodeUrl(string url, out string idcode);

        Task<(string url, bool permanent, bool preserveMethod)> TryDecode(string shortcode);
    }
}
