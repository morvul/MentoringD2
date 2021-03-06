﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace IQueryable.E3SClient
{
    public class E3SQueryClient
    {
        private readonly string _userName;
        private readonly string _password;
        private readonly Uri _baseAddress = new Uri(" https://telescope.epam.com/rest/e3s-eco-scripting-impl/0.1.0");


        public E3SQueryClient(string user, string password)
        {
            _userName = user;
            _password = password;
        }

        public IEnumerable<T> SearchFts<T>(string query, int start = 0, int limit = 0) where T : E3SEntity
        {
            var queryParts = new List<string> { query };
            return SearchFts<T>(queryParts, start, limit);
        }

        public IEnumerable<T> SearchFts<T>(List<string> queryParts, int start = 0, int limit = 0) where T : E3SEntity
        {
            HttpClient client = CreateClient();
            var requestGenerator = new FtsRequestGenerator(_baseAddress);

            Uri request = requestGenerator.GenerateRequestUrl<T>(queryParts, start, limit);

            var resultString = client.GetStringAsync(request).Result;

            return JsonConvert.DeserializeObject<FTSResponse<T>>(resultString).items.Select(t => t.data);
        }

        public IEnumerable SearchFts(Type type, string query, int start = 0, int limit = 0)
        {
            var queryParts = new List<string> { query };
            return SearchFts(type, queryParts, start, limit);
        }


        public IEnumerable SearchFts(Type type, List<string> queryParts, int start = 0, int limit = 0)
        {
            var list = Activator.CreateInstance(typeof(List<>).MakeGenericType(type)) as IList;
            if (queryParts.Count == 0)
            {
                return list;
            }

            HttpClient client = CreateClient();
            var requestGenerator = new FtsRequestGenerator(_baseAddress);

            Uri request = requestGenerator.GenerateRequestUrl(type, queryParts, start, limit);

            var resultString = client.GetStringAsync(request).Result;
            var endType = typeof(FTSResponse<>).MakeGenericType(type);
            var result = JsonConvert.DeserializeObject(resultString, endType);
            foreach (object item in (IEnumerable)endType.GetProperty("items").GetValue(result))
            {
                list?.Add(item.GetType().GetProperty("data").GetValue(item));
            }

            return list;
        }


        private HttpClient CreateClient()
        {
            var client = new HttpClient(new HttpClientHandler
            {
                AllowAutoRedirect = true,
                PreAuthenticate = true
            });

            var encoding = new ASCIIEncoding();
            var authHeader = new AuthenticationHeaderValue("Basic",
              Convert.ToBase64String(encoding.GetBytes(string.Format("{0}:{1}", _userName, _password))));
            client.DefaultRequestHeaders.Authorization = authHeader;

            return client;
        }
    }
}
