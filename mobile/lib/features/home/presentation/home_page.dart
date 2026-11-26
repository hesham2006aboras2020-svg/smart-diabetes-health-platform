import 'package:material_ui/material_ui.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../auth/presentation/auth_controller.dart';
import '../../medications/presentation/medications_page.dart';
import '../../notifications/presentation/notifications_page.dart';
import '../../profile/presentation/profile_page.dart';
import '../../health_reports/presentation/health_reports_page.dart';
import '../../glucose/presentation/glucose_analytics_page.dart';
import '../../doctor/presentation/doctor_dashboard_page.dart';
import '../../admin/presentation/admin_dashboard_page.dart';
import '../../alerts/presentation/alerts_page.dart';
import 'patient_dashboard_page.dart';

class HomePage extends ConsumerWidget {
  const HomePage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final role = ref.watch(authControllerProvider).role;

    if (role == 'Doctor') return const DoctorDashboardPage();
    if (role == 'Admin') return const AdminDashboardPage();
    return _PatientHome(
      onLogout: () => ref.read(authControllerProvider.notifier).logout(),
    );
  }
}

class _PatientHome extends StatelessWidget {
  const _PatientHome({required this.onLogout});

  final VoidCallback onLogout;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      appBar: AppBar(
        titleSpacing: 20,
        title: Row(
          children: [
            Container(
              width: 38,
              height: 38,
              decoration: BoxDecoration(
                color: theme.colorScheme.primary,
                borderRadius: BorderRadius.circular(13),
              ),
              child: const Icon(
                Icons.monitor_heart_outlined,
                color: Colors.white,
                size: 22,
              ),
            ),
            const SizedBox(width: 10),
            const Text(
              'منصة السكري الذكية',
              style: TextStyle(fontWeight: FontWeight.w800, fontSize: 17),
            ),
          ],
        ),
        actions: [
          IconButton(
            tooltip: 'تسجيل الخروج',
            onPressed: onLogout,
            icon: const Icon(Icons.logout_rounded),
          ),
          const SizedBox(width: 8),
        ],
      ),
      body: Column(
        children: [
          const Expanded(child: PatientDashboardPage()),
          Container(
            decoration: BoxDecoration(
              color: theme.colorScheme.surface,
              border: Border(
                top: BorderSide(
                  color: theme.colorScheme.outlineVariant.withValues(alpha: 0.45),
                ),
              ),
            ),
            padding: const EdgeInsets.fromLTRB(16, 12, 16, 14),
            child: SingleChildScrollView(
              scrollDirection: Axis.horizontal,
              child: Row(
                children: [
                  _ActionCard(
                    icon: Icons.medication_outlined,
                    label: 'الأدوية',
                    page: const MedicationsPage(),
                  ),
                  _ActionCard(
                    icon: Icons.notifications_none_rounded,
                    label: 'الإشعارات',
                    page: const NotificationsPage(),
                  ),
                  _ActionCard(
                    icon: Icons.person_outline_rounded,
                    label: 'الملف الشخصي',
                    page: const ProfilePage(),
                  ),
                  _ActionCard(
                    icon: Icons.assessment_outlined,
                    label: 'التقارير',
                    page: const HealthReportsPage(),
                  ),
                  _ActionCard(
                    icon: Icons.insights_rounded,
                    label: 'التحليل',
                    page: const GlucoseAnalyticsPage(),
                  ),
                  _ActionCard(
                    icon: Icons.warning_amber_rounded,
                    label: 'التنبيهات',
                    page: const AlertsPage(),
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _ActionCard extends StatelessWidget {
  const _ActionCard({
    required this.icon,
    required this.label,
    required this.page,
  });

  final IconData icon;
  final String label;
  final Widget page;

  @override
  Widget build(BuildContext context) {
    final color = Theme.of(context).colorScheme.primary;

    return Padding(
      padding: const EdgeInsetsDirectional.only(end: 10),
      child: InkWell(
        borderRadius: BorderRadius.circular(18),
        onTap: () => Navigator.of(context).push(
          MaterialPageRoute(builder: (_) => page),
        ),
        child: Ink(
          width: 104,
          padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 11),
          decoration: BoxDecoration(
            color: color.withValues(alpha: 0.07),
            borderRadius: BorderRadius.circular(18),
          ),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Icon(icon, color: color, size: 24),
              const SizedBox(height: 6),
              Text(
                label,
                maxLines: 1,
                overflow: TextOverflow.ellipsis,
                textAlign: TextAlign.center,
                style: const TextStyle(fontWeight: FontWeight.w700),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
