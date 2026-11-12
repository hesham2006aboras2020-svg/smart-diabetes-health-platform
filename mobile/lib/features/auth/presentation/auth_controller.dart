import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

import '../../../core/config/app_config.dart';
import '../../../core/network/api_client.dart';
import '../data/auth_models.dart';
import '../data/auth_repository.dart';

enum AuthStatus { loading, signedOut, authenticated, error }

class AuthState {
  const AuthState({
    this.status = AuthStatus.loading,
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

final sessionManagerProvider = Provider<SessionManager>((ref) {
  return SessionManager(
    ref.watch(secureStorageProvider),
    baseUrl: AppConfig.apiBaseUrl,
  );
});

final authRepositoryProvider = Provider<AuthRepository>((ref) {
  final session = ref.watch(sessionManagerProvider);
  final client = ApiClient(
    baseUrl: AppConfig.apiBaseUrl,
    sessionManager: session,
  );
  return AuthRepository(client);
});

final authControllerProvider =
    NotifierProvider<AuthController, AuthState>(AuthController.new);

class AuthController extends Notifier<AuthState> {
  late final SessionManager _session;
  late final AuthRepository _repository;
  bool _restoreStarted = false;

  @override
  AuthState build() {
    _session = ref.watch(sessionManagerProvider);
    _repository = ref.watch(authRepositoryProvider);
    _repository.setSessionExpiredHandler(markSignedOut);

    if (!_restoreStarted) {
      _restoreStarted = true;
      Future<void>.microtask(_restoreSession);
    }

    return const AuthState();
  }

  Future<void> _restoreSession() async {
    try {
      final response = await _session.restore();
      if (response == null) {
        state = const AuthState(status: AuthStatus.signedOut);
        return;
      }

      state = AuthState(
        status: AuthStatus.authenticated,
        role: response.role,
      );
    } catch (_) {
      await _session.clear();
      state = const AuthState(status: AuthStatus.signedOut);
    }
  }

  Future<void> login(String username, String password) async {
    state = const AuthState(status: AuthStatus.loading);
    try {
      final response = await _repository.login(
        username: username.trim(),
        password: password,
      );
      await _session.save(response);
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
        await _session.save(response.authentication!);
        state = AuthState(
          status: AuthStatus.authenticated,
          role: response.authentication!.role,
        );
      } else {
        state = AuthState(
          status: AuthStatus.signedOut,
          message: response.message,
        );
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
    await _session.clear();
    state = const AuthState(status: AuthStatus.signedOut);
  }

  Future<void> markSignedOut() async {
    await _session.clear();
    if (state.status != AuthStatus.signedOut) {
      state = const AuthState(
        status: AuthStatus.signedOut,
        message: 'انتهت الجلسة. يرجى تسجيل الدخول مرة أخرى.',
      );
    }
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
