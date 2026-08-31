import { ChangeDetectionStrategy, Component, input, type OnInit, output, signal } from '@angular/core';
import { Days, type IOpeningWindow } from '../models/IRestaurantDetails';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatListModule } from '@angular/material/list';

@Component({
  selector: 'app-restaurant-schedule-managment',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
    MatListModule,
  ],
  templateUrl: './restaurant-schedule-managment.html',
  styleUrl: './restaurant-schedule-managment.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RestaurantScheduleManagment implements OnInit {
  public readonly openingWindows = input<IOpeningWindow[]>([]);
  public readonly autoEdit = input<boolean>(false);

  public readonly isEditingSchedule = signal(false);
  public readonly draftSchedule = signal<IOpeningWindow[]>([]);
  public readonly openingWindowForm = new FormGroup({
    openDay: new FormControl<Days>(Days.Monday, { nonNullable: true, validators: [ Validators.required ]}),
    openTime: new FormControl<string>('', { nonNullable: true, validators: [ Validators.required ]}),
    closeDay: new FormControl<Days>(Days.Monday, { nonNullable: true, validators: [ Validators.required ]}),
    closeTime: new FormControl<string>('', { nonNullable: true, validators: [ Validators.required ]}),
  });

  public readonly outputDraftSchedule = output<IOpeningWindow[]>();

  protected readonly dayOptions: { label: string; value: Days }[] = [
    { label: 'Monday', value: Days.Monday },
    { label: 'Tuesday', value: Days.Tuesday },
    { label: 'Wednesday', value: Days.Wednesday },
    { label: 'Thursday', value: Days.Thursday },
    { label: 'Friday', value: Days.Friday },
    { label: 'Saturday', value: Days.Saturday },
    { label: 'Sunday', value: Days.Sunday },
  ];

  public ngOnInit(): void {
    if (this.autoEdit()) {
      this.startEditSchedule();
    }
  }

  public startEditSchedule(): void {
    this.draftSchedule.set([...this.openingWindows()]);
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
    this.isEditingSchedule.set(false);
  }

  protected dayName(day: Days): string {
    return this.dayOptions.find((d): boolean => d.value === day)?.label ?? '';
  }
}
