using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http.Json;

namespace DotNetBlog.WebAdmin.Service
{
    public class Api
    {
        private readonly HttpClient _httpClient;

        public Api(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(_httpClient.BaseAddress, "..");
            Console.WriteLine($"Base address set to: {_httpClient.BaseAddress}");
        }

        #region GET

        public async Task<string> GetAsync(string path, params (string key, string value)[] parameters)
        {
            var query = parameters?.Length > 0 ?
                parameters
                    .Where(w => w.value != null)
                    .Select(s => $"{WebUtility.UrlEncode(s.key)}={WebUtility.UrlEncode(s.value)}")
                    .Aggregate((o, n) => $"{o}&{n}") :
                null;

            if (query != null)
                path += $"{(path.Contains("?") ? "&" : "?")}{query}";

            return await _httpClient.GetStringAsync(path);
        }

        public async Task<TResult> GetAsync<TResult>(string path, params (string key, string value)[] parameters)
        {
            var query = parameters?.Length > 0 ?
               parameters
                   .Where(w => w.value != null)
                   .Select(s => $"{WebUtility.UrlEncode(s.key)}={WebUtility.UrlEncode(s.value)}")
                   .Aggregate((o, n) => $"{o}&{n}") :
               null;

            if (query != null)
                path += $"{(path.Contains("?") ? "&" : "?")}{query}";

            return await _httpClient.GetFromJsonAsync<TResult>(path, CancellationToken.None);
        }

        public Task<string> GetAsync<TParameter>(string path, TParameter parameters)
        {
            var type = typeof(TParameter);
            return GetAsync(path,
                parameters != null ?
                    type.GetProperties()
                        .Select(s => (s.Name, s.GetValue(parameters)?.ToString()))
                        .ToArray() : null);
        }

        public async Task<TResult> GetAsync<TParameter, TResult>(string path, TParameter parameters)
        {
            var type = typeof(TParameter);
            var paramItems = type.GetProperties()
                .Select(s => (s.Name, s.GetValue(parameters)?.ToString()))
                .ToArray();

            return await GetAsync<TResult>(path, paramItems);
        }

        #endregion

        #region POST

        public async Task<TResult> PostAsync<TResult>(
            string path,
            object content,
            params (string key, string value)[] parameters)
        {
            var query = parameters?.Length > 0 ?
                parameters
                    .Select(s => $"{WebUtility.UrlEncode(s.key)}={WebUtility.UrlEncode(s.value)}")
                    .Aggregate((o, n) => $"{o}&{n}") :
                null;

            if (query != null)
                path += $"{(path.Contains("?") ? "&" : "?")}{query}";

            var result = await _httpClient.PostAsJsonAsync(path, content);
            result.EnsureSuccessStatusCode();

            return await result.Content.ReadFromJsonAsync<TResult>();
        }

        public async Task<TResult> PostAsync<TResult>(
            string path,
            object content,
            object parameters)
        {
            var type = content?.GetType();
            return await PostAsync<TResult>(path,
                content,
                parameters != null ?
                    type.GetProperties()
                        .Select(s => (s.Name, s.GetValue(parameters)?.ToString()))
                        .ToArray() : null);
        }

        public async Task<string> PostAsync(
            string path,
            object content,
            params (string key, string value)[] parameters)
        {
            var query = parameters?.Length > 0 ?
                parameters
                    .Select(s => $"{WebUtility.UrlEncode(s.key)}={WebUtility.UrlEncode(s.value)}")
                    .Aggregate((o, n) => $"{o}&{n}") :
                null;

            if (query != null)
                path += $"{(path.Contains("?") ? "&" : "?")}{query}";

            var httpContent = new StringContent(
                JsonSerializer.Serialize(content),
                System.Text.Encoding.UTF8,
                "application/json");
            var result = await _httpClient.PostAsync(path, httpContent);
            var contentResult = result.EnsureSuccessStatusCode().Content;
            return await contentResult.ReadAsStringAsync();
        }

        public async Task<string> PostAsync(
            string path,
            object content,
            object parameters)
        {
            var type = content?.GetType();
            return await PostAsync(path,
                content,
                parameters != null ?
                    type.GetProperties()
                        .Select(s => (s.Name, s.GetValue(parameters)?.ToString()))
                        .ToArray() : null);
        }

        public async Task<TResult> PostAsync<TResult>(
            string path,
            HttpContent content,
            params (string key, string value)[] parameters)
        {
            var query = parameters?.Length > 0 ?
                parameters
                    .Select(s => $"{WebUtility.UrlEncode(s.key)}={WebUtility.UrlEncode(s.value)}")
                    .Aggregate((o, n) => $"{o}&{n}") :
                null;

            if (query != null)
                path += $"{(path.Contains("?") ? "&" : "?")}{query}";

            var result = await _httpClient.PostAsync(path, content);
            var contentResult = result.EnsureSuccessStatusCode().Content;
            using var stream = await contentResult.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<TResult>(stream);
        }


        #endregion
    }
}
