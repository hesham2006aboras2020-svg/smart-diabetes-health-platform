import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../auth/presentation/auth_controller.dart';
import '../../glucose/data/glucose_models.dart';
import '../../glucose/data/glucose_repository.dart';

final glucoseRepositoryProvider = Provider<GlucoseRepository>((ref) {
  return GlucoseRepository(ref.watch(apiClientProvider));
});

class PatientDashboardState {
  const PatientDashboardState({
    this.loading = true,
    this.summary,
    this.readings = const [],
    this.message,
  });

  final bool loading;
  final GlucoseSummary? summary;
  final List<GlucoseReading> readings;
  final String? message;
}

final patientDashboardProvider = NotifierProvider<
    PatientDashboardController, PatientDashboardState>(
  PatientDashboardController.new,
);

class PatientDashboardController
    extends Notifier<PatientDashboardState> {
  @override
  PatientDashboardState build() {
    Future<void>.microtask(load);
    return const PatientDashboardState();
  }

  Future<void> load() async {
    try {
      final repository = ref.read(glucoseRepositoryProvider);
      final results = await Future.wait([
        repository.getSummary(),
        repository.getRecentReadings(),
      ]);
      state = PatientDashboardState(
        loading: false,
        summary: results[0] as GlucoseSummary,
        readings: results[1] as List<GlucoseReading>,
      );
    } catch (_) {
      state = const PatientDashboardState(
        loading: false,
        message: 'تعذر تحميل بيانات السكر. حاول مرة أخرى.',
      );
    }
  }
}
