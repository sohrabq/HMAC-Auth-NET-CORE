using HMACAuthenticationWebAPI.Config;
using HMACAuthenticationWebAPI.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace HMACAuthenticationWebAPI.Filters
{
    public class HMACAuthAttribute : Attribute, IAuthorizationFilter
    {
        private static readonly Dictionary<string, string> dictionary = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> allowedApps = dictionary;
        private readonly ulong requestMaxAgeInSeconds = 300; // Means 5 min

        public WebHooks WebHooks { get; }

        public HMACAuthAttribute(WebHooks webHooks)
        {
            WebHooks = webHooks;
            if (allowedApps.Count == 0)
            {
                allowedApps.Add(webHooks.AppId, webHooks.SharedKey);
            }
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var req = context.HttpContext.Request;

            if (string.IsNullOrWhiteSpace(req.Headers["Authorization"]))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var authScheme = ((HMACAuthAttribute)((IFilterMetadata[])context.Filters)[4]).WebHooks.Scheme;

            var authHeader = AuthenticationHeaderValue.Parse(req?.Headers?["Authorization"]);

            if (!string.IsNullOrWhiteSpace(authHeader.Parameter) &&
                WebHooks.Scheme.Equals(authScheme, StringComparison.OrdinalIgnoreCase))
            {
                var autherizationHeaderArray = GetAutherizationHeaderValues(authHeader.Parameter);
                if (autherizationHeaderArray != null)
                {
                    var APPId = autherizationHeaderArray[0];
                    var incomingBase64Signature = autherizationHeaderArray[1];
                    var nonce = autherizationHeaderArray[2];
                    var requestTimeStamp = autherizationHeaderArray[3];
                    var isValid = IsValidRequest(req, APPId, incomingBase64Signature, nonce, requestTimeStamp);

                    if (!isValid)
                    {
                        context.Result = new UnauthorizedResult();
                    }
                }
                else
                {
                    context.Result = new UnauthorizedResult();
                }
            }
            else
            {
                context.Result = new UnauthorizedResult();
            }
        }
        private static string[] GetAutherizationHeaderValues(string rawAuthzHeader)
        {
            var credArray = rawAuthzHeader.Split(':');
            if (credArray.Length == 4)
            {
                return credArray;
            }
            else
            {
                return null;
            }
        }
        private bool IsValidRequest(HttpRequest req,
            string APPId, string incomingBase64Signature,
            string nonce, string requestTimeStamp)
        {
            string requestContentBase64String = "";
            string requestUri = GetFullPath(req);
            string requestHttpMethod = req.Method;
            if (!allowedApps.ContainsKey(APPId))
            {
                return false;
            }
            var sharedKey = allowedApps[APPId];

            if (IsReplayRequest(nonce, requestTimeStamp))
            {
                return false;
            }

            var _body = req.BodyToString();
            byte[] hash = ComputeHash(_body.Result);

            if (hash != null)
            {
                requestContentBase64String = Convert.ToBase64String(hash);
            }

            string data = string.Format("{0}{1}{2}{3}{4}{5}",
                APPId,
                requestHttpMethod,
                requestUri,
                requestTimeStamp,
                nonce,
                requestContentBase64String);

            var secretKeyBytes = Convert.FromBase64String(sharedKey);
            byte[] signature = Encoding.UTF8.GetBytes(data);
            using HMACSHA256 hmac = new HMACSHA256(secretKeyBytes);
            byte[] signatureBytes = hmac.ComputeHash(signature);
            var _localBase64Signature = Convert.ToBase64String(signatureBytes);
            return (incomingBase64Signature.Equals(_localBase64Signature, StringComparison.Ordinal));
        }
        private bool IsReplayRequest(string nonce, string requestTimeStamp)
        {
            if (System.Runtime.Caching.MemoryCache.Default.Contains(nonce))
            {
                return true;
            }
            DateTime epochStart = new(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan currentTs = DateTime.UtcNow - epochStart;
            var serverTotalSeconds = Convert.ToUInt64(currentTs.TotalSeconds);
            var requestTotalSeconds = Convert.ToUInt64(requestTimeStamp);
            if ((serverTotalSeconds - requestTotalSeconds) > requestMaxAgeInSeconds)
            {
                return true;
            }
            System.Runtime.Caching.MemoryCache.Default.Add(nonce, requestTimeStamp,
                DateTimeOffset.UtcNow.AddSeconds(requestMaxAgeInSeconds));
            return false;
        }
        private static byte[] ComputeHash(string content)
        {
            using MD5 md5 = MD5.Create();
            byte[] hash = null;
            var _content = Encoding.UTF8.GetBytes(content);
            if (content.Length != 0)
            {
                hash = md5.ComputeHash(_content);
            }
            return hash;
        }
        private static string GetFullPath(HttpRequest req)
        {
            return HttpUtility.UrlEncode(req.Scheme + "://" + req.Host.Value + req.Path.Value);
        }
    }
}
