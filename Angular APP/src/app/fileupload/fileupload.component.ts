import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { FileModel } from './FileModel';
import { FileResponce } from './FileResponce';

@Component({
  templateUrl: './fileupload.component.html'
})

export class FileuploadComponent {

  myFiles:string [] = [];

  baseUrl: string | undefined;
  progress!: number;
  public pageTitle = 'File uploader';
  attached: FileModel[];
  fileForm = new FormGroup({
    //altText: new FormControl(''),
    //description: new FormControl('')
  });
  fileToUpload: any;
  constructor(private http: HttpClient) {
    this.attached = [];
  }

  handleFileInput(e: any) {
    this.fileToUpload = e?.target?.files[0];
  }
  handleFilesInput(e: any) {
    this.myFiles = [];
    for (var i = 0; i < e.target.files.length; i++) { 
      this.myFiles.push(e.target.files[i]);
    }
  }

  downloadFile(id: number, contentType: string) {
    return this.http.get(`http://localhost:48608/FileManager/${id}`, { responseType: 'blob' })
      .subscribe((result: Blob) => {
        const blob = new Blob([result], { type: contentType }); // you can change the type
        const url = window.URL.createObjectURL(blob);
        window.open(url);
        console.log("Success");
      });
  }

  downloadFtpFile(id: number, contentType: string) {
    return this.http.get(`http://localhost:48608/getfile/${id}`, { responseType: 'blob' })
      .subscribe((result: Blob) => {
        const blob = new Blob([result], { type: contentType }); // you can change the type
        const url = window.URL.createObjectURL(blob);
        window.open(url);
        console.log("Success");
      });
  }

  removeFile(id: number) {
    const removeIndex = this.attached.findIndex(item => item.id === id);
    this.attached.splice(removeIndex, 1);
  }

  upload(files:any) {
    if (files.length === 0)
      return;
  
    const formData = new FormData();
  
    // for (const file of files) {
    //   formData.append(file.name, file);
    // }
    console.log('----------');
    console.log(files);

    formData.append('myFile', files);

    formData.append('moduleId', '1');
    return this.http.post('http://localhost:48608/FileManager', formData,
    {
      headers: new HttpHeaders()
    })
    .subscribe((r) => {
      console.log(r);
      let result = r as FileResponce;
      let f = new FileModel();
      f.id = result.id;
      f.docId = 1;
      f.path = result.path;
      f.contentType = result.mimeType;
      this.attached.push(f)
      alert("Saved")
    });
  
    // const uploadReq = new HttpRequest('POST', this.baseUrl + 'FileManagement/upload', formData, {
    //   reportProgress: true,
    // });
  
    // this.http.request(uploadReq).subscribe(event => {
    //   if (event.type === HttpEventType.UploadProgress) {
    //     this.progress = Math.round(100 * event.loaded / event.total);
    //   };
    // });
  }

  saveFileInfo() {
    debugger
    const formData: FormData = new FormData();
  //  formData.append('myFile', this.fileToUpload);
  for (var i = 0; i < this.myFiles.length; i++) { 
    formData.append("myFile", this.myFiles[i]);
  }
    //  formData.append('altText', this.fileForm.value.altText);
    // formData.append('description', this.fileForm.value.description);
    formData.append('moduleId', '1');
    return this.http.post('http://localhost:48608/FileManager', formData,
      {
        headers: new HttpHeaders()
      })
      .subscribe((r) => {
        console.log(r);
        let result = r as FileResponce[];
        this.attached= [];
        for(let  i = 0; i<result.length;i++)
        {
          let f = new FileModel();
          f.id = result[i].id;
          f.docId = 1;
          f.path = result[i].path;
          f.contentType = result[i].mimeType;
          this.attached.push(f)
        }       
      });
  }
}
