export class ModalUtility {

    static MODAL_CANCEL = 'cancel';
    static MODAL_ESCAPE_KEY = 'escape key press';

    static MODAL_SKIP_CONFIRM = 'SKIP_CONFIRM';

    static handleModalClose(reason): boolean {
        if (reason && reason !== this.MODAL_CANCEL && reason !== this.MODAL_ESCAPE_KEY) {
            console.error(reason);
            return false;
        }

        return true;
    }
}