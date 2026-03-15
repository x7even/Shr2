using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shr2.Interfaces
{
    public interface IConverter
    {

        Task<EncodeResult> TryEncodeUrl(string url);

        Task<(string url, bool permanent, bool preserveMethod)> TryDecode(string shortcode);
    }
}
