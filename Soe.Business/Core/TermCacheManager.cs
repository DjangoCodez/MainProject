using Google.API.Translate;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Evo.Connectors.Cache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Caching;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core
{
    public sealed class TermCacheManager
    {
        #region Variables

        public readonly static int NrOfLoads = 0;
        public readonly static int NrOfDispose = 0;
        public static string SysTermVersion { get; set; } = "";
        public static bool MailHasBeenSend { get; set; } = false;

        // Managers
        private readonly TermManager tm;

        //Cache
        private Hashtable sysTermCache;
        private readonly Hashtable sysTermByKeyCache;
        private readonly Hashtable languageCache;

        // Locks
        private static readonly object loadSysTermCacheLock = new object();
        private static readonly object getSysTermLock = new object();
        private static readonly object getSysTermGroupLock = new object();

        // Used for marking a hashtable if all terms is loaded or only a single one
        private readonly string allTermGroupDictLoadedKey = "##dictloaded##";
        private readonly string termGroupDictLoadedKey = "##dictloaded##";
        private readonly string dictLoadedValue = "True";

        private readonly string orleansCachekey = "orleansTermCachekey" + Environment.MachineName;

        #endregion

        #region Properties

        private bool IsLoaded
        {
            get
            {
                return sysTermCache != null && sysTermCache.Count > 0 && (sysTermCache[allTermGroupDictLoadedKey] != null);
            }
        }

        #endregion

        #region Singleton

        private TermCacheManager()
        {
            this.sysTermCache = new Hashtable();
            this.sysTermByKeyCache = new Hashtable();
            this.languageCache = new Hashtable();
            this.tm = new TermManager(null);

            foreach (SysLanguageDTO sysLanguage in new LanguageManager(null).GetSysLanguages())
            {
                languageCache.Add(sysLanguage.LangCode, Convert.ToString(sysLanguage.SysLanguageId));
            }
        }
        private static readonly Lazy<TermCacheManager> instance = new Lazy<TermCacheManager>(() => new TermCacheManager());
        public static TermCacheManager Instance
        {
            get => instance.Value;
        }

        #endregion

        #region Language

        public TermGroup_Languages GetLang()
        {
            return (TermGroup_Languages)GetLangId();
        }

        public int GetLangId()
        {
            return GetLangId(Thread.CurrentThread.CurrentCulture.ToString());
        }

        public int GetLangId(string cultureCode)
        {
            if (!String.IsNullOrEmpty(cultureCode) && languageCache != null && languageCache.ContainsKey(cultureCode))
            {
                string language = (string)languageCache[cultureCode];
                if (Int32.TryParse(language, out int langId))
                    return langId;
            }

            return Constants.SYSLANGUAGE_SYSLANGUAGEID_DEFAULT;
        }

        #region Help-methods

        private bool TermIsDuplicate(int langId, string term, string defaultTerm)
        {
            if (langId == Constants.SYSLANGUAGE_SYSLANGUAGEID_DEFAULT && !term.Equals(defaultTerm) && !String.IsNullOrEmpty(defaultTerm) && defaultTerm.Length > 0)
                return true;
            return false;
        }

        private string DuplicateTermMessage(int sysTermId, int sysTermGroupId, string term, string defaultTerm)
        {
            var slm = new SysLogManager(null);

            // Create a unique key based on method parameters
            string cacheKey = $"DuplicateTermMessage_{sysTermId}_{sysTermGroupId}";

            // Get the current count or initialize to 0 if not present
            int count = MemoryCache.Default.Contains(cacheKey) ? (int)MemoryCache.Default.Get(cacheKey) : 0;

            // Increment the count
            count++;

            // Update the cache with the new count
            MemoryCache.Default.Set(cacheKey, count, DateTimeOffset.UtcNow.AddMinutes(240));

            // Log only every 10th message
            if (count % 10 == 0)
            {
                slm.AddSysLogWarningMessage("TermCacheManager", "10 DuplicateTermMessages found", $"sysTermId:{sysTermId},sysTermGroupId:{sysTermGroupId},term:{term},defaultTerm:{defaultTerm}");
            }

            // Return the default term
            return defaultTerm;
        }

        #endregion

        #endregion

        #region Translation

        public string TranslateText(SoeTranslationClient translationClient, string text, int langIdFrom, int langIdTo)
        {
            switch (translationClient)
            {
                case SoeTranslationClient.GoogleTranslate:
                    return TranslateTextUsingGoogle(text, langIdFrom, langIdTo);
                case SoeTranslationClient.BingTranslate:
                    return TranslateTextUsingBing(text, langIdFrom, langIdTo);
                default:
                    return string.Empty;
            }
        }

        private string TranslateTextUsingGoogle(string text, int langIdFrom, int langIdTo)
        {
            string translation = "";

            Language languageFrom = GetGoogleLanguage(langIdFrom);
            Language languageTo = GetGoogleLanguage(langIdTo);

            try
            {
                TranslateClient client = new TranslateClient("http://code.google.com/p/google-api-for-dotnet/");
                translation = client.Translate(text, languageFrom, languageTo);
            }
            catch (Exception ex)
            {
                ex.ToString(); //prevent compiler warning
            }

            return translation;
        }

        private string TranslateTextUsingBing(string text, int langIdFrom, int langIdTo)
        {
            string translation = "";

            string languageFrom = GetBingLanguage(langIdFrom);
            string languageTo = GetBingLanguage(langIdTo);

            string format = "http://api.microsofttranslator.com/v2/Http.svc/Translate?appId={0}&text={1}&from={2}&to={3}";
            string uri = String.Format(format, Constants.SOE_SERVICES_BING_APPID, text, languageFrom, languageTo);

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            WebResponse response = null;

            try
            {
                response = httpWebRequest.GetResponse();
                using (Stream stream = response.GetResponseStream())
                {
                    DataContractSerializer dcs = new DataContractSerializer(Type.GetType("System.String"));
                    translation = (string)dcs.ReadObject(stream);
                }
            }
            catch (WebException ex)
            {
                ex.ToString(); //prevent compiler warning
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                    response = null;
                }
            }

            return translation;
        }

        public Language GetGoogleLanguage(int langId)
        {
            switch (langId)
            {
                case (int)TermGroup_Languages.Swedish:
                    return Language.Swedish;
                case (int)TermGroup_Languages.English:
                    return Language.English;
                case (int)TermGroup_Languages.Finnish:
                    return Language.Finnish;
                case (int)TermGroup_Languages.Norwegian:
                    return Language.Norwegian;
                case (int)TermGroup_Languages.Danish:
                    return Language.Danish;
                default:
                    return Language.Unknown;
            }
        }

        private string GetBingLanguage(int langId)
        {
            switch (langId)
            {
                case (int)TermGroup_Languages.Swedish:
                    return "sv";
                case (int)TermGroup_Languages.English:
                    return "en";
                case (int)TermGroup_Languages.Finnish:
                    return "fi";
                case (int)TermGroup_Languages.Norwegian:
                    return "no";
                case (int)TermGroup_Languages.Danish:
                    return "da";
                default:
                    return string.Empty;
            }
        }

        #endregion

        #region GetText

        public string GetText(int sysTermId, int sysTermGroupId, string defaultTerm)
        {
            int langId = GetLangId(Thread.CurrentThread.CurrentCulture.Name);
            return GetText(sysTermId, sysTermGroupId, defaultTerm, langId);
        }

        public string GetText(int sysTermId, int sysTermGroupId, string defaultTerm, string cultureCode)
        {
            int langId = GetLangId(cultureCode);
            return GetText(sysTermId, sysTermGroupId, defaultTerm, langId);
        }

        public string GetText(int sysTermId, int sysTermGroupId, string defaultTerm, int langId)
        {
            return GetSysTerm(sysTermId, sysTermGroupId, langId, defaultTerm);
        }

        public string GetText(string translationKey, int langId = 0)
        {
            if (langId == 0)
                langId = GetLangId(Thread.CurrentThread.CurrentCulture.Name);
            return GetSysTerm(translationKey, langId);
        }

        #endregion

        #region SysTerm

        public void SetupSysTermCacheTS(string server, string thread, bool force = false)
        {
            if (!force && IsLoaded)
                return;

            RestoreCacheFromOrleans();

            if (!IsLoaded)
                SetupSysTermCacheThreadSafe(true);
            else
                Task.Run(() => CompEntitiesProvider.RunWithTaskScopedReadOnlyEntities(() => SetupSysTermCacheThreadSafe(force)));
        }

        public void SetupSysTermCacheThreadSafe(bool force = false)
        {
            if (!force && IsLoaded)
                return;

            List<SysTermGroup> sysTermGroups = tm.GetSysTermGroupsFromDatabase();
            sysTermCache = new Hashtable();

            lock (loadSysTermCacheLock)
            {

                tm.LoadSysTermsFromDatabase(sysTermCache, termGroupDictLoadedKey, dictLoadedValue);

                foreach (SysTermGroup sysTermGroup in sysTermGroups)
                {
                    LoadSysTermCacheTS(sysTermGroup.SysTermGroupId, skipDBQuery: true);
                }

                // Mark that all SysTermGroups has been loaded into cache
                if (!sysTermCache.ContainsKey(allTermGroupDictLoadedKey))
                    sysTermCache.Add(allTermGroupDictLoadedKey, dictLoadedValue);

                // Save cache to Orleans
                UpsertToOrleansCache();
            }
        }

        private void UpsertToOrleansCache()
        {
            try
            {
                // Convert Hashtable to Dictionary before caching
                var dictionary = sysTermCache.HashtableToDictionary();
                EvoDistributionCacheConnector.UpsertCachedValue(orleansCachekey, dictionary);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString()); // Prevent compiler warning and log error
            }
        }

        private void RestoreCacheFromOrleans()
        {
            try
            {
                if (sysTermCache == null || sysTermCache.Count == 0)
                {
                    // Retrieve the cached Dictionary
                    var cachedDictionary = EvoDistributionCacheConnector.GetCachedValue<Dictionary<string, object>>(orleansCachekey);

                    if (cachedDictionary != null && cachedDictionary.Count > 0)
                    {
                        // Convert the Dictionary back to a Hashtable
                        sysTermCache = cachedDictionary.DictionaryToHashtable();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString()); // Prevent compiler warning and log error
            }
        }

        public void ClearSysTermCacheTS()
        {
            lock (loadSysTermCacheLock)
            {
                sysTermCache.Clear();
                sysTermCache = new Hashtable();
            }
        }

        public void RestoreSysTermCacheTS(string server, string thread)
        {
            ClearSysTermCacheTS();
            SetupSysTermCacheTS(server, thread, true);
        }

        #region Help-methods

        private string GetSysTerm(int sysTermId, int sysTermGroupId, int langId, string defaultTerm)
        {
            if (sysTermGroupId == 0)
                return String.Empty;

            //Get term from Cache
            string term = GetSysTermFromCache(sysTermId, sysTermGroupId, langId, defaultTerm);
            if (!String.IsNullOrEmpty(term))
                return term;

            //Get term from database and add to Cache
            return GetSysTermTS(sysTermId, sysTermGroupId, langId, defaultTerm);
        }

        private string GetSysTerm(string translationKey, int langId)
        {
            if (string.IsNullOrEmpty(translationKey))
                return string.Empty;

            //Get term from Cache
            string term = GetSysTermFromCache(translationKey, langId);
            if (!string.IsNullOrEmpty(term))
                return term;

            //Get term from database and add to Cache
            return GetSysTermTS(translationKey, langId);
        }

        private string GetSysTermTS(int sysTermId, int sysTermGroupId, int langId, string defaultTerm)
        {
            lock (getSysTermLock)
            {
                //Try get term from Cache. Could be added by other user when holding lock
                string term = GetSysTermFromCache(sysTermId, sysTermGroupId, langId, defaultTerm);
                if (!String.IsNullOrEmpty(term))
                    return term;

                //Try get term from database
                term = GetSysTermFromDatabase(sysTermId, sysTermGroupId, langId, defaultTerm);
                if (!String.IsNullOrEmpty(term))
                    return term;

                if (String.IsNullOrEmpty(defaultTerm))
                    return "";
                else
                    return "[" + defaultTerm + "]";
            }
        }

        private string GetSysTermFromCache(int sysTermId, int sysTermGroupId, int langId, string defaultTerm)
        {
            string term = "";
            string sysTermIdStr = sysTermId.ToString();
            string sysTermGroupIdStr = sysTermGroupId.ToString();

            // Try to get SysTermGroup cache from SysTerm cache
            Hashtable sysTermGroupCache = (Hashtable)sysTermCache[sysTermGroupIdStr];
            if (sysTermGroupCache != null)
            {
                // Try to get term from SysTermGroup cache
                TermObject termObject = (TermObject)sysTermGroupCache[sysTermIdStr];
                if (termObject != null)
                {
                    // Check if the term exists in cache for the given language
                    term = termObject.GetTerm(langId);
                    if (!String.IsNullOrEmpty(term))
                    {
                        if (TermIsDuplicate(langId, term, defaultTerm))
                            return DuplicateTermMessage(sysTermId, sysTermGroupId, term, defaultTerm);

                        return term;
                    }
                }
            }

            return term;
        }

        private string GetSysTermFromDatabase(int sysTermId, int sysTermGroupId, int langId, string defaultTerm)
        {
            string term = "";
            string sysTermIdStr = sysTermId.ToString();
            string sysTermGroupIdStr = sysTermGroupId.ToString();

            #region SysTermGroup Cache

            //Setup SysTermGroup Cache
            Hashtable sysTermGroupCache;
            TermObject termObject = null;
            if (sysTermCache.ContainsKey(sysTermGroupIdStr))
            {
                sysTermGroupCache = (Hashtable)sysTermCache[sysTermGroupIdStr];
                termObject = (TermObject)sysTermGroupCache[sysTermIdStr];
            }
            else
            {
                sysTermGroupCache = new Hashtable();
                sysTermCache.Add(sysTermGroupIdStr, sysTermGroupCache);
            }

            #endregion

            #region Get from database / Add to Cache

            SysTerm sysTerm = tm.GetSysTermFromDatabaseSafe(sysTermId, sysTermGroupId, langId);
            if (sysTerm != null)
            {
                term = sysTerm.Name;

                if (TermIsDuplicate(langId, term, defaultTerm))
                    return DuplicateTermMessage(sysTermId, sysTermGroupId, term, defaultTerm);

                //Set term in Cache for the given language
                if (termObject != null)
                {
                    // Add/update term
                    termObject.SetTerm(term, langId, replaceNewLineForLangIdOne: false);
                }
                else
                {
                    //Create term
                    termObject = new TermObject(sysTermId, sysTermGroupId);
                    termObject.SetTerm(term, langId, replaceNewLineForLangIdOne: false);

                    //Add term to SysTermGroup Cache
                    sysTermGroupCache.Add(sysTermIdStr, termObject);
                }

                return term;
            }

            #endregion

            #region Add to database / Add to Cache

            //Add term to database and Cache
            if (langId == Constants.SYSLANGUAGE_SYSLANGUAGEID_DEFAULT && !String.IsNullOrEmpty(defaultTerm))
            {
                term = defaultTerm;

                //Insert term in SysTerm using the default langid
                ActionResult result = tm.InsertSysTerm(sysTermId, sysTermGroupId, langId, defaultTerm);
                if (!result.Success)
                    return DuplicateTermMessage(sysTermId, sysTermGroupId, term, defaultTerm);

                return term;
            }

            #endregion

            return term;
        }

        private void LoadSysTermCacheTS(int sysTermGroupId, bool skipDBQuery = false)
        {
            string sysTermGroupIdStr = sysTermGroupId.ToString();

            if (sysTermCache == null || sysTermCache.Count == 0)
            {
                sysTermCache = new Hashtable();
                tm.LoadSysTermsFromDatabase(sysTermCache, termGroupDictLoadedKey, dictLoadedValue);
                return;
            }

            // Try to get SysTermGroup cache from SysTerm cache
            Hashtable sysTermGroupCache = (Hashtable)sysTermCache[sysTermGroupIdStr];
            if (sysTermGroupCache == null)
                sysTermGroupCache = new Hashtable();

            // If not whole collection is loaded, dump cache and reload it
            if (sysTermGroupCache[termGroupDictLoadedKey] == null && !skipDBQuery)
            {
                //Clear
                sysTermGroupCache.Clear();

                // Load SysTerms from database
                tm.LoadSysTermsFromDatabase(sysTermGroupCache, sysTermGroupId);

                // Mark that all SysTerms has been loaded into cache
                sysTermGroupCache.Add(termGroupDictLoadedKey, dictLoadedValue);
            }

            if (sysTermCache.ContainsKey(sysTermGroupIdStr))
                sysTermCache[sysTermGroupIdStr] = sysTermGroupCache;
            else
                sysTermCache.Add(sysTermGroupIdStr, sysTermGroupCache);
        }

        private string GetSysTermTS(string translationKey, int langId)
        {
            lock (getSysTermLock)
            {
                //Try get term from Cache. Could be added by other user when holding lock
                string term = GetSysTermFromCache(translationKey, langId);
                if (!string.IsNullOrEmpty(term))
                    return term;

                //Try get term from database
                term = GetSysTermFromDatabase(translationKey, langId);
                if (!string.IsNullOrEmpty(term))
                    return term;

                return string.Empty;
            }
        }

        private string GetSysTermFromCache(string translationKey, int langId)
        {
            TermByKeyObject termObject = (TermByKeyObject)sysTermByKeyCache[translationKey];
            return termObject?.GetTerm(langId) ?? string.Empty;
        }

        private string GetSysTermFromDatabase(string translationKey, int langId)
        {
            string term = "";

            #region Cache

            //Setup cache
            TermByKeyObject termObject = null;
            if (sysTermByKeyCache.ContainsKey(translationKey))
                termObject = (TermByKeyObject)sysTermByKeyCache[translationKey];

            #endregion

            #region Get from database / Add to Cache

            SysTerm sysTerm = tm.GetSysTermByKeyFromDatabaseSafe(translationKey, langId);
            if (sysTerm != null)
            {
                term = sysTerm.Name;

                //Set term in Cache for the given language
                if (termObject != null)
                {
                    // Add/update term
                    termObject.SetTerm(term, langId);
                }
                else
                {
                    //Create term
                    termObject = new TermByKeyObject(translationKey);
                    termObject.SetTerm(term, langId);

                    //Add term to Cache
                    sysTermByKeyCache.Add(translationKey, termObject);
                }

                return term;
            }

            #endregion

            return term;
        }

        #endregion

        #endregion

        #region SysTermGroup

        /// <summary>
        /// Get list of terms from specified TermGroup.
        /// </summary>
        /// <param name="termGroup">Id of expected SysTermGroup</param>
        /// <param name="langId">Language Id</param>
        /// <param name="addEmptyRow">If true, add an empty row at the beginning (will get Id = 0)</param>
        /// <param name="skipUnknown">If true, and the term group contain an 'unknown' value with Id = 0, this value will not be returned in the list</param>
        /// <returns>List of terms</returns>
        public List<GenericType> GetTermGroupContent(TermGroup termGroup, int langId = 0, bool addEmptyRow = false, bool skipUnknown = false, bool sortById = false)
        {
            try
            {
                if (langId == 0)
                    langId = GetLangId();

                int sysTermGroupId = (int)termGroup;
                List<GenericType> termGroupContent = new List<GenericType>();

                List<TermObject> terms = GetTermsForSysTermGroup(sysTermGroupId, langId);
                if (!terms.IsNullOrEmpty())
                {
                    foreach (TermObject term in terms.OrderBy(t => t.TermId))
                    {
                        if (skipUnknown && term.TermId == 0)
                            continue;

                        termGroupContent.Add(new GenericType
                        {
                            Id = term.TermId,
                            Name = term.GetTerm(langId),
                        });
                    }
                }

                if (addEmptyRow && !termGroupContent.Any(i => i.Id == 0))
                    termGroupContent.Insert(0, new GenericType { Id = 0, Name = " ", });

                return (sortById ? termGroupContent.OrderBy(i => i.Id) : termGroupContent.OrderBy(i => i.Name)).ToList();
            }
            catch (Exception ex)
            {
                ex.ToString(); //prevent compiler warning
                return new List<GenericType>();
            }
        }

        public Dictionary<int, string> GetTermGroupDict(TermGroup termGroup, int langId = 0, bool addEmptyRow = false, bool includeKey = false)
        {
            if (langId == 0)
                langId = GetLangId();

            int sysTermGroupId = (int)termGroup;

            string key = $"GetTermGroupDict#{sysTermGroupId}#{langId}";
            Dictionary<int, string> dict = BusinessMemoryCache<Dictionary<int, string>>.Get(key);
            if (dict != null)
                return dict;

            dict = new Dictionary<int, string>();

            List<TermObject> terms = GetTermsForSysTermGroup(sysTermGroupId, langId);
            if (!terms.IsNullOrEmpty())
            {
                foreach (TermObject term in terms)
                {
                    if (dict.ContainsKey(term.TermId))
                        continue;

                    if (includeKey)
                        dict.Add(term.TermId, $"{term.TermId}. {term.GetTerm(langId)}");
                    else
                        dict.Add(term.TermId, term.GetTerm(langId));
                }
            }

            if (dict.Any())
                BusinessMemoryCache<Dictionary<int, string>>.Set(key, dict, 180);

            if (addEmptyRow && !dict.ContainsKey(0))
                dict.Add(0, "");

            return dict;
        }

        //public Dictionary<string, string> GetTermGroupDictByKey(TermGroup termGroup, int langId = 0)
        //{
        //    if (langId == 0)
        //        langId = GetLangId();

        //    int sysTermGroupId = (int)termGroup;

        //    string key = $"GetTermGroupDictByKey#{sysTermGroupId}#{langId}";
        //    Dictionary<string, string> dict = BusinessMemoryCache<Dictionary<string, string>>.Get(key);
        //    if (dict != null)
        //        return dict;

        //    dict = new Dictionary<string, string>();

        //    List<TermObject> terms = GetTermsForSysTermGroup(sysTermGroupId, langId);
        //    if (!terms.IsNullOrEmpty())
        //    {
        //        foreach (TermObject term in terms)
        //        {
        //            if (dict.ContainsKey(term.TermId))
        //                continue;

        //            if (includeKey)
        //                dict.Add(term.TermId, $"{term.TermId}. {term.GetTerm(langId)}");
        //            else
        //                dict.Add(term.TermId, term.GetTerm(langId));
        //        }
        //    }

        //    if (dict.Any())
        //        BusinessMemoryCache<Dictionary<string, string>>.Set(key, dict, 180);

        //    return dict;
        //}

        /// <summary>
        /// Use GetTermGroupDict() instead
        /// </summary>
        /// <returns></returns>
        public SortedDictionary<int, string> GetTermGroupDictSorted(TermGroup termGroup, int langId = 0, bool addEmptyRow = false, bool includeKey = false, int? minKey = null, int? maxKey = null)
        {
            var sortedDict = new SortedDictionary<int, string>();

            var dict = GetTermGroupDict(termGroup, langId, addEmptyRow, includeKey);
            foreach (var pair in dict)
            {
                if (!sortedDict.ContainsKey(pair.Key) && (!minKey.HasValue || minKey.Value <= pair.Key) && (!maxKey.HasValue || maxKey.Value >= pair.Key))
                    sortedDict.Add(pair.Key, pair.Value);
            }

            return sortedDict;
        }


        /// <summary>
        /// Obsolete. This method is deprecated. Use GetTermGroupDict() instead.
        /// </summary>
        /// <param name="termGroup"></param>
        /// <param name="cultureCode"></param>
        /// <param name="addEmptyRow"></param>
        /// <param name="includeKey"></param>
        /// <param name="sortByValue"></param>
        /// <returns></returns>
        public Dictionary<int, string> GetTermGroupDictFromWeb(TermGroup termGroup, string cultureCode, bool addEmptyRow, bool includeKey, bool? sortByValue = null)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();

            if (addEmptyRow)
                dict.Add(0, " ");

            int sysTermGroupId = (int)termGroup;
            int langId = GetLangId(cultureCode);
            string sysTermGroupIdStr = sysTermGroupId.ToString();

            #region SysTermGroup Cache  

            Hashtable sysTermGroupCache = (Hashtable)sysTermCache[sysTermGroupIdStr];
            if (sysTermGroupCache == null || sysTermGroupCache[termGroupDictLoadedKey] == null)
            {
                //Load SysTermGroup Cache  
                LoadSysTermCacheTS(sysTermGroupId);

                sysTermGroupCache = (Hashtable)sysTermCache[sysTermGroupIdStr];
                if (sysTermGroupCache == null)
                {
                    //Should never happen  
                    return dict;
                }
            }

            #endregion

            #region SysTerm  

            string langIdStr = langId.ToString();
            foreach (string groupKey in sysTermGroupCache.Keys)
            {
                if (groupKey.Equals(termGroupDictLoadedKey))
                    continue;

                // Try to get term from SysTermGroup cache  
                TermObject termObject = (TermObject)sysTermGroupCache[groupKey];
                string termStr = termObject?.GetTerm(langIdStr) ?? string.Empty;

                int key = Convert.ToInt32(groupKey);
                if (!string.IsNullOrEmpty(termStr) && !dict.ContainsKey(key))
                {
                    if (includeKey)
                        dict.Add(key, $"{key}. {termStr}");
                    else
                        dict.Add(key, termStr);
                }
            }

            #endregion

            //Sort by value if not key is included  
            if (!sortByValue.HasValue)
                sortByValue = !includeKey;

            // TODO: Sort the dictionary, temporary solution  
            return dict.Sort(true, sortByValue.Value);
        }

        #region Help-methods

        private List<TermObject> GetTermsForSysTermGroup(int sysTermGroupId, int langId)
        {
            if (sysTermGroupId == 0)
                return null;

            Hashtable termGroup = GetSysTermGroupFromCache(sysTermGroupId) ?? GetSysTermGroupTS(sysTermGroupId);
            if (termGroup == null)
                return null;

            List<TermObject> terms = new List<TermObject>();
            foreach (string key in termGroup.Keys)
            {
                if (key == termGroupDictLoadedKey)
                    continue;

                TermObject termObject = (TermObject)termGroup[key];
                if (termObject != null && !string.IsNullOrEmpty(termObject.GetTerm(langId)))
                    terms.Add(termObject);
            }

            return terms;
        }

        /// <summary>
        /// Synchronizes all access to database and Cache.
        /// All terms are loaded at Application Start so this method shouldnt be called often.
        /// </summary>
        private Hashtable GetSysTermGroupTS(int sysTermGroupId)
        {
            lock (getSysTermGroupLock)
            {
                //Try get term from Cache. Could be added by other user when holding lock
                Hashtable termGroup = GetSysTermGroupFromCache(sysTermGroupId);
                if (termGroup != null)
                    return termGroup;

                LoadSysTermCacheTS(sysTermGroupId);

                //This time it must be in the cache
                return GetSysTermGroupFromCache(sysTermGroupId);
            }
        }

        private Hashtable GetSysTermGroupFromCache(int sysTermGroupId)
        {
            // Try to get SysTermGroup cache from SysTerm cache
            return (Hashtable)sysTermCache[sysTermGroupId.ToString()];
        }

        #endregion

        #endregion
    }
}
