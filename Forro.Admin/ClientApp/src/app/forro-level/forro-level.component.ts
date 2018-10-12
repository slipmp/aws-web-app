import { Component, Inject } from '@angular/core'
import { HttpClient } from '@angular/common/http';
import { ForroLevel } from './ForroLevel';

@Component({
  selector: 'app-forro-level',
  templateUrl: './forro-level.component.html',
  styleUrls: ['./forro-level.component.css']
})

export class ForroLevelComponent {
  public forroLevels: ForroLevel[];
  public addingNew = false;
  public forroLevelIdInvalid = false;
  public forroLevelModel: ForroLevel;
  public selectedFile: File;

  private apiUrl: string;

  private error: string;

  constructor(private http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.apiUrl = baseUrl + 'api/ForroLevel';
    this.forroLevelModel = new ForroLevel(1, "", "");
    this.forroLevels = new Array<ForroLevel>();
    this.updateGrid();
  }

  handleError(error) {
    console.error(error);
  }

  updateGrid() {
    this.http.get<ForroLevel[]>(this.apiUrl)
      .subscribe(result => {
        this.forroLevels = result;
      }, error => this.handleError(error));
  }

  insert() {
    if (this.forroLevelModel.forroLevelId <= 0 || this.forroLevelModel.forroLevelId > 10) {
      this.forroLevelIdInvalid = true;
    }
    else
    {
      const uploadData = new FormData();

      if (this.selectedFile != null)
        uploadData.append('myFile', this.selectedFile, this.selectedFile.name);

      var stringfy = JSON.stringify(this.forroLevelModel);
      uploadData.append('forroLevelModel', stringfy);

      this.http.post(this.apiUrl, uploadData).subscribe(result => {
        console.log('Result from Http Post: ' + result);
        this.updateGrid();
        this.addingNew = false;
        this.forroLevelModel = new ForroLevel(1, "","");
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
    if (this.forroLevels.length > 0) {
      var newId = this.forroLevels[this.forroLevels.length - 1].forroLevelId + 1;
      this.forroLevelModel.forroLevelId = newId;
    }
  }

  onFileChanged(event) {
    this.selectedFile = event.target.files[0]
  }
}
