import {Injectable} from '@angular/core';
import {StorageService} from '../storage/storage.service';

@Injectable({
  providedIn: 'root'
})
export class LocalizationService {

  private readonly languageKey = 'language';
  private dateTimeFormats: DateTimeFormats;
  private numberFormats: NumberFormats;

  constructor(
    private storageService: StorageService
  ) {
    this.dateTimeFormats = new DateTimeFormats(this.storageService, this.language);
    this.numberFormats = new NumberFormats(this.storageService, this.language);
  }

  public get language(): string {
    return this.storageService.get(this.languageKey, navigator.languages[0].substring(0, 2));
  }

  public set language(value: string) {
    this.storageService.set(this.languageKey, value);
    this.dateTimeFormats = new DateTimeFormats(this.storageService, this.language);
    this.numberFormats = new NumberFormats(this.storageService, this.language);
  }

  public get dateTime(): DateTimeFormats {
    return this.dateTimeFormats;
  }

  public get numbers(): NumberFormats {
    return this.numberFormats;
  }
}

class DateTimeFormats {
  private readonly dateFormatKey = 'dateFormat';
  private readonly timeFormatKey = 'timeFormat';
  private readonly dateTimeFormatKey = 'dateTimeFormat';
  private readonly guessedDateFormat: string;
  private readonly guessedTimeFormat: string;
  private readonly guessedDateTimeFormat: string;

  constructor(
    private storageService: StorageService,
    private language: string
  ) {
    this.guessedDateFormat = this.guessDateFormat();
    this.guessedTimeFormat = this.guessTimeFormat();
    this.guessedDateTimeFormat = this.guessDateTimeFormat();
  }

  public get dateFormat(): string {
    return this.storageService.get(this.dateFormatKey, this.guessedDateFormat);
  }

  public set dateFormat(value: string) {
    this.storageService.set(this.dateFormatKey, value);
  }

  public get timeFormat(): string {
    return this.storageService.get(this.timeFormatKey, this.guessedTimeFormat);
  }

  public set timeFormat(value: string) {
    this.storageService.set(this.timeFormatKey, value);
  }

  public get dateTimeFormat(): string {
    return this.storageService.get(this.dateTimeFormatKey, this.guessedDateTimeFormat);
  }

  public set dateTimeFormat(value: string) {
    this.storageService.set(this.dateTimeFormatKey, value);
  }

  private guessDateFormat(): string {
    const intl = new Intl.DateTimeFormat(this.language, {day: '2-digit', month: '2-digit', year: 'numeric'});
    return this.toDateTimeFormat(intl.formatToParts());
  }

  private guessTimeFormat(): string {
    const intl = new Intl.DateTimeFormat(this.language, {hour: '2-digit', minute: '2-digit', second: '2-digit', hour12: false});
    return this.toDateTimeFormat(intl.formatToParts());
  }

  private guessDateTimeFormat(): string {
    const intl = new Intl.DateTimeFormat(this.language, {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
      hour12: false
    });

    return this.toDateTimeFormat(intl.formatToParts());
  }

  private toDateTimeFormat(parts: Intl.DateTimeFormatPart[]): string {
    return parts
      .map((x: Intl.DateTimeFormatPart) => {
        switch (x.type) {
          case 'month':
            return 'MM';
          case 'day':
            return 'dd';
          case 'year':
            return 'yyyy';
          case 'hour':
            return 'HH';
          case 'minute':
            return 'mm';
          case 'second':
            return 'ss';
          default:
            return x.value;
        }
      })
      .join('');
  }
}

class NumberFormats {
  private readonly decimalSymbolKey = 'decimalSymbol';
  private readonly digitGroupingSymbolKey = 'digitGroupingSymbol';

  constructor(
    private storageService: StorageService,
    private language: string
  ) {}

  public get decimalSymbol(): string {
    return this.storageService.get(this.decimalSymbolKey, this.guessDecimalSymbol());
  }

  public set decimalSymbol(value: string) {
    this.storageService.set(this.decimalSymbolKey, value);
  }

  public get digitGroupingSymbol(): string {
    return this.storageService.get(this.digitGroupingSymbolKey, this.guessDigitGroupingSymbol());
  }

  public set digitGroupingSymbol(value: string) {
    this.storageService.set(this.digitGroupingSymbolKey, value);
  }

  private guessDecimalSymbol(): string {
    const intl = new Intl.NumberFormat(this.language);
    // @ts-ignore
    return intl.formatToParts(1234.5).find((x: any) => x.type === 'decimal')?.value ?? '.';
  }

  private guessDigitGroupingSymbol(): string {
    const intl = new Intl.NumberFormat(this.language);
    // @ts-ignore
    return intl.formatToParts(1234.5).find((x: any) => x.type === 'group')?.value ?? ',';
  }
}
