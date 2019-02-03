import { Component, Inject } from '@angular/core'
import { HttpClient } from '@angular/common/http';
import { ForroLevel } from './ForroLevel';
import { ForroLevelModel } from './ForroLevelModel';

@Component({
  selector: 'app-forro-level',
  templateUrl: './forro-level.component.html',
  styleUrls: ['./forro-level.component.css']
})

export class ForroLevelComponent {
  public addingNew = false;
  public forroLevelIdInvalid = false;
  public forroLevelModel: ForroLevelModel;
  public selectedFile: File;

  private apiUrl: string;

  private error: string;

  constructor(private http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.apiUrl = baseUrl + 'api/ForroLevel';
    this.assignNewForroLevelModel();
    this.updateGrid();
  }

  assignNewForroLevelModel() {
    this.forroLevelModel = new ForroLevelModel(new ForroLevel(0, "", ""), new Array(), "");
    this.selectedFile = null;
  }

  handleError(error) {
    console.log("There was an error calling BackEnd API - handleError(): ");
    console.error(error);
    this.forroLevelModel.errorMessage = error.error.errorMessage;
  }

  updateGrid() {
    this.http.get<ForroLevelModel>(this.apiUrl)
      .subscribe(result => {

        console.log("Result from BackEnd GetAll: ");
        console.log(result);

        this.forroLevelModel.forroLevelList = result.forroLevelList;
      }, error => this.handleError(error));
  }

  insert() {
    if (this.forroLevelModel.forroLevel.forroLevelId <= 0 || this.forroLevelModel.forroLevel.forroLevelId > 10) {
      this.forroLevelIdInvalid = true;
    }
    else
    {
      const uploadData = new FormData();

      if (this.selectedFile != null)
        uploadData.append('myFile', this.selectedFile, this.selectedFile.name);

      var stringfy = JSON.stringify(this.forroLevelModel.forroLevel);
      uploadData.append('forroLevel', stringfy);

      this.http.post(this.apiUrl, uploadData).subscribe(result => {
        console.log('Result from Http Post: ' + result);
        this.updateGrid();
        this.addingNew = false;
        this.assignNewForroLevelModel();
      }, error => this.handleError(error));
    }
  }

  deleteSelectedForroLevel(id:number) {
    this.http.delete(this.apiUrl + '/' + id).subscribe(result => {
      //Modal must close once operation is finished
      this.updateGrid();
    }, error => this.handleError(error));
  }

  addingNewMethod() {
    this.addingNew = true;

    //Next available ID for Forró Level
    if (this.forroLevelModel.forroLevelList.length > 0) {
      var newId = this.forroLevelModel.forroLevelList[this.forroLevelModel.forroLevelList.length - 1].forroLevelId + 1;
      this.forroLevelModel.forroLevel.forroLevelId = newId;
    }
  }

  onFileChanged(event) {
    this.selectedFile = event.target.files[0]
  }
}
