import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../auth/presentation/auth_controller.dart';

class HomePage extends ConsumerWidget {
  const HomePage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final role = ref.watch(authControllerProvider).role;

    return Scaffold(
      appBar: AppBar(
        title: const Text('الرئيسية'),
        actions: [
          IconButton(
            tooltip: 'تسجيل الخروج',
            onPressed: () =>
                ref.read(authControllerProvider.notifier).logout(),
            icon: const Icon(Icons.logout),
          ),
        ],
      ),
      body: Center(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Text(
            role == 'Doctor'
                ? 'مرحبًا بك في لوحة الطبيب'
                : 'مرحبًا بك في منصة السكري الذكية',
            textAlign: TextAlign.center,
            style: Theme.of(context).textTheme.headlineSmall,
          ),
        ),
      ),
    );
  }
}
