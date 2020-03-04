import { NgModule } from '@angular/core';
import { SharedModule } from '@shared/shared.module';
import { ReportComponent } from './components/report/report.component';
import { ReportFormComponent } from './components/report-form/report-form.component';
import { ReportChartComponent } from './components/report-chart/report-chart.component';

@NgModule({
  declarations: [
    ReportComponent,
    ReportFormComponent,
    ReportChartComponent
  ],
  imports: [
    SharedModule
  ],
  exports: [
    ReportComponent
  ]
})
export class ReportModule { }
