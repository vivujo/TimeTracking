import {AfterViewInit, Directive, ElementRef, forwardRef, HostListener, Input, OnDestroy, Optional} from '@angular/core';
import {LocalizationService} from '../services/internationalization/localization.service';
import {DateTime} from 'luxon';
import {ControlValueAccessor, NG_VALUE_ACCESSOR} from '@angular/forms';
import {UtilityService} from '../services/utility.service';
import {DateObjectUnits} from 'luxon/src/datetime';

const CUSTOM_VALUE_ACCESSOR: any = {
  provide: NG_VALUE_ACCESSOR,
  useExisting: forwardRef(() => DatePickerDirective),
  multi: true,
};

@Directive({
  selector: '[tsDatePicker]',
  providers: [CUSTOM_VALUE_ACCESSOR],
})
export class DatePickerDirective implements AfterViewInit, OnDestroy, ControlValueAccessor {
  private readonly format: string;
  private value?: DateTime = DateTime.min();
  private originTimePart: DateObjectUnits = {hour: 0, minute: 0, second: 0, millisecond: 0};
  private disabled = false;
  private datePicker: any;
  private datePickerOptions = {};

  constructor(
    private elementRef: ElementRef,
    private localizationService: LocalizationService,
    private utilityService: UtilityService,
  ) {
    this.format = localizationService.dateTime.dateFormat;

    this.datePickerOptions = {
      format: this.toDatePickerFormat(this.format),
      todayHighlight: true,
      calendarWeeks: true,
      weekStart: 1,
      autoclose: true,
      language: this.localizationService.language,
      todayBtn: 'linked',
      showOnFocus: false,
      assumeNearbyYear: true,
      forceParse: false,
      keyboardNavigation: false
    };
  }

  @HostListener('click')
  public onClick(): void {
    this.datePicker?.datepicker('show');
  }

  @HostListener('blur', ['$event.target.value'])
  public onBlur(rawInput: string | undefined) {
    const parsedDate = this.utilityService.parseDate(rawInput);
    if (parsedDate?.isValid) {
      this.elementRef.nativeElement.value = parsedDate.toFormat(this.format);
      this.emitValue(parsedDate);
    } else {
      this.elementRef.nativeElement.value = '';
      this.emitValue(undefined);
    }
  }

  public ngAfterViewInit(): void {
    this.datePicker = $(this.elementRef.nativeElement) as any;
    this.datePicker
      .datepicker(this.datePickerOptions)
      .on('hide', () => {
        this.datePicker.focus();
      })
      .on('changeDate', (event: any) => {
        const parsedDate = DateTime.fromJSDate(event.date);
        this.elementRef.nativeElement.value = parsedDate.toFormat(this.format);
        this.emitValue(parsedDate);
      })
    ;
  }

  public writeValue(obj?: DateTime): void {
    this.value = obj;
    this.originTimePart = obj?.isValid
      ? {hour: obj.hour, minute: obj.minute, second: obj.second, millisecond: obj.millisecond}
      : {hour: 0, minute: 0, second: 0, millisecond: 0};
    this.datePicker?.datepicker('update', this.value?.toJSDate());
    this.elementRef.nativeElement.value = this.value?.toFormat(this.format) ?? '';
  }

  public registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  public registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  public setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  public emitValue(value?: DateTime): void {
    if (value?.isValid)
      value = value?.set(this.originTimePart);

    if ((!this.value && !value) || (value && this.value?.equals(value)))
      return;

    this.value = value;
    this.onChange(this.value);
    this.onTouched();
  }

  public ngOnDestroy(): void {
    this.datePicker?.datepicker('destroy');
  }

  private toDatePickerFormat(format: string): string {
    // see https://moment.github.io/luxon/docs/manual/formatting.html
    // see https://bootstrap-datepicker.readthedocs.io/en/stable/options.html#format
    return format
      .replace(/EEEE/g, 'DD')
      .replace(/cccc/g, 'DD')
      .replace(/MMMM/g, 'MM')
      .replace(/EEE/g, 'D')
      .replace(/ccc/g, 'D')
      .replace(/MMM/g, 'M')
      .replace(/MM/g, 'mm')
      .replace(/M/g, 'm');
  }

  private onChange: (obj: any) => void = (_: any) => {};
  private onTouched: () => void = () => {};
}
