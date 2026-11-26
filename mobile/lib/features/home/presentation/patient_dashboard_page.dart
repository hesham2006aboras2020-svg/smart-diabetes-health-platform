import 'package:material_ui/material_ui.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/widgets/app_states.dart';
import '../../glucose/data/glucose_models.dart';
import '../../glucose/presentation/add_glucose_page.dart';
import '../../glucose/presentation/glucose_history_page.dart';
import 'patient_dashboard_controller.dart';

class PatientDashboardPage extends ConsumerWidget {
  const PatientDashboardPage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final state = ref.watch(patientDashboardProvider);
    if (state.loading) {
      return const AppLoadingState(label: 'جارٍ تجهيز ملخص صحتك...');
    }
    if (state.message != null) {
      return AppErrorState(
        message: state.message!,
        onRetry: () => ref.read(patientDashboardProvider.notifier).load(),
      );
    }

    final summary = state.summary!;
    final latest = state.readings.isEmpty ? null : state.readings.first;
    return RefreshIndicator(
      onRefresh: () => ref.read(patientDashboardProvider.notifier).load(),
      child: LayoutBuilder(
        builder: (context, constraints) {
          final wide = constraints.maxWidth >= 900;
          return ListView(
            physics: const AlwaysScrollableScrollPhysics(),
            padding: EdgeInsets.fromLTRB(wide ? 32 : 18, 18, wide ? 32 : 18, 28),
            children: [
              ConstrainedBox(
                constraints: const BoxConstraints(maxWidth: 1180),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    _WelcomeHeader(latest: latest),
                    const SizedBox(height: 16),
                    _HeroGlucoseCard(latest: latest),
                    const SizedBox(height: 14),
                    const _QuickActions(),
                    const SizedBox(height: 22),
                    SectionHeader(
                      title: 'ملخص السكر',
                      subtitle: 'آخر 7 أيام • ' + summary.numberOfReadings.toString() + ' قراءة',
                    ),
                    const SizedBox(height: 12),
                    _MetricsGrid(summary: summary),
                    const SizedBox(height: 18),
                    if (state.readings.length >= 2)
                      _GlucoseChartCard(readings: state.readings)
                    else
                      const AppEmptyState(
                        icon: Icons.insights_rounded,
                        title: 'الرسم البياني سيظهر قريبًا',
                        message: 'أضف قراءتين أو أكثر لعرض اتجاه السكر بصريًا.',
                      ),
                    const SizedBox(height: 22),
                    SectionHeader(
                      title: 'آخر القراءات',
                      subtitle: 'أحدث 5 قراءات مسجلة',
                      action: TextButton(
                        onPressed: () => Navigator.of(context).push(
                          MaterialPageRoute(builder: (_) => const GlucoseHistoryPage()),
                        ),
                        child: const Text('عرض الكل'),
                      ),
                    ),
                    const SizedBox(height: 10),
                    if (state.readings.isEmpty)
                      const AppEmptyState(
                        icon: Icons.monitor_heart_outlined,
                        title: 'لا توجد قراءات بعد',
                        message: 'سجّل أول قراءة لتبدأ متابعة المؤشرات.',
                      )
                    else
                      ...state.readings.take(5).map(
                        (reading) => Padding(
                          padding: const EdgeInsets.only(bottom: 8),
                          child: _ReadingTile(reading: reading),
                        ),
                      ),
                  ],
                ),
              ),
            ],
          );
        },
      ),
    );
  }
}

class _WelcomeHeader extends StatelessWidget {
  const _WelcomeHeader({required this.latest});

  final GlucoseReading? latest;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final status = latest == null ? 'لا توجد قراءة' : _statusLabel(latest!.status);
    return Row(
      children: [
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                'صحتك في لمحة',
                style: theme.textTheme.headlineSmall?.copyWith(fontWeight: FontWeight.w900),
              ),
              const SizedBox(height: 4),
              Text(
                latest == null ? 'ابدأ بإضافة قراءة السكر الأولى.' : 'آخر حالة: ' + status,
                style: theme.textTheme.bodyMedium?.copyWith(
                  color: theme.colorScheme.onSurfaceVariant,
                ),
              ),
            ],
          ),
        ),
        Container(
          padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 9),
          decoration: BoxDecoration(
            color: theme.colorScheme.primaryContainer,
            borderRadius: BorderRadius.circular(16),
          ),
          child: Row(
            children: [
              Icon(Icons.calendar_today_rounded, size: 16, color: theme.colorScheme.primary),
              const SizedBox(width: 7),
              Text(
                '7 أيام',
                style: TextStyle(
                  color: theme.colorScheme.onPrimaryContainer,
                  fontWeight: FontWeight.w800,
                ),
              ),
            ],
          ),
        ),
      ],
    );
  }
}

