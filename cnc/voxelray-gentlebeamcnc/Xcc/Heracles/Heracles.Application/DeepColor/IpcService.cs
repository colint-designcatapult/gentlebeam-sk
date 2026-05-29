using Heracles.Application.DeepColor.DataTypes;
using Heracles.Core.Models;

using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Heracles.Application.DeepColor;

public class IpcService(IImagingSettings imagingSettings)
{
    public IImagingSettings ImagingSettings { get; } = imagingSettings;
    private HttpClientHelper HttpClientHelper { get; } = new($"http://{imagingSettings.ImagingEndpoint.Address()}");


    #region '/test' method
    public async Task Test(int timeout)
    {
        var result = await HttpClientHelper.GetAsync("/apiV1/test", timeout);

        if (result.IsSuccessStatusCode)
            return;
            
        throw new Exception($"Failed to connect imaging server. Error: {result.StatusCode}");
    }
    #endregion '/test' methods



    #region '/status' method
    public async Task<Status?> GetStatus()
    {
        var result = await HttpClientHelper.GetAsync("/apiV1/status", ImagingSettings.HttpRequestTimeout);

        if (result.IsSuccessStatusCode)
        {
            var jsonResponse = await result.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<Status>(jsonResponse, JsonSerializerOptions.Default) 
                   ?? throw new Exception($"Failed to deserialize {nameof(Status)}");
        }

        return result.StatusCode switch
        {
            HttpStatusCode.NotFound => null,
            _ => throw new Exception($"Failed to get status. Error: {result.StatusCode}")
        };
    }
    #endregion '/status' methods



    #region '/version' method
    public async Task<VersionInfo> Version(int timeout)
    {
        string errorMessage = "Failed to get version";

        var result = await HttpClientHelper.GetAsync("/apiV1/version", timeout);

        if (result.IsSuccessStatusCode)
        {
            var jsonResponse = await result.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<VersionInfo>(jsonResponse, JsonSerializerOptions.Default)
                  ?? throw new Exception($"{errorMessage}. Failed to deserialize {nameof(VersionInfo)}");
        }

        throw new Exception($"Failed to get version. Error: {result.StatusCode}");
    }
    #endregion '/version' method



    #region '/site' methods
    public async Task CreateSite(string siteName)
    {
        string errorMessage = $"Failed to create site Name='{siteName}'";

        var result = await HttpClientHelper.PostAsJsonAsync("/apiV1/site/create", new { siteName }, ImagingSettings.HttpRequestTimeout);

        if (result.StatusCode == HttpStatusCode.Created)
            return;

        throw result.StatusCode switch
        {
            HttpStatusCode.BadRequest => new Exception($"{errorMessage}. Invalid input"),
            HttpStatusCode.Conflict => new Exception($"{errorMessage}. Site/session with given name already exists"),
            _ => new Exception($"{errorMessage}. Error: {result.StatusCode}")
        };
    }

    public async Task<Site[]> GetSiteList()
    {
        string errorMessage = "Failed to get site list";

        var result = await HttpClientHelper.GetAsync("/apiV1/site/list", ImagingSettings.HttpRequestTimeout);

        if (result.IsSuccessStatusCode)
        {
            var jsonResponse = await result.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<Site[]>(jsonResponse, JsonSerializerOptions.Default)
                   ?? throw new Exception($"{errorMessage}. Failed to deserialize {nameof(Site)} array");
        }

        throw new Exception($"{errorMessage}. Error: {result.StatusCode}");
    }

    public async Task<bool> LoadSite(string siteName)
    {
        var result = await HttpClientHelper.GetAsync($"/apiV1/site/load?siteName={siteName}", ImagingSettings.HttpRequestTimeout);

        return result.StatusCode switch
        {
            HttpStatusCode.OK => true,
            HttpStatusCode.NotFound => false,
            _ => throw new Exception($"Failed to load site with Name='{siteName}'. Error: {result.StatusCode}")
        };
    }


    public async Task DeleteSite(string siteName)
    {
        string errorMessage = $"Failed to delete site with Name='{siteName}'";

        var result = await HttpClientHelper.PostAsJsonAsync("/apiV1/site/delete", new { siteName }, ImagingSettings.HttpRequestTimeout);

        if (result.IsSuccessStatusCode)
            return;

        throw result.StatusCode switch
        {
            HttpStatusCode.NotFound => new Exception($"{errorMessage}. Site/session not found"),
            HttpStatusCode.BadRequest => new Exception($"{errorMessage}. Invalid input"),
            HttpStatusCode.InternalServerError => new Exception($"{errorMessage}. Failed to delete current site/session"),
            _ => new Exception($"{errorMessage}. Error: {result.StatusCode}")
        };
    }
    #endregion '/site' methods



    #region '/patient' methods
    public async Task CreatePatient(string patientName)
    {
        string errorMessage = $"Failed to create patient with Name='{patientName}'";

        var result = await HttpClientHelper.PostAsJsonAsync("/apiV1/patient/create", new { patientName }, ImagingSettings.HttpRequestTimeout);

        if (result.StatusCode == HttpStatusCode.Created)
            return;

        throw result.StatusCode switch
        {
            HttpStatusCode.BadRequest => new Exception($"{errorMessage}. Invalid input"),
            HttpStatusCode.Conflict => new Exception($"{errorMessage}. Error while creating the folder"),
            _ => new Exception($"{errorMessage}. Error: {result.StatusCode}")
        };
    }

