import { Component, OnInit } from '@angular/core';

import { ActiveOutage } from '@shared/models/outage.model';
import { OutageService } from '@services/outage/outage.service';

export interface PeriodicElement {
  name: string;
  position: number;
  weight: number;
  symbol: string;
};

// const AO_MOCK: ActiveOutage[] = [
//   { Id: 1, ElementId: 2321619, ReportedAt: new Date(), AfectedConsumers: [] },
//   { Id: 2, ElementId: 3311516, ReportedAt: new Date(), AfectedConsumers: [] },
//   { Id: 3, ElementId: 4321512, ReportedAt: new Date(), AfectedConsumers: [] },
//   { Id: 4, ElementId: 5684515, ReportedAt: new Date(), AfectedConsumers: [] },
//   { Id: 5, ElementId: 6151715, ReportedAt: new Date(), AfectedConsumers: [] },
//   { Id: 6, ElementId: 7236541, ReportedAt: new Date(), AfectedConsumers: [] }
// ];

@Component({
  selector: 'app-active-browser',
  templateUrl: './active-browser.component.html',
  styleUrls: ['./active-browser.component.css']
})

export class ActiveBrowserComponent implements OnInit {
  private activeOutages: ActiveOutage[];
  private columns: string[] = ["id", "elementId", "reportedAt"];

  constructor(private outageService: OutageService) { }

  ngOnInit() {
    // subscribe to outage service and get real data from db
    
    this.activeOutages = [];
    this.GetActiveOutages();
    //console.log(this.activeOutages);
  }

  private GetActiveOutages(): void {
    this.outageService.getAllActiveOutages().subscribe(
      outages => { 
        this.activeOutages = outages;
      }, 
      err => {
        console.log(err);
      }
    );

    console.log(this.activeOutages);
  }

}

