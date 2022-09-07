import {ChangeDetectorRef, Component, OnDestroy, OnInit, ViewChild} from '@angular/core';
import {Observable, Subscription} from 'rxjs';
import {OrderChartService, OrderWorkTimeDto} from '../../../../../api/timetracking';
import {single, switchMap} from 'rxjs/operators';
import {Column, Configuration, DataCellClickEvent, DataCellTemplate, FooterCellTemplate} from '../../../../../core/app/components/simple-table/simple-table.component';
import {LocalizationService} from '../../../../../core/app/services/internationalization/localization.service';
import {FormatService} from '../../../../../core/app/services/format.service';
import {Filter, FilteredRequestParams, FilterName} from '../../../../../core/app/components/filter/filter.component';
import {ApexAxisChartSeries,} from "ng-apexcharts";
import {DateTime} from 'luxon';
import {ChartOptions, ChartService} from '../../services/chart.service';
import {UtilityService} from '../../../../../core/app/services/utility.service';
import {EntityService} from '../../../../../core/app/services/state-management/entity.service';

type ColoredOrderWorkTimeDto = OrderWorkTimeDto & { color: string, selected: boolean };

@Component({
  selector: 'ts-chart-orders',
  templateUrl: './chart-orders.component.html',
  styleUrls: ['./chart-orders.component.scss']
})
export class ChartOrdersComponent implements OnInit, OnDestroy {
  @ViewChild('orderDataTemplate', {static: true}) private orderDataTemplate?: DataCellTemplate<ColoredOrderWorkTimeDto>;
  @ViewChild('orderPeriodHeadTemplate', {static: true}) private orderPeriodHeadTemplate?: DataCellTemplate<ColoredOrderWorkTimeDto>;
  @ViewChild('orderPeriodDataTemplate', {static: true}) private orderPeriodDataTemplate?: DataCellTemplate<ColoredOrderWorkTimeDto>;
  @ViewChild('daysDifferenceTemplate', {static: true}) private daysDifferenceTemplate?: DataCellTemplate<ColoredOrderWorkTimeDto>;
  @ViewChild('orderPeriodFooterTemplate', {static: true}) private orderPeriodFooterTemplate?: FooterCellTemplate<ColoredOrderWorkTimeDto>;
  @ViewChild('infoFooterTemplate', {static: true}) private infoFooterTemplate?: FooterCellTemplate<ColoredOrderWorkTimeDto>;
  @ViewChild('infoCellTemplate', {static: true}) private infoCellTemplate?: DataCellTemplate<ColoredOrderWorkTimeDto>;

  public filters: (Filter | FilterName)[];
  public chartOptions: ChartOptions;
  public chartSeries?: ApexAxisChartSeries;
  public plannedArePartial: boolean = false;
  public overbookedOrders: number = 0;
  public chartRows: ColoredOrderWorkTimeDto[] = [];
  public chartRowsSelected: ColoredOrderWorkTimeDto[] = [];

  public tableConfiguration: Partial<Configuration<ColoredOrderWorkTimeDto>>;
  public tableColumns?: Column<ColoredOrderWorkTimeDto>[];
  public tableRows: ColoredOrderWorkTimeDto[] = [];
  public tableFooter: Partial<ColoredOrderWorkTimeDto> = {};
  private readonly subscriptions = new Subscription();

  public readonly LOCALIZED_DAYS = $localize`:@@Abbreviations.Days:[i18n] days`;
  public readonly LOCALIZED_PERCENTAGE = $localize`:@@Page.Chart.Common.Percentage:[i18n] %`;

  constructor(
    public formatService: FormatService,
    private utilityService: UtilityService,
    private orderChartService: OrderChartService,
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
    const filterChanged = this.entityService.filterChanged
      .pipe(switchMap(filter => this.loadData(filter)))
      .subscribe(rows => this.setTableData(rows));
    this.subscriptions.add(filterChanged);

    this.tableColumns = this.createTableColumns();
  }

  public ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  public tableRowsChanged(rows: ColoredOrderWorkTimeDto[]): void {
    const maxYValue = Math.max(...rows.map(row => Math.max(row.daysWorked, row.daysPlanned)));
    this.chartOptions = this.chartService.createChartOptions(rows.length, maxYValue);
    this.chartSeries = this.createSeries(rows);
    this.plannedArePartial = rows.some(row => row.plannedIsPartial);
    this.overbookedOrders = rows.filter(row => row.daysPlanned && row.daysPlanned < row.daysWorked).length;
    this.changeDetector.detectChanges();
  }

  public dataCellClick(event: DataCellClickEvent<ColoredOrderWorkTimeDto>) {
    if (event.column.customId === 'info')
      return;
    event.row.selected = !event.row.selected;
    this.chartRowsSelected = event.table.rows.filter(row => row.selected);
    this.chartRows = this.chartRowsSelected.length > 0 ? this.chartRowsSelected : this.tableRows;
  }

  public getMinPlanned(): string {
    const rows = this.tableRows.filter(row => row.plannedStart);
    if (rows.length === 0)
      return '';

    const minPlannedDate = DateTime.fromMillis(Math.min(...rows.map(row => row.plannedStart!.toMillis())));
    return this.formatService.formatDate(minPlannedDate);
  }

  public getMaxPlanned(): string {
    const rows = this.tableRows.filter(row => row.plannedEnd);
    if (rows.length === 0)
      return '';

    const minPlannedDate = DateTime.fromMillis(Math.max(...rows.map(row => row.plannedEnd!.toMillis())));
    return this.formatService.formatDate(minPlannedDate);
  }

  private loadData(filter: FilteredRequestParams): Observable<OrderWorkTimeDto[]> {
    return this.orderChartService.getWorkTimesPerOrder(filter).pipe(single());
  }

