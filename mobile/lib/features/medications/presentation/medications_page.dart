import 'package:material_ui/material_ui.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../auth/presentation/auth_controller.dart';
import '../../core/widgets/app_states.dart';
import '../data/medication_repository.dart';

final medicationRepositoryProvider =
    Provider((ref) => MedicationRepository(ref.watch(apiClientProvider)));
final medicationsProvider = FutureProvider.autoDispose(
  (ref) => ref.watch(medicationRepositoryProvider).getAll(),
);
final adherenceProvider = FutureProvider.autoDispose(
  (ref) => ref.watch(medicationRepositoryProvider).adherence(),
);

class MedicationsPage extends ConsumerWidget {
  const MedicationsPage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final meds = ref.watch(medicationsProvider);
    final adherence = ref.watch(adherenceProvider);
    final theme = Theme.of(context);

    return Scaffold(
      appBar: AppBar(
        title: const Text('الأدوية'),
        actions: [
          IconButton(
            tooltip: 'تحديث',
            onPressed: () {
              ref.invalidate(medicationsProvider);
              ref.invalidate(adherenceProvider);
            },
            icon: const Icon(Icons.refresh_rounded),
          ),
        ],
      ),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () => _add(context, ref),
        icon: const Icon(Icons.add_rounded),
        label: const Text('إضافة دواء'),
      ),
      body: LayoutBuilder(
        builder: (context, constraints) {
          final wide = constraints.maxWidth >= 900;
          return ListView(
            padding: EdgeInsets.fromLTRB(wide ? 32 : 18, 18, wide ? 32 : 18, 90),
            children: [
              ConstrainedBox(
                constraints: const BoxConstraints(maxWidth: 1000),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    adherence.when(
                      loading: () => const LinearProgressIndicator(),
                      error: (_, _) => const SizedBox.shrink(),
                      data: (value) => Card(
                        child: Padding(
                          padding: const EdgeInsets.all(18),
                          child: Row(
                            children: [
                              Container(
                                width: 48,
                                height: 48,
                                decoration: BoxDecoration(
                                  color: theme.colorScheme.primaryContainer,
                                  borderRadius: BorderRadius.circular(16),
                                ),
                                child: Icon(
                                  Icons.fact_check_outlined,
                                  color: theme.colorScheme.primary,
                                ),
                              ),
                              const SizedBox(width: 14),
                              Expanded(
                                child: Column(
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  children: [
                                    const Text(
                                      'الالتزام الدوائي',
                                      style: TextStyle(fontWeight: FontWeight.w900),
                                    ),
                                    const SizedBox(height: 4),
                                    Text(
                                      'آخر ' + value.days.toString() + ' أيام',
                                      style: theme.textTheme.bodySmall?.copyWith(
                                        color: theme.colorScheme.onSurfaceVariant,
                                      ),
                                    ),
                                    const SizedBox(height: 9),
                                    ClipRRect(
                                      borderRadius: BorderRadius.circular(8),
                                      child: LinearProgressIndicator(
                                        value: (value.percentage / 100).clamp(0, 1),
                                        minHeight: 7,
                                      ),
                                    ),
                                  ],
                                ),
                              ),
                              const SizedBox(width: 16),
                              Text(
                                value.percentage.toStringAsFixed(0) + '%',
                                style: theme.textTheme.titleLarge?.copyWith(
                                  fontWeight: FontWeight.w900,
                                  color: theme.colorScheme.primary,
                                ),
                              ),
                            ],
                          ),
                        ),
                      ),
                    ),
                    const SizedBox(height: 22),
                    const SectionHeader(
                      title: 'قائمة الأدوية',
                      subtitle: 'فعّل أو أوقف الدواء من هنا',
                    ),
                    const SizedBox(height: 10),
                    meds.when(
                      loading: () => const AppLoadingState(label: 'جارٍ تحميل الأدوية...'),
                      error: (_, _) => AppErrorState(
                        message: 'تعذر تحميل قائمة الأدوية.',
                        onRetry: () => ref.invalidate(medicationsProvider),
                      ),
                      data: (items) {
                        if (items.isEmpty) {
                          return const AppEmptyState(
                            icon: Icons.medication_outlined,
                            title: 'لا توجد أدوية مضافة',
                            message: 'أضف أول دواء لتبدأ بتنظيم قائمة العلاج.',
                          );
                        }
                        return Column(
                          children: [
                            for (final medication in items)
                              Padding(
                                padding: const EdgeInsets.only(bottom: 10),
                                child: Card(
                                  child: ListTile(
                                    contentPadding: const EdgeInsets.symmetric(
                                      horizontal: 16,
                                      vertical: 7,
                                    ),
                                    leading: Container(
                                      width: 46,
                                      height: 46,
                                      decoration: BoxDecoration(
                                        color: medication.isActive
                                            ? theme.colorScheme.primaryContainer
                                            : theme.colorScheme.surfaceContainerHighest,
                                        borderRadius: BorderRadius.circular(15),
                                      ),
                                      child: Icon(
                                        Icons.medication_rounded,
                                        color: medication.isActive
                                            ? theme.colorScheme.primary
                                            : theme.colorScheme.onSurfaceVariant,
                                      ),
                                    ),
                                    title: Text(
                                      medication.name,
                                      style: const TextStyle(fontWeight: FontWeight.w900),
                                    ),
                                    subtitle: Padding(
                                      padding: const EdgeInsets.only(top: 4),
                                      child: Text(
                                        [
                                          medication.dosage,
                                          medication.instructions,
                                        ].whereType<String>().where((x) => x.isNotEmpty).join(' • '),
                                      ),
                                    ),
                                    trailing: Switch.adaptive(
                                      value: medication.isActive,
                                      onChanged: (value) async {
                                        await ref
                                            .read(medicationRepositoryProvider)
                                            .setStatus(medication.id, value);
                                        ref.invalidate(medicationsProvider);
                                        ref.invalidate(adherenceProvider);
                                      },
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

  Future<void> _add(BuildContext context, WidgetRef ref) async {
    final name = TextEditingController();
    final dose = TextEditingController();
    final instructions = TextEditingController();

    final ok = await showDialog<bool>(
      context: context,
      builder: (dialogContext) => AlertDialog(
        title: const Text('إضافة دواء'),
        content: SingleChildScrollView(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              TextField(
                controller: name,
                decoration: const InputDecoration(
                  labelText: 'اسم الدواء',
                  prefixIcon: Icon(Icons.medication_outlined),
                ),
              ),
              const SizedBox(height: 12),
              TextField(
                controller: dose,
                decoration: const InputDecoration(
                  labelText: 'الجرعة',
                  prefixIcon: Icon(Icons.straighten_outlined),
                ),
              ),
              const SizedBox(height: 12),
              TextField(
                controller: instructions,
                maxLines: 2,
                decoration: const InputDecoration(
                  labelText: 'التعليمات',
                  prefixIcon: Icon(Icons.notes_outlined),
                ),
              ),
            ],
          ),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(dialogContext),
            child: const Text('إلغاء'),
          ),
          FilledButton.icon(
            onPressed: () => Navigator.pop(dialogContext, true),
            icon: const Icon(Icons.check_rounded),
            label: const Text('حفظ'),
          ),
        ],
      ),
    );

    if (ok == true && name.text.trim().isNotEmpty) {
      await ref.read(medicationRepositoryProvider).create(
            name.text.trim(),
            dose.text.trim().isEmpty ? null : dose.text.trim(),
            instructions.text.trim().isEmpty ? null : instructions.text.trim(),
          );
      ref.invalidate(medicationsProvider);
      ref.invalidate(adherenceProvider);
    }

    name.dispose();
    dose.dispose();
    instructions.dispose();
  }
}
