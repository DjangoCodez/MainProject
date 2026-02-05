
// API can be found here:
// https://github.com/nervgh/angular-file-upload/wiki/Module-API

export interface IFileUploaderFactoryProvider {
    setSoeParameters(soeParameters: string): void;
    setLanguage(language: string): void;
}

export interface IFileUploaderFactory {
    create(path: string): Promise<any>;
}

export class FileUploaderFactoryProvider implements IFileUploaderFactoryProvider {

    private soeParameters: string;
    private language: string;

    setSoeParameters(soeParameters: string) {
        this.soeParameters = soeParameters;
    }

    setLanguage(language: string) {
        this.language = language;
    }

    //@ngInject
    $get(FileUploader) {
        return new FileUploaderFactoryOidc(FileUploader, this.soeParameters, this.language)
    }
}

export class FileUploaderFactoryOidc implements IFileUploaderFactory {
    constructor(private FileUploader, private soeParameters: string, private language: string) {
    }

    create(path: string): any {
        return new this.FileUploader({
            url: path,
            headers: {
                "soeparameters": this.soeParameters,
                'Accept-Language': this.language
            }
        });
    }
}