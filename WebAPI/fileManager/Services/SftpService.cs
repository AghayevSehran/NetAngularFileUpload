using Microsoft.Extensions.Logging;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace fileManager.Services
{
    public static class SFTPHelper
    {
        public static SFTPUser SFTPUser { get; set; }
        static SFTPHelper()
        {
            SFTPUser = new SFTPUser()
            {
                UserName = "one",
                Password = "WWWeeerrr123",
                Host = "26.197.183.236",
                Port = 22
            };
        }
   
        public static void UploadFile(string workingdirectory, MemoryStream stream, string fileName)
        {

            try
            {
            
                using (var client = new SftpClient(SFTPUser.Host, SFTPUser.Port, SFTPUser.UserName, SFTPUser.Password))
                {
                   
                    client.Connect();
                    string path = string.Concat("\\", workingdirectory);
                    client.CreateDirectoryRecursively(path);
                    client.BufferSize = 4 * 1024;
                    client.UploadFile(stream, string.Concat(path, "\\", fileName), null);
                    client.Disconnect();
                    stream.Close();
                    stream.Dispose();
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        public static void UploadFile(string workingdirectory, FileStream fs, string fileName)
        {

            try
            {
                using (var client = new SftpClient(SFTPUser.Host, SFTPUser.Port, SFTPUser.UserName, SFTPUser.Password))
                {
                    client.Connect();
                    string path = string.Concat('/', '/' + workingdirectory);
                    client.CreateDirectoryRecursively(path);
                    client.BufferSize = 4 * 1024;
                    client.UploadFile(fs, string.Concat(path, '/', fileName), null);
                    client.Disconnect();

                    fs.Close();
                    fs.Dispose();
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public static void DeleteFile(string fileFullName)
        {

            try
            {

                
                using (var client = new SftpClient(SFTPUser.Host, SFTPUser.Port, SFTPUser.UserName, SFTPUser.Password))
                {
                   
                    client.Connect();
                    client.DeleteFile(fileFullName);
                    client.Disconnect();

                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        public static void CreateDirectoryRecursively(this SftpClient client, string path)
        {
            string current = "";

            if (path[0] == '\\')
            {
                path = path.Substring(1);
            }

            while (!string.IsNullOrEmpty(path))
            {
                int p = path.IndexOf('\\');
                current += '\\';
                if (p >= 0)
                {
                    current += path.Substring(0, p);
                    path = path.Substring(p + 1);
                }
                else
                {
                    current += path;
                    path = "";
                }

                try
                {
                    SftpFileAttributes attrs = client.GetAttributes(current);
                    if (!attrs.IsDirectory)
                    {
                        throw new Exception("not directory");
                    }
                }
                catch (SftpPathNotFoundException)
                {
                    client.CreateDirectory(current);
                }
            }
        }



        public static bool Download(string pathRemoteFile, string pathLocalFile)
        {
            try
            {
                using (var client = new SftpClient(SFTPUser.Host, SFTPUser.Port, SFTPUser.UserName, SFTPUser.Password))
                {

                    client.Connect();

                    using (Stream fileStream = File.OpenWrite(pathLocalFile))
                    {
                        client.DownloadFile(pathRemoteFile, fileStream);
                    }

                    client.Disconnect();
                    return true;
                }
            }
            catch (Exception ex)
            {

                return false;
            }
        }


        public static MemoryStream Download(string pathRemoteFile)
        {
            try
            {
                using (var client = new SftpClient(SFTPUser.Host, SFTPUser.Port, SFTPUser.UserName, SFTPUser.Password))
                {

                    client.Connect();
                    var fileStream = new MemoryStream();
                    client.DownloadFile(pathRemoteFile, fileStream,null);
                    return fileStream;
                    client.Disconnect();
                }
            }
            catch (Exception ex)
            {

                return null;
            }
        }
    }

}