  private setTableData(rows: OrderWorkTimeDto[]) {
    this.tableRows = this.chartService
      .addColors(rows)
      .map(row => ({...row, selected: false}));
    this.chartRows = this.tableRows;

    this.tableFooter = {
      daysPlanned: this.utilityService.sum(rows.map(row => row.daysPlanned)),
      timePlanned: this.utilityService.sumDuration(rows.map(row => row.timePlanned)),
      daysWorked: this.utilityService.sum(rows.map(row => row.daysWorked)),
      timeWorked: this.utilityService.sumDuration(rows.map(row => row.timeWorked)),
      daysDifference: this.utilityService.sum(rows.map(row => row.daysDifference)),
      timeDifference: this.utilityService.sumDuration(rows.map(row => row.timeDifference)),
      budgetPlanned: this.utilityService.sum(rows.map(row => row.budgetPlanned)),
      budgetWorked: this.utilityService.sum(rows.map(row => row.budgetWorked)),
      budgetDifference: this.utilityService.sum(rows.map(row => row.budgetDifference)),
      completed: this.utilityService.sum(rows.map(row => row.daysWorked)) / this.utilityService.sum(rows.map(row => row.daysPlanned)),
      totalPlannedPercentage: 1,
      currency: rows[0]?.currency,
    };
  }

  private createSeries(workTimes: OrderWorkTimeDto[]): ApexAxisChartSeries {
    return [
      {
        name: $localize`:@@Page.Chart.Common.Worked:[i18n] Worked`,
        data: workTimes.map(workTime => ({
          x: (this.isOverbooked(workTime) ? '◆ ' : '') + workTime.orderTitle,
          y: workTime.daysWorked,
          meta: {days: workTime.daysWorked, time: workTime.timeWorked}
        }))
      },
      {
        name: $localize`:@@Page.Chart.Common.Planned:[i18n] Planned`,
        data: workTimes.map(workTime => ({
          x: workTime.orderTitle,
          y: workTime.daysPlanned > workTime.daysWorked ? workTime.daysPlanned - workTime.daysWorked : 0,
          meta: {days: workTime.daysPlanned, time: workTime.timePlanned}
        }))
      },
    ];
  }

  private isOverbooked(entity: OrderWorkTimeDto): boolean {
    return (entity.daysPlanned != null) && (entity.daysPlanned < entity.daysWorked);
  }

  private createTableConfiguration(): Partial<Configuration<ColoredOrderWorkTimeDto>> {
    return {
      cssWrapper: 'table-responsive',
      cssTable: 'table table-hover',
      glyphSortAsc: '',
      glyphSortDesc: '',
      locale: this.localizationService.language,
      cssDataRow: row => `cursor-pointer ${row.selected ? 'selected' : ''}`,
      cssFooterRow: 'fw-bold',
    };
  }

  private createTableColumns(): Column<ColoredOrderWorkTimeDto>[] {
    const cssHeadCell = 'text-nowrap';
    const cssHeadCellMd = 'd-none d-md-table-cell';
    const cssDataCellMd = cssHeadCellMd;

    return [
      {
        title: $localize`:@@DTO.WorkTimeDto.OrderTitle:[i18n] Order`,
        prop: 'orderTitle',
        cssHeadCell: cssHeadCell,
        footer: $localize`:@@Common.Summary:[i18n] Summary`,
        dataCellTemplate: this.orderDataTemplate,
      }, {
        title: $localize`:@@DTO.WorkTimeDto.OrderPeriod:[i18n] Order period`,
        cssHeadCell: `${cssHeadCell}`,
        prop: 'plannedStart',
        headCellTemplate: this.orderPeriodHeadTemplate,
        dataCellTemplate: this.orderPeriodDataTemplate,
        footerCellTemplate: this.orderPeriodFooterTemplate,
      }, {
        title: $localize`:@@Page.Chart.Common.DaysWorked:[i18n] Days worked`,
        prop: 'daysWorked',
        cssHeadCell: `${cssHeadCell} text-nowrap text-end`,
        cssDataCell: 'text-nowrap text-end',
        cssFooterCell: 'text-nowrap text-end',
        format: row => `${this.formatService.formatDays(row.daysWorked)} ${this.LOCALIZED_DAYS}`,
        footer: () => `${this.formatService.formatDays(this.tableFooter.daysWorked)} ${this.LOCALIZED_DAYS}`,
      }, {
        title: $localize`:@@Page.Chart.Common.DaysPlanned:[i18n] Days planned`,
        prop: 'daysPlanned',
        cssHeadCell: `${cssHeadCell} ${cssHeadCellMd} text-end`,
        cssDataCell: `${cssDataCellMd} text-nowrap text-end`,
        cssFooterCell: `${cssDataCellMd} text-nowrap text-end`,
        format: row => row.daysPlanned ? `${this.formatService.formatDays(row.daysPlanned)} ${this.LOCALIZED_DAYS}` : '',
        footer: () => `${this.formatService.formatDays(this.tableFooter.daysPlanned)} ${this.LOCALIZED_DAYS}`,
      }, {
        title: $localize`:@@Page.Chart.Common.DaysLeft:[i18n] Days left`,
        prop: 'daysDifference',
        cssHeadCell: `${cssHeadCell} text-end`,
        cssDataCell: 'text-nowrap text-end',
        cssFooterCell: 'text-nowrap text-end',
        dataCellTemplate: this.daysDifferenceTemplate,
        footer: () => `${this.formatService.formatDays(this.tableFooter.daysDifference)} ${this.LOCALIZED_DAYS}`,
        sort: (rowA, rowB, direction) => this.chartService.sortByDaysDifference(rowA, rowB, direction),
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
