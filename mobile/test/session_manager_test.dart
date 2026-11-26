import 'package:flutter_test/flutter_test.dart';
import 'package:smart_diabetes_mobile/core/network/api_client.dart';
import 'package:smart_diabetes_mobile/features/auth/data/auth_models.dart';

void main() {
  late _MemorySessionStorage storage;

  setUp(() {
    storage = _MemorySessionStorage();
  });

  test('remember me stores the refresh token and restores the session', () async {
    final session = SessionManager(storage, baseUrl: 'https://example.test');
    await session.save(_createAuthResponse(), rememberMe: true);

    final restored = await SessionManager(
      storage,
      baseUrl: 'https://example.test',
    ).restore();

    expect(storage.values[SessionManager.refreshTokenKey], 'refresh');
    expect(restored?.accessToken, 'access');
    expect(restored?.role, 'Patient');
  });

  test('disabled remember me keeps refresh token in memory only', () async {
    final session = SessionManager(storage, baseUrl: 'https://example.test');
    await session.save(_createAuthResponse(), rememberMe: false);

    expect(await session.accessToken(), 'access');
    expect(storage.values.containsKey(SessionManager.refreshTokenKey), isFalse);
    expect(
      await SessionManager(
        storage,
        baseUrl: 'https://example.test',
      ).restore(),
      isNull,
    );
    expect(await session.accessToken(), isNull);
  });

  test('logout clears all persisted session values', () async {
    final session = SessionManager(storage, baseUrl: 'https://example.test');
    await session.save(_createAuthResponse());

    await session.clear();

    expect(storage.values, isEmpty);
    expect(await session.accessToken(), isNull);
    expect(await session.role(), isNull);
  });

  test('existing sessions without a remember preference remain restorable',
      () async {
    storage.values[SessionManager.accessTokenKey] = 'access';
    storage.values[SessionManager.refreshTokenKey] = 'refresh';
    storage.values[SessionManager.expiresAtKey] =
        DateTime.now().toUtc().add(const Duration(hours: 1)).toIso8601String();
    storage.values[SessionManager.roleKey] = 'Patient';

    final restored = await SessionManager(
      storage,
      baseUrl: 'https://example.test',
    ).restore();

    expect(restored?.refreshToken, 'refresh');
    expect(restored?.role, 'Patient');
  });
}

AuthResponse _createAuthResponse() => AuthResponse(
      accessToken: 'access',
      refreshToken: 'refresh',
      accessTokenExpiresAt:
          DateTime.now().toUtc().add(const Duration(hours: 1)),
      role: 'Patient',
      accountStatus: 'Active',
    );

class _MemorySessionStorage implements SessionStorage {
  final Map<String, String> values = {};

  @override
  Future<String?> read({required String key}) async => values[key];

  @override
  Future<void> write({required String key, required String value}) async {
    values[key] = value;
  }

  @override
  Future<void> delete({required String key}) async {
    values.remove(key);
  }
}
