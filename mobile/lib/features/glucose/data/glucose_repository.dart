import '../../../core/network/api_client.dart';
import 'glucose_models.dart';

class GlucoseRepository {
  GlucoseRepository(this._client);

  final ApiClient _client;

  Future<GlucoseSummary> getSummary({int days = 7}) async {
    final response = await _client.dio.get<Map<String, dynamic>>(
      '/api/glucose/summary',
      queryParameters: {'days': days},
    );
    return GlucoseSummary.fromJson(response.data!);
  }

  Future<List<GlucoseReading>> getRecentReadings({int days = 7}) async {
    final response = await _client.dio.get<List<dynamic>>(
      '/api/glucose/readings',
      queryParameters: {
        'from': DateTime.now()
            .toUtc()
            .subtract(Duration(days: days))
            .toIso8601String(),
      },
    );
    return response.data!
        .map((item) => GlucoseReading.fromJson(item as Map<String, dynamic>))
        .toList();
  }
}
