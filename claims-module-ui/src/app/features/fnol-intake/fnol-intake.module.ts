import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { FNOL_INTAKE_ROUTES } from './fnol-intake.routes';
import { FnolIntakeComponent } from './fnol-intake.component';

@NgModule({
  imports: [RouterModule.forChild(FNOL_INTAKE_ROUTES), FnolIntakeComponent],
})
export class FnolIntakeModule {}
