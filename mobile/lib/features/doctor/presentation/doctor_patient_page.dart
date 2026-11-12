import 'package:material_ui/material_ui.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'doctor_dashboard_page.dart';
import 'patient_notes_button.dart';

class DoctorPatientPage extends ConsumerWidget {
 const DoctorPatientPage({super.key, required this.patientId});
 final String patientId;
 @override Widget build(BuildContext context, WidgetRef ref) {
  final provider = FutureProvider.autoDispose((ref) => ref.watch(doctorRepositoryProvider).details(patientId));
  final state = ref.watch(provider);
  return Scaffold(
   appBar: AppBar(title: const Text('ملف المريض')),
   body: state.when(
    loading: () => const Center(child: CircularProgressIndicator()),
    error: (_, _) => const Center(child: Text('تعذر تحميل بيانات المريض.')),
    data: (p) => ListView(padding: const EdgeInsets.all(20), children: [
     Text(p.name, style: Theme.of(context).textTheme.headlineSmall), Text(p.email),
     const SizedBox(height: 20),
     _metric('آخر قراءة', p.lastReading?.toStringAsFixed(0) ?? '—'),
     _metric('متوسط 7 أيام', p.average?.toStringAsFixed(0) ?? '—'),
     _metric('ضمن النطاق', (p.inRange?.toStringAsFixed(0) ?? '—')+'%'),
     _metric('الاتجاه', _trend(p.trend)),
     _metric('الأدوية النشطة', p.activeMeds.toString()),
     _metric('التنبيهات غير المقروءة', p.unreadAlerts.toString()),
     const SizedBox(height: 12), PatientNotesButton(patientId: patientId),
    ]),
   ),
  );
 }
 Widget _metric(String title,String value)=>Card(child:ListTile(title:Text(title),trailing:Text(value)));
 String _trend(String v)=>switch(v){'Improving'=>'تحسن','Worsening'=>'تراجع','Stable'=>'مستقر',_=>'غير كافٍ'};
}