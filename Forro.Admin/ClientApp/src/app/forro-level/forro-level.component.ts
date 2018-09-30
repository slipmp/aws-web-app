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

  private http: HttpClient;
  private apiUrl: string;

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.http = http;
    this.apiUrl = baseUrl + 'api/ForroLevel';
    this.forroLevelModel = new ForroLevel(1, "", "");
    this.forroLevels = new Array<ForroLevel>();
    this.updateGrid();
  }

  updateGrid() {
    this.http.get<ForroLevel[]>(this.apiUrl)
      .subscribe(result => {
        this.forroLevels = result;
      }, error => console.error(error));
  }

  insert() {
    if (this.forroLevelModel.forroLevelId <= 0 || this.forroLevelModel.forroLevelId > 10) {
      this.forroLevelIdInvalid = true;
    }
    else
    {
      this.http.post(this.apiUrl, this.forroLevelModel).subscribe(result => {
        console.log('Result from Http Post: ' + result);
        this.updateGrid();
        this.addingNew = false;
        this.forroLevelModel = new ForroLevel(1, "","");
      }, error => console.error(error));
    }
  }

  deleteSelectedForroLevel(id:number) {
    this.http.delete(this.apiUrl + '/' + id).subscribe(result => {
      //Modal must close once operation is finished
      this.updateGrid();
    }, error => console.error(error));
  }
}
