using System;
using System.Collections.Generic;
using System.Dynamic;
using PusherServer;
using RestSharp;
using RestSharp.Serializers;

namespace pusher_notification_dotnet
{
    public class PusherNotification
    {
        public static IRestResponse Notify(string data, string interest)
        {
            var PusehrAppId = "app_id";
            var PusehrAppKey = "app_key";
            var PusehrAppSecret = "app_secret";

            int timeNow = (int)((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds);
            var auth_version = "1.0";

            SortedDictionary<string, string> queryParams = new SortedDictionary<string, string>();
            queryParams.Add("auth_key", PusehrAppKey);
            queryParams.Add("auth_timestamp", timeNow.ToString());
            queryParams.Add("auth_version", "1.0");

            dynamic apnsPayload = new ExpandoObject();
            apnsPayload.aps = new ExpandoObject();
            apnsPayload.aps.alert = new
            {
                body = data
            };
            string gcmPayload = "";
            string fcmPayload = "";

            var body = new
            {
                interests = new string[] { interest },
/*                webhook_url = "",
                webhook_level = "",*/
                apns = apnsPayload,
                /*gcm = gcmPayload,
                fcm = fcmPayload,*/
            };
            JsonSerializer serializer = new JsonSerializer();
            var serializedBody = serializer.Serialize( body);

            var body_md5 = CryptoHelper.GetMd5Hash(serializedBody);
            queryParams.Add("body_md5", body_md5);

            string queryString = string.Empty;
            foreach (KeyValuePair<string, string> parameter in queryParams)
            {
                queryString += parameter.Key + "=" + parameter.Value + "&";
            }
            queryString = queryString.TrimEnd('&');

            var resource = "notifications";
            resource = resource.TrimStart('/');
            string path = string.Format("/server_api/v1/apps/{0}/{1}", PusehrAppId, resource);

            string authToSign = String.Format(
                "POST" +
                "\n{0}\n{1}",
                path,
                queryString);

            var authSignature = CryptoHelper.GetHmac256(PusehrAppSecret, authToSign);

            var basePath = "https://nativepush-cluster1.pusher.com";
            var requestUrl = path + "?" + queryString + "&auth_signature=" + authSignature;
            var request = new RestRequest(requestUrl);
            request.RequestFormat = DataFormat.Json;
            request.Method = RestSharp.Method.POST;
            request.AddBody(body);

            var client = new RestClient(basePath);
            IRestResponse response = client.Execute(request);
            return response;
        }
    }
}
