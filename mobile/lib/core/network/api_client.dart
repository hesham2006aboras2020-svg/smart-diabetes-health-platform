import 'package:dio/dio.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

import '../../features/auth/data/auth_models.dart';

class SessionManager {
  SessionManager(this._storage, {required this.baseUrl});

  static const accessTokenKey = 'access_token';
  static const refreshTokenKey = 'refresh_token';
  static const expiresAtKey = 'access_token_expires_at';
  static const roleKey = 'role';

  final FlutterSecureStorage _storage;
  final String baseUrl;

  Future<void> save(AuthResponse response) async {
    await _storage.write(key: accessTokenKey, value: response.accessToken);
    await _storage.write(key: refreshTokenKey, value: response.refreshToken);
    await _storage.write(
      key: expiresAtKey,
      value: response.accessTokenExpiresAt.toUtc().toIso8601String(),
    );
    await _storage.write(key: roleKey, value: response.role);
  }

  Future<String?> accessToken() => _storage.read(key: accessTokenKey);

  Future<String?> role() => _storage.read(key: roleKey);

  Future<AuthResponse?> restore() async {
    final refreshToken = await _storage.read(key: refreshTokenKey);
    if (refreshToken == null || refreshToken.isEmpty) {
      return null;
    }

    final accessToken = await _storage.read(key: accessTokenKey);
    final expiresAtText = await _storage.read(key: expiresAtKey);
    if (accessToken != null &&
        accessToken.isNotEmpty &&
        expiresAtText != null) {
      final expiresAt = DateTime.tryParse(expiresAtText);
      if (expiresAt != null &&
          expiresAt.isAfter(
            DateTime.now().toUtc().add(const Duration(seconds: 30)),
          )) {
        final savedRole = await role();
        if (savedRole != null && savedRole.isNotEmpty) {
          return AuthResponse(
            accessToken: accessToken,
            refreshToken: refreshToken,
            accessTokenExpiresAt: expiresAt,
            role: savedRole,
            accountStatus: 'Active',
          );
        }
      }
    }

    return refresh();
  }

  Future<AuthResponse> refresh() async {
    final refreshToken = await _storage.read(key: refreshTokenKey);
    if (refreshToken == null || refreshToken.isEmpty) {
      throw StateError('No refresh token is available.');
    }

    final client = Dio(
      BaseOptions(
        baseUrl: baseUrl,
        connectTimeout: const Duration(seconds: 15),
        receiveTimeout: const Duration(seconds: 20),
        headers: const {'Content-Type': 'application/json'},
      ),
    );

    try {
      final response = await client.post<Map<String, dynamic>>(
        '/api/auth/refresh',
        data: {'refreshToken': refreshToken},
      );
      final auth = AuthResponse.fromJson(response.data!);
      await save(auth);
      return auth;
    } finally {
      client.close(force: true);
    }
  }

  Future<void> clear() async {
    await _storage.delete(key: accessTokenKey);
    await _storage.delete(key: refreshTokenKey);
    await _storage.delete(key: expiresAtKey);
    await _storage.delete(key: roleKey);
  }
}

class ApiClient {
  ApiClient({
    required this.baseUrl,
    required this.sessionManager,
  }) : dio = Dio(
          BaseOptions(
            baseUrl: baseUrl,
            connectTimeout: const Duration(seconds: 15),
            receiveTimeout: const Duration(seconds: 20),
            headers: const {'Content-Type': 'application/json'},
          ),
        ) {
    dio.interceptors.add(
      InterceptorsWrapper(
        onRequest: (options, handler) async {
          final token = await sessionManager.accessToken();
          if (token != null && token.isNotEmpty) {
            options.headers['Authorization'] = 'Bearer $token';
          }
          handler.next(options);
        },
        onError: (error, handler) async {
          final request = error.requestOptions;
          final isUnauthorized = error.response?.statusCode == 401;
          final isRefreshRequest = request.path == '/api/auth/refresh';
          final alreadyRetried = request.extra['authRetry'] == true;

          if (!isUnauthorized || isRefreshRequest || alreadyRetried) {
            handler.next(error);
            return;
          }

          final accessToken = await _refreshAccessToken();
          if (accessToken == null || accessToken.isEmpty) {
            await sessionManager.clear();
            onSessionExpired?.call();
            handler.next(error);
            return;
          }

          try {
            request.extra['authRetry'] = true;
            request.headers['Authorization'] = 'Bearer $accessToken';
            final response = await dio.fetch(request);
            handler.resolve(response);
          } catch (_) {
            handler.next(error);
          }
        },
      ),
    );
  }

  final String baseUrl;
  final SessionManager sessionManager;
  final Dio dio;

  Future<void> Function()? onSessionExpired;
  Future<String?>? _refreshFuture;

  Future<String?> _refreshAccessToken() async {
    final existing = _refreshFuture;
    if (existing != null) return existing;

    final future = _performRefresh();
    _refreshFuture = future;
    try {
      return await future;
    } finally {
      if (identical(_refreshFuture, future)) {
        _refreshFuture = null;
      }
    }
  }

  Future<String?> _performRefresh() async {
    try {
      final response = await sessionManager.refresh();
      return response.accessToken;
    } catch (_) {
      return null;
    }
  }
}
