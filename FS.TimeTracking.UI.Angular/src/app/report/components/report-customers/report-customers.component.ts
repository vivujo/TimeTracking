import {ChangeDetectorRef, Component, OnDestroy, OnInit, ViewChild} from '@angular/core';
import {Column, Configuration, DataCellTemplate, FooterCellTemplate} from '../../../shared/components/simple-table/simple-table.component';
import {CustomerReportService, CustomerWorkTimeDto} from '../../../shared/services/api';
import {Observable, Subject, Subscription} from 'rxjs';
import {Filter, FilteredRequestParams, FilterName} from '../../../shared/components/filter/filter.component';
import {ChartOptions, ReportChartService} from '../../services/report-chart.service';
import {ApexAxisChartSeries} from 'ng-apexcharts';
import {FormatService} from '../../../shared/services/format.service';
import {LocalizationService} from '../../../shared/services/internationalization/localization.service';
import {DateTime} from 'luxon';
import {single, switchMap} from 'rxjs/operators';
import {UtilityService} from '../../../shared/services/utility.service';

@Component({
  selector: 'ts-report-customers',
  templateUrl: './report-customers.component.html',
  styleUrls: ['./report-customers.component.scss']
})
export class ReportCustomersComponent implements OnInit, OnDestroy {
  @ViewChild('infoCellTemplate', {static: true}) private infoCellTemplate?: DataCellTemplate<CustomerWorkTimeDto>;
  @ViewChild('orderPeriodHeadTemplate', {static: true}) private orderPeriodHeadTemplate?: DataCellTemplate<CustomerWorkTimeDto>;
  @ViewChild('orderPeriodDataTemplate', {static: true}) private orderPeriodDataTemplate?: DataCellTemplate<CustomerWorkTimeDto>;
  @ViewChild('orderPeriodFooterTemplate', {static: true}) private orderPeriodFooterTemplate?: FooterCellTemplate<CustomerWorkTimeDto>;

  public filterChanged = new Subject<FilteredRequestParams>();
  public filters: (Filter | FilterName)[];
  public chartOptions: ChartOptions;
  public chartSeries?: ApexAxisChartSeries;
  public plannedArePartial: boolean = false;

  public tableConfiguration: Partial<Configuration<CustomerWorkTimeDto>>;
  public tableColumns?: Column<CustomerWorkTimeDto>[];
  public tableRows: CustomerWorkTimeDto[] = [];
  private readonly subscriptions = new Subscription();

  public localizedDays = $localize`:@@Abbreviations.Days:[i18n] days`;
  public localizedHours = $localize`:@@Abbreviations.Hours:[i18n] h`;

  constructor(
    public formatService: FormatService,
    private utilityService: UtilityService,
    private reportService: CustomerReportService,
    private localizationService: LocalizationService,
    private reportChartService: ReportChartService,
    private changeDetector: ChangeDetectorRef,
  ) {
    const defaultStartDate = DateTime.now().startOf('year');
    const defaultEndDate = DateTime.now().endOf('year');

    this.filters = [
      {name: 'timeSheetStartDate', defaultValue: defaultStartDate},
      {name: 'timeSheetEndDate', defaultValue: defaultEndDate},
      {name: 'customerId'},
      {name: 'orderId'},
      {name: 'projectId'},
      {name: 'activityId'},
      {name: 'timeSheetIssue'},
      {name: 'timeSheetBillable', defaultValue: true},
    ];

    this.chartOptions = this.reportChartService.createChartOptions();

    this.tableConfiguration = this.createTableConfiguration();
  }

  public ngOnInit(): void {
    const filterChanged = this.filterChanged
      .pipe(switchMap(filter => this.loadData(filter)))
      .subscribe(x => this.tableRows = x);
    this.subscriptions.add(filterChanged);

    this.tableColumns = this.createTableColumns();
  }

  public ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  public tableRowsChanged(rows: Array<CustomerWorkTimeDto>): void {
    this.chartSeries = this.createSeries(rows);
    this.plannedArePartial = rows.some(r => r.plannedIsPartial);
    this.changeDetector.detectChanges();
  }

  public getMinPlanned(): string {
    const minPlannedDate = DateTime.fromMillis(Math.min(...this.tableRows.filter(row => row.plannedStart).map(row => row.plannedStart!.toMillis())));
    return this.formatService.formatDate(minPlannedDate);
  }

  public getMaxPlanned(): string {
    const minPlannedDate = DateTime.fromMillis(Math.max(...this.tableRows.filter(row => row.plannedEnd).map(row => row.plannedEnd!.toMillis())));
    return this.formatService.formatDate(minPlannedDate);
  }

  private loadData(filter: FilteredRequestParams): Observable<CustomerWorkTimeDto[]> {
    return this.reportService.getWorkTimesPerCustomer(filter).pipe(single());
  }

  private createSeries(workTimes: CustomerWorkTimeDto[]): ApexAxisChartSeries {
    return [
      {
        name: $localize`:@@Page.Report.Common.Worked:[i18n] Worked`,
        data: workTimes.map(workTime => ({
          x: workTime.customerTitle,
          y: workTime.daysWorked,
          meta: {time: workTime.timeWorked}
        }))
      },
      {
        name: $localize`:@@Page.Report.Common.Planned:[i18n] Planned`,
        data: workTimes.map(workTime => ({
          x: workTime.customerTitle,
          y: workTime.daysPlanned,
          meta: {time: workTime.timePlanned}
        }))
      },
    ];
  }

