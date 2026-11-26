import 'package:material_ui/material_ui.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../auth/presentation/auth_controller.dart';
import '../../core/widgets/app_states.dart';
import '../data/notification_repository.dart';

final notificationRepositoryProvider =
    Provider((ref) => NotificationRepository(ref.watch(apiClientProvider)));
final notificationsProvider = FutureProvider.autoDispose(
  (ref) => ref.watch(notificationRepositoryProvider).get(),
);

class NotificationsPage extends ConsumerWidget {
  const NotificationsPage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final state = ref.watch(notificationsProvider);

    return Scaffold(
      appBar: AppBar(
        title: const Text('الإشعارات'),
        actions: [
          TextButton.icon(
            onPressed: () async {
              await ref.read(notificationRepositoryProvider).readAll();
              ref.invalidate(notificationsProvider);
            },
            icon: const Icon(Icons.done_all_rounded, size: 18),
            label: const Text('قراءة الكل'),
          ),
          const SizedBox(width: 6),
        ],
      ),
      body: state.when(
        loading: () => const AppLoadingState(label: 'جارٍ تحميل الإشعارات...'),
        error: (_, _) => AppErrorState(
          message: 'تعذر تحميل الإشعارات.',
          onRetry: () => ref.invalidate(notificationsProvider),
        ),
        data: (items) {
          final unread = items.where((item) => !item.isRead).length;
          if (items.isEmpty) {
            return const AppEmptyState(
              icon: Icons.notifications_none_rounded,
              title: 'لا توجد إشعارات',
              message: 'ستظهر هنا رسائل المتابعة والتنبيهات الجديدة.',
            );
          }

          return ListView(
            padding: const EdgeInsets.fromLTRB(18, 18, 18, 28),
            children: [
              if (unread > 0)
                Card(
                  color: Theme.of(context).colorScheme.primaryContainer,
                  child: ListTile(
                    leading: const Icon(Icons.notifications_active_rounded),
                    title: Text(
                      unread.toString() + ' إشعارات غير مقروءة',
                      style: const TextStyle(fontWeight: FontWeight.w900),
                    ),
                    subtitle: const Text('راجع الرسائل المهمة أولًا.'),
                  ),
                ),
              if (unread > 0) const SizedBox(height: 16),
              ...items.map(
                (item) => Padding(
                  padding: const EdgeInsets.only(bottom: 10),
                  child: Card(
                    color: item.isRead
                        ? null
                        : Theme.of(context).colorScheme.surfaceContainerLow,
                    child: ListTile(
                      contentPadding: const EdgeInsets.symmetric(
                        horizontal: 16,
                        vertical: 8,
                      ),
                      leading: Container(
                        width: 44,
                        height: 44,
                        decoration: BoxDecoration(
                          color: item.isRead
                              ? Theme.of(context).colorScheme.surfaceContainerHighest
                              : Theme.of(context).colorScheme.primaryContainer,
                          borderRadius: BorderRadius.circular(15),
                        ),
                        child: Icon(
                          item.isRead
                              ? Icons.notifications_none_rounded
                              : Icons.notifications_active_rounded,
                          color: item.isRead
                              ? Theme.of(context).colorScheme.onSurfaceVariant
                              : Theme.of(context).colorScheme.primary,
                        ),
                      ),
                      title: Text(
                        item.title,
                        style: TextStyle(
                          fontWeight: item.isRead ? FontWeight.w600 : FontWeight.w900,
                        ),
                      ),
                      subtitle: Padding(
                        padding: const EdgeInsets.only(top: 5),
                        child: Text(
                          item.body,
                          style: const TextStyle(height: 1.4),
                        ),
                      ),
                      trailing: item.isRead
                          ? null
                          : Icon(
                              Icons.circle,
                              size: 9,
                              color: Theme.of(context).colorScheme.primary,
                            ),
                      onTap: item.isRead
                          ? null
                          : () async {
                              await ref
                                  .read(notificationRepositoryProvider)
                                  .read(item.id);
                              ref.invalidate(notificationsProvider);
                            },
                    ),
                  ),
                ),
              ),
            ],
          );
        },
      ),
    );
  }
}
