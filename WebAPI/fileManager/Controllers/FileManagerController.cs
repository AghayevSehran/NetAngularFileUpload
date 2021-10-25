using fileManager.Models;
using fileManager.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace fileManager.Controllers
{
    [ApiController]
    [EnableCors("MyPolicy")]
    [Route("[controller]")]
    public class FileManagerController : ControllerBase
    {
        //configden oxumaq 
        private readonly string AppDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        //

        private static List<FileRecord> fileDB = new List<FileRecord>();
        OfficeDBContext dBContext = new OfficeDBContext();

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<List<FileRequest>> PostAsync([FromForm] FileModel model)
        {
            try
            {
                // List<FileRecord> files = await SaveFileAsync(model);
                List<FileRecord> files = await SaveFileFtpAsync(model);
                List<FileRequest> lFileRequets = new List<FileRequest>();
                foreach (var file in files)
                {
                    if (!string.IsNullOrEmpty(file.Path))
                    {
                        file.AltText = model.AltText;
                        file.Description = model.Description;
                        var fileResult = await SaveToDB(file);
                        lFileRequets.Add(fileResult);
                    }
                }

                return lFileRequets;
            }
            catch (Exception ex)
            {
                var t = ex.Message;
                return null;
            }
        }

        private async Task<List<FileRecord>> SaveFileAsync(FileModel fileData)
        {
            var files = new List<FileRecord>();
            var myFile = fileData.MyFile;
            if (myFile != null)
            {
                foreach (var item in myFile)
                {
                    var file = new FileRecord();
                    var module = fileData.ModuleId.ToString();
                    var now = DateTime.Now;
                    var yearName = now.ToString("yyyy");
                    var monthName = now.ToString("MM");
                    var dayName = now.ToString("dd");

                    var dbFilePath = Path.Combine(module, Path.Combine(yearName,
                                     Path.Combine(monthName,
                                       dayName)));
                    ;
                    var folder = Path.Combine(AppDirectory, dbFilePath);

                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    var fileName = DateTime.Now.Ticks.ToString() + Path.GetExtension(item.FileName);
                    var path = Path.Combine(dbFilePath, fileName);

                    file.Id = fileDB.Count() + 1;
                    file.Path = path;
                    file.Name = fileName;
                    file.Format = Path.GetExtension(item.FileName);
                    file.ContentType = item.ContentType;

                    var fullPath = Path.Combine(folder, fileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await item.CopyToAsync(stream);
                    }
                    files.Add(file);
                }

                return files;
            }
            return files;
        }
        private async Task<List<FileRecord>> SaveFileFtpAsync(FileModel fileData)
        {
            var files = new List<FileRecord>();
            var myFile = fileData.MyFile;
            if (myFile != null)
            {
                foreach (var item in myFile)
                {
                    var file = new FileRecord();
                    var module = fileData.ModuleId.ToString();
                    var now = DateTime.Now;
                    var yearName = now.ToString("yyyy");
                    var monthName = now.ToString("MM");
                    var dayName = now.ToString("dd");

                    var dbFilePath = Path.Combine(module, Path.Combine(yearName,
                                     Path.Combine(monthName,
                                       dayName)));

                    var fileName = DateTime.Now.Ticks.ToString() + Path.GetExtension(item.FileName);
                    var path = Path.Combine(dbFilePath, fileName);

                    file.Id = fileDB.Count() + 1;
                    file.Path = path;
                    file.Name = fileName;
                    file.Format = Path.GetExtension(item.FileName);
                    file.ContentType = item.ContentType;

                    using (MemoryStream stream = new MemoryStream())
                    {
                        await item.CopyToAsync(stream);
                        stream.Position = 0;
                        SFTPHelper.UploadFile(dbFilePath, stream, fileName);
                    }
                  
                    files.Add(file);
                }

                return files;
            }
            return files;
        }

        private async Task<FileRequest> SaveToDB(FileRecord record)
        {
            if (record == null)
                throw new ArgumentNullException($"{nameof(record)}");

            Models.FileRequest fileData = new Models.FileRequest();
            fileData.Path = record.Path;
            fileData.Name = record.Name;
            fileData.Extension = record.Format;
            fileData.MimeType = record.ContentType;

            dBContext.Files.Add(fileData);
            await dBContext.SaveChangesAsync();
            return fileData;
        }

        [HttpGet]
        public List<FileRecord> GetAllFiles()
        {

            return dBContext.Files.Select(n => new FileRecord
            {
                Id = n.Id,
                ContentType = n.MimeType,
                Format = n.Extension,
                Name = n.Name,
                Path = Path.Combine(AppDirectory, n.Path)
            }).ToList();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> DownloadFile(int id)
        {
            if (!Directory.Exists(AppDirectory))
                Directory.CreateDirectory(AppDirectory);
            var file = dBContext.Files.Where(n => n.Id == id).FirstOrDefault();
            var path = Path.Combine(AppDirectory, file?.Path);
            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            var contentType = "APPLICATION/octet-stream";
            var fileName = Path.GetFileName(path);
            return File(memory, contentType, fileName);
        }

        [HttpGet]
        [Route("/getfile/{id}")]
        public async Task<IActionResult> DownloadFtpFile(int id)
        {      
            var file = dBContext.Files.Where(n => n.Id == id).FirstOrDefault();
            var memory = SFTPHelper.Download(file.Path);            
            memory.Position = 0;
            var contentType = "APPLICATION/octet-stream";
            var fileName = Path.GetFileName(file.Path);
            return File(memory, contentType, fileName);
        }
    }
}
