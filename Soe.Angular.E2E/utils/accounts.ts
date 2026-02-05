import path from 'path';

export type Account = {
	userName: string;
	password: string;
	domain: string;
};

export type AccountExended = Account & {
	loginUrl: string;
	baseUrl: string;
	storageState: string;
};

export const getBaseUrl = (dom) => {
	const env = process.env.ENVIRONMENT;

	if (env === 'local') return `https://main.softone.se/`;

	if (env === 'dev') return `https://dev${dom}.softone.se/`;

	if (env === 'stage') return `https://stage${dom}.softone.se/`;
};

export const get_storage_state = (projectName = 'auth') => {
	return path.join(process.cwd(), `.auth/${projectName}.json`);
};

export const setDefault = (account): AccountExended => {
	return {
		...account,
		baseUrl: getBaseUrl(account.domain),
		storageState: get_storage_state(account.userName),
		loginUrl: process.env.LOGIN_URL
	};
};

export const defaultAccount: Account = {
	userName: '10@931',
	password: 'Sommar2021',
	domain: 's1d1'
};