    public async Task<Patient[]> GetPatientList()
    {
        string errorMessage = "Failed to get patient list";

        var result = await HttpClientHelper.GetAsync("/apiV1/patient/list", ImagingSettings.HttpRequestTimeout);

        if (result.IsSuccessStatusCode)
        {
            var jsonResponse = await result.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<Patient[]>(jsonResponse, JsonSerializerOptions.Default) 
                   ?? throw new Exception($"Failed to deserialize {nameof(Patient)} array");
        }

        throw new Exception($"{errorMessage}. Error: {result.StatusCode}");
    }

    public async Task<bool> LoadPatient(string patientName)
    {
        string errorMessage = $"Failed to load patient with Name='{patientName}'";

        var result = await HttpClientHelper.GetAsync($"/apiV1/patient/load?patientName={patientName}", ImagingSettings.HttpRequestTimeout);

        return result.StatusCode switch
        {
            HttpStatusCode.OK => true,
            HttpStatusCode.NotFound => false,
            HttpStatusCode.BadRequest => throw new Exception($"{errorMessage}. Invalid input"),
            _ => throw new Exception($"{errorMessage}. Error: {result.StatusCode}")
        };
    }

    public async Task DeletePatient(string patientName)
    {
        string errorMessage = $"Failed to delete patient with Name='{patientName}'";

        var result = await HttpClientHelper.PostAsJsonAsync("/apiV1/patient/delete", new { patientName }, ImagingSettings.HttpRequestTimeout);

        if (result.IsSuccessStatusCode)
            return;

        throw result.StatusCode switch
        {
            HttpStatusCode.NotFound => new Exception($"{errorMessage}. Patient folder not found"),
            HttpStatusCode.BadRequest => new Exception($"{errorMessage}. Invalid input"),
            HttpStatusCode.Conflict => new Exception($"{errorMessage}. Cannot delete current patient folder"),
            _ => new Exception($"{errorMessage}. Error: {result.StatusCode}")
        };
    }
    #endregion '/patient' methods



    #region '/acquisition' methods
    public async Task<Acquisition[]> GetAcquisitionList()
    {
        string errorMessage = "Failed to get acquisition list";

        var result = await HttpClientHelper.GetAsync("/apiV1/acquisition/list", ImagingSettings.HttpRequestTimeout);

        if (result.IsSuccessStatusCode)
        {
            var jsonResponse = await result.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<Acquisition[]>(jsonResponse, JsonSerializerOptions.Default)
                   ?? throw new Exception($"{errorMessage}. Failed to deserialize {nameof(Acquisition)} array");
        }

        throw result.StatusCode switch
        {
            HttpStatusCode.Forbidden => new Exception($"{errorMessage}. No site loaded"),
            _ => new Exception($"{errorMessage}. Error: {result.StatusCode}")
        };
    }

    public async Task<bool> LoadAcquisition(int acquisitionId)
    {
        string errorMessage = $"Failed to load acquisition with Id='{acquisitionId}'";

        var result = await HttpClientHelper.GetAsync($"/apiV1/acquisition/load?acqId={acquisitionId}", ImagingSettings.HttpRequestTimeout);

        return result.StatusCode switch
        {
            HttpStatusCode.OK => true,
            HttpStatusCode.NotFound => false,
            HttpStatusCode.Forbidden => throw new Exception($"{errorMessage}. No site loaded"),
            _ => throw new Exception($"{errorMessage}. Error: {result.StatusCode}")
        };
    }

    public async Task DeleteAcquisition(int acquisitionId)
    {
        string errorMessage = $"Failed to delete acquisition with Id='{acquisitionId}'";

        var json = new { acqId = acquisitionId };
        var result = await HttpClientHelper.PostAsJsonAsync("/apiV1/acquisition/delete", json, ImagingSettings.HttpRequestTimeout);

        if (result.IsSuccessStatusCode)
            return;

        throw result.StatusCode switch
        {
            HttpStatusCode.NotFound => new Exception($"{errorMessage}. Acquisition not found"),
            HttpStatusCode.BadRequest => new Exception($"{errorMessage}. Invalid input"),
            _ => new Exception($"{errorMessage}. Error: {result.StatusCode}")
        };
    }
    #endregion '/acquisition' methods



    #region '/measurement' methods
    public async Task<Measurement[]> GetMeasurementsList()
    {
        string errorMessage = "Failed to get measurements list";

        var result = await HttpClientHelper.GetAsync("/apiV1/measurements/list", ImagingSettings.HttpRequestTimeout);

        if (result.IsSuccessStatusCode)
        {
            var jsonResponse = await result.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<Measurement[]>(jsonResponse, JsonSerializerOptions.Default)
                   ?? throw new Exception($"{errorMessage}. Failed to deserialize {nameof(Measurement)} array");
        }

        throw result.StatusCode switch
        {
            HttpStatusCode.NotFound => new Exception($"{errorMessage}. No session or acquisition found"),
            _ => new Exception($"{errorMessage}. Error: {result.StatusCode}")
        };
    }
    #endregion '/acquisition' methods
}