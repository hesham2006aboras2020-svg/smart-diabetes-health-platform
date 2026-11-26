import 'package:material_ui/material_ui.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'patient_dashboard_controller.dart';
import '../../glucose/presentation/add_glucose_page.dart';
import '../../glucose/presentation/glucose_history_page.dart';

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
    final theme = Theme.of(context);
    return RefreshIndicator(
      onRefresh: () => ref.read(patientDashboardProvider.notifier).load(),
      child: ListView(
        padding: const EdgeInsets.all(20),
        children: [
          FilledButton.icon(
            onPressed: () => Navigator.of(context).push(
              MaterialPageRoute(builder: (_) => const AddGlucosePage()),
            ),
            icon: const Icon(Icons.add),
            label: const Text('تسجيل قراءة جديدة'),
          ),
          const SizedBox(height: 8),
          OutlinedButton.icon(
            onPressed: () => Navigator.of(context).push(
              MaterialPageRoute(builder: (_) => const GlucoseHistoryPage()),
            ),
            icon: const Icon(Icons.history),
            label: const Text('عرض سجل القراءات'),
          ),
          const SizedBox(height: 20),
          Text('ملخص السكر', style: Theme.of(context).textTheme.headlineSmall),
          const SizedBox(height: 6),
          const Text('آخر 7 أيام'),
          const SizedBox(height: 20),
          GridView.count(
            crossAxisCount: 2, shrinkWrap: true,
            physics: const NeverScrollableScrollPhysics(),
            crossAxisSpacing: 12, mainAxisSpacing: 12, childAspectRatio: 1.35,
            children: [
              _MetricCard(title: 'المتوسط', value: _value(summary.average), suffix: 'mg/dL', icon: Icons.monitor_heart_outlined),
              _MetricCard(title: 'ضمن النطاق', value: _percentage(summary.inRangePercentage), suffix: '%', icon: Icons.check_circle_outline),
              _MetricCard(title: 'مرتفع', value: _percentage(summary.highPercentage), suffix: '%', icon: Icons.arrow_upward_rounded),
              _MetricCard(title: 'منخفض', value: _percentage(summary.lowPercentage), suffix: '%', icon: Icons.arrow_downward_rounded),
              _MetricCard(title: 'أعلى قراءة', value: _value(summary.maximum), suffix: 'mg/dL', icon: Icons.keyboard_double_arrow_up_rounded),
              _MetricCard(title: 'أقل قراءة', value: _value(summary.minimum), suffix: 'mg/dL', icon: Icons.keyboard_double_arrow_down_rounded),
            ],
          ),
          const SizedBox(height: 16),
          Card(
            child: Padding(
              padding: const EdgeInsets.all(18),
              child: Row(
                children: [
                  _StatusIcon(icon: Icons.trending_flat_rounded, color: theme.colorScheme.primary),
                  const SizedBox(width: 14),
                  Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                    const Text('الاتجاه خلال آخر 7 أيام', style: TextStyle(fontWeight: FontWeight.w800)),
                    const SizedBox(height: 4),
                    Text(_trendLabel(summary.trend), style: theme.textTheme.bodyMedium?.copyWith(color: theme.colorScheme.onSurfaceVariant)),
                  ])),
                  Text('${summary.numberOfReadings}', style: theme.textTheme.headlineSmall?.copyWith(fontWeight: FontWeight.w900, color: theme.colorScheme.primary)),
                  const SizedBox(width: 4), const Text('قراءة'),
                ],
              ),
            ),
          ),
          const SizedBox(height: 24),
          Text('أحدث القراءات', style: Theme.of(context).textTheme.titleLarge),, style: Theme.of(context).textTheme.titleLarge),
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
  const _MetricCard({required this.title, required this.value, required this.suffix, required this.icon});
  final String title;
  final String value;
  final String suffix;
  final IconData icon;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(15),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            _StatusIcon(icon: icon, color: theme.colorScheme.primary),
            const Spacer(),
            Text(title, style: theme.textTheme.bodyMedium?.copyWith(color: theme.colorScheme.onSurfaceVariant, fontWeight: FontWeight.w600)),
            const SizedBox(height: 3),
            Row(crossAxisAlignment: CrossAxisAlignment.end, children: [
              Text(value, style: theme.textTheme.headlineSmall?.copyWith(fontWeight: FontWeight.w900)),
              const SizedBox(width: 5),
              Padding(padding: const EdgeInsets.only(bottom: 3), child: Text(suffix, style: theme.textTheme.labelSmall?.copyWith(color: theme.colorScheme.onSurfaceVariant))),
            ]),
          ],
        ),
      ),
    );
  }
}

class _StatusIcon extends StatelessWidget {
  const _StatusIcon({required this.icon, required this.color});
  final IconData icon;
  final Color color;

  @override
  Widget build(BuildContext context) => Container(
    width: 42, height: 42,
    decoration: BoxDecoration(color: color.withValues(alpha: 0.10), borderRadius: BorderRadius.circular(14)),
    child: Icon(icon, color: color, size: 22),
  );
}
