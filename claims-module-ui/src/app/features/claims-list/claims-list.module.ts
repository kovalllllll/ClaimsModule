import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CLAIMS_LIST_ROUTES } from './claims-list.routes';
import { ClaimsListComponent } from './claims-list.component';

@NgModule({
  imports: [RouterModule.forChild(CLAIMS_LIST_ROUTES), ClaimsListComponent],
})
export class ClaimsListModule {}
