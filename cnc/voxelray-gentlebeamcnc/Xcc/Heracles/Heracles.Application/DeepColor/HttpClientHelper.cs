using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Heracles.Application.DeepColor;

public class HttpClientHelper(string baseAddress)
{
    public async Task<HttpResponseMessage> GetAsync(string url, int timeout = 360)
    {
        try
        {
            return await NewHttpClient(timeout).GetAsync(url);
        }
        catch (Exception e)
        {
            throw new Exception($"Failed to get {url}: {e.Message}", e);
        }
    }

    public async Task<HttpResponseMessage> PostAsync(string url, HttpContent? content, int timeout = 360)
    {
        try
        {
            return await NewHttpClient(timeout).PostAsync(url, content);
        }
        catch (Exception e)
        {
            throw new Exception($"Failed to post {url}: {e.Message}", e);
        }
    }

    public async Task<HttpResponseMessage> PostAsJsonAsync<T>(string url, T? content, int timeout = 360)
    {
        try
        {
            var json = JsonSerializer.Serialize(content);
            return await NewHttpClient(timeout).PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
        }
        catch (Exception e)
        {
            throw new Exception($"Failed to post as json {url}: {e.Message}", e);
        }
    }

    public async Task<HttpResponseMessage> PutAsync(string url, HttpContent? content, int timeout = 360)
    {
        try
        {
            return await NewHttpClient(timeout).PutAsync(url, content);
        }
        catch (Exception e)
        {
            throw new Exception($"Failed to put {url}: {e.Message}", e);
        }
    }

    public async Task<HttpResponseMessage> DeleteAsync(string url, int timeout = 360)
    {
        try
        {
            return await NewHttpClient(timeout).DeleteAsync(url);
        }
        catch (Exception e)
        {
            throw new Exception($"Failed to delete {url}: {e.Message}", e);
        }
    }

    private HttpClient NewHttpClient(int timeout)
    {
        var client = new HttpClient { Timeout = TimeSpan.FromSeconds(timeout) };
        client.BaseAddress = new Uri(baseAddress);
        return client;
    }
}