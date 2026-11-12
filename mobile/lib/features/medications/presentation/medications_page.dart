import 'package:material_ui/material_ui.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../data/medication_repository.dart';
import '../../auth/presentation/auth_controller.dart';
final medicationRepositoryProvider=Provider((ref)=>MedicationRepository(ref.watch(apiClientProvider)));
final medicationsProvider=FutureProvider.autoDispose((ref)=>ref.watch(medicationRepositoryProvider).getAll());
final adherenceProvider=FutureProvider.autoDispose((ref)=>ref.watch(medicationRepositoryProvider).adherence());
class MedicationsPage extends ConsumerWidget{
 const MedicationsPage({super.key});
 @override Widget build(BuildContext context,WidgetRef ref){
  final meds=ref.watch(medicationsProvider),a=ref.watch(adherenceProvider);
  return Scaffold(appBar:AppBar(title:const Text('الأدوية')),floatingActionButton:FloatingActionButton.extended(onPressed:()=>_add(context,ref),icon:const Icon(Icons.add),label:const Text('إضافة دواء')),
   body:ListView(padding:const EdgeInsets.all(20),children:[
    a.when(loading:()=>const LinearProgressIndicator(),error:(_,_)=>const SizedBox(),data:(x)=>Card(child:ListTile(title:const Text('الالتزام بالأدوية'),subtitle:Text('آخر '+x.days.toString()+' أيام • '+x.taken.toString()+' جرعات مأخوذة من '+x.total.toString()),trailing:Text(x.percentage.toStringAsFixed(0)+'%')))),
    const SizedBox(height:12),
    meds.when(loading:()=>const Center(child:CircularProgressIndicator()),error:(_,_)=>const Center(child:Text('تعذر تحميل الأدوية.')),data:(items){
     if(items.isEmpty)return const Card(child:Padding(padding:EdgeInsets.all(20),child:Text('لا توجد أدوية مضافة حتى الآن.')));
     return Column(children:items.map((m)=>Card(child:ListTile(title:Text(m.name),subtitle:Text([m.dosage,m.instructions].whereType<String>().where((s)=>s.isNotEmpty).join(' • ')),trailing:Switch(value:m.isActive,onChanged:(v)async{await ref.read(medicationRepositoryProvider).setStatus(m.id,v);ref.invalidate(medicationsProvider);}))).toList());
    }),
   ]));
 }
 Future<void>_add(BuildContext context,WidgetRef ref)async{
  final name=TextEditingController(),dose=TextEditingController(),instructions=TextEditingController();
  final ok=await showDialog<bool>(context:context,builder:(_)=>AlertDialog(title:const Text('إضافة دواء'),content:SingleChildScrollView(child:Column(children:[
   TextField(controller:name,decoration:const InputDecoration(labelText:'اسم الدواء')),
   TextField(controller:dose,decoration:const InputDecoration(labelText:'الجرعة')),
   TextField(controller:instructions,decoration:const InputDecoration(labelText:'التعليمات')),
  ])),actions:[TextButton(onPressed:()=>Navigator.pop(context),child:const Text('إلغاء')),FilledButton(onPressed:()=>Navigator.pop(context,true),child:const Text('حفظ'))]));
  if(ok==true&&name.text.trim().isNotEmpty){await ref.read(medicationRepositoryProvider).create(name.text.trim(),dose.text.trim().isEmpty?null:dose.text.trim(),instructions.text.trim().isEmpty?null:instructions.text.trim());ref.invalidate(medicationsProvider);ref.invalidate(adherenceProvider);}
 }
}