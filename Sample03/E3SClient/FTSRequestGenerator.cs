using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sample03.E3SClient
{
	public class FtsRequestGenerator
	{
		private readonly UriTemplate _ftsSearchTemplate = new UriTemplate(@"data/searchFts?metaType={metaType}&query={query}&fields={fields}");
		private readonly Uri _baseAddress;

		public FtsRequestGenerator(string baseAddres) : this(new Uri(baseAddres))
		{
		}

		public FtsRequestGenerator(Uri baseAddress)
		{
			_baseAddress = baseAddress;
		}

		public Uri GenerateRequestUrl<T>(List<string> queryParts, int start = 0, int limit = 10)
		{
			return GenerateRequestUrl(typeof(T), queryParts, start, limit);
		}

		public Uri GenerateRequestUrl(Type type, List<string> queryParts, int start = 0, int limit = 10)
		{
			string metaTypeName = GetMetaTypeName(type);

			var ftsQueryRequest = new FTSQueryRequest
			{
				Statements = queryParts.Select(queryPart => new Statement { Query = queryPart }).ToList(),
				Start = start,
				Limit = limit
			};

			var ftsQueryRequestString = JsonConvert.SerializeObject(ftsQueryRequest);

			var uri = _ftsSearchTemplate.BindByName(_baseAddress,
				new Dictionary<string, string>()
				{
					{ "metaType", metaTypeName },
					{ "query", ftsQueryRequestString }
				});

			return uri;
		}

		private string GetMetaTypeName(Type type)
		{
			var attributes = type.GetCustomAttributes(typeof(E3SMetaTypeAttribute), false);

			if (attributes.Length == 0)
				throw new Exception(string.Format("Entity {0} do not have attribute E3SMetaType", type.FullName));

			return ((E3SMetaTypeAttribute)attributes[0]).Name;
		}

	}
}
