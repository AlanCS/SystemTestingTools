using System;
using System.IO;

namespace SystemTestingTools
{
    /// <summary>
    /// Existing folder absolute path
    /// </summary>
    public class FolderAbsolutePath : StringValueObject
    {
        internal IFileSystemFacade _fileSystem = new FileSystemFacade();

        /// <summary>
        /// Existing folder absolute path
        /// </summary>
        public FolderAbsolutePath(string value) : base(value)
        {
            if (!_fileSystem.FolderExists(_value)) throw new ArgumentException($"Folder does not exist: {_value}");
            TestPermissions();
        }

        /// <summary>
        /// We test for the right permissions from the start, to check before we need if something could go wrong later
        /// </summary>
        private void TestPermissions()
        {
            var testFolder = Path.Combine(_value, "FolderTestPermissionSystemTestingTools");

            try
            {
                _fileSystem.CreateFolder(testFolder);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Could not create folders inside '{_value}'", e);
            }

            try
            {
                _fileSystem.CreateFile(Path.Combine(testFolder, "test.txt"), "testing to see if we have the needed permissions for stub saving");
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Could not create a file inside '{testFolder}'", e);
            }
            finally
            {
                _fileSystem.DeleteFolder(testFolder);
            }
        }

        /// <summary>
        /// convert from string
        /// </summary>
        public static implicit operator FolderAbsolutePath(string value)
        {
            if (value == null) return null;

            return new FolderAbsolutePath(value);
        }

        /// <summary>
        /// convert to string
        /// </summary>
        public static implicit operator string(FolderAbsolutePath obj) => obj._value;

        public string AppendPath(FolderRelativePath path2)
        {
            if (path2 == null) return _value;
            return Path.Combine(_value, path2);
        }
    }
}
