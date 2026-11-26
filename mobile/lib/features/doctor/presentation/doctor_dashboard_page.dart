import 'package:material_ui/material_ui.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../auth/presentation/auth_controller.dart';
import '../../core/widgets/app_states.dart';
import '../data/doctor_repository.dart';
import '../data/doctor_models.dart';
import 'doctor_patient_page.dart';

final doctorRepositoryProvider =
    Provider((ref) => DoctorRepository(ref.watch(apiClientProvider)));
final doctorPatientsProvider = FutureProvider.autoDispose(
  (ref) => ref.watch(doctorRepositoryProvider).patients(),
);

class DoctorDashboardPage extends ConsumerWidget {
  const DoctorDashboardPage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final state = ref.watch(doctorPatientsProvider);
    final theme = Theme.of(context);

    return Scaffold(
      appBar: AppBar(
        title: const Text('لوحة الطبيب'),
        actions: [
          IconButton(
            tooltip: 'تحديث',
            onPressed: () => ref.invalidate(doctorPatientsProvider),
            icon: const Icon(Icons.refresh_rounded),
          ),
          IconButton(
            tooltip: 'تسجيل الخروج',
            onPressed: () => ref.read(authControllerProvider.notifier).logout(),
            icon: const Icon(Icons.logout_rounded),
          ),
          const SizedBox(width: 6),
        ],
      ),
      body: state.when(
        loading: () => const AppLoadingState(label: 'جارٍ تحميل مرضاك...'),
        error: (_, _) => AppErrorState(
          message: 'تعذر تحميل قائمة المرضى.',
          onRetry: () => ref.invalidate(doctorPatientsProvider),
        ),
        data: (items) {
          if (items.isEmpty) {
            return const AppEmptyState(
              icon: Icons.groups_outlined,
              title: 'لا يوجد مرضى مرتبطون بك',
              message: 'ستظهر هنا قائمة المرضى بعد ربطهم بحسابك.',
            );
          }

          final alerts = items.fold<int>(
            0,
            (sum, patient) => sum + patient.unreadAlerts,
          );
          final attention = items.where((p) => p.priority != 'Low').length;

          return LayoutBuilder(
            builder: (context, constraints) {
              final wide = constraints.maxWidth >= 900;
              return ListView(
                padding: EdgeInsets.fromLTRB(wide ? 32 : 18, 18, wide ? 32 : 18, 28),
                children: [
                  ConstrainedBox(
                    constraints: const BoxConstraints(maxWidth: 1180),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Card(
                          child: Padding(
                            padding: const EdgeInsets.all(20),
                            child: Row(
                              children: [
                                Container(
                                  width: 54,
                                  height: 54,
                                  decoration: BoxDecoration(
                                    color: theme.colorScheme.primaryContainer,
                                    borderRadius: BorderRadius.circular(18),
                                  ),
                                  child: Icon(
                                    Icons.medical_services_rounded,
                                    color: theme.colorScheme.primary,
                                  ),
                                ),
                                const SizedBox(width: 14),
                                Expanded(
                                  child: Column(
                                    crossAxisAlignment: CrossAxisAlignment.start,
                                    children: [
                                      const Text(
                                        'متابعة المرضى',
                                        style: TextStyle(
                                          fontSize: 19,
                                          fontWeight: FontWeight.w900,
                                        ),
                                      ),
                                      const SizedBox(height: 4),
                                      Text(
                                        items.length.toString() +
                                            ' مرضى • ' +
                                            attention.toString() +
                                            ' يحتاجون متابعة',
                                        style: theme.textTheme.bodySmall?.copyWith(
                                          color: theme.colorScheme.onSurfaceVariant,
                                        ),
                                      ),
                                    ],
                                  ),
                                ),
                                _MiniStat(
                                  value: alerts.toString(),
                                  label: 'تنبيهات',
                                  color: const Color(0xFFC26A00),
                                ),
                              ],
                            ),
                          ),
                        ),
                        const SizedBox(height: 22),
                        const SectionHeader(
                          title: 'قائمة المرضى',
                          subtitle: 'مرتبة حسب أولوية المتابعة',
                        ),
                        const SizedBox(height: 10),
                        ...items.map(
                          (patient) => Padding(
                            padding: const EdgeInsets.only(bottom: 10),
                            child: _PatientCard(patient: patient),
                          ),
                        ),
                      ],
                    ),
                  ),
                ],
              );
            },
          );
        },
      ),
    );
  }
}

