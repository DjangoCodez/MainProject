import { Injectable } from '@angular/core';

import { ChartBuilderFn } from '../interfaces/chart-builder.interface';
import { BaseChart } from '../interfaces/chart-type.interface';
import {
  AgCartesianChartOptions,
  AgChartInstance,
  AgCharts,
  AgPolarChartOptions,
} from 'ag-charts-enterprise';

@Injectable({ providedIn: 'root' })
export class ChartRegistryService {
  private charts = new Map<HTMLElement, AgChartInstance>();

  // Registry of builders keyed by chart type
  private builders = new Map<string, ChartBuilderFn<any, any>>();

  constructor() {
    this.registerDefaultCharts();
  }

  /**
   * Register a new chart type dynamically.
   */
  registerChart<TConfig extends BaseChart<any>, TOptions>(
    type: string,
    builder: ChartBuilderFn<TConfig, TOptions>
  ): void {
    this.builders.set(type, builder);
  }

  /**
   * Register built-in charts (bar, line, area, pie)
   */
  private registerDefaultCharts(): void {
    this.registerChart<
      BaseChart<any> & { xKey: string; yKey: string },
      AgCartesianChartOptions
    >('bar', config => ({
      title: { text: config.title },
      data: config.data,
      series: [{ type: 'bar', xKey: config.xKey, yKey: config.yKey }],
    }));

    this.registerChart<
      BaseChart<any> & { xKey: string; yKey: string },
      AgCartesianChartOptions
    >('line', config => ({
      title: { text: config.title },
      data: config.data,
      series: [{ type: 'line', xKey: config.xKey, yKey: config.yKey }],
    }));

    this.registerChart<
      BaseChart<any> & { xKey: string; yKey: string },
      AgCartesianChartOptions
    >('area', config => ({
      title: { text: config.title },
      data: config.data,
      series: [{ type: 'area', xKey: config.xKey, yKey: config.yKey }],
    }));

    this.registerChart<
      BaseChart<any> & { angleKey: string; labelKey: string },
      AgPolarChartOptions
    >('pie', config => ({
      title: { text: config.title },
      data: config.data,
      series: [
        { type: 'pie', angleKey: config.angleKey, labelKey: config.labelKey },
      ],
    }));
  }

  /**
   * Build chart options dynamically
   */
  build<TConfig>(
    type: string,
    config: TConfig
  ): AgCartesianChartOptions | AgPolarChartOptions {
    const builder = this.builders.get(type);
    if (!builder) throw new Error(`Chart type "${type}" is not registered.`);
    return builder(config);
  }

  /**
   * Create chart instance
   */
  createChart<TConfig>(
    container: HTMLElement,
    type: string,
    config: TConfig
  ): AgChartInstance {
    const options = this.build(type, config);
    const chart = AgCharts.create({ ...options, container });
    this.charts.set(container, chart);
    return chart;
  }

  /**
   * Update chart instance
   */
  updateChart<TConfig>(
    container: HTMLElement,
    type: string,
    config: TConfig
  ): void {
    const chart = this.charts.get(container);
    if (!chart) throw new Error('Chart not found for container.');
    const options = this.build(type, config);
    chart.update(options);
  }

  /**
   * Destroy chart instance
   */
  destroyChart(container: HTMLElement): void {
    const chart = this.charts.get(container);
    if (chart) {
      chart.destroy();
      this.charts.delete(container);
    }
  }
}
