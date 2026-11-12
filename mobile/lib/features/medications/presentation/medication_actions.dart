import 'package:material_ui/material_ui.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../data/medication_repository.dart';
import '../../auth/presentation/auth_controller.dart';
class MedicationActions extends ConsumerWidget{
 const MedicationActions({super.key,required this.medicationId});final String medicationId;
 @override Widget build(BuildContext context,WidgetRef ref)=>Wrap(spacing:8,children:[
  OutlinedButton(onPressed:()=>_log(context,ref,'Taken'),child:const Text('تم تناولها')),
  OutlinedButton(onPressed:()=>_log(context,ref,'Missed'),child:const Text('لم تُتناول')),
  OutlinedButton(onPressed:()=>_schedule(context,ref),child:const Text('إضافة موعد')),
 ]);
 Future<void>_log(BuildContext c,WidgetRef ref,String status)async{
  final repo=MedicationRepository(ref.read(apiClientProvider));
  await repo.log(medicationId,DateTime.now().toUtc().toIso8601String(),status);
  if(c.mounted)ScaffoldMessenger.of(c).showSnackBar(const SnackBar(content:Text('تم تسجيل الجرعة.')));
 }
 Future<void>_schedule(BuildContext c,WidgetRef ref)async{
  final time=TextEditingController(text:'08:00:00');final frequency=TextEditingController(text:'Daily');
  final start=DateTime.now().toIso8601String().substring(0,10);
  final ok=await showDialog<bool>(context:c,builder:(_)=>AlertDialog(title:const Text('إضافة موعد'),content:Column(mainAxisSize:MainAxisSize.min,children:[
   TextField(controller:time,decoration:const InputDecoration(labelText:'الوقت HH:mm:ss')),
   TextField(controller:frequency,decoration:const InputDecoration(labelText:'التكرار')),
  ]),actions:[TextButton(onPressed:()=>Navigator.pop(c),child:const Text('إلغاء')),FilledButton(onPressed:()=>Navigator.pop(c,true),child:const Text('حفظ'))]));
  if(ok==true){await MedicationRepository(ref.read(apiClientProvider)).addSchedule(medicationId,time.text.trim(),frequency.text.trim(),start,null);if(c.mounted)ScaffoldMessenger.of(c).showSnackBar(const SnackBar(content:Text('تمت إضافة الموعد.')));}
 }
}