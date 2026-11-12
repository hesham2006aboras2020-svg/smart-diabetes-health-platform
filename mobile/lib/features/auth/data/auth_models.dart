class AuthResponse {
  const AuthResponse({
    required this.accessToken,
    required this.refreshToken,
    required this.accessTokenExpiresAt,
    required this.role,
    required this.accountStatus,
  });

  factory AuthResponse.fromJson(Map<String, dynamic> json) {
    return AuthResponse(
      accessToken: json['accessToken'] as String,
      refreshToken: json['refreshToken'] as String,
      accessTokenExpiresAt:
          DateTime.parse(json['accessTokenExpiresAt'] as String),
      role: json['role'] as String,
      accountStatus: json['accountStatus'] as String,
    );
  }

  final String accessToken;
  final String refreshToken;
  final DateTime accessTokenExpiresAt;
  final String role;
  final String accountStatus;
}

class RegisterResponse {
  const RegisterResponse({
    required this.message,
    required this.role,
    required this.accountStatus,
    required this.requiresApproval,
    this.authentication,
  });

  factory RegisterResponse.fromJson(Map<String, dynamic> json) {
    return RegisterResponse(
      message: json['message'] as String,
      role: json['role'] as String,
      accountStatus: json['accountStatus'] as String,
      requiresApproval: json['requiresApproval'] as bool,
      authentication: json['authentication'] == null
          ? null
          : AuthResponse.fromJson(
              json['authentication'] as Map<String, dynamic>,
            ),
    );
  }

  final String message;
  final String role;
  final String accountStatus;
  final bool requiresApproval;
  final AuthResponse? authentication;
}
