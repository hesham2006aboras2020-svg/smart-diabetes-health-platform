import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

import '../../../core/config/app_config.dart';
import '../../../core/network/api_client.dart';
import '../data/auth_models.dart';
import '../data/auth_repository.dart';

enum AuthStatus { signedOut, loading, authenticated, error }

class AuthState {
  const AuthState({
    this.status = AuthStatus.signedOut,
    this.role,
    this.message,
  });

  final AuthStatus status;
  final String? role;
  final String? message;
}

final secureStorageProvider = Provider<FlutterSecureStorage>(
  (ref) => FlutterSecureStorage(),
);

final authRepositoryProvider = Provider<AuthRepository>((ref) {
  final storage = ref.watch(secureStorageProvider);
  final client = ApiClient(
    baseUrl: AppConfig.apiBaseUrl,
    tokenProvider: () => storage.read(key: 'access_token'),
  );
  return AuthRepository(client);
});

final authControllerProvider =
    NotifierProvider<AuthController, AuthState>(AuthController.new);

class AuthController extends Notifier<AuthState> {
  late final FlutterSecureStorage _storage;
  late final AuthRepository _repository;

  @override
  AuthState build() {
    _storage = ref.watch(secureStorageProvider);
    _repository = ref.watch(authRepositoryProvider);
    return const AuthState();
  }

  Future<void> login(String username, String password) async {
    state = const AuthState(status: AuthStatus.loading);
    try {
      final response = await _repository.login(
        username: username.trim(),
        password: password,
      );
      await _saveSession(response);
      state = AuthState(
        status: AuthStatus.authenticated,
        role: response.role,
      );
    } catch (error) {
      state = AuthState(
        status: AuthStatus.error,
        message: _messageFromError(error),
      );
    }
  }

  Future<RegisterResponse?> register({
    required String fullName,
    required String email,
    required String password,
    required String role,
    String? licenseNumber,
    String? specialization,
    String? clinicName,
  }) async {
    state = const AuthState(status: AuthStatus.loading);
    try {
      final response = await _repository.register(
        fullName: fullName,
        email: email,
        password: password,
        role: role,
        licenseNumber: licenseNumber,
        specialization: specialization,
        clinicName: clinicName,
      );
      if (response.authentication != null) {
        await _saveSession(response.authentication!);
        state = AuthState(
          status: AuthStatus.authenticated,
          role: response.authentication!.role,
        );
      } else {
        state = AuthState(message: response.message);
      }
      return response;
    } catch (error) {
      state = AuthState(
        status: AuthStatus.error,
        message: _messageFromError(error),
      );
      return null;
    }
  }

  Future<void> logout() async {
    await _storage.delete(key: 'access_token');
    await _storage.delete(key: 'refresh_token');
    await _storage.delete(key: 'access_token_expires_at');
    await _storage.delete(key: 'role');
    state = const AuthState(status: AuthStatus.signedOut);
  }

  Future<void> _saveSession(AuthResponse response) async {
    await _storage.write(key: 'access_token', value: response.accessToken);
    await _storage.write(key: 'refresh_token', value: response.refreshToken);
    await _storage.write(
      key: 'access_token_expires_at',
      value: response.accessTokenExpiresAt.toIso8601String(),
    );
    await _storage.write(key: 'role', value: response.role);
  }

  String _messageFromError(Object error) {
    if (error is DioException) {
      final data = error.response?.data;
      if (data is Map && data['message'] is String) {
        return data['message'] as String;
      }
      return 'تعذر الاتصال بالخادم. حاول مرة أخرى.';
    }
    return 'حدث خطأ غير متوقع. حاول مرة أخرى.';
  }
}