class _HeroGlucoseCard extends StatelessWidget {
  const _HeroGlucoseCard({required this.latest});

  final GlucoseReading? latest;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final color = latest == null ? theme.colorScheme.primary : _statusColor(latest!.status);
    return Card(
      clipBehavior: Clip.antiAlias,
      child: Container(
        decoration: BoxDecoration(
          gradient: LinearGradient(
            colors: [
              color.withValues(alpha: 0.96),
              Color.lerp(color, theme.colorScheme.primary, 0.55)!,
            ],
            begin: Alignment.topRight,
            end: Alignment.bottomLeft,
          ),
        ),
        padding: const EdgeInsets.all(22),
        child: Row(
          children: [
            Container(
              width: 58,
              height: 58,
              decoration: BoxDecoration(
                color: Colors.white.withValues(alpha: 0.16),
                borderRadius: BorderRadius.circular(19),
              ),
              child: const Icon(Icons.monitor_heart_rounded, color: Colors.white, size: 30),
            ),
            const SizedBox(width: 16),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    'آخر قراءة',
                    style: TextStyle(color: Colors.white.withValues(alpha: 0.86), fontWeight: FontWeight.w700),
                  ),
                  const SizedBox(height: 3),
                  Text(
                    latest == null ? '—' : latest!.glucoseValue.toStringAsFixed(0) + ' ' + latest!.unit,
                    style: const TextStyle(color: Colors.white, fontSize: 34, fontWeight: FontWeight.w900),
                  ),
                  Text(
                    latest == null
                        ? 'سجّل قراءة جديدة الآن'
                        : _contextLabel(latest!.measurementContext) + ' • ' + _dateLabel(latest!.measurementDateTime),
                    style: TextStyle(color: Colors.white.withValues(alpha: 0.86), fontSize: 12),
                  ),
                ],
              ),
            ),
            if (latest == null)
              FilledButton.tonal(
                onPressed: () => Navigator.of(context).push(
                  MaterialPageRoute(builder: (_) => const AddGlucosePage()),
                ),
                child: const Text('إضافة'),
              )
            else
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
                decoration: BoxDecoration(
                  color: Colors.white.withValues(alpha: 0.14),
                  borderRadius: BorderRadius.circular(14),
                ),
                child: Text(
                  _statusLabel(latest!.status),
                  style: const TextStyle(color: Colors.white, fontWeight: FontWeight.w800),
                ),
              ),
          ],
        ),
      ),
    );
  }
}

class _QuickActions extends StatelessWidget {
  const _QuickActions();

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        Expanded(
          child: OutlinedButton.icon(
            onPressed: () => Navigator.of(context).push(
              MaterialPageRoute(builder: (_) => const AddGlucosePage()),
            ),
            icon: const Icon(Icons.add_circle_outline_rounded),
            label: const Text('قراءة جديدة'),
          ),
        ),
        const SizedBox(width: 10),
        Expanded(
          child: OutlinedButton.icon(
            onPressed: () => Navigator.of(context).push(
              MaterialPageRoute(builder: (_) => const GlucoseHistoryPage()),
            ),
            icon: const Icon(Icons.history_rounded),
            label: const Text('السجل'),
          ),
        ),
      ],
    );
  }
}

class _MetricsGrid extends StatelessWidget {
  const _MetricsGrid({required this.summary});

