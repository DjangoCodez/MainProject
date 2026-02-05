


//Available methods for TextBlockDialogController

//get
export const getTextBlock = (textBlockId: number) => `V2/Core/TextBlock/${textBlockId}`;

//get
export const getTextBlocks = (entity: number, textBlockId?: number) => `V2/Core/TextBlocks/${entity}/${textBlockId || ''}`;

//post, takes args: (textBlockModel: number)
export const saveTextBlock = () => `V2/Core/TextBlock`;

//delete
export const deleteTextBlock = (textBlockId: number) => `V2/Core/TextBlock/${textBlockId}`;


