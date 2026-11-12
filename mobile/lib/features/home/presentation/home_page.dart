import 'package:material_ui/material_ui.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../auth/presentation/auth_controller.dart';
import 'patient_dashboard_page.dart';

class HomePage extends ConsumerWidget {
  const HomePage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final role = ref.watch(authControllerProvider).role;

    return Scaffold(
      appBar: AppBar(
        title: Text(role == 'Doctor' ? 'لوحة الطبيب' : 'الرئيسية'),
        actions: [
          IconButton(
            tooltip: 'تسجيل الخروج',
            onPressed: () =>
                ref.read(authControllerProvider.notifier).logout(),
            icon: const Icon(Icons.logout),
          ),
        ],
      ),
      body: role == 'Patient'
          ? const PatientDashboardPage()
          : const Center(
              child: Padding(
                padding: EdgeInsets.all(24),
                child: Text(
                  'مرحبًا بك في لوحة الطبيب. سيتم تجهيز أدوات المتابعة في الخطوة التالية.',
                  textAlign: TextAlign.center,
                ),
              ),
            ),
    );
  }
}
