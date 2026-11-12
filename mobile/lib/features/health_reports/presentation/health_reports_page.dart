import 'package:material_ui/material_ui.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../data/health_report_repository.dart';
import '../../auth/presentation/auth_controller.dart';
final healthReportRepositoryProvider=Provider((ref)=>HealthReportRepository(ref.watch(apiClientProvider)));
final healthReportsProvider=FutureProvider.autoDispose((ref)=>ref.watch(healthReportRepositoryProvider).all());
class HealthReportsPage extends ConsumerWidget {
 const HealthReportsPage({super.key});
 @override Widget build(BuildContext context,WidgetRef ref){
  final state=ref.watch(healthReportsProvider);
  return Scaffold(appBar:AppBar(title:const Text('التقارير الصحية')),body:state.when(
   loading:()=>const Center(child:CircularProgressIndicator()),
   error:(_,_)=>Center(child:FilledButton(onPressed:()=>ref.invalidate(healthReportsProvider),child:const Text('إعادة المحاولة'))),
   data:(items)=>ListView(padding:const EdgeInsets.all(20),children:[
    FilledButton.icon(onPressed:()=>_create(context,ref),icon:const Icon(Icons.add_chart),label:const Text('إنشاء تقرير جديد')),
    const SizedBox(height:12),
    if(items.isEmpty) const Text('لا توجد تقارير محفوظة.') else ...items.map((x)=>Card(child:ListTile(
     title:Text(x.startDate+' → '+x.endDate),
     subtitle:Text('القراءات: '+x.number.toString()+' • المتوسط: '+(x.average?.toStringAsFixed(1)??'—')),
     trailing:Text(x.stability==null?'—':x.stability!.toStringAsFixed(0)+'%'),
    ))),
   ]),
  ));
 }
 Future<void>_create(BuildContext context,WidgetRef ref)async{
  final now=DateTime.now();
  final start=await showDatePicker(context:context,initialDate:now.subtract(const Duration(days:7)),firstDate:DateTime(2020),lastDate:now);
  if(start==null||!context.mounted)return;
  final end=await showDatePicker(context:context,initialDate:now,firstDate:start,lastDate:now);
  if(end==null||!context.mounted)return;
  await ref.read(healthReportRepositoryProvider).create(start.toIso8601String().substring(0,10),end.toIso8601String().substring(0,10));
  ref.invalidate(healthReportsProvider);
  if(context.mounted)ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content:Text('تم إنشاء التقرير.')));
 }
}