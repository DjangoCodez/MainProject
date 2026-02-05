import { FullConfig } from '@playwright/test';
import { readdirSync, rmSync } from 'fs';
import path from 'path';

export default async function globalTeardown(pwConfig: FullConfig) {
	//Remove existing auth state
	const authPath = path.join(process.cwd(), `.auth`);
	try {
		readdirSync(authPath).forEach((f) => rmSync(`${authPath}/${f}`));
	} catch (ex) {
		console.log("No .auth file.")
	}
}
