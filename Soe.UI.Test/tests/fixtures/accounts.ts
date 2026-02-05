import path from 'path';
import { getAccountValue, getDomainValue } from '../../utils/properties';

export type Account = {
	domain: string;
	user: string
};

export type AccountExended = Account & {
	loginUrl: string;
	baseUrl: string;
	storageState: string;
};


export const get_storage_state = (projectName = 'auth') => {
	return path.join(process.cwd(), `.auth/${projectName}.json`);
};

export const setDefault = (account: Account, id: number): AccountExended => {
	return {
		...account,
		baseUrl: getDomainValue(account, 'url')?.toString() ?? '',
		storageState: get_storage_state(getAccountValue(account, 'username')?.toString() ?? '' + id),
		loginUrl: process.env.URL ?? ''
	};
};

export const defaultAccount: Account = {
	domain: process.env.defualt_domain!,
	user: process.env.default_user!
};

