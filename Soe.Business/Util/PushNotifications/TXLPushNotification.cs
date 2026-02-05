using SoftOne.Soe.Common.Util;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace SoftOne.Soe.Business.Util.PushNotifications
{
    class TXLPushNotification : SoePushNotification
    {
        //readonly private string paramStrGO = "key=z828mrn4sphs35h5wxy7cj2w8eyd7xaj&message={0}&userid={1}";
        //readonly private string paramStrGODev = "key=ym66x4pzuzq2886teuxj2j66x46b4cuw&message={0}&userid={1}";
        readonly private string paramStrGO = "key={0}&message={1}&userid={2}&badge=-1";

        //SoftOne
        readonly private string key0 = "z828mrn4sphs35h5wxy7cj2w8eyd7xaj";
        readonly private string key1 = "j3ryrfhmgypvnepx658x5prk2q2b43ah";
        readonly private string key2 = "q6czqhva87femsey3taacgh6cctpzxd3";
        readonly private string key3 = "dfzy5s67stbggksgktw8fsxcakrkcyng";
        readonly private string key4 = "k3hcr8gqnwngyjmatbf6mezufs2rbbnj";

        readonly private string key0Dev = "ym66x4pzuzq2886teuxj2j66x46b4cuw";
        readonly private string key1Dev = "a43pkhhu68aemeux35ctjagrbpe8zv86";
        readonly private string key2Dev = "s7t32v3ud8v2apczch3eeq747nnvhspk";
        readonly private string key3Dev = "bmpsp4w3z8fg4deskgscs8gq37sf8bqh";
        readonly private string key4Dev = "nmhgmvjy5g44xhy27r6svz6cujh83wq3";

        //Flexibla kontoret
        readonly private string FK_key0 = "6yavpe5pp2dr8hvndqd7hetbchhgxswb";
        readonly private string FK_key0Dev = "egm4qp58tgvwpfubqh2e5cpty3gp7pyu";


        // this is where we will send it        
        readonly string uri = "https://notifications.recaptureit.se/send.ashx";
        readonly string uriDev = "https://dev.recaptureit.se/TXL/Notifications/send.ashx";

        public TXLPushNotification(string recieverId, String message, PushNotificationType type, int recordId, bool releaseMode, TermGroup_BrandingCompanies brandingCompany)
        {
            this.recieverId = recieverId;
            this.message = message;
            this.notificationType = type;
            this.recordId = recordId;
            this.releaseMode = releaseMode;
            this.brandingCompany = brandingCompany;
        }

        public override ActionResult Send(SoeMobileAppType appType)
        {            
            HttpWebResponse response = null;
            try
            {
                string post_data = CreatePostData(appType);

                // create a request
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(GetUri());
                request.KeepAlive = false;
                request.ProtocolVersion = HttpVersion.Version10;
                request.Method = "POST";

                // turn our request string into a byte stream
                byte[] postBytes = Encoding.UTF8.GetBytes(post_data);
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = postBytes.Length;
                Stream requestStream = request.GetRequestStream();

                // send it
                requestStream.Write(postBytes, 0, postBytes.Length);
                requestStream.Close();

                // grab te response and get result
                response = (HttpWebResponse)request.GetResponse();
                if (response != null)
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        if (stream != null)
                        {
                            using (StreamReader readStream = new StreamReader(stream, Encoding.UTF8))
                            {
                                if (readStream != null)
                                {
                                    String resultStr = readStream.ReadToEnd();
                                    if (resultStr.ToLower().Contains("succes"))
                                        return new ActionResult(true);
                                    else
                                    {
                                        return new ActionResult((int)ActionResultSave.PushNotificationSendFailed, resultStr);
                                    }
                                }
                                else
                                {
                                    return new ActionResult((int)ActionResultSave.PushNotificationReadFailed, "readStream is null");
                                }
                            }
                        }
                        else
                        {
                            return new ActionResult((int)ActionResultSave.PushNotificationReadFailed, "stream is null");
                        }
                    }
                }
                else
                {
                    return new ActionResult((int)ActionResultSave.PushNotificationReadFailed, "response is null");
                }
            }
            catch (Exception exp)
            {
                return new ActionResult(exp);
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                    response = null;
                }
            }
        }

        public override string GetUri()
        {
            return releaseMode ? uri : uriDev;
        }

        public override string GetParamStr()
        {
            return String.Format(paramStrGO, GetKey(), message, recieverId);
        }

        private string GetKey()
        {
            if (brandingCompany == TermGroup_BrandingCompanies.FlexiblaKontoret)
            {
                return releaseMode ? FK_key0 : FK_key0Dev;
            }
            else
            {
                Random random = new Random();
                int number = random.Next(0, 5);
                switch (number)
                {
                    case 0:
                        return releaseMode ? key0 : key0Dev;
                    case 1:
                        return releaseMode ? key1 : key1Dev;
                    case 2:
                        return releaseMode ? key2 : key2Dev;
                    case 3:
                        return releaseMode ? key3 : key3Dev;
                    case 4:
                        return releaseMode ? key4 : key4Dev;
                    default:
                        return releaseMode ? key0 : key0Dev;
                }
            }
            
        }

        private String CreatePostData(SoeMobileAppType appType)
        {
            String str = "";

            switch (appType)
            {
                case SoeMobileAppType.GO:
                    str = GetParamStr();
                    break;
            }

            switch (this.notificationType)
            {
                case PushNotificationType.XEMail:
                    str += "&data.xemail=" + this.recordId;
                    break;
                case PushNotificationType.Order:
                    str += "&data.order=" + this.recordId;
                    break;
                case PushNotificationType.CompInformation:
                    str += "&data.compinfo=" + this.recordId;
                    break;
                case PushNotificationType.SysInformation:
                    str += "&data.sysinfo=" + this.recordId;
                    break;
                default:
                    break;
            }

            return str;
        }

        public override ActionResult Validate()
        {
            ActionResult result = base.Validate();

            if (!result.Success)
                return result;

            //Specific TXL validation


            return result;
        }
    }
}
