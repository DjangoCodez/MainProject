


//Available methods for UnionFeeController

//get
export const getUnionFeesGrid = (unionFeeId?: number) => `V2/Time/UnionFee/Grid/${unionFeeId || ''}`;

//get
export const getUnionFee = (unionFeeId: number) => `V2/Time/UnionFee/${unionFeeId}`;

//post, takes args: (model: number)
export const saveUnionFee = () => `V2/Time/UnionFee`;

//delete
export const deleteUnionFee = (unionFeeId: number) => `V2/Time/UnionFee/${unionFeeId}`;

//get
export const getPayrollPriceTypesDict = () => `V2/Time/UnionFee/PayrollPriceTypesDict`;

//get
export const getUnionFeePayrollProducts = () => `V2/Time/UnionFee/UnionFeePayrollProducts`;


