import 'package:material_ui/material_ui.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../auth/presentation/auth_controller.dart';
import '../../core/widgets/app_states.dart';
import '../data/admin_repository.dart';

final adminRepositoryProvider =
    Provider((ref) => AdminRepository(ref.watch(apiClientProvider)));
final adminSummaryProvider = FutureProvider.autoDispose(
  (ref) => ref.watch(adminRepositoryProvider).summary(),
);
final approvalsProvider = FutureProvider.autoDispose(
  (ref) => ref.watch(adminRepositoryProvider).approvals(),
);

class AdminDashboardPage extends ConsumerWidget {
  const AdminDashboardPage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final summary = ref.watch(adminSummaryProvider);
    final approvals = ref.watch(approvalsProvider);
    final theme = Theme.of(context);

    return Scaffold(
      appBar: AppBar(
        title: const Text('لوحة الإدارة'),
        actions: [
          IconButton(
            tooltip: 'تحديث',
            onPressed: () {
              ref.invalidate(adminSummaryProvider);
              ref.invalidate(approvalsProvider);
            },
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
      body: LayoutBuilder(
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
                    summary.when(
                      loading: () => const LinearProgressIndicator(),
                      error: (_, _) => AppErrorState(
                        message: 'تعذر تحميل مؤشرات الإدارة.',
                        onRetry: () => ref.invalidate(adminSummaryProvider),
                      ),
                      data: (value) => LayoutBuilder(
                        builder: (context, gridConstraints) {
                          final columns = gridConstraints.maxWidth >= 900 ? 4 : 2;
                          return GridView.count(
                            crossAxisCount: columns,
                            shrinkWrap: true,
                            physics: const NeverScrollableScrollPhysics(),
                            crossAxisSpacing: 10,
                            mainAxisSpacing: 10,
                            childAspectRatio: 1.55,
                            children: [
                              _AdminMetric(
                                title: 'المرضى',
                                value: value.patients,
                                icon: Icons.groups_rounded,
                                color: theme.colorScheme.primary,
                              ),
                              _AdminMetric(
                                title: 'الأطباء',
                                value: value.doctors,
                                icon: Icons.medical_services_rounded,
                                color: const Color(0xFF4D6FA8),
                              ),
                              _AdminMetric(
                                title: 'طلبات معلقة',
                                value: value.pending,
                                icon: Icons.pending_actions_rounded,
                                color: const Color(0xFFC26A00),
                              ),
                              _AdminMetric(
                                title: 'تنبيهات غير مقروءة',
                                value: value.alerts,
                                icon: Icons.warning_amber_rounded,
                                color: const Color(0xFFC53D4A),
                              ),
                            ],
                          );
                        },
                      ),
                    ),
                    const SizedBox(height: 24),
                    const SectionHeader(
                      title: 'اعتماد الأطباء',
                      subtitle: 'طلبات التسجيل التي تحتاج مراجعة',
                    ),
                    const SizedBox(height: 10),
                    approvals.when(
                      loading: () => const AppLoadingState(label: 'جارٍ تحميل الطلبات...'),
                      error: (_, _) => AppErrorState(
                        message: 'تعذر تحميل طلبات الاعتماد.',
                        onRetry: () => ref.invalidate(approvalsProvider),
                      ),
                      data: (items) {
                        if (items.isEmpty) {
                          return const AppEmptyState(
                            icon: Icons.verified_user_outlined,
                            title: 'لا توجد طلبات معلقة',
                            message: 'لا توجد طلبات اعتماد جديدة حاليًا.',
                          );
                        }

                        return Column(
                          children: [
                            for (final item in items)
                              Padding(
                                padding: const EdgeInsets.only(bottom: 10),
                                child: Card(
                                  child: Padding(
                                    padding: const EdgeInsets.all(16),
                                    child: Row(
                                      children: [
                                        CircleAvatar(
                                          backgroundColor: theme.colorScheme.primaryContainer,
                                          child: Icon(
                                            Icons.person_rounded,
                                            color: theme.colorScheme.primary,
                                          ),
                                        ),
                                        const SizedBox(width: 12),
                                        Expanded(
                                          child: Column(
                                            crossAxisAlignment: CrossAxisAlignment.start,
                                            children: [
                                              Text(
                                                item.name,
                                                style: const TextStyle(fontWeight: FontWeight.w900),
                                              ),
                                              const SizedBox(height: 4),
                                              Text(
                                                item.license +
                                                    (item.specialization == null
                                                        ? ''
                                                        : ' • ' + item.specialization!),
                                                style: theme.textTheme.bodySmall?.copyWith(
                                                  color: theme.colorScheme.onSurfaceVariant,
                                                ),
                                              ),
                                            ],
                                          ),
                                        ),
                                        IconButton.filledTonal(
                                          tooltip: 'اعتماد',
                                          onPressed: () async {
                                            await ref
                                                .read(adminRepositoryProvider)
                                                .approve(item.id);
                                            ref.invalidate(approvalsProvider);
                                            ref.invalidate(adminSummaryProvider);
                                          },
                                          icon: const Icon(Icons.check_rounded),
                                        ),
                                        const SizedBox(width: 6),
                                        IconButton(
                                          tooltip: 'رفض',
                                          onPressed: () async {
                                            await ref
                                                .read(adminRepositoryProvider)
                                                .reject(item.id, 'تم الرفض من الإدارة');
                                            ref.invalidate(approvalsProvider);
                                            ref.invalidate(adminSummaryProvider);
                                          },
                                          icon: const Icon(Icons.close_rounded),
                                        ),
                                      ],
                                    ),
                                  ),
                                ),
                              ),
                          ],
                        );
                      },
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

class _AdminMetric extends StatelessWidget {
  const _AdminMetric({
    required this.title,
    required this.value,
    required this.icon,
    required this.color,
  });

  final String title;
  final int value;
  final IconData icon;
  final Color color;

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(15),
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
              style: Theme.of(context).textTheme.bodySmall?.copyWith(
                    color: Theme.of(context).colorScheme.onSurfaceVariant,
                    fontWeight: FontWeight.w700,
                  ),
            ),
            Text(
              value.toString(),
              style: Theme.of(context).textTheme.titleLarge?.copyWith(
                    fontWeight: FontWeight.w900,
                  ),
            ),
          ],
        ),
      ),
    );
  }
}
