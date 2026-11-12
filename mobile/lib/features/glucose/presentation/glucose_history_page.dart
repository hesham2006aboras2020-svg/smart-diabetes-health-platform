import 'package:material_ui/material_ui.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../data/glucose_repository.dart';
import '../data/glucose_models.dart';

final glucoseHistoryDaysProvider = StateProvider<int>((ref) => 7);
final glucoseHistoryProvider =
    FutureProvider.autoDispose.family<List<GlucoseReading>, int>(
  (ref, days) => ref.watch(glucoseRepositoryProvider).getRecentReadings(days: days),
);

class GlucoseHistoryPage extends ConsumerWidget {
  const GlucoseHistoryPage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final days = ref.watch(glucoseHistoryDaysProvider);
    final readings = ref.watch(glucoseHistoryProvider(days));

    return Scaffold(
      appBar: AppBar(title: const Text('سجل قراءات السكر')),
      body: Column(
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(20, 16, 20, 8),
            child: SegmentedButton<int>(
              segments: const [
                ButtonSegment(value: 7, label: Text('7 أيام')),
                ButtonSegment(value: 14, label: Text('14 يومًا')),
                ButtonSegment(value: 30, label: Text('30 يومًا')),
              ],
              selected: {days},
              onSelectionChanged: (selected) {
                ref.read(glucoseHistoryDaysProvider.notifier).state = selected.first;
              },
            ),
          ),
          Expanded(
            child: readings.when(
              loading: () => const Center(child: CircularProgressIndicator()),
              error: (_, __) => Center(
                child: FilledButton(
                  onPressed: () => ref.invalidate(glucoseHistoryProvider(days)),
                  child: const Text('إعادة المحاولة'),
                ),
              ),
              data: (items) => items.isEmpty
                  ? const Center(child: Text('لا توجد قراءات في هذه الفترة.'))
                  : RefreshIndicator(
                      onRefresh: () async {
                        ref.invalidate(glucoseHistoryProvider(days));
                      },
                      child: ListView.separated(
                        padding: const EdgeInsets.all(20),
                        itemCount: items.length,
                        separatorBuilder: (_, __) => const SizedBox(height: 8),
                        itemBuilder: (_, index) => _ReadingCard(items[index]),
                      ),
                    ),
            ),
          ),
        ],
      ),
    );
  }
}

class _ReadingCard extends StatelessWidget {
  const _ReadingCard(this.reading);
  final GlucoseReading reading;

  @override
  Widget build(BuildContext context) {
    return Card(
      child: ListTile(
        leading: CircleAvatar(child: Icon(_statusIcon(reading.status))),
        title: Text(
          '${reading.glucoseValue.toStringAsFixed(0)} ${_unit(reading.unit)}',
          style: Theme.of(context).textTheme.titleMedium,
        ),
        subtitle: Text(
          '${_context(reading.measurementContext)} • ${_date(reading.measurementDateTime)}',
        ),
        trailing: Text(_status(reading.status)),
      ),
    );
  }

  static String _unit(String value) => value == 'MmolL' ? 'mmol/L' : 'mg/dL';
  static String _context(String value) => switch (value) {
        'Fasting' => 'صائم',
        'BeforeMeal' => 'قبل الوجبة',
        'AfterMeal' => 'بعد الوجبة',
        'BeforeSleep' => 'قبل النوم',
        'Random' => 'عشوائي',
        _ => 'أخرى',
      };
  static String _status(String value) => switch (value) {
        'Low' => 'منخفض',
        'High' => 'مرتفع',
        _ => 'ضمن النطاق',
      };
  static IconData _statusIcon(String value) => switch (value) {
        'Low' => Icons.arrow_downward,
        'High' => Icons.arrow_upward,
        _ => Icons.check,
      };
  static String _date(DateTime value) {
    final local = value.toLocal();
    return '${local.day}/${local.month}/${local.year} '
        '${local.hour.toString().padLeft(2, '0')}:${local.minute.toString().padLeft(2, '0')}';
  }
}
