using System;
using System.IO;
using System.Text.RegularExpressions;
using Xcc.Core.Constants;

namespace Xcc.Core.Helpers
{
    public class FilePathValidationException : Exception
    {
        public FilePathValidationException()
        {
        }

        public FilePathValidationException(string? message) : base(message)
        {
        }
    }

    public static class FilePathValidation
    {
        /// <summary>
        /// Verifies filepath against path traversal attacks
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="validExtension"></param>
        /// <exception cref="FilePathValidationException"></exception>
        public static void CheckTraversalSecurity(string filepath, string validExtension = "")
        {
            Regex validNameRegex =
                string.IsNullOrEmpty(validExtension)
                ? new Regex(@"([a-zA-Z0-9s_\.-:])+$")
                : new Regex($"([a-zA-Z0-9s_\\.-:])+({validExtension})$");
            Match match = validNameRegex.Match(filepath);

            if (!match.Success)
            {
                throw new FilePathValidationException(StringConstants.Common.Validation.FilePathInvalidContentError);
            }

            if (!File.Exists(filepath))
            {
                throw new FilePathValidationException(StringConstants.Common.Validation.FilePathNotExistError);
            }

            Regex validLocationRegex = new Regex(@"^([a-zA-Z]:)");
            var locationMatch = validLocationRegex.Match(Path.GetFullPath(filepath));
            if (!locationMatch.Success)
            {
                throw new FilePathValidationException(StringConstants.Common.Validation.FilePathNotOnDiskError);
            }
        }
    }
}
