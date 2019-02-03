import { ForroLevel } from "./ForroLevel";

export class ForroLevelModel {

  constructor(
    public forroLevel: ForroLevel,
    public forroLevelList: ForroLevel[],
    public errorMessage: string) {
  }
}
