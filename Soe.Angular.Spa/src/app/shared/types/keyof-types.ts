/** From https://stackoverflow.com/questions/50851263/how-do-i-require-a-keyof-to-be-for-a-property-of-a-specific-type */
type PickProperties<T, TFilter> = { [K in keyof T as (T[K] extends TFilter ? K : never)]: T[K] };

/** KeyOf but castable to string */
type StringKey<T> = Extract<keyof T, string>;

/** The name of a number property */
export type StringKeyOfNumberProperty<T> = StringKey<PickProperties<T, number | undefined>>;

/** The name of a string property */
export type StringKeyOfStringProperty<T> = StringKey<PickProperties<T, string | undefined>>;