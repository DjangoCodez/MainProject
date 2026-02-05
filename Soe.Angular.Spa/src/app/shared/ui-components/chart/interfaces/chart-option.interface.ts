import {
  AgCartesianChartOptions,
  AgPolarChartOptions,
} from 'ag-charts-enterprise';
import { ChartConfig } from './chart-type.interface';

export interface ChartTypeToOptionsMap {
  bar: AgCartesianChartOptions;
  line: AgCartesianChartOptions;
  area: AgCartesianChartOptions;
  pie: AgPolarChartOptions;
}

export type ChartOptionsFor<T extends ChartConfig<any>> =
  T['type'] extends keyof ChartTypeToOptionsMap
    ? ChartTypeToOptionsMap[T['type']]
    : never;
