class GlucoseSummary {
  const GlucoseSummary({
    required this.numberOfReadings,
    this.average,
    this.minimum,
    this.maximum,
    this.inRangePercentage,
    this.highPercentage,
    this.lowPercentage,
    required this.trend,
  });

  factory GlucoseSummary.fromJson(Map<String, dynamic> json) {
    return GlucoseSummary(
      numberOfReadings: json['numberOfReadings'] as int,
      average: (json['averageGlucoseMgDl'] as num?)?.toDouble(),
      minimum: (json['minimumGlucoseMgDl'] as num?)?.toDouble(),
      maximum: (json['maximumGlucoseMgDl'] as num?)?.toDouble(),
      inRangePercentage: (json['inRangePercentage'] as num?)?.toDouble(),
      highPercentage: (json['highPercentage'] as num?)?.toDouble(),
      lowPercentage: (json['lowPercentage'] as num?)?.toDouble(),
      trend: json['trend'] as String,
    );
  }

  final int numberOfReadings;
  final double? average;
  final double? minimum;
  final double? maximum;
  final double? inRangePercentage;
  final double? highPercentage;
  final double? lowPercentage;
  final String trend;
}

class GlucoseReading {
  const GlucoseReading({
    required this.glucoseValue,
    required this.unit,
    required this.measurementDateTime,
    required this.measurementContext,
    required this.status,
  });

  factory GlucoseReading.fromJson(Map<String, dynamic> json) {
    return GlucoseReading(
      glucoseValue: (json['glucoseValue'] as num).toDouble(),
      unit: json['unit'] as String,
      measurementDateTime:
          DateTime.parse(json['measurementDateTime'] as String).toLocal(),
      measurementContext: json['measurementContext'] as String,
      status: json['status'] as String,
    );
  }

  final double glucoseValue;
  final String unit;
  final DateTime measurementDateTime;
  final String measurementContext;
  final String status;
}
