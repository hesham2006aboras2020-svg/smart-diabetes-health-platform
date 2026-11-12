import 'package:material_ui/material_ui.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../data/doctor_repository.dart';
import '../../auth/presentation/auth_controller.dart';
import 'doctor_patient_page.dart';

final doctorRepositoryProvider = Provider((ref) => DoctorRepository(ref.watch(apiClientProvider)));
final doctorPatientsProvider = FutureProvider.autoDispose((ref) => ref.watch(doctorRepositoryProvider).patients());

class DoctorDashboardPage extends ConsumerWidget {
 const DoctorDashboardPage({super.key});
 @override Widget build(BuildContext context, WidgetRef ref) {
  final state = ref.watch(doctorPatientsProvider);
  return Scaffold(
   appBar: AppBar(title: const Text('متابعة المرضى')),
   body: state.when(
    loading: () => const Center(child: CircularProgressIndicator()),
    error: (_, _) => Center(child: FilledButton(onPressed: () => ref.invalidate(doctorPatientsProvider), child: const Text('إعادة المحاولة'))),
    data: (items) => items.isEmpty
      ? const Center(child: Text('لا يوجد مرضى مرتبطون بك حتى الآن.'))
      : ListView.separated(
       padding: const EdgeInsets.all(16), itemCount: items.length,
       separatorBuilder: (_, _) => const SizedBox(height: 8),
       itemBuilder: (_, i) => Card(child: ListTile(
        title: Text(items[i].name),
        subtitle: Text('المتوسط: '+(items[i].average?.toStringAsFixed(0) ?? '—')+' • ضمن النطاق: '+(items[i].inRange?.toStringAsFixed(0) ?? '—')+'%'),
        trailing: Text(items[i].priority),
        onTap: () => Navigator.of(context).push(MaterialPageRoute(builder: (_) => DoctorPatientPage(patientId: items[i].id))),
       )),
      ),
   ),
  );
 }
}