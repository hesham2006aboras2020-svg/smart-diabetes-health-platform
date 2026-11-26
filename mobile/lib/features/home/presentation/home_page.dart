import 'package:material_ui/material_ui.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../admin/presentation/admin_dashboard_page.dart';
import '../../alerts/presentation/alerts_page.dart';
import '../../auth/presentation/auth_controller.dart';
import '../../doctor/presentation/doctor_dashboard_page.dart';
import '../../glucose/presentation/glucose_analytics_page.dart';
import '../../health_reports/presentation/health_reports_page.dart';
import '../../medications/presentation/medications_page.dart';
import '../../notifications/presentation/notifications_page.dart';
import '../../profile/presentation/profile_page.dart';
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

class _PatientHome extends StatefulWidget {
  const _PatientHome({required this.onLogout});

  final VoidCallback onLogout;

  @override
  State<_PatientHome> createState() => _PatientHomeState();
}

class _PatientHomeState extends State<_PatientHome> {
  int _index = 0;

  List<Widget> get _pages => const [
        PatientDashboardPage(),
        MedicationsPage(),
        NotificationsPage(),
        ProfilePage(),
      ];

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final pages = _pages;
    final wide = MediaQuery.sizeOf(context).width >= 900;

    return Scaffold(
      appBar: AppBar(
        titleSpacing: 18,
        title: Row(
          children: [
            Container(
              width: 38,
              height: 38,
              decoration: BoxDecoration(
                gradient: LinearGradient(
                  colors: [
                    theme.colorScheme.primary,
                    Color.lerp(theme.colorScheme.primary, Colors.black, 0.18)!,
                  ],
                ),
                borderRadius: BorderRadius.circular(13),
              ),
              child: const Icon(
                Icons.monitor_heart_rounded,
                color: Colors.white,
                size: 22,
              ),
            ),
            const SizedBox(width: 10),
            const Text(
              'صحتي',
              style: TextStyle(fontWeight: FontWeight.w900, fontSize: 19),
            ),
          ],
        ),
        actions: [
          PopupMenuButton<String>(
            tooltip: 'المزيد',
            icon: const Icon(Icons.apps_rounded),
            onSelected: (value) {
              final page = switch (value) {
                'reports' => const HealthReportsPage(),
                'analytics' => const GlucoseAnalyticsPage(),
                _ => const AlertsPage(),
              };
              Navigator.of(context).push(
                MaterialPageRoute(builder: (_) => page),
              );
            },
            itemBuilder: (context) => const [
              PopupMenuItem(
                value: 'analytics',
                child: ListTile(
                  leading: Icon(Icons.insights_rounded),
                  title: Text('تحليل القراءات'),
                ),
              ),
              PopupMenuItem(
                value: 'reports',
                child: ListTile(
                  leading: Icon(Icons.assessment_outlined),
                  title: Text('التقارير الصحية'),
                ),
              ),
              PopupMenuItem(
                value: 'alerts',
                child: ListTile(
                  leading: Icon(Icons.warning_amber_rounded),
                  title: Text('التنبيهات'),
                ),
              ),
            ],
          ),
          IconButton(
            tooltip: 'تسجيل الخروج',
            onPressed: widget.onLogout,
            icon: const Icon(Icons.logout_rounded),
          ),
          const SizedBox(width: 6),
        ],
      ),
      body: Row(
        children: [
          if (wide)
            NavigationRail(
              selectedIndex: _index,
              onDestinationSelected: (value) => setState(() => _index = value),
              labelType: NavigationRailLabelType.all,
              backgroundColor: Colors.white,
              destinations: const [
                NavigationRailDestination(
                  icon: Icon(Icons.home_outlined),
                  selectedIcon: Icon(Icons.home_rounded),
                  label: Text('الرئيسية'),
                ),
                NavigationRailDestination(
                  icon: Icon(Icons.medication_outlined),
                  selectedIcon: Icon(Icons.medication_rounded),
                  label: Text('الأدوية'),
                ),
                NavigationRailDestination(
                  icon: Icon(Icons.notifications_none_rounded),
                  selectedIcon: Icon(Icons.notifications_rounded),
                  label: Text('الإشعارات'),
                ),
                NavigationRailDestination(
                  icon: Icon(Icons.person_outline_rounded),
                  selectedIcon: Icon(Icons.person_rounded),
                  label: Text('الملف'),
                ),
              ],
            ),
          Expanded(
            child: IndexedStack(
              index: _index,
              children: pages,
            ),
          ),
        ],
      ),
      bottomNavigationBar: wide
          ? null
          : NavigationBar(
              selectedIndex: _index,
              onDestinationSelected: (value) => setState(() => _index = value),
              destinations: const [
                NavigationDestination(
                  icon: Icon(Icons.home_outlined),
                  selectedIcon: Icon(Icons.home_rounded),
                  label: 'الرئيسية',
                ),
                NavigationDestination(
                  icon: Icon(Icons.medication_outlined),
                  selectedIcon: Icon(Icons.medication_rounded),
                  label: 'الأدوية',
                ),
                NavigationDestination(
                  icon: Icon(Icons.notifications_none_rounded),
                  selectedIcon: Icon(Icons.notifications_rounded),
                  label: 'الإشعارات',
                ),
                NavigationDestination(
                  icon: Icon(Icons.person_outline_rounded),
                  selectedIcon: Icon(Icons.person_rounded),
                  label: 'الملف',
                ),
              ],
            ),
    );
  }
}
