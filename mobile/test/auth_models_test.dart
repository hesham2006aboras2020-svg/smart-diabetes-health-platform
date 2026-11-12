import 'package:flutter_test/flutter_test.dart';
import 'package:smart_diabetes_mobile/features/auth/data/auth_models.dart';

void main() {
  test('parses authentication response', () {
    final response = AuthResponse.fromJson({
      'accessToken': 'access',
      'refreshToken': 'refresh',
      'accessTokenExpiresAt': '2026-11-12T12:00:00Z',
      'role': 'Patient',
      'accountStatus': 'Active',
    });

    expect(response.accessToken, 'access');
    expect(response.refreshToken, 'refresh');
    expect(response.role, 'Patient');
    expect(response.accountStatus, 'Active');
  });

  test('parses pending doctor registration response without authentication', () {
    final response = RegisterResponse.fromJson({
      'message': 'تم إرسال طلب التسجيل للمراجعة.',
      'role': 'Doctor',
      'accountStatus': 'Pending',
      'requiresApproval': true,
      'authentication': null,
    });

    expect(response.requiresApproval, isTrue);
    expect(response.authentication, isNull);
  });
}
