export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  extensions?: {
    code?: string;
    [key: string]: unknown;
  };
}
