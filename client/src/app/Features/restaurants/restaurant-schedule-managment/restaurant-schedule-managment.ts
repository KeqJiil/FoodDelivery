import { ChangeDetectionStrategy, Component, input, output, signal, type ResourceRef } from '@angular/core';
import { Days, type IOpeningWindow, type IRestaurantDetails } from '../models/IRestaurantDetails';
import { FormControl, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-restaurant-schedule-managment',
  imports: [],
  templateUrl: './restaurant-schedule-managment.html',
  styleUrl: './restaurant-schedule-managment.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RestaurantScheduleManagment {
  public readonly restaurant = input.required<ResourceRef<IRestaurantDetails>>();

  public readonly isEditingSchedule = signal(false);
  public readonly draftSchedule = signal<IOpeningWindow[]>([]);
  public readonly openingWindowForm = new FormGroup({
    openDay: new FormControl<Days>(Days.Monday, { nonNullable: true, validators: [ Validators.required ]}),
    openTime: new FormControl<string>('', { nonNullable: true, validators: [ Validators.required ]}),
    closeDay: new FormControl<Days>(Days.Monday, { nonNullable: true, validators: [ Validators.required ]}),
    closeTime: new FormControl<string>('', { nonNullable: true, validators: [ Validators.required ]}),
  });

  public readonly outputDraftSchedule = output<IOpeningWindow[]>();

  public startEditSchedule(): void {
    this.draftSchedule.set([...(this.restaurant().value().openingWindows)]);
    this.isEditingSchedule.set(true);
  }

  public addWindow(): void {
    if (!this.isEditingSchedule()) return;
    if (this.openingWindowForm.invalid) {
      this.openingWindowForm.markAllAsTouched();
      return;
    }
    const data: IOpeningWindow = this.openingWindowForm.getRawValue();
    
    this.draftSchedule.update((schedule): IOpeningWindow[] => [
      ...schedule,
      data,
    ]);

    this.openingWindowForm.reset({
      openDay: Days.Monday,
      openTime: '',
      closeDay: Days.Monday,
      closeTime: '',
    });
  }

  public removeWindow(index: number): void {
    this.draftSchedule.update((windows): IOpeningWindow[] => windows.filter((_, i): boolean => i !== index));
  }

  public cancelEditSchedule(): void {
    this.draftSchedule.set([]);
    this.isEditingSchedule.set(false);
  }

  public onSaveSchedule(): void {
    this.outputDraftSchedule.emit([...this.draftSchedule()]);
  }
}
