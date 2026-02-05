


//Available methods for LiquidityPlanningController

//post, takes args: (model: number)
export const getLiquidityPlanning = () => `V2/Economy/LiquidityPlanning/LiquidityPlanning/Get`;

//post, takes args: (model: number)
export const getLiquidityPlanningv2 = () => `V2/Economy/LiquidityPlanning/LiquidityPlanning/Get/new`;

//post, takes args: (model: number)
export const saveLiquidityPlanningTransaction = () => `V2/Economy/LiquidityPlanning/LiquidityPlanning`;

//delete
export const deleteLiquidityPlanningTransaction = (liquidityPlanningTransactionId: number) => `V2/Economy/LiquidityPlanning/LiquidityPlanning/${liquidityPlanningTransactionId}`;


