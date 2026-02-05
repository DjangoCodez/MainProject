import { IPurchaseService } from "../../Purchase/Purchase/PurchaseService";


export class DeliveryAddressesPurchaseController {
    private noAddresses = false;
    private addresses: string[] = [];

    //@ngInject
    constructor(
        private $uibModalInstance,
        purchaseService: IPurchaseService,
        customerOrderId: number) {
        purchaseService.getDeliveryAddresses(customerOrderId)
            .then(data => {
                console.log("data", data)
                this.addresses = data;
                this.noAddresses = (this.addresses.length === 0);
            })
    }

    private select(address: string) {
        this.close(address)
    }

    private buttonCancelClick() {
        this.close(undefined);
    }

    close(result: any) {
        if (!result) {
            this.$uibModalInstance.dismiss('cancel');
        }
        else {
            this.$uibModalInstance.close(result);
        }
    }
}
