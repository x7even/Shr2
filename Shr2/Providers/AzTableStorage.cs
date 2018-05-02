using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

using Shr2.Interfaces;

namespace Shr2.Providers
{
    public class AzTableStorage : IStorageProvider
    {
        private static ConcurrentDictionary<string, long> TableIndex = new ConcurrentDictionary<string, long>();

        private CloudStorageAccount _cloudStorage; 
            //CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=shrtr;AccountKey=JpcjuHcIz4/Fe8SUK6b5rztXAa0GLinz70jcwPA3pQgQJPEVNyugQXQ7Z1EJrkaFs45QJEnDZ6x/szqzTpDmnQ==;EndpointSuffix=core.windows.net");

        public AzTableStorage(IConfig config)
        {
            _cloudStorage = CloudStorageAccount.Parse(config.GetConfig().StorageConnectionString);
        }

        private int retryCount = 0;

        public async Task<bool> Init()
        {
            var table = await TableExecutor<TableEntity>.CreateTableAsync("shorturls", _cloudStorage);
            for (int i = 0; i < 99; i++)
            {
                var partkey = i.ToString("00");
                var index = await TableExecutor<IndexEntity>.PointQueryAsync(table, partkey, "index");

                if(index == null)
                {
                    index = new IndexEntity(partkey, "index", 0L);
                    await TableExecutor<IndexEntity>.InsertOrMergeEntityAsync(table, index);
                }
                TableIndex.AddOrUpdate(partkey, index.Index, (k, v) => v = index.Index);
            }
            return true;
        }

        public async Task<string> TryAddNewUrlAsync(string url, bool permanent = false, bool preserve = true, bool statsCount = false)
        {
            CloudTable table = await TableExecutor<TableEntity>.CreateTableAsync("shorturls", _cloudStorage);

            var id = GetNext();
            var result = await TableExecutor<ShorterData>.InsertOnlyEntityAsync(table, new ShorterData() { Url = url, PartitionKey = id.table, RowKey = id.id, Permanent = permanent, PreserveMethod = preserve, Count = 0, StatCount = statsCount });

            if (result != null)
            {
                if (result.RowKey != "conflict")
                {
                    //fnf
                    Task.Factory.StartNew(() => {
                        if (TableIndex.TryGetValue(result.PartitionKey, out long indexvalue))
                            TableExecutor<IndexEntity>.InsertOrMergeEntityAsync(table, new IndexEntity(result.PartitionKey, "index", indexvalue));
                    });
                    return (result.PartitionKey + result.RowKey);
                }
                else
                {
                    //deal with conflict, retry 3 times then return fail (empty)
                    retryCount++;
                    if (retryCount <= 3)
                        return await TryAddNewUrlAsync(url, permanent, preserve);
                    else
                        return string.Empty;
                }
            }
            return "error";
        }

        private static (string id, string table) GetNext()
        {
            var tablekey = new Random().Next(99).ToString("00"); //DateTime.UtcNow.Day.ToString("00");
            var rownum = TableIndex[tablekey] + 1;
            while (!TableIndex.TryUpdate(tablekey, rownum, rownum - 1))
                return GetNext();
            return (rownum.ToString(), tablekey.ToString());
        }

        public async Task<(string url, bool permanent, bool preserveMethod)> TryGetUrlAsync(string idcode)
        {
            if (idcode.Length >= 3 && Int64.TryParse(idcode, out long discard))
            {
                var partkey = "";
                var rowkey = "";
                var chars = idcode.ToCharArray();
                for (int i = 0; i < idcode.Length; i++)
                {
                    if (i <= 1)
                        partkey += idcode[i];
                    else
                        rowkey += idcode[i];
                }
                CloudTable table = await TableExecutor<TableEntity>.CreateTableAsync("shorturls", _cloudStorage);
                var result = await TableExecutor<ShorterData>.PointQueryAsync(table, partkey, rowkey);
                if (result != null)
                    return (result.Url, result.Permanent, result.PreserveMethod);
            }
            return (String.Empty, false, true);
        }

        internal class ShorterData : TableEntity
        {
            public ShorterData() { }

            public ShorterData(string partKey, string idKey)
            {
                PartitionKey = partKey;
                RowKey = idKey;
            }

            public string Url {get;set;}

            public bool Permanent { get; set; }

            public bool PreserveMethod { get; set; }

            public bool StatCount { get; set; }

            public Int64 Count { get; set; }
        }

        internal class IndexEntity : TableEntity
        {
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

            public long Index { get; set; }
        }

    }

    public static class TableExecutor<T> where T : TableEntity
    {

        public static async Task<T> PointQueryAsync(CloudTable table, string partitionKey, string rowKey)
        {
            try
            {
                TableOperation retrieveOp = TableOperation.Retrieve<T>(partitionKey, rowKey);
                TableResult result = await table.ExecuteAsync(retrieveOp);
                return result.Result as T;
                //T customer = result.Result as T;
                //if (customer != null)
                //{
                //    Console.WriteLine("\t{0}\t{1}\t{2}\t{3}", customer.PartitionKey, customer.RowKey, customer.Email, customer.PhoneNumber);
                //}

                //return customer;
            }
            catch (StorageException sex)
            {
                //log
                throw;
            }
        }

        public static async Task<T> InsertOrMergeEntityAsync(CloudTable table, T entity)
        {
            try
            {
                TableOperation tableop = TableOperation.InsertOrMerge(entity);
                TableResult result = await table.ExecuteAsync(tableop);
                return result.Result as T;
                //T insertedCustomer = result.Result as T;

                //return insertedCustomer;
            }
            catch (StorageException sex)
            {
                //log
                //throw;
                return null;
            }
        }

        public static async Task<T> InsertOnlyEntityAsync(CloudTable table, T entity)
        {
            try
            {
                TableOperation tableop = TableOperation.Insert(entity, true);
                TableResult result = await table.ExecuteAsync(tableop);
                return result.Result as T;
                //T insertedCustomer = result.Result as T;

                //return insertedCustomer;
            }
            catch (StorageException sex)
            {
                if (sex.RequestInformation.HttpStatusCode == 409)
                {
                    entity.RowKey = "conflict";
                    return entity;
                }
                else
                    return null;
            }
        }

        public static async Task<CloudTable> CreateTableAsync(string tableName, CloudStorageAccount storageAccount)
        {
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(); 
            CloudTable table = tableClient.GetTableReference(tableName);
            try
            {
                var res = await table.CreateIfNotExistsAsync();
            }
            catch (StorageException)
            {
                throw;
            }
            return table;
        }
    }
}