  final GlucoseSummary summary;

  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(
      builder: (context, constraints) {
        final columns = constraints.maxWidth >= 900 ? 4 : constraints.maxWidth >= 560 ? 3 : 2;
        return GridView.count(
          crossAxisCount: columns,
          shrinkWrap: true,
          physics: const NeverScrollableScrollPhysics(),
          crossAxisSpacing: 10,
          mainAxisSpacing: 10,
          childAspectRatio: constraints.maxWidth < 560 ? 1.28 : 1.55,
          children: [
            _MetricCard('المتوسط', _value(summary.average), 'mg/dL', Icons.monitor_heart_outlined, _glucoseColor(summary.average)),
            _MetricCard('ضمن النطاق', _percentage(summary.inRangePercentage), '%', Icons.check_circle_outline_rounded, const Color(0xFF2E8B70)),
            _MetricCard('مرتفع', _percentage(summary.highPercentage), '%', Icons.arrow_upward_rounded, const Color(0xFFC26A00)),
            _MetricCard('منخفض', _percentage(summary.lowPercentage), '%', Icons.arrow_downward_rounded, const Color(0xFFC53D4A)),
            _MetricCard('أعلى قراءة', _value(summary.maximum), 'mg/dL', Icons.keyboard_double_arrow_up_rounded, _glucoseColor(summary.maximum)),
            _MetricCard('أقل قراءة', _value(summary.minimum), 'mg/dL', Icons.keyboard_double_arrow_down_rounded, _glucoseColor(summary.minimum)),
          ],
        );
      },
    );
  }
}

class _MetricCard extends StatelessWidget {
  const _MetricCard(this.title, this.value, this.suffix, this.icon, this.color);