class _MiniStat extends StatelessWidget {
  const _MiniStat({
    required this.value,
    required this.label,
    required this.color,
  });

  final String value;
  final String label;
  final Color color;

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        Text(
          value,
          style: Theme.of(context).textTheme.titleLarge?.copyWith(
                fontWeight: FontWeight.w900,
                color: color,
              ),
        ),
        Text(label, style: Theme.of(context).textTheme.labelSmall),
      ],
    );
  }
}

class _PatientCard extends StatelessWidget {
  const _PatientCard({required this.patient});

  final DoctorPatient patient;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final status = _priorityStyle(theme, patient.priority);
    return Card(
      child: InkWell(
        borderRadius: BorderRadius.circular(24),
        onTap: () => Navigator.of(context).push(
          MaterialPageRoute(
            builder: (_) => DoctorPatientPage(patientId: patient.id),
          ),
        ),
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Row(
            children: [
              CircleAvatar(
                radius: 24,
                backgroundColor: theme.colorScheme.primaryContainer,
                child: Text(
                  patient.name.isEmpty ? '?' : patient.name.characters.first,
                  style: TextStyle(
                    color: theme.colorScheme.primary,
                    fontWeight: FontWeight.w900,
                  ),
                ),
              ),
              const SizedBox(width: 13),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      patient.name,
                      style: const TextStyle(fontWeight: FontWeight.w900),
                    ),
                    const SizedBox(height: 6),
                    Wrap(
                      spacing: 10,
                      runSpacing: 4,
                      children: [
                        _Value(label: 'آخر قراءة', value: patient.lastReading == null ? '—' : patient.lastReading!.toStringAsFixed(0)),
                        _Value(label: 'المتوسط', value: patient.average == null ? '—' : patient.average!.toStringAsFixed(0)),
                        _Value(label: 'ضمن النطاق', value: patient.inRange == null ? '—' : patient.inRange!.toStringAsFixed(0) + '%'),
                      ],
                    ),
                  ],
                ),
              ),
              const SizedBox(width: 8),
              Column(
                crossAxisAlignment: CrossAxisAlignment.end,
                children: [
                  if (patient.unreadAlerts > 0)
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 5),
                      decoration: BoxDecoration(
                        color: const Color(0xFFC53D4A).withValues(alpha: 0.10),
                        borderRadius: BorderRadius.circular(9),
                      ),
                      child: Text(
                        patient.unreadAlerts.toString() + ' تنبيه',
                        style: const TextStyle(
                          color: Color(0xFFC53D4A),
                          fontWeight: FontWeight.w800,
                          fontSize: 11,
                        ),
                      ),
                    ),
                  const SizedBox(height: 7),
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 9, vertical: 6),
                    decoration: BoxDecoration(
                      color: status.$1.withValues(alpha: 0.10),
                      borderRadius: BorderRadius.circular(10),
                    ),
                    child: Text(
                      status.$2,
                      style: TextStyle(
                        color: status.$1,
                        fontWeight: FontWeight.w800,
                        fontSize: 11,
                      ),
                    ),
                  ),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }

  (Color, String) _priorityStyle(ThemeData theme, String priority) {
    return switch (priority) {
      'High' => (const Color(0xFFC53D4A), 'أولوية عالية'),
      'Medium' => (const Color(0xFFC26A00), 'متابعة'),
      _ => (theme.colorScheme.primary, 'مستقر'),
    };
  }
}

class _Value extends StatelessWidget {
  const _Value({required this.label, required this.value});

  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    return Text(
      label + ': ' + value,
      style: Theme.of(context).textTheme.labelSmall?.copyWith(
            color: Theme.of(context).colorScheme.onSurfaceVariant,
          ),
    );
  }
}
