using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SystemTestingTools
{
    /// <summary>
    /// Helper methods for environment setup
    /// </summary>
    public static class EnvironmentHelper
    {
        private static Regex folderCleanerRegex = new Regex(@"(\\bin\\.*)|(\/bin\/.*)", RegexOptions.Compiled);

        /// <summary>
        /// Gets the path where the project is setup, and appends the extra folder name, useful to find your stubs
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns></returns>
        public static string GetProjectFolder(string folderName)
        {
            var projectFolder = folderCleanerRegex.Replace(System.Environment.CurrentDirectory, "");
            var finalFolder = System.IO.Path.Combine(projectFolder, folderName);
            if (!Directory.Exists(finalFolder)) throw new ArgumentException($"Folder doesn't exist: {finalFolder}");
            return finalFolder;
        }
    }
}
