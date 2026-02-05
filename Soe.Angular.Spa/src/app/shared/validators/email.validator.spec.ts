import { EmailValidator } from './email.validator';

describe('EmailValidator', () => {
  it('should be OK to have an empty email', () => {
    expect(EmailValidator.isValid('')).toBe(true);
  });
  it('test email with no @', () => {
    expect(EmailValidator.isValid('test.com')).toBe(false);
    expect(EmailValidator.isValid('aaa')).toBe(false);
    expect(EmailValidator.isValid('test email.se')).toBe(false);
  });
  it('test email with no .', () => {
    expect(EmailValidator.isValid('test@com')).toBe(false);
    expect(EmailValidator.isValid('test@aaa')).toBe(false);
    expect(EmailValidator.isValid('test@se')).toBe(false);
  });
  it('test email with no domain', () => {
    expect(EmailValidator.isValid('test@.com')).toBe(false);
    expect(EmailValidator.isValid('test@.se')).toBe(false);
  });
  it('test email with no user', () => {
    expect(EmailValidator.isValid('@com')).toBe(false);
    expect(EmailValidator.isValid('@se')).toBe(false);
  });
  it('test email with no user and no domain', () => {
    expect(EmailValidator.isValid('@.com')).toBe(false);
    expect(EmailValidator.isValid('@.se')).toBe(false);
  });
  it('prevents one letter main domains', () => {
    expect(EmailValidator.isValid('test@test.c')).toBe(false);
    expect(EmailValidator.isValid('test@test.s')).toBe(false);
  });
  it('test normal emails', () => {
    expect(EmailValidator.isValid('test.test@email.com')).toBe(true);
    expect(EmailValidator.isValid('test@email.com')).toBe(true);
    expect(EmailValidator.isValid('test@test.test.test.com')).toBe(true);
  });
  it('other invalid emails', () => {
    expect(EmailValidator.isValid('plainaddress')).toBe(false);
    expect(EmailValidator.isValid('#@%^%#$@#$@#.com')).toBe(false);
    expect(EmailValidator.isValid('@example.com')).toBe(false);
    expect(EmailValidator.isValid('Joe Smith <email@example.com>')).toBe(false);
    expect(EmailValidator.isValid('email.example.com')).toBe(false);
    expect(EmailValidator.isValid('email.@example.com')).toBe(false);
    expect(EmailValidator.isValid('あいうえお@example.com')).toBe(false);
    expect(EmailValidator.isValid('email@example.com (Joe Smith)')).toBe(false);
    expect(EmailValidator.isValid('email@example')).toBe(false);
    expect(EmailValidator.isValid('email@-example.com')).toBe(false);
    expect(EmailValidator.isValid('email@example..com')).toBe(false);
    expect(EmailValidator.isValid('test..test@email.com')).toBe(false);
    expect(EmailValidator.isValid('email"@example.com')).toBe(false);
  });
  it('other valid emails', () => {
    // Source: https://gist.github.com/cjaoude/fd9910626629b53c4d25
    expect(EmailValidator.isValid('email@example.com')).toBe(true);
    expect(EmailValidator.isValid('firstname.lastname@example.com')).toBe(true);
    expect(EmailValidator.isValid('email@subdomain.example.com')).toBe(true);
    expect(EmailValidator.isValid('firstname+lastname@example.com')).toBe(true);
    expect(EmailValidator.isValid('email@[123.123.123.123]')).toBe(true);
    expect(EmailValidator.isValid('"email"@example.com')).toBe(true);
    expect(EmailValidator.isValid('"email@example.com')).toBe(true);
    expect(EmailValidator.isValid('1234567890@example.com')).toBe(true);
    expect(EmailValidator.isValid('email@example-one.com')).toBe(true);
    expect(EmailValidator.isValid('_______@example.com')).toBe(true);
    expect(EmailValidator.isValid('email@example.name')).toBe(true);
    expect(EmailValidator.isValid('email@example.museum')).toBe(true);
    expect(EmailValidator.isValid('email@example.co.jp')).toBe(true);
    expect(EmailValidator.isValid('firstname-lastname@example.com')).toBe(true);
  });
});
