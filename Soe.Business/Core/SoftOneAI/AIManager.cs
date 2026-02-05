using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOneAI.Shared.Models;
using SoftOneAI.Shared.SOE;
using SoftOneAI.Shared.TenantMetrics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.SoftOneAI
{
    public class AIManager : ManagerBase
    {
        private SoftOneAISoeClient _client;
        public AIManager(ParameterObject parameterObject) : base(parameterObject)
        {
            this.Setup();
        }

        private void Setup()
        {
            var context = new UserContext
            {
                IdLoginGuid = parameterObject.IdLoginGuid,
                CompanyGuid = parameterObject.CompanyGuid,
                LicenseGuid = parameterObject.LicenseGuid
            };
            _client = new SoftOneAISoeClient("no token", context);
        }

        private string _language
        {
            get
            {
                return Thread.CurrentThread.CurrentCulture.Name;
            }
        }

        public string ProfessionalizeText(string text)
        {
            if (!HasAIPermission()) return null;

            var result = this._client.ProfessionalizeText(text, _language);

            return result.Data;
        }

        public string SimpleQuery(string text)
        {
            var client = new SoftOneAITenantMetricClient("no token");
            var response = Task.Run(() => client.SimpleQueryAsync(new PromptQuery()
            {
                Prompt = text,
                Provider = AIProvider.AzureOpenAI,
            })).GetAwaiter().GetResult();

            return response.Text;
        }

        public Dictionary<int, string> TranslateText(string original, List<TermGroup_Languages> translateTo)
        {
            if (!HasAIPermission()) return null;

            var languagesAsText = translateTo
                .Select(t => t.ToString())
                .ToList();

            var response = this._client.TranslateText(original, languagesAsText);

            if (!response.Success) return null;

            var result = new Dictionary<int, string>();
            foreach (var kvp in response.Data)
            {
                if (Enum.TryParse(kvp.Key, true, out TermGroup_Languages languageEnum))
                {
                    result.Add((int)languageEnum, kvp.Value);
                }
            }

            return result;
        }

        private bool HasAIPermission()
        {
            return FeatureManager.HasRolePermission(Feature.Common_AI, Permission.Modify, RoleId, ActorCompanyId);
        }
    }
}
