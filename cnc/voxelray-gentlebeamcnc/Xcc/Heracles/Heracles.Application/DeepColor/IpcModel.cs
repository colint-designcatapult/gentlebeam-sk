using Heracles.Application.DeepColor.DataTypes;
using Heracles.Application.Models.Treatment;

using System.Threading.Tasks;

namespace Heracles.Application.DeepColor;

public class IpcModel(IpcService ipcService, ITreatmentInfoStore treatmentInfoStore)
{
    public IpcService IpcService { get; } = ipcService;
    public ITreatmentInfoStore TreatmentInfoStore { get; } = treatmentInfoStore;

    public async Task TestConnectionAsync(int timeout)
    { 
        await IpcService.Test(timeout);
    }

    public async Task LoadAcquisition(int acquisitionId)
    {
        await IpcService.LoadAcquisition(acquisitionId);
    }


    public async Task PrepareAcquisition()
    {
        var status = await IpcService.GetStatus();
        var siteName = $"site-{TreatmentInfoStore.Diagnosis.Id}";
        var patientName = $"patient-{TreatmentInfoStore.Patient.Id}";

        if (status?.PatientName != patientName)
        {
            //this case should be executed if the status is null (no patient loaded) or patient IDs don't match
            await LoadOrCreatePatient(patientName);
            await LoadOrCreateSite(siteName);
        }
        else if(status.CurrentSite?.Name != siteName)
        {
            //this case should be executed if the status.CurrentSite is null (no site loaded) or site IDs don't match

            await LoadOrCreateSite(siteName);
        }
    }


    /// <summary>
    /// Attempts to load a patient with the specified <paramref name="name"/>.
    /// If the patient does not exist, creates a new patient with the given <paramref name="name"/>, then retries loading it.
    /// </summary>
    /// <param name="name">The name of the patient to load or create.</param>
    private async Task LoadOrCreatePatient(string name)
    {
        if (await IpcService.LoadPatient(name))
            return;

        await IpcService.CreatePatient(name);
        //TODO: save patient folder to the Moses
        await IpcService.LoadPatient(name);
    }


    /// <summary>
    /// Attempts to load a site with the specified <paramref name="name"/>.
    /// If the site does not exist, creates a new site with the given <paramref name="name"/>, then retries loading it.
    /// </summary>
    /// <param name="name">The name of the site to load or create.</param>
    private async Task LoadOrCreateSite(string name)
    {
        if (await IpcService.LoadSite(name))
            return;

        await IpcService.CreateSite($"site-{TreatmentInfoStore.Diagnosis.Id}");
        await IpcService.LoadSite(name);
    }


    public async Task<Acquisition[]> GetAcquisitionList()
    {
        return await IpcService.GetAcquisitionList();
    }
}