  final String title;
  final String value;
  final String suffix;
  final IconData icon;
  final Color color;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(14),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Container(
              width: 38,
              height: 38,
              decoration: BoxDecoration(
                color: color.withValues(alpha: 0.11),
                borderRadius: BorderRadius.circular(13),
              ),
              child: Icon(icon, color: color, size: 20),
            ),
            const Spacer(),
            Text(
              title,
              style: theme.textTheme.bodySmall?.copyWith(
                color: theme.colorScheme.onSurfaceVariant,
                fontWeight: FontWeight.w700,
              ),
            ),
            const SizedBox(height: 2),
            Row(
              crossAxisAlignment: CrossAxisAlignment.end,
              children: [
                Text(value, style: theme.textTheme.titleLarge?.copyWith(fontWeight: FontWeight.w900)),
                const SizedBox(width: 4),
                Padding(
                  padding: const EdgeInsets.only(bottom: 2),
                  child: Text(suffix, style: theme.textTheme.labelSmall?.copyWith(color: theme.colorScheme.onSurfaceVariant)),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}

class _GlucoseChartCard extends StatelessWidget {
  const _GlucoseChartCard({required this.readings});

  final List<GlucoseReading> readings;

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.fromLTRB(18, 18, 18, 14),
        child: Column(
          children: [
            const SectionHeader(title: 'اتجاه القراءات', subtitle: 'آخر 7 أيام'),
            const SizedBox(height: 18),
            SizedBox(
              height: 190,
              width: double.infinity,
              child: CustomPaint(
                painter: _GlucoseChartPainter(readings.reversed.take(20).toList(), Theme.of(context).colorScheme),
              ),
            ),
            const SizedBox(height: 8),
            Row(
              children: [
                const _LegendDot(color: Color(0xFF2E8B70), label: 'ضمن النطاق'),
                const SizedBox(width: 14),
                const _LegendDot(color: Color(0xFFC26A00), label: 'خارج النطاق'),
                const Spacer(),
                Text(
                  '70–180 mg/dL',
                  style: Theme.of(context).textTheme.labelSmall?.copyWith(color: Theme.of(context).colorScheme.onSurfaceVariant),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}

class _LegendDot extends StatelessWidget {
  const _LegendDot({required this.color, required this.label});

  final Color color;
  final String label;

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        Container(width: 8, height: 8, decoration: BoxDecoration(color: color, shape: BoxShape.circle)),
        const SizedBox(width: 5),
        Text(label, style: Theme.of(context).textTheme.labelSmall),
      ],
    );
  }
}

class _GlucoseChartPainter extends CustomPainter {
  _GlucoseChartPainter(this.items, this.scheme);

  final List<GlucoseReading> items;
  final ColorScheme scheme;

  @override
  void paint(Canvas canvas, Size size) {
    if (items.length < 2) return;
    const minValue = 50.0;
    const maxValue = 220.0;
    final chartHeight = size.height - 18;
    final gridPaint = Paint()
      ..color = scheme.outlineVariant.withValues(alpha: 0.45)
      ..strokeWidth = 1;

    for (final value in [70.0, 125.0, 180.0]) {
      final y = chartHeight - ((value - minValue) / (maxValue - minValue)) * chartHeight;
      canvas.drawLine(Offset(0, y), Offset(size.width, y), gridPaint);
    }

    final line = Paint()
      ..color = scheme.primary
      ..strokeWidth = 3
      ..strokeCap = StrokeCap.round
      ..style = PaintingStyle.stroke;
    final fill = Paint()
      ..color = scheme.primary.withValues(alpha: 0.08)
      ..style = PaintingStyle.fill;
    final path = Path();
    final area = Path();
    final step = size.width / (items.length - 1);

    for (var i = 0; i < items.length; i++) {
      final value = items[i].glucoseValue.clamp(minValue, maxValue);
      final x = i * step;
      final y = chartHeight - ((value - minValue) / (maxValue - minValue)) * chartHeight;
      if (i == 0) {
        path.moveTo(x, y);
        area.moveTo(x, chartHeight);
        area.lineTo(x, y);
      } else {
        path.lineTo(x, y);
        area.lineTo(x, y);
      }
    }
    area.lineTo(size.width, chartHeight);
    area.close();
    canvas.drawPath(area, fill);
    canvas.drawPath(path, line);

    for (var i = 0; i < items.length; i++) {
      final value = items[i].glucoseValue.clamp(minValue, maxValue);
      final x = i * step;
      final y = chartHeight - ((value - minValue) / (maxValue - minValue)) * chartHeight;
      final color = items[i].status == 'Low' || items[i].status == 'High'
          ? const Color(0xFFC26A00)
          : const Color(0xFF2E8B70);
      canvas.drawCircle(Offset(x, y), 4.5, Paint()..color = color);
    }
  }

  @override
  bool shouldRepaint(covariant _GlucoseChartPainter oldDelegate) => oldDelegate.items != items;
}

class _ReadingTile extends StatelessWidget {
  const _ReadingTile({required this.reading});

  final GlucoseReading reading;

  @override
  Widget build(BuildContext context) {
    final color = _statusColor(reading.status);
    return Card(
      child: ListTile(
        contentPadding: const EdgeInsets.symmetric(horizontal: 14, vertical: 3),
        leading: Container(
          width: 42,
          height: 42,
          decoration: BoxDecoration(
            color: color.withValues(alpha: 0.11),
            borderRadius: BorderRadius.circular(14),
          ),
          child: Icon(Icons.monitor_heart_outlined, color: color, size: 21),
        ),
        title: Text(
          reading.glucoseValue.toStringAsFixed(0) + ' ' + reading.unit,
          style: const TextStyle(fontWeight: FontWeight.w900),
        ),
        subtitle: Text(_contextLabel(reading.measurementContext) + ' • ' + _dateLabel(reading.measurementDateTime)),
        trailing: Container(
          padding: const EdgeInsets.symmetric(horizontal: 9, vertical: 6),
          decoration: BoxDecoration(
            color: color.withValues(alpha: 0.10),
            borderRadius: BorderRadius.circular(10),
          ),
          child: Text(
            _statusLabel(reading.status),
            style: TextStyle(color: color, fontWeight: FontWeight.w800),
          ),
        ),
      ),
    );
  }
}

Color _glucoseColor(double? value) {
  if (value == null) return const Color(0xFF0B7A75);
  if (value < 70) return const Color(0xFFC53D4A);
  if (value > 180) return const Color(0xFFC26A00);
  return const Color(0xFF2E8B70);
}

Color _statusColor(String status) => switch (status) {
      'Low' => const Color(0xFFC53D4A),
      'High' => const Color(0xFFC26A00),
      _ => const Color(0xFF2E8B70),
    };

String _statusLabel(String value) => switch (value) {
      'Low' => 'منخفض',
      'High' => 'مرتفع',
      _ => 'ضمن النطاق',
    };

String _contextLabel(String value) => switch (value) {
      'BeforeMeal' => 'قبل الوجبة',
      'AfterMeal' => 'بعد الوجبة',
      'Fasting' => 'صائم',
      'BeforeSleep' => 'قبل النوم',
      'Random' => 'عشوائي',
      _ => 'عام',
    };

String _dateLabel(DateTime value) {
  final local = value.toLocal();
  return local.day.toString() + '/' + local.month.toString() + ' ' +
      local.hour.toString().padLeft(2, '0') + ':' +
      local.minute.toString().padLeft(2, '0');
}

String _value(double? value) => value == null ? '—' : value.toStringAsFixed(0);
String _percentage(double? value) => value == null ? '—' : value.toStringAsFixed(1);
