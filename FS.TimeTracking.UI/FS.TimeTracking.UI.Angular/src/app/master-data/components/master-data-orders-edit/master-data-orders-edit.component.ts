import {AfterViewInit, Component, ElementRef, TemplateRef, ViewChild} from '@angular/core';
import {FormValidationService, ValidationFormGroup} from '../../../shared/services/form-validation/form-validation.service';
import {Observable} from 'rxjs';
import {OrderDto, OrderService, StringTypeaheadDto, TypeaheadService} from '../../../shared/services/api';
import {ActivatedRoute, Router} from '@angular/router';
import {EntityService} from '../../../shared/services/state-management/entity.service';
import {single} from 'rxjs/operators';
import {Modal} from 'bootstrap';
import {GuidService} from '../../../shared/services/state-management/guid.service';
import {NgbModal, NgbModalRef} from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'ts-master-data-orders-edit',
  templateUrl: './master-data-orders-edit.component.html',
  styleUrls: ['./master-data-orders-edit.component.scss']
})
export class MasterDataOrdersEditComponent implements AfterViewInit {
  public orderForm: ValidationFormGroup;
  public isNewRecord: boolean;
  public customers$: Observable<StringTypeaheadDto[]>;

  @ViewChild('orderEdit') private orderEdit?: TemplateRef<any>;

  private modal?: NgbModalRef

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private orderService: OrderService,
    private entityService: EntityService,
    private formValidationService: FormValidationService,
    typeaheadService: TypeaheadService,
    private modalService: NgbModal
  ) {
    this.isNewRecord = this.route.snapshot.params['id'] === GuidService.guidEmpty;
    this.orderForm = this.formValidationService
      .getFormGroup<OrderDto>('OrderDto', {
        id: GuidService.guidEmpty,
        hourlyRate: 0,
        hidden: false
      });

    if (!this.isNewRecord)
      this.orderService
        .get({id: this.route.snapshot.params['id']})
        .pipe(single())
        .subscribe(order => this.orderForm.patchValue(order));

    this.customers$ = typeaheadService.getCustomers({showHidden: true});
  }

  public ngAfterViewInit(): void {
    this.modal = this.modalService.open(this.orderEdit, {size: 'lg', scrollable: true});
    this.modal.hidden.pipe(single()).subscribe(() => this.router.navigate(['..'], {relativeTo: this.route}));
  }

  public save(): void {
    if (!this.orderForm.valid)
      return;

    const apiAction = this.isNewRecord
      ? this.orderService.create({orderDto: this.orderForm.value})
      : this.orderService.update({orderDto: this.orderForm.value});

    const orderChangedAction = this.isNewRecord
      ? 'created'
      : 'updated';

    apiAction
      .pipe(single())
      .subscribe(order => {
        this.modal?.close();
        this.entityService.orderChanged.next({entity: order, action: orderChangedAction});
      });
  }
}
