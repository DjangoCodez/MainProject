import 'postal';

declare var postal: any;

export interface ISubscriptionDescription {
    event: string;
    callback: (data) => void;
}
export interface IMessagingService {
    publish(event: string, data: any): any;
    subscribe(event: string, callback: (data) => void): ISubscription;
    subscribe(event: string, callback: (data) => void, $scope: ng.IScope): ISubscription;
    subscribeMany(subscription: ISubscriptionDescription[]): ISubscription;
    subscribeMany(subscriptions: ISubscriptionDescription[], $scope: ng.IScope): ISubscription;
}

export class MessagingService implements IMessagingService {
    private channel = postal.channel();

    public publish(event: string, data: any): any {
        return this.channel.publish({
            topic: event,
            data: data
        });
    }

    public subscribe(event: string, callback: (data) => void): ISubscription;
    public subscribe(event: string, callback: (data) => void, $scope?: ng.IScope): ISubscription {
        var subscription = this.channel.subscribe({
            topic: event,
            callback: callback
        });
        if ($scope) {
            var registration = $scope.$on("$destroy", () => {
                subscription.unsubscribe();
                registration();
            });
        }
        return subscription;
    }

    public subscribeMany(subscription: ISubscriptionDescription[]): ISubscription;
    public subscribeMany(subscriptions: ISubscriptionDescription[], $scope?: ng.IScope): ISubscription {
        var subs = new Array<ISubscription>();

        _.forEach(subscriptions, (x) => {
            subs.push(this.channel.subscribe({
                topic: x.event,
                callback: x.callback
            }));
        });

        var s: ISubscription = {
            unsubscribe: () => {
                _.forEach(subs, (x: ISubscription) => { x.unsubscribe() });
            }
        };

        if ($scope) {
            var registration = $scope.$on("$destroy", () => {
                s.unsubscribe();
                registration();
            });

            (<any>s).unsubscribe2 = s.unsubscribe;
            s.unsubscribe = () => {
                (<any>s).unsubscribe2();
                registration();
            }
        }

        return s;
    }
}

export interface ISubscription {
    unsubscribe(): void;
}