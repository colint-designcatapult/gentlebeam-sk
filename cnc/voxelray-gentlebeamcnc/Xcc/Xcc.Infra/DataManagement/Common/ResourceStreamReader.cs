using System;
using System.IO;
using System.Windows;

namespace Xcc.Infra.DataManagement.Common
{
    public static class ResourceStreamReader
    {
        public static byte[] LoadImageFromPackUri(Uri uri)
        {
            var streamInfo = Application.GetResourceStream(uri);

            if (streamInfo == null)
                throw new ArgumentException($"Resource not found: {uri}");

            using var stream = streamInfo.Stream;
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
