import 'package:material_ui/material_ui.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'patient_dashboard_controller.dart';

class PatientDashboardPage extends ConsumerWidget {
  const PatientDashboardPage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final state = ref.watch(patientDashboardProvider);
    if (state.loading) return const Center(child: CircularProgressIndicator());
    if (state.message != null) {
      return Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Text(state.message!),
            const SizedBox(height: 12),
            FilledButton(
              onPressed: () => ref.read(patientDashboardProvider.notifier).load(),
              child: const Text('إعادة المحاولة'),
            ),
          ],
        ),
      );
    }

    final summary = state.summary!;
    return RefreshIndicator(
      onRefresh: () => ref.read(patientDashboardProvider.notifier).load(),
      child: ListView(
        padding: const EdgeInsets.all(20),
        children: [
          Text('ملخص السكر', style: Theme.of(context).textTheme.headlineSmall),
          const SizedBox(height: 6),
          const Text('آخر 7 أيام'),
          const SizedBox(height: 20),
          Wrap(
            spacing: 12,
            runSpacing: 12,
            children: [
              _MetricCard(title: 'المتوسط', value: _value(summary.average), suffix: 'mg/dL'),
              _MetricCard(title: 'ضمن النطاق', value: _percentage(summary.inRangePercentage), suffix: '%'),
              _MetricCard(title: 'أعلى قراءة', value: _value(summary.maximum), suffix: 'mg/dL'),
              _MetricCard(title: 'أقل قراءة', value: _value(summary.minimum), suffix: 'mg/dL'),
            ],
          ),
          const SizedBox(height: 24),
          Card(
            child: ListTile(
              leading: const Icon(Icons.trending_up),
              title: const Text('الاتجاه'),
              subtitle: Text(_trendLabel(summary.trend)),
              trailing: Text('${summary.numberOfReadings} قراءة'),
            ),
          ),
          const SizedBox(height: 24),
          Text('أحدث القراءات', style: Theme.of(context).textTheme.titleLarge),
          const SizedBox(height: 8),
          if (state.readings.isEmpty)
            const Card(
              child: Padding(
                padding: EdgeInsets.all(20),
                child: Text('لا توجد قراءات مسجلة خلال الفترة الحالية.', textAlign: TextAlign.center),
              ),
            )
          else
            ...state.readings.take(10).map(
              (reading) => Card(
                child: ListTile(
                  leading: const Icon(Icons.monitor_heart_outlined),
                  title: Text('${reading.glucoseValue.toStringAsFixed(0)} ${reading.unit}'),
                  subtitle: Text(
                    '${_contextLabel(reading.measurementContext)} • ${_dateLabel(reading.measurementDateTime)}',
                  ),
                  trailing: Text(_statusLabel(reading.status)),
                ),
              ),
            ),
        ],
      ),
    );
  }

  static String _value(double? value) => value == null ? '—' : value.toStringAsFixed(0);
  static String _percentage(double? value) => value == null ? '—' : value.toStringAsFixed(1);

  static String _trendLabel(String value) => switch (value) {
        'Improving' => 'تحسن',
        'Worsening' => 'تراجع',
        'Stable' => 'مستقر',
        _ => 'بيانات غير كافية',
      };

  static String _statusLabel(String value) => switch (value) {
        'Low' => 'منخفض',
        'High' => 'مرتفع',
        _ => 'ضمن النطاق',
      };

  static String _contextLabel(String value) => switch (value) {
        'BeforeMeal' => 'قبل الوجبة',
        'AfterMeal' => 'بعد الوجبة',
        'Fasting' => 'صائم',
        'BeforeSleep' => 'قبل النوم',
        _ => 'عام',
      };

  static String _dateLabel(DateTime value) {
    final local = value.toLocal();
    return '${local.day}/${local.month} '
        '${local.hour.toString().padLeft(2, '0')}:'
        '${local.minute.toString().padLeft(2, '0')}';
  }
}

class _MetricCard extends StatelessWidget {
  const _MetricCard({required this.title, required this.value, required this.suffix});
  final String title;
  final String value;
  final String suffix;

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      width: 165,
      child: Card(
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(title),
              const SizedBox(height: 8),
              Text(value, style: Theme.of(context).textTheme.headlineSmall),
              Text(suffix),
            ],
          ),
        ),
      ),
    );
  }
}
