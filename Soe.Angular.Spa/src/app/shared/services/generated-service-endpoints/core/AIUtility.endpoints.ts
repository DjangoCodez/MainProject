


//Available methods for AIUtilityController

//get
export const getTranslationSuggestions = (originalText: string, languages: string) => `V2/Core/AI-utility/translations?originalText=${encodeURIComponent(originalText)}&languages=${encodeURIComponent(languages)}`;

//get
export const getProfessionalizedText = (text: string) => `V2/Core/AI-utility/professionalized?text=${encodeURIComponent(text)}`;


