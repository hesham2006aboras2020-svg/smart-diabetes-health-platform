import 'package:material_ui/material_ui.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../auth/presentation/auth_controller.dart';
import 'patient_dashboard_page.dart';
import '../../medications/presentation/medications_page.dart';
import '../../notifications/presentation/notifications_page.dart';
import '../../profile/presentation/profile_page.dart';
import '../../health_reports/presentation/health_reports_page.dart';
import '../../glucose/presentation/glucose_analytics_page.dart';
import '../../doctor/presentation/doctor_dashboard_page.dart';
import '../../admin/presentation/admin_dashboard_page.dart';

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
          ? role == 'Doctor' ? const DoctorDashboardPage() : role == 'Admin' ? const AdminDashboardPage() : Column(children: [const Expanded(child: PatientDashboardPage()), SingleChildScrollView(scrollDirection: Axis.horizontal, padding: const EdgeInsets.all(12), child: Row(children: [ _ActionButton(icon: Icons.medication_outlined, label: 'الأدوية', page: const MedicationsPage()), _ActionButton(icon: Icons.notifications_outlined, label: 'الإشعارات', page: const NotificationsPage()), _ActionButton(icon: Icons.person_outline, label: 'الملف الشخصي', page: const ProfilePage()), _ActionButton(icon: Icons.assessment_outlined, label: 'التقارير', page: const HealthReportsPage()), _ActionButton(icon: Icons.show_chart, label: 'التحليل', page: const GlucoseAnalyticsPage())]))])
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


class _ActionButton extends StatelessWidget { const _ActionButton({required this.icon,required this.label,required this.page}); final IconData icon; final String label; final Widget page; @override Widget build(BuildContext context)=>Padding(padding:const EdgeInsets.only(bottom:10),child:OutlinedButton.icon(onPressed:()=>Navigator.of(context).push(MaterialPageRoute(builder:(_)=>page)),icon:Icon(icon),label:Text(label))); }
