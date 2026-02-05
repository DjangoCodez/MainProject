export type SoeChartType = 'bar' | 'line' | 'area' | 'pie';

export interface BaseChart<T> {
  title: string;
  data: T[];
}

export interface BarChart<T> extends BaseChart<T> {
  type: 'bar';
  xKey: keyof T;
  yKey: keyof T;
}

export interface LineChart<T> extends BaseChart<T> {
  type: 'line';
  xKey: keyof T;
  yKey: keyof T;
}

export interface AreaChart<T> extends BaseChart<T> {
  type: 'area';
  xKey: keyof T;
  yKey: keyof T;
}

export interface PieChart<T> extends BaseChart<T> {
  type: 'pie';
  angleKey: keyof T;
  radiusKey: keyof T;
}

export type ChartConfig<T> =
  | BarChart<T>
  | LineChart<T>
  | AreaChart<T>
  | PieChart<T>;

export type ChartType = ChartConfig<any>['type'];
