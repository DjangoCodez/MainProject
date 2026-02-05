


//Available methods for TrackChangesController

//get
export const getTrackChangesLog = (entity: number, recordId: number, dateFromString: string, dateToString: string) => `V2/Core/TrackChanges/TrackChangesLog/${entity}/${recordId}/${encodeURIComponent(dateFromString)}/${encodeURIComponent(dateToString)}`;


