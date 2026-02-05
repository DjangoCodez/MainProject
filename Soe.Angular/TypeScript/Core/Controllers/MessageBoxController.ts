import { ITranslationService } from "../Services/TranslationService";
import { StringUtility } from "../../Util/StringUtility";
import { SOEMessageBoxImage, SOEMessageBoxButtons, SOEMessageBoxButton } from "../../Util/Enumerations";

export class MessageBoxController {

    // Icons
    showIconInfo: boolean = false;
    showIconWarning: boolean = false;
    showIconError: boolean = false;
    showIconQuestion: boolean = false;
    showIconForbidden: boolean = false;
    showIconOk: boolean = false;
    showCustomIcon: boolean = false;
    noIcon: boolean = false;

    // Buttons
    buttonOkLabel: string = '';
    buttonYesLabel: string = '';
    buttonNoLabel: string = '';
    buttonCancelLabel: string = '';
    buttonCancelAllLabel: string = '';

    showButtonOk: boolean = false;
    showButtonYes: boolean = false;
    showButtonNo: boolean = false;
    showButtonCancel: boolean = false;

    focusValue: string;

    // Text
    showHiddenText: boolean = false;

    private get okDisabled(): boolean {
        return this.useTextValidation && !this.textBoxValue;
    }

    //@ngInject
    constructor(private $uibModalInstance,
        private translationService: ITranslationService,
        private title: string,
        private text: string,
        private hiddenText: string,
        private image: SOEMessageBoxImage,
        private buttons: SOEMessageBoxButtons,
        private customIcon: string,
        isFromGrid: boolean,
        private showCheckBox: boolean,
        private checkBoxLabel: string,
        private isChecked: boolean,
        private showTextBox: boolean,
        private textBoxLabel: string,
        private textBoxValue: string,
        private textBoxRows: number,
        private textBoxType: string,
        private showDatePicker: boolean,
        private datePickerLabel: string,
        private datePickerValue: Date,
        private showButtonCancelAll: boolean,
        private buttonOkLabelKey: string,
        private buttonYesLabelKey: string,
        private buttonNoLabelKey: string,
        private buttonCancelLabelKey: string,
        private buttonCancelAllLabelKey: string,
        initialFocusButton: SOEMessageBoxButton = SOEMessageBoxButton.None,
        private returnNullIfCancel: boolean = false,
        private useTextValidation: boolean = false) {

        if (!this.textBoxType)
            this.textBoxType = 'text';

        if (this.datePickerValue)
            this.datePickerValue = new Date(<any>this.datePickerValue);

        this.setupLabels();
        this.setupImage();
        this.setupButtons();

        // Replace \n with <br/>
        this.text = StringUtility.ToBr(text);
        this.hiddenText = StringUtility.ToBr(hiddenText);

        if (isFromGrid) { //the grid tries to steal focus back to itself if you trigger a dialog during edit.
            $uibModalInstance.rendered.then(() => setTimeout(() => {//we use setTimeout since this has nothing to do with angular.
                //Hack, better to send configuration variable instead
                this.setInitialButtonFocus(buttons, initialFocusButton);
            }));
        } else {
            $uibModalInstance.rendered.then(() => {
                this.setInitialButtonFocus(buttons, initialFocusButton);
            });
        }
    }

    // SETUP
    private setInitialButtonFocus(buttons: SOEMessageBoxButtons, initialFocusButton: SOEMessageBoxButton) {
        if (this.showTextBox || this.showDatePicker) {
            return;
        }

        // If only showing the OK button, set focus to it
        if (this.buttons === SOEMessageBoxButtons.OK) {
            this.focusValue = "ok";
            return;
        }

        if (initialFocusButton == SOEMessageBoxButton.First) {

            //This code set focus on first element in dialog
            var inputs = angular.element('.messagebox input');
            var focus = null;

            if ((inputs && inputs.length))
                focus = inputs[0];

            if (!focus) {
                var buttonElements = angular.element('.messagebox button');
                if ((buttonElements && buttonElements.length))
                    focus = buttonElements[0];
            }

            if (focus)
                angular.element(focus).focus();

            return;
        }
        switch (initialFocusButton) {
            case SOEMessageBoxButton.OK:
                this.focusValue = "ok";
                break;
            case SOEMessageBoxButton.Cancel:
                this.focusValue = "cancel";
                break;
            case SOEMessageBoxButton.Yes:
                this.focusValue = "yes";
                break;
            case SOEMessageBoxButton.No:
                this.focusValue = "no";
                break;
            case SOEMessageBoxButton.CancelAll:
                this.focusValue = "cancelAll";
                break;
        }
    }

