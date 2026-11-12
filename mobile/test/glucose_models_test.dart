import 'package:flutter_test/flutter_test.dart';
import 'package:smart_diabetes_mobile/features/glucose/data/glucose_models.dart';

void main() {
  test('parses glucose summary', () {
    final summary = GlucoseSummary.fromJson({
      'numberOfReadings': 12,
      'averageGlucoseMgDl': 126.5,
      'minimumGlucoseMgDl': 72,
      'maximumGlucoseMgDl': 184,
      'inRangePercentage': 83.3,
      'highPercentage': 8.3,
      'lowPercentage': 8.4,
      'trend': 'Stable',
    });

    expect(summary.numberOfReadings, 12);
    expect(summary.average, 126.5);
    expect(summary.inRangePercentage, 83.3);
    expect(summary.trend, 'Stable');
  });

  test('parses glucose reading and converts timestamp to local time', () {
    final reading = GlucoseReading.fromJson({
      'glucoseValue': 110,
      'unit': 'MgDl',
      'measurementDateTime': '2026-11-12T10:30:00Z',
      'measurementContext': 'Fasting',
      'status': 'Normal',
    });

    expect(reading.glucoseValue, 110);
    expect(reading.unit, 'MgDl');
    expect(reading.measurementContext, 'Fasting');
    expect(reading.status, 'Normal');
  });
}
