import {ChangeDetectorRef, Component, OnDestroy, OnInit, ViewChild} from '@angular/core';
import {Column, Configuration, DataCellTemplate, FooterCellTemplate} from '../../../../../core/app/components/simple-table/simple-table.component';
import {IssueChartService, IssueWorkTimeDto} from '../../../../../api/timetracking';
import {Observable, Subscription} from 'rxjs';
import {Filter, FilteredRequestParams, FilterName} from '../../../../../core/app/components/filter/filter.component';
import {ChartOptions, ChartService} from '../../services/chart.service';
import {ApexAxisChartSeries} from 'ng-apexcharts';
import {FormatService} from '../../../../../core/app/services/format.service';
import {LocalizationService} from '../../../../../core/app/services/internationalization/localization.service';
import {DateTime} from 'luxon';
import {single, switchMap} from 'rxjs/operators';
import {UtilityService} from '../../../../../core/app/services/utility.service';
import {EntityService} from '../../../../../core/app/services/state-management/entity.service';

@Component({
  selector: 'ts-chart-issues',
  templateUrl: './chart-issues.component.html',
  styleUrls: ['./chart-issues.component.scss']
})
export class ChartIssuesComponent implements OnInit, OnDestroy {
  @ViewChild('infoCellTemplate', {static: true}) private infoCellTemplate?: DataCellTemplate<IssueWorkTimeDto>;
  @ViewChild('orderPeriodHeadTemplate', {static: true}) private orderPeriodHeadTemplate?: DataCellTemplate<IssueWorkTimeDto>;
  @ViewChild('orderPeriodDataTemplate', {static: true}) private orderPeriodDataTemplate?: DataCellTemplate<IssueWorkTimeDto>;
  @ViewChild('infoFooterTemplate', {static: true}) private infoFooterTemplate?: FooterCellTemplate<IssueWorkTimeDto>;

  public filters: (Filter | FilterName)[];
  public chartOptions: ChartOptions;
  public chartSeries?: ApexAxisChartSeries;

  public tableConfiguration: Partial<Configuration<IssueWorkTimeDto>>;
  public tableColumns?: Column<IssueWorkTimeDto>[];
  public tableRows: IssueWorkTimeDto[] = [];
  public tableFooter: Partial<IssueWorkTimeDto> = {};
  private readonly subscriptions = new Subscription();

  public LOCALIZED_DAYS = $localize`:@@Abbreviations.Days:[i18n] days`;

  constructor(
    public formatService: FormatService,
    private utilityService: UtilityService,
    private issueChartService: IssueChartService,
    private localizationService: LocalizationService,
    private chartService: ChartService,
    private changeDetector: ChangeDetectorRef,
    private entityService: EntityService,
  ) {
    const defaultStartDate = DateTime.now().startOf('year');
    const defaultEndDate = DateTime.now().endOf('year');

    this.filters = [
      {name: 'timeSheetStartDate', defaultValue: defaultStartDate, isPrimary: true},
      {name: 'timeSheetEndDate', defaultValue: defaultEndDate, isPrimary: true},
      {name: 'customerId', isPrimary: true, showHidden: true},
      {name: 'orderId', isPrimary: true, showHidden: true},
      {name: 'projectId', showHidden: true},
      {name: 'activityId', showHidden: true},
      {name: 'timeSheetIssue'},
      {name: 'timeSheetBillable', isPrimary: true, defaultValue: true},
    ];

    this.chartOptions = this.chartService.createChartOptions();
    this.tableConfiguration = this.createTableConfiguration();
  }

  public ngOnInit(): void {
    const filterChanged = this.entityService.reloadRequested
      .pipe(switchMap(filter => this.loadData(filter)))
      .subscribe(rows => this.setTableData(rows));
    this.subscriptions.add(filterChanged);

    this.tableColumns = this.createTableColumns();
  }

