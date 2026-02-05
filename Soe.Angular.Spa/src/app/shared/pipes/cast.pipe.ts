import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'cast',
  pure: true,
})
export class CastPipe implements PipeTransform {
  transform<T>(input: unknown, baseItem: T | undefined): T {
    return input as unknown as T;
  }
}
