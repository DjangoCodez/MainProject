


//Available methods for ImageController

//get
export const getImages = (imageType: number, entity: number, recordId: number, useThumbnails: boolean, projectId: number) => `V2/Core/Image/${imageType}/${entity}/${recordId}/${useThumbnails}/${projectId}`;

//get
export const getImage = (imageId: number) => `V2/Core/Image/${imageId}`;