  public ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  public tableRowsChanged(rows: Array<IssueWorkTimeDto>): void {
    const maxYValue = Math.max(...rows.map(row => row.daysWorked));
    this.chartOptions = this.chartService.createChartOptions(rows.length, maxYValue);
    this.chartSeries = this.createSeries(rows);
    this.changeDetector.detectChanges();
  }

  private loadData(filter: FilteredRequestParams): Observable<IssueWorkTimeDto[]> {
    return this.issueChartService.getWorkTimesPerIssue(filter).pipe(single());
  }

  private setTableData(rows: IssueWorkTimeDto[]) {
    this.tableRows = rows;
    this.tableFooter = {
      daysWorked: this.utilityService.sum(rows.map(row => row.daysWorked)),
      timeWorked: this.utilityService.sumDuration(rows.map(row => row.timeWorked)),
      budgetWorked: this.utilityService.sum(rows.map(row => row.budgetWorked)),
      totalWorkedPercentage: 1,
      currency: rows[0]?.currency,
    };
  }

  private createSeries(workTimes: IssueWorkTimeDto[]): ApexAxisChartSeries {
    return [
      {
        name: $localize`:@@Page.Chart.Common.Worked:[i18n] Worked`,
        data: workTimes.map(workTime => ({
          x: workTime.issue,
          y: workTime.daysWorked,
          meta: {time: workTime.timeWorked}
        }))
      }
    ];
  }

  private createTableConfiguration(): Partial<Configuration<IssueWorkTimeDto>> {
    return {
      cssWrapper: 'table-responsive',
      cssTable: 'table',
      glyphSortAsc: '',
      glyphSortDesc: '',
      locale: this.localizationService.language,
      cssFooterRow: 'fw-bold',
    };
  }

  private createTableColumns(): Column<IssueWorkTimeDto>[] {
    const cssHeadCell = 'border-0 text-nowrap';
    const cssHeadCellLg = 'd-none d-lg-table-cell';
    const cssDataCellLg = cssHeadCellLg;

    return [
      {
        title: $localize`:@@DTO.WorkTimeDto.Issue:[i18n] Issue`,
        prop: 'issue',
        cssHeadCell: cssHeadCell,
        footer: $localize`:@@Common.Summary:[i18n] Summary`,
      }, {
        title: $localize`:@@DTO.WorkTimeDto.CustomerTitle:[i18n] Customer`,
        prop: 'customerTitle',
        cssHeadCell: cssHeadCellLg,
        cssDataCell: cssDataCellLg,
        cssFooterCell: cssDataCellLg,
      }, {
        title: $localize`:@@Page.Chart.Common.DaysWorked:[i18n] Days worked`,
        prop: 'daysWorked',
        cssHeadCell: `${cssHeadCell} text-nowrap text-end`,
        cssDataCell: 'text-nowrap text-end',
        cssFooterCell: 'text-nowrap text-end',
        format: row => `${this.formatService.formatDays(row.daysWorked)} ${this.LOCALIZED_DAYS}`,
        footer: () => `${this.formatService.formatDays(this.tableFooter.daysWorked)} ${this.LOCALIZED_DAYS}`,
      }, {
        title: $localize`:@@Page.Chart.Common.PercentageOfTotalTime:[i18n] Percentage of total time`,
        prop: 'totalWorkedPercentage',
        cssHeadCell: `${cssHeadCell} ${cssHeadCellLg} text-end`,
        cssDataCell: `${cssDataCellLg} text-nowrap text-end`,
        cssFooterCell: `${cssDataCellLg} text-nowrap text-end`,
        format: row => `${this.formatService.formatRatio(row.totalWorkedPercentage)} %`,
        footer: () => `${this.formatService.formatRatio(this.tableFooter.totalWorkedPercentage)} %`,
      }, {
        title: '',
        customId: 'info',
        cssHeadCell: `${cssHeadCell} ps-3 text-center`,
        cssDataCell: 'ps-3 text-center',
        cssFooterCell: 'ps-3 text-center',
        dataCellTemplate: this.infoCellTemplate,
        footerCellTemplate: this.infoFooterTemplate,
        sortable: false,
        width: '1%',
      }
    ];
  }
}
