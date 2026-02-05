using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using SO.Internal.Shared.Api.Cache.DistrubutedCache;
using SoftOne.Soe.Business.Evo.Connectors.Cache;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Evo.Connectors.Cache.Tests
{
    [TestClass()]
    public class EvoDistributionCacheConnectorTests
    {
        [TestMethod()]
        public void UpsertCachedValueTest()
        {
            AccountingPrioDTO accountingPrioDTO = new AccountingPrioDTO()
            {
                AccountId = 1,
                AccountNr = "test",
            };
            var key = Guid.NewGuid().ToString();
            var result = EvoDistributionCacheConnector.UpsertCachedValue<AccountingPrioDTO>(key, accountingPrioDTO);

            Assert.IsTrue(result.Success);

            var cachedValue = EvoDistributionCacheConnector.GetCachedValue<AccountingPrioDTO>(key);

            Assert.IsNotNull(cachedValue);
            Assert.AreEqual(accountingPrioDTO.AccountId, cachedValue.AccountId);
            Assert.AreEqual(accountingPrioDTO.AccountNr, cachedValue.AccountNr);
        }


        [TestMethod()]
        public void TestPerformanceByLoopWithDifferentValuesAndObjects100times()
        {
            for (int i = 0; i < 100; i++)
            {
                AccountingPrioDTO accountingPrioDTO = new AccountingPrioDTO()
                {
                    AccountId = i,
                    AccountNr = "test" + i,
                };
                var key = Guid.NewGuid().ToString();
                var result = EvoDistributionCacheConnector.UpsertCachedValue<AccountingPrioDTO>(key, accountingPrioDTO);

                Assert.IsTrue(result.Success);

                var cachedValue = EvoDistributionCacheConnector.GetCachedValue<AccountingPrioDTO>(key);

                Assert.IsNotNull(cachedValue);
                Assert.AreEqual(accountingPrioDTO.AccountId, cachedValue.AccountId);
                Assert.AreEqual(accountingPrioDTO.AccountNr, cachedValue.AccountNr);
            }
        }

        [TestMethod()]
        public void TestPerformanceWithLargeList()
        {
            List<AccountingPrioDTO> accountingPrioDTOs = new List<AccountingPrioDTO>();
            for (int i = 0; i < 100000; i++)
            {
                accountingPrioDTOs.Add(new AccountingPrioDTO()
                {
                    AccountId = i,
                    AccountNr = "test" + i,
                });
            }

            var key = Guid.NewGuid().ToString();
            var result = EvoDistributionCacheConnector.UpsertCachedValue<List<AccountingPrioDTO>>(key, accountingPrioDTOs);
            Assert.IsTrue(result.Success);
            var cachedValue = EvoDistributionCacheConnector.GetCachedValue<List<AccountingPrioDTO>>(key);
            foreach (var accountingPrioDTO in cachedValue)
            {
                var match = accountingPrioDTOs.FirstOrDefault(x => x.AccountId == accountingPrioDTO.AccountId && x.AccountNr == accountingPrioDTO.AccountNr);
            }
        }

        [TestMethod]
        public void TestBatchPerformance()
        {
            var batch = new DistributedCacheUpsertBatchRequest();
            var requests = new List<DistributedCacheUpsertRequest>();
            var untouched = new List<DistributedCacheUpsertRequest>();
            for (int i = 0; i < 10; i++)
            {
                AccountingPrioDTO accountingPrioDTO = new AccountingPrioDTO()
                {
                    AccountId = i,
                    AccountNr = "test" + i,
                };
                var key = Guid.NewGuid().ToString();
                var request = new DistributedCacheUpsertRequest()
                {
                    Key = key,
                    Expiration = TimeSpan.FromHours(1),
                    Value = JsonConvert.SerializeObject(accountingPrioDTO)
                };
                requests.Add(request);
                untouched.Add(request.CloneDTO());
            }

            batch.Requests = requests.ToArray();

            var result = EvoDistributionCacheConnector.UpsertCacheValuesBatch(batch);

            foreach (var req in untouched)
            {
                var cachedValue = EvoDistributionCacheConnector.GetCachedValue<AccountingPrioDTO>(req.Key);
                Assert.IsNotNull(cachedValue);
                var originalRequest = requests.Find(x => x.Key == req.Key);
                var originalValue = JsonConvert.DeserializeObject<AccountingPrioDTO>(originalRequest.Value);
                Assert.AreEqual(originalValue.AccountId, cachedValue.AccountId);
                Assert.AreEqual(originalValue.AccountNr, cachedValue.AccountNr);
            }
        }
    }
}