using fileManager.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<FileRequest> PostAsync([FromForm] FileModel model)
        {
            try
            {
                FileRecord file = await SaveFileAsync(model);

                if (!string.IsNullOrEmpty(file.Path))
                {
                    file.AltText = model.AltText;
                    file.Description = model.Description;
                    var fileResult = await SaveToDB(file);
                    return fileResult;
                }
                else
                    return  null;
            }
            catch 
            {
                return null;
            }
        }

        private async Task<FileRecord> SaveFileAsync(FileModel fileData)
        {
            FileRecord file = new FileRecord();
            var myFile = fileData.MyFile;
            if (myFile != null)
            {
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

                var fileName = DateTime.Now.Ticks.ToString() + Path.GetExtension(myFile.FileName);
                var path = Path.Combine(dbFilePath, fileName);

                file.Id = fileDB.Count() + 1;
                file.Path = path;
                file.Name = fileName;
                file.Format = Path.GetExtension(myFile.FileName);
                file.ContentType = myFile.ContentType;

                var fullPath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await myFile.CopyToAsync(stream);
                }

                return file;
            }
            return file;
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
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            var contentType = "APPLICATION/octet-stream";
            var fileName = Path.GetFileName(path);

            return File(memory, contentType, fileName);
        }
    }
}
