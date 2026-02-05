import { IAuthenticationResponseDTO } from '@shared/models/generated-interfaces/AuthenticationResponseDTO';

export class AuthenticationFormData implements IAuthenticationResponseDTO {
  signatoryContractAuthenticationRequestId: number = 0;
  username: string = '';
  password: string = '';
  code: string = '';
}
