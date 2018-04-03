using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LuisEntityEnumGenerator
{
    class Program
    {
        public static string appId;
        public static string appKey;
        public static string appVersion;
        public static string directory;
        public static string namespaceName;

        public static void AskInformation(ref string value, string phrase)
        {
            Console.WriteLine(phrase);
            value = Console.ReadLine();
            if (value.ToUpper() == "EXIT")
            {
                throw new ArgumentException();
            }
            if (String.IsNullOrEmpty(value))
                AskInformation(ref value, phrase);
        }


        static void Main(string[] args)
        {
            AskInformation(ref appId, "Please provide your application id or EXIT:");
            AskInformation(ref appKey, "Please provide your application key or EXIT:");
            AskInformation(ref appVersion, "Please provide your app version or EXIT:");
            AskInformation(ref directory, "Please provide your directory for saving the files or EXIT:");
            AskInformation(ref namespaceName, "Please provide your desired namespace name or EXIT:");

            Dictionary<string, List<string>> listEntities = new Dictionary<string, List<string>>();

            var entityTypeList = SendGetRequest("closedLists").Result as JArray;

            foreach (var item in entityTypeList)
            {
                var key = item.SelectToken("name").Value<string>();
                var value = new List<string>();
                foreach (var subItem in item.SelectToken("subLists").Value<JArray>())
                {
                    value.Add(subItem.SelectToken("canonicalForm").Value<string>());
                }
                listEntities.Add(key, value);
            }

            foreach (var entity in listEntities)
            {
                var enumName = $"E{ToCamelCase(entity.Key)}";
                var sb = new StringBuilder();
                sb.AppendLine($"namespace {namespaceName}");
                sb.AppendLine("{");
                sb.AppendLine($"\tpublic enum {enumName}");
                sb.AppendLine("\t{");

                var index = 0;
                foreach (var value in entity.Value)
                {
                    if (index == entity.Value.Count - 1)
                    {
                        sb.AppendLine($"\t\t{ToCamelCase(value)} = \"{value}\"");
                    }
                    else
                    {
                        sb.AppendLine($"\t\t{ToCamelCase(value)} = \"{value}\",");
                    }
                    index++;
                }
                sb.AppendLine("\t}");
                sb.AppendLine("}");
                using (var writer = new StreamWriter($"{directory}/{enumName}.cs", false, Encoding.UTF8))
                {
                    writer.Write(sb.ToString());
                }
                Console.WriteLine(sb);
            }
        }

        private static async Task<dynamic> SendGetRequest(String path)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", appKey);
                httpClient.BaseAddress = new Uri($"https://westus.api.cognitive.microsoft.com/");
                var result = await httpClient.GetAsync($"luis/api/v2.0/apps/{appId}/versions/{appVersion}/{path}");
                var content = await result.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<dynamic>(content);
            }
        }

        private static string ToCamelCase(string value)
        {
            var replacedValue = value.ToLower().Replace("_", " ");
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(replacedValue).Replace(" ", string.Empty);
        }
    }
}
