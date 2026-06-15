namespace Xcc.Core.Models;

public interface IDebugSettings
{
    bool UseDummyDatabase { get; }
    bool UseDummyServices { get; }
    bool UseDummyRobot { get; }
    bool UseDummyAlignmentEngine { get; }
    /// <summary>
    /// Specifies that application will show controls intended for debugging.
    /// </summary>
    bool ShowDebugButtons { get; }
    string? DummyDeviceSerial { get; }
    bool DebugPopulateEmptyDBWithDummyData { get; }
    string? DebugAuthUsername { get; }
    string? DebugAuthPassword { get; }
    string? DummyCollimatorSerial { get; }
    long DebugLoadedPlanId { get; }
    long DebugLoadedImagingPlanId { get; }
    /// <summary>
    /// Is specified, application will try to use this path to show image in the dcm-viewer.
    /// </summary>
    string? PathToDummyImage { get; }
    /// <summary>
    /// Is specified, application will try to use this path to april tags screenshot instead of detector camera image.
    /// </summary>
    string? PathToTagScreenshot { get; }
    /// <summary>
    /// Specifies that the window will not be maximized to full screen.
    /// </summary>
    bool DoNotExpandFullscreen { get; }
    bool IsUpsActivated { get; }
}