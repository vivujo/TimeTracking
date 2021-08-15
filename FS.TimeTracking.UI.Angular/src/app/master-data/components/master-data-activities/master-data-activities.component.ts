import {Component, OnDestroy, OnInit, ViewChild} from '@angular/core';
import {EntityService} from '../../../shared/services/state-management/entity.service';
import {ActivatedRoute, Router} from '@angular/router';
import {ActivityListDto, ActivityService} from '../../../shared/services/api';
import {LocalizationService} from '../../../shared/services/internationalization/localization.service';
import {
  Column,
  Configuration,
  DataCellClickEvent,
  DataCellTemplate,
  SimpleTableComponent
} from '../../../shared/components/simple-table/simple-table.component';
import {Subscription} from 'rxjs';
import {single} from 'rxjs/operators';
import {GuidService} from '../../../shared/services/state-management/guid.service';

@Component({
  selector: 'ts-master-data-activities',
  templateUrl: './master-data-activities.component.html',
  styleUrls: ['./master-data-activities.component.scss']
})
export class MasterDataActivitiesComponent implements OnInit, OnDestroy {

  @ViewChild(SimpleTableComponent) private activityTable?: SimpleTableComponent<ActivityListDto>;
  @ViewChild('dataCellTemplate', {static: true}) private dataCellTemplate?: DataCellTemplate<ActivityListDto>;
  @ViewChild('actionCellTemplate', {static: true}) private actionCellTemplate?: DataCellTemplate<ActivityListDto>;

  public rows: ActivityListDto[];
  public columns!: Column<ActivityListDto>[];
  public configuration?: Partial<Configuration<ActivityListDto>>;

  private subscriptions = new Subscription();

  constructor(
    public guidService: GuidService,
    private entityService: EntityService,
    private router: Router,
    private route: ActivatedRoute,
    private activityService: ActivityService,
    private localizationService: LocalizationService,
  ) {
    this.rows = [];
  }

  public ngOnInit(): void {
    this.activityService.list().pipe(single()).subscribe(x => this.rows = x);

    const activityChanged = this.entityService.activityChanged
      .pipe(this.entityService.replaceEntityWithListDto(this.activityService))
      .subscribe(changedEvent => {
          const updatedRows = this.entityService.updateCollection(this.rows, 'id', changedEvent);
          this.rows = [...updatedRows];
        }
      );
    this.subscriptions.add(activityChanged);

    this.configuration = {
      cssWrapper: 'table-responsive',
      cssTable: 'table table-borderless table-hover small',
      glyphSortAsc: '',
      glyphSortDesc: '',
      locale: this.localizationService.language,
    };

    const dataCellCss = (row: ActivityListDto) => row.hidden ? 'text-secondary text-decoration-line-through' : '';
    this.columns = [
      {title: $localize`:@@DTO.ActivityListDto.Title:[i18n] Activity`, prop: 'title', cssDataCell: dataCellCss, dataCellTemplate: this.dataCellTemplate},
      {
        title: $localize`:@@DTO.ActivityListDto.ProjectTitle:[i18n] Project`,
        prop: 'projectTitle',
        cssDataCell: dataCellCss,
        dataCellTemplate: this.dataCellTemplate
      },
      {
        title: $localize`:@@Common.Action:[i18n] Action`,
        customId: 'delete',
        dataCellTemplate: this.actionCellTemplate,
        cssDataCell: 'text-nowrap align-middle action-cell',
        width: '1px',
        sortable: false
      },
    ];
  }

  public ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  public dataCellClick($event: DataCellClickEvent<ActivityListDto>): void {
    if ($event.column.customId !== 'delete') {
      // noinspection JSIgnoredPromiseFromCall
      this.router.navigate([$event.row.id], {relativeTo: this.route});
    }
  }

  public deleteItem(id: string): void {
    this.activityService
      .delete(id)
      .pipe(single())
      .subscribe(() => {
        this.entityService.activityChanged.next({entity: {id} as ActivityListDto, action: 'deleted'});
      });
  }
}
