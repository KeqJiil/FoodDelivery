export interface IErrorDetails {
  title: string;
  status: number;
  detail?: string;
  errors?: Record<string, string[]>;
}