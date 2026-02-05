import { Account, AccountExended } from "../tests/fixtures/accounts";


export const getAccountValue = (account: Account, key: string) => {
    let val: string = '';

    if (account.domain) {
        val = account.domain;
    }
    if (account.user) {
        val = val + '_' + account.user;
    }
    return process.env[val + '_' + key];

};

export const getAccountExValue = (accountEx: AccountExended, key: string) => {
    let val: string = '';

    if (accountEx.domain) {
        val = accountEx.domain;
    }
    if (accountEx.user) {
        val = val + '_' + accountEx.user;
    }
    return process.env[val + '_' + key];

};

export const getDomainValue = (account: Account, key: string) => {
    let val: string = '';

    if (account.domain) {
        val = account.domain;
    }
    return process.env[val + '_' + key];

};

export const getEnvironmentValue = (key: string) => {
    return process.env[key];
};

export const getPages = (runVersion: string): string[] => {
    const value = process.env[runVersion + '_pages'];
    return value ? value.split(',') : [];
};