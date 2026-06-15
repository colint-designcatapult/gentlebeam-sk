README – KukaRdpHost Integration Guide

Project Structure

Main Projects

* KukaRdpHost – Remote Desktop thin client wrapper (WinForms in WPF)
* Morpheus.Main – WPF application that hosts the final RDP executable and required interop DLLs

Build Instructions

1. Open the solution in Visual Studio.
2. Set KukaRdpHost as the startup project and build it once in Release or Debug mode.
3. Ensure the output folder contains the following files:

   * KukaRdpHost.exe
   * AxMSTSCLib.dll
   * MSTSCLib.dll
   * KukaRdpHost.exe.config (if applicable)
4. Copy these files manually into:

   * Morpheus.Main/Resources/RdpClient/

You can automate this step via a post-build script if desired.

Final Integration

After copying, rebuild the entire solution:

1. Right-click Morpheus.Main → Rebuild
2. Alternatively, rebuild the whole solution if dependencies are not resolving
3. When running Morpheus.Main, the app should spawn KukaRdpHost.exe with a dynamic IP

Runtime Notes

* The helper method LaunchSmartpadRdp(string ipAddress) constructs the relative path to KukaRdpHost.exe within the application directory.
* The executable uses embedded logic to animate a loader while the RDP session connects.

Troubleshooting

* If you see "file not found" errors during build:

  * Ensure the filenames match exactly and are marked as Content with CopyToOutputDirectory set to PreserveNewest.

* If AxMSTSCLib.dll or MSTSCLib.dll is not created:

  * Re-add the COM reference: Microsoft Terminal Services Active Client (MsTscAx.dll)
  * Clean, then rebuild KukaRdpHost

Cleanup

* You may omit KukaRdpHost.pdb from the final bundle
* Keep KukaRdpHost.exe.config if the executable needs config settings at runtime

You're all set. The loader will animate while waiting for a smartpad to connect.
