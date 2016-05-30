using System;
using System.IO;
using Newtonsoft.Json;
using System.Web;

namespace OpenLawOffice.Web
{
    public static class Extensions
    {
        public static T JsonDeserialize<T>(this Stream stream)
        {
            JsonSerializer serializer = new JsonSerializer();
            using (StreamReader sr = new StreamReader(stream))
            {
                string str = sr.ReadToEnd();
                T obj = JsonConvert.DeserializeObject<T>(str);
                return obj;
                //using (JsonTextReader jtr = new JsonTextReader(sr))
                //{
                //    return serializer.Deserialize<T>(jtr);
                //}
            }
        }

        public static Guid? GetToken(this HttpRequestBase request)
        {
            Guid token;
            string authTokenHeader = request.Headers["OLO-AUTH-TOKEN"];

            if (string.IsNullOrEmpty(authTokenHeader))
                return null;
            
            if (!Guid.TryParse(authTokenHeader, out token))
                return null;

            return token;
        }
    }
}