import { Component, Inject } from '@angular/core'
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-forro-level',
  templateUrl: './forro-level.component.html'
})

export class ForroLevelComponent {
  public forroLevels: ForroLevel[];
  public addingNew = false;

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    //this.forroLevels = new Array<ForroLevel>();
    //this.forroLevels[0] = { name: 'test' }

    var apiUrl = baseUrl + 'api/ForroLevel';
    http.get<ForroLevel[]>(apiUrl)
      .subscribe(result => {
        this.forroLevels = result;
      }, error => console.error(error));
  }

  insert() {

  }
}

interface ForroLevel {
  forroLevelId: number;
  name: string;
}