  private createTableConfiguration(): Partial<Configuration<CustomerWorkTimeDto>> {
    return {
      cssWrapper: 'table-responsive',
      cssTable: 'table table-card table-sm align-middle text-break border',
      glyphSortAsc: '',
      glyphSortDesc: '',
      locale: this.localizationService.language,
      cssFooterRow: 'text-strong',
    };
  }

  private createTableColumns(): Column<CustomerWorkTimeDto>[] {
    const cssHeadCell = 'border-0 text-nowrap';
    const cssHeadCellMd = 'd-none d-md-table-cell';
    const cssDataCellMd = cssHeadCellMd;

    return [
      {
        title: $localize`:@@DTO.WorkTimeDto.CustomerTitle:[i18n] Customer`,
        prop: 'customerTitle',
        cssHeadCell: cssHeadCell,
        footer: $localize`:@@Common.Summary:[i18n] Summary`,
      }, {
        title: $localize`:@@DTO.WorkTimeDto.OrderPeriod:[i18n] Order period`,
        cssHeadCell: `${cssHeadCell}`,
        prop: 'plannedStart',
        headCellTemplate: this.orderPeriodHeadTemplate,
        dataCellTemplate: this.orderPeriodDataTemplate,
        footerCellTemplate: this.orderPeriodFooterTemplate,
      }, {
        title: $localize`:@@Page.Report.Common.Planned:[i18n] Planned`,
        prop: 'daysPlanned',
        cssHeadCell: `${cssHeadCell} ${cssHeadCellMd} text-end`,
        cssDataCell: `${cssDataCellMd} text-nowrap text-end`,
        cssFooterCell: `${cssDataCellMd} text-nowrap text-end`,
        format: row => `${this.formatService.formatDays(row.daysPlanned)} ${this.localizedDays}`,
        footer: () => `${this.formatService.formatDays(this.utilityService.sum(this.tableRows.map(row => row.daysPlanned)))} ${this.localizedDays}`,
      }, {
        title: $localize`:@@Page.Report.Common.Ratio:[i18n] %`,
        prop: 'ratioTotalPlanned',
        cssHeadCell: `${cssHeadCell} ${cssHeadCellMd} text-end`,
        cssDataCell: `${cssDataCellMd} text-nowrap text-end`,
        cssFooterCell: `${cssDataCellMd} text-nowrap text-end`,
        format: row => `${this.formatService.formatRatio(row.ratioTotalPlanned)} %`,
        footer: '100 %',
      }, {
        title: $localize`:@@Page.Report.Common.Worked:[i18n] Worked`,
        prop: 'daysWorked',
        cssHeadCell: `${cssHeadCell} text-nowrap text-end`,
        cssDataCell: 'text-nowrap text-end',
        cssFooterCell: 'text-nowrap text-end',
        format: row => `${this.formatService.formatDays(row.daysWorked)} ${this.localizedDays}`,
        footer: () => `${this.formatService.formatDays(this.utilityService.sum(this.tableRows.map(row => row.daysWorked)))} ${this.localizedDays}`,
      }, {
        title: $localize`:@@Page.Report.Common.Ratio:[i18n] %`,
        prop: 'ratioTotalWorked',
        cssHeadCell: `${cssHeadCell} ${cssHeadCellMd} text-end`,
        cssDataCell: `${cssDataCellMd} text-nowrap text-end`,
        cssFooterCell: `${cssDataCellMd} text-nowrap text-end`,
        format: row => `${this.formatService.formatRatio(row.ratioTotalWorked)} %`,
        footer: '100 %',
      }, {
        title: $localize`:@@Page.Report.Common.Remain:[i18n] Remain`,
        prop: 'daysDifference',
        cssHeadCell: `${cssHeadCell} text-end`,
        cssDataCell: 'text-nowrap text-end',
        cssFooterCell: 'text-nowrap text-end',
        format: row => `${this.formatService.formatDays(row.daysDifference)} ${this.localizedDays}`,
        footer: () => `${this.formatService.formatDays(this.utilityService.sum(this.tableRows.map(row => row.daysDifference)))} ${this.localizedDays}`,
      }, {
        title: $localize`:@@Page.Report.Common.Ratio:[i18n] %`,
        prop: 'percentDifference',
        cssHeadCell: `${cssHeadCell} ${cssHeadCellMd} text-end`,
        cssDataCell: `${cssDataCellMd} text-nowrap text-end`,
        cssFooterCell: `${cssDataCellMd} text-nowrap text-end`,
        format: row => `${this.formatService.formatRatio(row.percentDifference)} %`,
        footer: () => `Ø ${this.formatService.formatRatio(this.utilityService.avg(this.tableRows.map(row => row.percentDifference).filter((x => x > 0))))} %`,
      }, {
        title: $localize`:@@Common.Details:[i18n] Details`,
        customId: 'info',
        cssHeadCell: `${cssHeadCell} ps-3 text-center`,
        cssDataCell: 'ps-3 text-center',
        dataCellTemplate: this.infoCellTemplate,
        sortable: false,
        width: '1%',
      }
    ];
  }
}
