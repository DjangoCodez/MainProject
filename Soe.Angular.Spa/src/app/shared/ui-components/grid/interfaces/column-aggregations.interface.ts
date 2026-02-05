import { StringKeyOfNumberProperty } from "@shared/types";

export enum AggregationType {
    Sum = 1,
    Max = 2,
    Min = 3
}

export type ISoeAggregationConfig<T> = {
    [field in StringKeyOfNumberProperty<T>]?: AggregationType;
};

export type ISoeAggregationResult<T> = {
    [field in StringKeyOfNumberProperty<T>]?: number;
};