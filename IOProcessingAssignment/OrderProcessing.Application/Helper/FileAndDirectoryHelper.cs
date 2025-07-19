using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessing.Application.Helper
{
    public static class FileAndDirectoryHelper
    {
        /// <summary>
        /// Checks and create following folders
        /// Parent Folder - Files
        /// Child Folders - Input, Output, Error, Archive, Log
        /// </summary>
        public static void CreateIODirectories(string rootDirectory)
        {
            if (string.IsNullOrEmpty(rootDirectory))
            {
                string currentDirectory = Directory.GetCurrentDirectory();
                rootDirectory = Path.GetPathRoot(currentDirectory) ?? "C:";
            }

            List<string> filesDirectories = new List<string> { "Input", "Output", "Error", "Archive", "Log" };
            var basePath = Path.Combine(rootDirectory, "ProcessedFiles");

            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);

                foreach (var file in filesDirectories)
                {
                    var filePath = Path.Combine(basePath, file);
                    if (!Directory.Exists(filePath))
                    {
                        Directory.CreateDirectory(filePath);
                        GivePermissionsToCreatedDirectories(filePath);
                    }
                }
            }
        }
        /// <summary>
        /// Enhanced permission to created folders
        /// </summary>
        /// <param name="folderPath"></param>
        public static void GivePermissionsToCreatedDirectories(string folderPath)
        {
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
                DirectorySecurity dirSecurity = dirInfo.GetAccessControl();

                // Add full control for the current user
                dirSecurity.AddAccessRule(new FileSystemAccessRule(
                    "Everyone", // Current user
                    FileSystemRights.FullControl,
                    InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                    PropagationFlags.None,
                    AccessControlType.Allow));

                dirInfo.SetAccessControl(dirSecurity);
            }
            catch (UnauthorizedAccessException ex)
            {
                //($"Access to the path '{folderPath}' is denied. Run as administrator.");
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Move the file from Input to Archive folder
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="inputFullFilePath"></param>
        public static void ProcessingSuccess(string basePath, string inputFullFilePath)
        {
            var archiveFilePath = Path.Combine(basePath, "Archive");
            string inputFileName = "InputTransactions_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
            var archiveFullFilePath = Path.Combine(archiveFilePath, inputFileName);
            File.Move(inputFullFilePath, archiveFullFilePath);
        }

        /// <summary>
        /// Move the file from Input to Error folder
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="inputFullFilePath"></param>
        public static void ProcessingFailure(string basePath, string inputFullFilePath)
        {
            var errorFilePath = Path.Combine(basePath, "Error");
            string inputFileName =  "InputTransactions_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
            var errorFullFilePath = Path.Combine(errorFilePath, inputFileName);
            File.Move(inputFullFilePath, errorFullFilePath);
        }
    }
}
