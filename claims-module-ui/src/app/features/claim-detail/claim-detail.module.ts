import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CLAIM_DETAIL_ROUTES } from './claim-detail.routes';
import { ClaimDetailComponent } from './claim-detail.component';

@NgModule({
  imports: [RouterModule.forChild(CLAIM_DETAIL_ROUTES), ClaimDetailComponent],
})
export class ClaimDetailModule {}
