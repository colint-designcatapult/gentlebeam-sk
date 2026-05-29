namespace Xcc.Core.Models;

public interface IUpsSettings
{
    bool UpsIsRemote { get; }
    int UpsHidVendorId { get; }
    int UpsHidProductId { get; }
    string PrimaryUpsModel { get; }
}