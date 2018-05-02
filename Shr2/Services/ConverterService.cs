using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Shr2.Interfaces;

namespace Shr2.Services
{
    public class ConverterService : IConverter
    {
        private readonly IStorageProvider _storageProvider;

        private const string _charSet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"; //"^[A-Za-z][A-Za-z0-9]{2,62}$"

        // Removes ambiguity for human reading in various fonts but significantly lower base
        private const string _charSetReadable = "23456789abcdefghjkmnpqrstuvwxyz";

        public ConverterService(IStorageProvider storageProvider) => _storageProvider = storageProvider;

        /// <summary>
        /// Returns a 'shortened' idcode of a given stored url ready to be encoded.
        /// </summary>
        /// <returns></returns>
        public async Task<string> TryEncodeUrl(string url)
        {
            var storagekey = await _storageProvider.TryAddNewUrlAsync(url);
            if (!String.IsNullOrWhiteSpace(storagekey) && storagekey != "error")
                return Encode(storagekey);
            else
                return storagekey;
        }

        /// <summary>
        /// Returns not empty string when decoded param is found within a storage provider
        /// the stored value is returned.
        /// </summary>
        /// <param name="param"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<(string, bool, bool)> TryDecode(string shortcode)
        {
            var storagekey = Decode(shortcode);
            return await _storageProvider.TryGetUrlAsync(storagekey);
        }


        /** Base62 resources for conversion (or less if use _charsetreadable) */

        public string Encode(string value)
        {
            var arr = new int[value.Length];
            for (var i = 0; i < arr.Length; i++)
            {
                arr[i] = value[i];
            }

            return Encode(arr);
        }

        private string Encode(int[] value)
        {
            var converted = BaseConvert(value, 256, 62);
            var builder = new StringBuilder();
            for (var i = 0; i < converted.Length; i++)
            {
                builder.Append(_charSet[converted[i]]);
            }
            return builder.ToString();
        }




        public string Decode(string value)
        {
            var arr = new int[value.Length];
            for (var i = 0; i < arr.Length; i++)
            {
                arr[i] = _charSet.IndexOf(value[i]);
            }

            return Decode(arr);
        }

        private string Decode(int[] value)
        {
            var converted = BaseConvert(value, 62, 256);
            var builder = new StringBuilder();
            for (var i = 0; i < converted.Length; i++)
            {
                builder.Append((char)converted[i]);
            }
            return builder.ToString();
        }

        private static int[] BaseConvert(int[] source, int sourceBase, int targetBase)
        {
            var result = new List<int>();
            int count = 0;
            while ((count = source.Length) > 0)
            {
                var quotient = new List<int>();
                int remainder = 0;
                for (var i = 0; i != count; i++)
                {
                    int accumulator = source[i] + remainder * sourceBase;
                    int digit = accumulator / targetBase;
                    remainder = accumulator % targetBase;
                    if (quotient.Count > 0 || digit > 0)
                    {
                        quotient.Add(digit);
                    }
                }

                result.Insert(0, remainder);
                source = quotient.ToArray();
            }

            return result.ToArray();
        }
    }
}
