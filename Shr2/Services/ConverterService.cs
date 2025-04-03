﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Shr2.Interfaces;

namespace Shr2.Services
{
    public class ConverterService : IConverter
    {
        private readonly IStorageProvider _storageProvider;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ConverterService>? _logger;

        private const string _charSet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"; //"^[A-Za-z][A-Za-z0-9]{2,62}$"

        // Removes ambiguity for human reading in various fonts but significantly lower base
        private const string _charSetReadable = "23456789abcdefghjkmnpqrstuvwxyz";

        public ConverterService(
            IStorageProvider storageProvider, 
            IMemoryCache cache,
            ILogger<ConverterService>? logger = null)
        {
            _storageProvider = storageProvider;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Returns a 'shortened' idcode of a given stored url ready to be encoded.
        /// </summary>
        /// <param name="url">The URL to encode</param>
        /// <returns>The encoded short URL identifier</returns>
        public async Task<string> TryEncodeUrl(string url)
        {
            _logger?.LogInformation("Encoding URL: {Url}", url);
            
            try
            {
                var storagekey = await _storageProvider.TryAddNewUrlAsync(url);
                
                if (!string.IsNullOrWhiteSpace(storagekey) && storagekey != "error")
                {
                    var encoded = Encode(storagekey);
                    _logger?.LogInformation("URL encoded successfully: {ShortCode}", encoded);
                    return encoded;
                }
                else
                {
                    _logger?.LogWarning("Failed to encode URL: {Url}", url);
                    return storagekey;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error encoding URL: {Url}", url);
                throw;
            }
        }

        /// <summary>
        /// Returns not empty string when decoded param is found within a storage provider
        /// the stored value is returned.
        /// </summary>
        /// <param name="shortcode">The short code to decode</param>
        /// <returns>Tuple containing the URL, permanent flag, and preserveMethod flag</returns>
        public async Task<(string, bool, bool)> TryDecode(string shortcode)
        {
            _logger?.LogInformation("Decoding shortcode: {ShortCode}", shortcode);
            
            // Try to get from cache first
            var cacheKey = $"url_{shortcode}";
            if (_cache.TryGetValue(cacheKey, out (string url, bool permanent, bool preserveMethod) result))
            {
                _logger?.LogInformation("Cache hit for shortcode: {ShortCode}", shortcode);
                return result;
            }
            
            try
            {
                var storagekey = Decode(shortcode);
                result = await _storageProvider.TryGetUrlAsync(storagekey);
                
                // Cache the result if URL was found
                if (!string.IsNullOrEmpty(result.Item1))
                {
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromMinutes(10))
                        .SetAbsoluteExpiration(TimeSpan.FromHours(1));
                    
                    _cache.Set(cacheKey, result, cacheOptions);
                    _logger?.LogInformation("Cached result for shortcode: {ShortCode}", shortcode);
                }
                else
                {
                    _logger?.LogWarning("No URL found for shortcode: {ShortCode}", shortcode);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error decoding shortcode: {ShortCode}", shortcode);
                throw;
            }
        }

        /// <summary>
        /// Encodes a string value to a base62 representation
        /// </summary>
        /// <param name="value">The string to encode</param>
        /// <returns>The encoded string</returns>
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

        /// <summary>
        /// Decodes a base62 encoded string back to its original form
        /// </summary>
        /// <param name="value">The encoded string</param>
        /// <returns>The decoded string</returns>
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

        /// <summary>
        /// Converts a number from one base to another
        /// </summary>
        /// <param name="source">The source number as an array of digits</param>
        /// <param name="sourceBase">The base of the source number</param>
        /// <param name="targetBase">The base to convert to</param>
        /// <returns>The converted number as an array of digits in the target base</returns>
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
