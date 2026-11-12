import '../../../core/network/api_client.dart';
import 'auth_models.dart';

class AuthRepository {
  AuthRepository(this._client);

  final ApiClient _client;

  Future<AuthResponse> login({
    required String username,
    required String password,
  }) async {
    final response = await _client.dio.post<Map<String, dynamic>>(
      '/api/auth/login',
      data: {'username': username, 'password': password},
    );
    return AuthResponse.fromJson(response.data!);
  }

  Future<RegisterResponse> register({
    required String fullName,
    required String email,
    required String password,
    required String role,
    String? licenseNumber,
  }) async {
    final response = await _client.dio.post<Map<String, dynamic>>(
      '/api/auth/register',
      data: {
        'fullName': fullName,
        'email': email,
        'password': password,
        'role': role,
        if (licenseNumber != null && licenseNumber.isNotEmpty)
          'licenseNumber': licenseNumber,
      },
    );
    return RegisterResponse.fromJson(response.data!);
  }
}
