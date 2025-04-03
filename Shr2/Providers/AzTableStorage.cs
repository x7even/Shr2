﻿﻿using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;

using Shr2.Interfaces;

namespace Shr2.Providers
{
    public class AzTableStorage : IStorageProvider
    {
        private static ConcurrentDictionary<string, long> TableIndex = new ConcurrentDictionary<string, long>();
        private readonly TableServiceClient _tableServiceClient;
        private readonly TableClient _tableClient;
        private readonly ILogger<AzTableStorage>? _logger;
        private int retryCount = 0;

        public AzTableStorage(IConfig config, ILogger<AzTableStorage>? logger = null)
        {
            _tableServiceClient = new TableServiceClient(config.GetConfig().StorageConnectionString);
            _tableClient = _tableServiceClient.GetTableClient("shorturls");
            _logger = logger;
        }

        public async Task<bool> Init()
        {
            await _tableClient.CreateIfNotExistsAsync();
            
            for (int i = 0; i < 99; i++)
            {
                var partkey = i.ToString("00");
                var index = await GetEntityAsync<IndexEntity>(partkey, "index");

                if (index == null)
                {
                    index = new IndexEntity(partkey, "index", 0L);
                    await UpsertEntityAsync(index);
                }
                
                TableIndex.AddOrUpdate(partkey, index.Index, (k, v) => index.Index);
            }
            
            return true;
        }

        public async Task<string> TryAddNewUrlAsync(string url, bool permanent = false, bool preserve = true, bool statsCount = false)
        {
            var id = GetNext();
            var entity = new ShorterData
            {
                PartitionKey = id.table,
                RowKey = id.id,
                Url = url,
                Permanent = permanent,
                PreserveMethod = preserve,
                Count = 0,
                StatCount = statsCount
            };

            try
            {
                await _tableClient.AddEntityAsync(entity);
                
                // Update index in background
                _ = Task.Run(async () =>
                {
                    if (TableIndex.TryGetValue(entity.PartitionKey, out long indexValue))
                    {
                        await UpsertEntityAsync(new IndexEntity(entity.PartitionKey, "index", indexValue));
                    }
                });
                
                return entity.PartitionKey + entity.RowKey;
            }
            catch (RequestFailedException ex) when (ex.Status == 409) // Conflict
            {
                retryCount++;
                if (retryCount <= 3)
                {
                    return await TryAddNewUrlAsync(url, permanent, preserve, statsCount);
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error adding URL: {Url}", url);
                return "error";
            }
        }

        private static (string id, string table) GetNext()
        {
            var tablekey = new Random().Next(99).ToString("00");
            var rownum = TableIndex[tablekey] + 1;
            
            while (!TableIndex.TryUpdate(tablekey, rownum, rownum - 1))
            {
                return GetNext();
            }
            
            return (rownum.ToString(), tablekey);
        }

        public async Task<(string url, bool permanent, bool preserveMethod)> TryGetUrlAsync(string idcode)
        {
            if (idcode.Length >= 3 && Int64.TryParse(idcode, out long _))
            {
                var partkey = "";
                var rowkey = "";
                
                for (int i = 0; i < idcode.Length; i++)
                {
                    if (i <= 1)
                        partkey += idcode[i];
                    else
                        rowkey += idcode[i];
                }
                
                var result = await GetEntityAsync<ShorterData>(partkey, rowkey);
                
                if (result != null)
                {
                    return (result.Url, result.Permanent, result.PreserveMethod);
                }
            }
            
            return (string.Empty, false, true);
        }

        private async Task<T?> GetEntityAsync<T>(string partitionKey, string rowKey) where T : class, ITableEntity, new()
        {
            try
            {
                var response = await _tableClient.GetEntityAsync<T>(partitionKey, rowKey);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving entity: {PartitionKey}/{RowKey}", partitionKey, rowKey);
                throw;
            }
        }

        private async Task<T> UpsertEntityAsync<T>(T entity) where T : ITableEntity
        {
            try
            {
                await _tableClient.UpsertEntityAsync(entity);
                return entity;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error upserting entity: {PartitionKey}/{RowKey}", entity.PartitionKey, entity.RowKey);
                throw;
            }
        }

        internal class ShorterData : ITableEntity
        {
            public string PartitionKey { get; set; } = string.Empty;
            public string RowKey { get; set; } = string.Empty;
            public DateTimeOffset? Timestamp { get; set; }
            public ETag ETag { get; set; }
            
            public string Url { get; set; } = string.Empty;
            public bool Permanent { get; set; }
            public bool PreserveMethod { get; set; }
            public bool StatCount { get; set; }
            public long Count { get; set; }
        }

        internal class IndexEntity : ITableEntity
        {
            public string PartitionKey { get; set; } = string.Empty;
            public string RowKey { get; set; } = string.Empty;
            public DateTimeOffset? Timestamp { get; set; }
            public ETag ETag { get; set; }
            
            public long Index { get; set; }

            public IndexEntity() { }

            public IndexEntity(string partKey, string rowKey)
            {
                PartitionKey = partKey;
                RowKey = rowKey;
            }

            public IndexEntity(string partKey, string rowKey, long index)
            {
                PartitionKey = partKey;
                RowKey = rowKey;
                Index = index;
            }
        }
    }
}
