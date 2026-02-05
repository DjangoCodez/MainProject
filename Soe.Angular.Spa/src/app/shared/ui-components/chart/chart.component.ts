import {
  AfterViewInit,
  Component,
  ElementRef,
  Input,
  OnChanges,
  OnDestroy,
  SimpleChanges,
  ViewChild,
} from '@angular/core';
import { ChartRegistryService } from './util/chart-registry.service';

export interface ChartConfigBase {
  type: string; // dynamic type
  title?: string;
  data: any[];
}

// Simple debounce utility
function debounce<F extends (...args: any[]) => void>(func: F, wait = 150) {
  let timeout: ReturnType<typeof setTimeout>;
  return (...args: Parameters<F>) => {
    clearTimeout(timeout);
    timeout = setTimeout(() => func(...args), wait);
  };
}

@Component({
  selector: 'soe-chart',
  standalone: true,
  template: `<div #chartContainer class="w-full h-full"></div>`,
})
export class ChartComponent<T extends ChartConfigBase>
  implements AfterViewInit, OnChanges, OnDestroy
{
  @Input() config!: T;
  @ViewChild('chartContainer', { static: true })
  chartContainer!: ElementRef<HTMLDivElement>;

  private initialized = false;
  private resizeObserver?: ResizeObserver;

  constructor(private chartRegistry: ChartRegistryService) {}

  ngAfterViewInit(): void {
    if (!this.config) return;
    const container = this.chartContainer.nativeElement;

    // Create chart via registry
    this.chartRegistry.createChart(container, this.config.type, this.config);
    this.initialized = true;

    // Debounced resize observer
    const debouncedUpdate = debounce(() => {
      if (this.initialized) {
        this.chartRegistry.updateChart(
          container,
          this.config.type,
          this.config
        );
      }
    }, 200);

    this.resizeObserver = new ResizeObserver(debouncedUpdate);
    this.resizeObserver.observe(container);
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (
      this.initialized &&
      changes['config'] &&
      !changes['config'].firstChange
    ) {
      const container = this.chartContainer.nativeElement;
      this.chartRegistry.updateChart(container, this.config.type, this.config);
    }
  }

  ngOnDestroy(): void {
    const container = this.chartContainer?.nativeElement;
    if (container) {
      this.chartRegistry.destroyChart(container);
    }
    this.resizeObserver?.disconnect();
  }
}
