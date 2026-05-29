using System;
using System.IO;
using System.Xml.Serialization;

namespace Xcc.Application.Helpers
{
    public static class XmlSerializerExtensions
    {
        // To avoid System.IO.FileNotFoundException on XmlSerializer constructor call:
        //  
        // 1. Open the .csproj file of the project containing a serializable type manually.
        //
        // 2. Immediately after the <TargetFrameworkVersion>v?.?</TargetFrameworkVersion> element, add the following elements:
        // <SGenUseProxyTypes>false</SGenUseProxyTypes>
        // <SGenPlatformTarget>$(Platform)</SGenPlatformTarget>
        //
        // 3. In the .csproj file, for each platform configuration e.g. <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'Debug|x86'"> add the following line:
        // <GenerateSerializationAssemblies>On</GenerateSerializationAssemblies>
        //
        // 4. Save the .csproj file and reload project in Visual Studio.
        // 
        // Explanation https://stackoverflow.com/a/15538240/20514933

        public static void Save(this XmlSerializer serializer, string fileName, object obj)
        {
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                serializer.Serialize(writer, obj);
            }
        }

        public static object Load(this XmlSerializer serializer, string fileName)
        {
            using StreamReader reader = new StreamReader(fileName);

            return serializer.Deserialize(reader)
                ?? throw new Exception($"Failed to deserialize xml file {fileName}. Deserialization result is null.");
        }
    }
}
