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

  Future<GlucoseReading> addReading({
    required double glucoseValue,
    required String unit,
    required DateTime measurementDateTime,
    required String measurementContext,
    String? notes,
  }) async {
    final response = await _client.dio.post<Map<String, dynamic>>(
      '/api/glucose/readings',
      data: {
        'glucoseValue': glucoseValue,
        'unit': _enumValue(unit, const ['MgDl', 'MmolL']),
        'measurementDateTime': measurementDateTime.toUtc().toIso8601String(),
        'measurementContext': _enumValue(
          measurementContext,
          const [
            'Fasting',
            'BeforeMeal',
            'AfterMeal',
            'BeforeSleep',
            'Random',
            'Other',
          ],
        ),
        if (notes != null && notes.trim().isNotEmpty) 'notes': notes.trim(),
      },
    );
    return GlucoseReading.fromJson(response.data!);
  }

  static int _enumValue(String value, List<String> names) {
    final index = names.indexOf(value);
    if (index < 0) {
      throw ArgumentError('قيمة غير مدعومة: $value');
    }
    return index + 1;
  }
}