    setupLabels() {
        var keys: string[] = [];

        if (!this.buttonOkLabelKey)
            this.buttonOkLabelKey = "core.ok";
        if (!this.buttonYesLabelKey)
            this.buttonYesLabelKey = "core.yes";
        if (!this.buttonNoLabelKey)
            this.buttonNoLabelKey = "core.no";
        if (!this.buttonCancelLabelKey)
            this.buttonCancelLabelKey = "core.cancel";
        if (!this.buttonCancelAllLabelKey)
            this.buttonCancelAllLabelKey = "core.cancelall";

        keys.push(this.buttonOkLabelKey);
        keys.push(this.buttonYesLabelKey);
        keys.push(this.buttonNoLabelKey);
        keys.push(this.buttonCancelLabelKey);
        keys.push(this.buttonCancelAllLabelKey);

        this.translationService.translateMany(keys).then(terms => {
            this.buttonOkLabel = terms[this.buttonOkLabelKey];
            this.buttonYesLabel = terms[this.buttonYesLabelKey];
            this.buttonNoLabel = terms[this.buttonNoLabelKey];
            this.buttonCancelLabel = terms[this.buttonCancelLabelKey];
            this.buttonCancelAllLabel = terms[this.buttonCancelAllLabelKey];
        });
    }

    setupImage() {
        switch (this.image) {
            case SOEMessageBoxImage.Information:
                this.showIconInfo = true;
                break;
            case SOEMessageBoxImage.Warning:
                this.showIconWarning = true;
                break;
            case SOEMessageBoxImage.Error:
                this.showIconError = true;
                break;
            case SOEMessageBoxImage.Question:
                this.showIconQuestion = true;
                break;
            case SOEMessageBoxImage.Forbidden:
                this.showIconForbidden = true;
                break;
            case SOEMessageBoxImage.OK:
                this.showIconOk = true;
                break;
            case SOEMessageBoxImage.Custom:
                this.showCustomIcon = true;
                break;
            default:
                this.noIcon = true;
                break;
        }
    }

    setupButtons() {
        switch (this.buttons) {
            case SOEMessageBoxButtons.OK:
                this.showButtonOk = true;
                break;
            case SOEMessageBoxButtons.OKCancel:
                this.showButtonOk = true;
                this.showButtonCancel = true;
                break;
            case SOEMessageBoxButtons.YesNo:
                this.showButtonYes = true;
                this.showButtonNo = true;
                break;
            case SOEMessageBoxButtons.YesNoCancel:
                this.showButtonYes = true;
                this.showButtonNo = true;
                this.showButtonCancel = true;
                break;
        }
    }

    // EVENTS

    private doubleClickCount: number = 0;
    private iconDoubleClick() {
        if (!this.hiddenText)
            return;

        this.doubleClickCount++;
        if (this.doubleClickCount >= 2) {
            this.showHiddenText = true;
            this.doubleClickCount = 0;
        }
    }

    buttonOkClick() {
        if (this.showCheckBox || this.showTextBox || this.showDatePicker)
            this.$uibModalInstance.close({ result: true, isChecked: this.isChecked, textBoxValue: this.textBoxValue, datePickerValue: this.datePickerValue });
        else
            this.$uibModalInstance.close(true);
    }

    buttonYesClick() {
        if (this.showCheckBox || this.showTextBox || this.showDatePicker)
            this.$uibModalInstance.close({ result: true, isChecked: this.isChecked, textBoxValue: this.textBoxValue, datePickerValue: this.datePickerValue });
        else
            this.$uibModalInstance.close(true);
    }

    buttonNoClick() {
        if (this.showCheckBox || this.showTextBox || this.showDatePicker)
            this.$uibModalInstance.close({ result: false, isChecked: this.isChecked, textBoxValue: this.textBoxValue, datePickerValue: this.datePickerValue });
        else
            this.$uibModalInstance.close(false);
    }

    buttonCancelClick() {
        if (this.returnNullIfCancel)
            this.$uibModalInstance.close(null);
        else
            this.$uibModalInstance.dismiss('cancel');
    }

    buttonCancelAllClick() {
        this.$uibModalInstance.dismiss('cancelAll');
    }
}