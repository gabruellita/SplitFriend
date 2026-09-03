import axios from 'axios';
import { axiosClient } from './axiosClient';
import { API_ENDPOINTS } from './endpoints';
import type {
  LoginRequest, LoginResponse,
  RegisterRequest, RegisterResponse,
  ConfirmEmailRequest,
  MeResponse,
  UpdateProfileRequest,
  ChangePasswordRequest,
  ForgotPasswordRequest,
  ResetPasswordRequest,
} from '@/types/auth.types';

export const authApi = {
  login: async (payload: LoginRequest): Promise<LoginResponse> => {
    const { data } = await axiosClient.post<LoginResponse>(API_ENDPOINTS.AUTH.LOGIN, payload);
    return data;
  },

  register: async (payload: RegisterRequest): Promise<RegisterResponse> => {
    const { data } = await axiosClient.post<RegisterResponse>(API_ENDPOINTS.AUTH.REGISTER, payload);
    return data;
  },

  confirmEmail: async (payload: ConfirmEmailRequest): Promise<{ message: string }> => {
    const { data } = await axiosClient.post(API_ENDPOINTS.AUTH.CONFIRM_EMAIL, payload);
    return data;
  },

  /**
   * Uses plain axios (not axiosClient) to avoid the 401-refresh interceptor loop.
   */
  refresh: async (refreshToken: string): Promise<LoginResponse> => {
    // Uses plain axios (not axiosClient) to avoid the 401-refresh interceptor loop.
    const { data } = await axios.post<LoginResponse>(
      `${import.meta.env.VITE_API_BASE_URL ?? ''}${API_ENDPOINTS.AUTH.REFRESH}`,
      { refreshToken }
    );
    return data;
  },

  logout: async (refreshToken: string): Promise<void> => {
    await axiosClient.post(API_ENDPOINTS.AUTH.LOGOUT, { refreshToken }).catch(() => {});
  },

  getMe: async (): Promise<MeResponse> => {
    const { data } = await axiosClient.get<MeResponse>(API_ENDPOINTS.AUTH.ME);
    return data;
  },

  updateProfile: async (payload: UpdateProfileRequest): Promise<MeResponse> => {
    const { data } = await axiosClient.patch<MeResponse>(API_ENDPOINTS.ME.BASE, payload);
    return data;
  },

  changePassword: async (payload: ChangePasswordRequest): Promise<void> => {
    await axiosClient.post(API_ENDPOINTS.ME.CHANGE_PASSWORD, payload);
  },

  forgotPassword: async (payload: ForgotPasswordRequest): Promise<{ message: string }> => {
    const { data } = await axiosClient.post(API_ENDPOINTS.AUTH.FORGOT_PASSWORD, payload);
    return data;
  },

  resetPassword: async (payload: ResetPasswordRequest): Promise<{ message: string }> => {
    const { data } = await axiosClient.post(API_ENDPOINTS.AUTH.RESET_PASSWORD, payload);
    return data;
  },
};
