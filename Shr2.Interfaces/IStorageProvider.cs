using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shr2.Interfaces
{
    public interface IStorageProvider
    {
        Task<bool> Init();

        Task<string> TryAddNewUrlAsync(string url, bool permanent = false, bool preserve = true, bool statsCount = false);

        Task<(string url, bool permanent, bool preserveMethod)> TryGetUrlAsync(string idcode);
    }
}
